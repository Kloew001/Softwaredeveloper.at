namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SensitiveData;

[ApplicationConfiguration]
public class SensitiveDataConfiguration
{
    public bool Enabled { get; set; } = true;

    public Dictionary<string, string[]> SensitiveKeywords { get; set; } = new();
}
