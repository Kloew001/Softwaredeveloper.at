namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class ObjectExtensions
{
    public static bool IsNull(this object date)
    {
        return date == null;
    }
    public static bool IsNotNull(this object date)
    {
        return date != null;
    }

    public static T As<T>(this object obj)
        where T : class
    {
        return obj as T;
    }
    public static T To<T>(this object obj)
        where T : class
    {
        return (T)obj;
    }

    public static bool IsFalse(this bool boolean)
    {
        return boolean == false;
    }

    public static bool IsTrue(this bool boolean)
    {
        return boolean == true;
    }

}
