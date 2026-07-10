namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SensitiveData;

public interface ISensitiveDataService
{
    string RedactedValue { get; }

    bool IsSensitive(SensitiveDataType dataType, string key, bool useContainsMatch = true);

    IReadOnlyCollection<string> GetSensitiveKeywords(SensitiveDataType dataType);

    HashSet<string> GetMergedKeywords(SensitiveDataType dataType, IEnumerable<string> customKeywords);

    bool MatchesKeyword(string key, HashSet<string> sensitiveKeywords, bool useContainsMatch);

    string RedactIfSensitive(SensitiveDataType dataType, string key, string value, bool useContainsMatch = true);
}
