using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class EncodingUtility
{
    public static Encoding DetectEncoding(byte[] content)
    {
        if (content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF)
        {
            return Encoding.UTF8; // UTF-8 with BOM
        }
        else if (content.Length >= 2 && content[0] == 0xFF && content[1] == 0xFE)
        {
            return Encoding.Unicode; // UTF-16 LE
        }
        else if (content.Length >= 2 && content[0] == 0xFE && content[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode; // UTF-16 BE
        }

        try
        {
            Encoding utf8 = new UTF8Encoding(false, true);
            utf8.GetString(content);
            return utf8;
        }
        catch
        {
            try
            {
                var ansi = Encoding.GetEncoding(1252); // ANSI encoding (Windows-1252)
                ansi.GetString(content);
                return ansi;
            }
            catch
            {
                return Encoding.Default;
            }
        }
    }
}