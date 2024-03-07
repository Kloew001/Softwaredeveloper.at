namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
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
            return (T)obj;
        }
    }
}
