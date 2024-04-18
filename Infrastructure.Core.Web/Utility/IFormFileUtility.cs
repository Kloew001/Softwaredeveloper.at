using Microsoft.AspNetCore.Http;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Utility
{
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
}
