using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class StringSanitizer
{
    private const string TruncationSuffix = "...(truncated)";

    private static readonly Regex HtmlTagRegex =
        new("<.*?>", RegexOptions.Compiled);

    private static readonly Regex MultipleWhitespaceRegex =
        new(@"\s{2,}", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes a string for safe logging: removes control/format/private-use characters
    /// and limits the maximum length.
    /// Sample: input "Hello\r\nWorld\u001b", output "Hello World".
    /// </summary>
    public static string Sanitize(string input, int maxLength = 1024)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (input.Length > maxLength)
        {
            int cutLength = Math.Max(0, maxLength - TruncationSuffix.Length);
            input = input.Substring(0, cutLength) + TruncationSuffix;
        }

        var sb = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            if (ch == '\r' || ch == '\n')
            {
                sb.Append(' ');
                continue;
            }

            if (char.IsControl(ch))
                continue;

            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);

            switch (cat)
            {
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                    continue;
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Normalizes whitespace: trims and collapses multiple whitespace characters into a single space.
    /// Sample: input "  Hello\t  World  ", output "Hello World".
    /// </summary>
    public static string NormalizeWhitespace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input.Trim();
        return MultipleWhitespaceRegex.Replace(trimmed, " ");
    }

    /// <summary>
    /// Removes diacritics (accents) from characters.
    /// Sample: input "ÄÖÜ äöü éèç", output "AOU aou eec".
    /// </summary>
    public static string RemoveDiacritics(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Strips HTML tags and decodes HTML entities, then normalizes whitespace.
    /// Sample: input "&lt;p&gt;Hello &lt;strong&gt;World&lt;/strong&gt;&lt;/p&gt;", output "Hello World".
    /// </summary>
    public static string StripHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var decoded = WebUtility.HtmlDecode(input);
        var noTags = HtmlTagRegex.Replace(decoded, string.Empty);

        return NormalizeWhitespace(noTags);
    }

    /// <summary>
    /// HTML-encodes a string for safe HTML output.
    /// Sample: input "&lt;script&gt;alert('x')&lt;/script&gt;", output "&amp;lt;script&amp;gt;alert(&amp;#39;x&amp;#39;)&amp;lt;/script&amp;gt;".
    /// </summary>
    public static string HtmlEncode(string? input)
    {
        if (input == null)
            return string.Empty;

        return WebUtility.HtmlEncode(input);
    }

    /// <summary>
    /// Allows only characters that match the given whitelist pattern.
    /// Sample: input "User_123@example.com", pattern "a-zA-Z0-9_" => "User_123".
    /// </summary>
    public static string AllowOnly(string? input, string whitelistPattern)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(whitelistPattern))
            throw new ArgumentException("Whitelist pattern must not be empty.");

        var regex = new Regex($"[^{whitelistPattern}]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return regex.Replace(input, string.Empty);
    }

    /// <summary>
    /// Creates a filesystem-safe file name from an arbitrary string.
    /// Sample: input "Mein *Dokument* 2024?.pdf", output ähnlich zu "Mein_Dokument_2024_.pdf".
    /// </summary>
    public static string ToSafeFileName(string? input, int maxLength = 255)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var cleaned = RemoveDiacritics(input);
        cleaned = NormalizeWhitespace(cleaned);

        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(cleaned.Length);

        foreach (var ch in cleaned)
        {
            if (invalidChars.Contains(ch))
                sb.Append('_');
            else
                sb.Append(ch);
        }

        var result = Regex.Replace(sb.ToString(), @"\s+", "_");
        result = Regex.Replace(result, @"[^\w\-.()_]", "_");

        if (result.Length > maxLength)
            result = result.Substring(0, maxLength);

        return result.Trim('_', ' ');
    }

    /// <summary>
    /// Generates a URL-friendly slug from a string.
    /// Sample: input "Änderungen im März 2024", output "anderungen-im-marz-2024".
    /// </summary>
    public static string ToUrlSlug(string? input, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = RemoveDiacritics(input)
            .ToLowerInvariant();

        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');

        if (slug.Length > maxLength)
            slug = slug.Substring(0, maxLength).Trim('-');

        return slug;
    }

    /// <summary>
    /// Escapes single quotes for use in legacy SQL string concatenation.
    /// Sample: input "O'Hara", output "O''Hara".
    /// </summary>
    public static string SanitizeForSql(string? input)
    {
        if (input == null)
            return string.Empty;

        return input.Replace("'", "''");
    }
}