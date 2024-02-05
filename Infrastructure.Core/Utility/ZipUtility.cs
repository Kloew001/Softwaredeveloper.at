using System.Formats.Asn1;
using System.IO.Compression;
using System.Text;

using static SoftwaredeveloperDotAt.Infrastructure.Core.Utility.ZipUtility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public class ZipUtility
    {
        public class ZipEntryInfo
        {
            public string EntryName { get; set; }
            public byte[] Content { get; set; }
        }

        public static byte[] CreateZip(IEnumerable<ZipEntryInfo> zipEntryInfos, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding(true); //.UTF8;

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding: encoding))
                {
                    foreach (var zipEntryInfo in zipEntryInfos)
                    {
                        var entry = zipArchive.CreateEntry(zipEntryInfo.EntryName);

                        using (var entryStream = entry.Open())
                        {
                            entryStream.Write(zipEntryInfo.Content, 0, zipEntryInfo.Content.Length);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
