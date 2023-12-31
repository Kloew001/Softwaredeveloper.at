using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class StringUtility
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string GenerateRandomString(string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", int length = 6)
        {
            var randomString = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < length; i++)
                randomString.Append(chars[random.Next(chars.Length)]);

            return randomString.ToString();
        }

        public static IReadOnlyDictionary<string, string> SPECIAL_DIACRITICS = new Dictionary<string, string>
                                                                   {
                                                                        { "ä".Normalize(NormalizationForm.FormD), "ae".Normalize(NormalizationForm.FormD) },
                                                                        { "Ä".Normalize(NormalizationForm.FormD), "Ae".Normalize(NormalizationForm.FormD) },
                                                                        { "ö".Normalize(NormalizationForm.FormD), "oe".Normalize(NormalizationForm.FormD) },
                                                                        { "Ö".Normalize(NormalizationForm.FormD), "Oe".Normalize(NormalizationForm.FormD) },
                                                                        { "ü".Normalize(NormalizationForm.FormD), "ue".Normalize(NormalizationForm.FormD) },
                                                                        { "Ü".Normalize(NormalizationForm.FormD), "Ue".Normalize(NormalizationForm.FormD) },
                                                                        { "ß".Normalize(NormalizationForm.FormD), "ss".Normalize(NormalizationForm.FormD) },
                                                                   };

        public static string ReformatToUpper(this string fieldName)
        {
            if (fieldName.IsNullOrEmpty())
                return fieldName;

            return fieldName
                .Trim()
                .Replace("\"", "")
                .Replace(" ", "_")
                .RemoveDiacritics()
                .ToUpper();
        }

        public static string RemoveDiacritics(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var stringBuilder = new StringBuilder(s.Normalize(NormalizationForm.FormD));

            foreach (KeyValuePair<string, string> keyValuePair in SPECIAL_DIACRITICS)
                stringBuilder.Replace(keyValuePair.Key, keyValuePair.Value);

            for (int i = 0; i < stringBuilder.Length; i++)
            {
                char c = stringBuilder[i];

                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    stringBuilder.Remove(i, 1);
            }

            return stringBuilder.ToString();
        }
    }
}
