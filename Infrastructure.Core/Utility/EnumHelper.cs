namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class EnumHelper
{
    public static T GetRandom<T>()
        where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        var random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static T GetAttributeOfType<T>(this Enum enumVal)
        where T : Attribute
    {
        if (enumVal == null)
            return null;

        var type = enumVal.GetType();

        var memInfo = type.GetMember(enumVal.ToString());

        if (memInfo.Any() == false)
            return null;

        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);

        return attributes.Length > 0 ? (T)attributes[0] : null;
    }

    public static string GetName(this Enum enumVal)
    {
        if (enumVal == null)
            return null;

        var type = enumVal.GetType();
        return Enum.GetName(type, enumVal);
    }

    public static string GetDisplayName(this Enum enumVal)
    {
        return enumVal?.GetAttributeOfType<EnumExtensionAttribute>()?.DisplayName;
    }

    public static Guid GetId(this Enum enumVal)
    {
        return Guid.Parse(enumVal?.GetAttributeOfType<EnumExtensionAttribute>()?.Id);
    }

    public static T GetEnumFromId<T>(this Guid id) where T : Enum
    {
        foreach (T enumVal in Enum.GetValues(typeof(T)))
        {
            var attribute = enumVal.GetAttributeOfType<EnumExtensionAttribute>();
            if (attribute != null && Guid.Parse(attribute.Id) == id)
            {
                return enumVal;
            }
        }

        throw new ArgumentException($"No enum of type {typeof(T)} with ID {id} found.");
    }

    public static string GetShortName(this Enum enumVal)
    {
        return enumVal?.GetAttributeOfType<EnumExtensionAttribute>()?.ShortName;
    }

    public static string GetDescription(this Enum enumVal)
    {
        return enumVal?.GetAttributeOfType<EnumExtensionAttribute>()?.Description;
    }

    public static int? GetSortOrder(this Enum enumVal)
    {
        var sortOrder = enumVal?.GetAttributeOfType<EnumExtensionAttribute>()?.SortOrder;

        return sortOrder == -1 ? null : sortOrder;
    }

    public static IEnumerable<T> OrderBySortOrder<T>(this IEnumerable<T> enums)
        where T : Enum
    {
        return enums
            .OrderByDescending(e => e.GetSortOrder().HasValue)
            .ThenBy(e => e.GetSortOrder())
            .ThenBy(e => e.GetDisplayName());
    }

    public static IEnumerable<T> OrderByDisplayName<T>(this IEnumerable<T> enums)
        where T : Enum
    {
        return enums
            .OrderBy(e => e.GetDisplayName());
    }
}

public class EnumExtensionAttribute : Attribute
{
    public string Id { get; set; }
    public int SortOrder { get; set; } = -1;
    public string ShortName { get; set; }
    public string DisplayName { get; set; }
    public string MultilingualDisplayNameKey { get; set; }
    public string Description { get; set; }
}