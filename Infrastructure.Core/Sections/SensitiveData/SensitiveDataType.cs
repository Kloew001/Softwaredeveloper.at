using ExtendableEnums;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SensitiveData;

public sealed class SensitiveDataType(int value, string displayName) : ExtendableEnum<SensitiveDataType>(value, displayName)
{
    public static readonly SensitiveDataType Field = new(1, nameof(Field));

    public static readonly SensitiveDataType Header = new(2, nameof(Header));

    public static readonly SensitiveDataType Cookie = new(3, nameof(Cookie));

    public static readonly SensitiveDataType UrlSegment = new(4, nameof(UrlSegment));
}
