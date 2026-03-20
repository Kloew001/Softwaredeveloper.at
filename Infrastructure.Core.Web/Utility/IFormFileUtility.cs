using Microsoft.AspNetCore.Http;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Utility;

public static class IFormFileUtilityExtension
{
    public static Core.Utility.FileInfo GetFileInfo(this IFormFile file)
    {
        return new Core.Utility.FileInfo
        {
            FileName = file.FileName,
            FileContentType = file.ContentType,
            Content = file.GetContent()
        };
    }

    public static Core.Utility.FileInfo GetFileInfo(this BinaryContent binaryContent)
    {
        return new Core.Utility.FileInfo
        {
            FileName = binaryContent.Name,
            FileContentType = binaryContent.MimeType,
            Content = binaryContent.Data.Bytes
        };
    }

    public static byte[] GetContent(this IFormFile file)
    {
        byte[] content;

        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            content = memoryStream.ToArray();
        }

        return content;
    }
}