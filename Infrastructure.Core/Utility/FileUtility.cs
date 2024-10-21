namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public class FileInfo
{
    public string FileName { get; set; }
    public string FileContentType { get; set; }
    public byte[] Content { get; set; }
}

public static class FileUtiltiy
{
    public static byte[] GetContent(string filePath)
    {
        byte[] content;

        using (var fileStream = File.OpenRead(filePath))
        using (var memoryStream = new MemoryStream())
        {
            fileStream.CopyTo(memoryStream);
            content = memoryStream.ToArray();
        }

        return content;
    }
}
