using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class StringUtility
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNotNullOrEmpty(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static string EmptyToNull(this string str)
    {
        if (str == null)
            return null;

        if (str.IsNullOrWhiteSpace())
            return null;

        return str;
    }

    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
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

    public static string HashString(this string text, string salt = "")
    {
        if (String.IsNullOrEmpty(text))
        {
            return String.Empty;
        }

        // Uses SHA256 to create the hash
        using (var sha = SHA256.Create())
        {
            // Convert the string to a byte array first, to be processed
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
            byte[] hashBytes = sha.ComputeHash(textBytes);

            // Convert back to a string, removing the '-' that BitConverter adds
            string hash = BitConverter
                .ToString(hashBytes)
                .Replace("-", String.Empty);

            return hash;
        }
    }

    public static string CombineUrl(this string baseUrl, string relativeUrl)
    {
        var baseUri = new UriBuilder(baseUrl);

        if (Uri.TryCreate(baseUri.Uri, relativeUrl, out Uri newUri))
            return newUri.ToString();
        else
            throw new ArgumentException($"Unable to combine specified url values: '{baseUrl}', '{relativeUrl}'");
    }
}
