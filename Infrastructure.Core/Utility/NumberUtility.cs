namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class NumberUtility
{
    public static bool IsNotNullOrZero(this decimal? number)
    {
        return IsNullOrZero(number) == false;
    }
    public static bool IsNullOrZero(this decimal? number)
    {
        return number == null || number == 0;
    }

    public static bool IsNotNullOrZero(this int? number)
    {
        return IsNullOrZero(number) == false;
    }

    public static bool IsNullOrZero(this int? number)
    {
        return number == null || number == 0;
    }
}
