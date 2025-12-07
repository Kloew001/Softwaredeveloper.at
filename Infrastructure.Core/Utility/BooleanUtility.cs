namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class BooleanUtility
{
    public static bool? ToBoolean(this string booleanStr)
    {
        return booleanStr == "y" ? true :
               booleanStr == "n" ? false :
               null;
    }

    public static string ToStringBoolean(this bool boolean)
    {
        return boolean ? "y" : "n";
    }
}