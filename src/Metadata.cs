public static class Metadata
{
    public static FileData[] GetJPEGSize(
        string[] paths
    )
    {
        return paths.Select(path =>
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            var soi = new byte[2];
            fs.ReadExactly(soi, 0, 2);
            if (soi[0] != 0xFF || soi[1] != 0xD8)
                throw new Exception("Not a damn jpeg!");

            var marker = new byte[2];
            while (fs.Read(marker, 0, 2) == 2)
            {
                if (marker[0] != 0xFF)
                    throw new Exception("Invalid marker");

                if (marker[1] == 0xD9)
                    throw new Exception("Reached end of file prematurely.");

                if (marker[1] == 0xC0 || marker[1] == 0xC1 || marker[1] == 0xC2)
                {
                    var sofData = new byte[7];
                    fs.ReadExactly(sofData, 0, 7);

                    int height = (sofData[3] << 8) | sofData[4];
                    int width = (sofData[5] << 8) | sofData[6];

                    return new FileData(path, width, height);
                }

                var lengthBytes = new byte[2];
                fs.ReadExactly(lengthBytes, 0, 2);
                int length = (lengthBytes[0] << 8) | lengthBytes[1];
                fs.Seek(length - 2, SeekOrigin.Current);
            }

            throw new Exception("No size markers found");
        }).ToArray();
    }
    
    public sealed record FileData(string Path, int Height, int Width);
}
