namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SensitiveData;

[SingletonDependency<ISensitiveDataService>]
public class SensitiveDataService : ISensitiveDataService
{
    private readonly SensitiveDataConfiguration _configuration;

    public string RedactedValue => "***";

    private static readonly Dictionary<string, string[]> DefaultSensitiveKeywords = new()
    {
        [nameof(SensitiveDataType.Field)] =
        [
            "password", "passwort", "pwd", "passwd",
            "token", "jwt", "bearer", "auth",
            "secret", "apikey", "api_key", "api-key",
            "credit", "card", "cvv", "cvc", "pin",
            "social", "ssn", "taxid", "tax_id",
            "private", "key", "cert", "certificate",
            "session", "cookie", "csrf",
            "access_token", "refresh_token", "id_token",
            "client_secret", "client_id",
            "signature", "hash", "salt",
            "code", "authorization", "proxy-authorization",
            "account", "routing"
        ],
        [nameof(SensitiveDataType.Header)] =
        [
            "Authorization",
            "Proxy-Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "Api-Key",
            "X-Auth-Token",
            "X-Amz-Security-Token",
            "X-Csrf-Token"
        ],
        [nameof(SensitiveDataType.Cookie)] =
        [
            "auth",
            "session",
            "token",
            "jwt",
            "aspxauth",
            ".aspxauth"
        ],
        [nameof(SensitiveDataType.UrlSegment)] =
        [
            "password",
            "token",
            "secret",
            "key",
            "apikey"
        ]
    };

    private readonly Dictionary<string, Lazy<HashSet<string>>> _allSensitiveKeywords;

    public SensitiveDataService(IApplicationSettings applicationSettings)
    {
        _configuration = applicationSettings.SensitiveData;

        // Initialize lazy-loaded keyword sets for each data type
        _allSensitiveKeywords = new Dictionary<string, Lazy<HashSet<string>>>();

        foreach (var dataType in SensitiveDataType.GetAll())
        {
            var typeName = dataType.DisplayName;
            _allSensitiveKeywords[typeName] = new Lazy<HashSet<string>>(() =>
            {
                var defaults = DefaultSensitiveKeywords.TryGetValue(typeName, out var def) ? def : [];
                var customs = _configuration.SensitiveKeywords.TryGetValue(typeName, out var cust) ? cust : [];
                return MergeKeywords(customs, defaults);
            });
        }
    }

    public bool IsSensitive(SensitiveDataType dataType, string key, bool useContainsMatch = true)
    {
        if (!_configuration.Enabled)
            return false;

        if (string.IsNullOrWhiteSpace(key))
            return false;

        var keywords = GetKeywordsForType(dataType);

        if (keywords.Contains(key))
            return true;

        if (!useContainsMatch)
            return false;

        var lowerKey = key.ToLowerInvariant();
        return keywords.Any(keyword => lowerKey.Contains(keyword));
    }

    public IReadOnlyCollection<string> GetSensitiveKeywords(SensitiveDataType dataType)
    {
        return GetKeywordsForType(dataType);
    }

    public HashSet<string> GetMergedKeywords(SensitiveDataType dataType, IEnumerable<string> customKeywords)
    {
        var existing = GetKeywordsForType(dataType);
        return MergeKeywords(customKeywords, existing);
    }

    public bool MatchesKeyword(string key, HashSet<string> sensitiveKeywords, bool useContainsMatch)
    {
        if (!_configuration.Enabled)
            return false;

        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (sensitiveKeywords.Contains(key))
            return true;

        if (!useContainsMatch)
            return false;

        return sensitiveKeywords.Any(sensitive => key.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }

    public string RedactIfSensitive(SensitiveDataType dataType, string key, string value, bool useContainsMatch = true)
    {
        return IsSensitive(dataType, key, useContainsMatch) ? RedactedValue : value;
    }

    private HashSet<string> GetKeywordsForType(SensitiveDataType dataType)
    {
        var typeName = dataType.DisplayName;

        if (_allSensitiveKeywords.TryGetValue(typeName, out var lazy))
        {
            return lazy.Value;
        }

        // If type is not found (custom type), return empty set
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> MergeKeywords(IEnumerable<string> customKeywords, IEnumerable<string> defaultKeywords)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in defaultKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
        {
            result.Add(item);
        }

        if (customKeywords != null)
        {
            foreach (var item in customKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                result.Add(item);
            }
        }

        return result;
    }
}
