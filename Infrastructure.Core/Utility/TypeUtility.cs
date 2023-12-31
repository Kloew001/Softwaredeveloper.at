namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class TypeUtility
    {
        public static Type UnProxy(this Type entityType)
        {
            if(entityType == null) 
                return null;

            if (entityType.Namespace == "Castle.Proxies")
                return entityType.BaseType;

            return entityType;
        }
    }
}
