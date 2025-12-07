namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class TypeUtility
{
    public static IEnumerable<Type> GetAllTypesWithAttribute(Type attributeType, bool inherit = true)
    {
        return AssemblyUtils.AllLoadedTypes()
            .Where(p => p.IsAbstract == false &&
                        p.IsInterface == false &&
                        p.GetCustomAttributes(attributeType, inherit).Length > 0)
            .ToList();
    }

    public static Attribute GetAttribute(this Type objType, Type attributeType, bool inherit = true)
    {
        return objType.GetCustomAttributes(attributeType, inherit)
            .OfType<Attribute>()
            .SingleOrDefault();
    }
    public static TAttrbitute GetAttribute<TAttrbitute>(this Type objType, bool inherit = true)
        where TAttrbitute : Attribute
    {
        return GetAttributes<TAttrbitute>(objType, inherit)
            .SingleOrDefault();
    }

    public static bool HasAttribute<TAttrbitute>(this Type objType, bool inherit = true)
        where TAttrbitute : Attribute
    {
        return GetAttributes<TAttrbitute>(objType, inherit).Any();
    }

    public static bool HasAttribute(this Type objType, Type attributeType, bool inherit = true)
    {
        return GetAttributes(objType, attributeType, inherit).Any();
    }

    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type objType, bool inherit = true)
        where TAttribute : Attribute
    {
        return GetAttributes(objType, typeof(TAttribute), inherit)
            .OfType<TAttribute>();
    }

    public static IEnumerable<Attribute> GetAttributes(this Type objType, Type attributeType, bool inherit = true)
    {
        return objType.GetCustomAttributes(attributeType, inherit)
            .OfType<Attribute>()
            .ToList();
    }

    public static Type UnProxy(this Type entityType)
    {
        if (entityType == null)
            return null;

        if (entityType.Namespace == "Castle.Proxies")
            return entityType.BaseType;

        return entityType;
    }

    private static TEntity Unproxy<TEntity>(TEntity efobject)
        where TEntity : IEntity, new()
    {
        var type = efobject.GetType();

        if (type.Namespace == "Castle.Proxies")
        {
            //var propertyValues = context.Set<TEntity>().Entry(eintrag)
            //    .Properties
            //.Select(_ => new { _.Metadata.Name, _.CurrentValue })
            //.ToList();

            //var x = new TEntity();

            //propertyValues
            //    .ForEach(_ =>
            //    {
            //        x.GetType()
            //        .GetProperty(_.Name)
            //        .SetValue(x, _.CurrentValue);
            //    });

            type.GetProperties().ToList().ForEach(_ =>
            {
                var value = type.GetProperty(_.Name).GetValue(efobject);//force load
            });

            var basetype = type.BaseType;
            var returnobject = new TEntity();

            foreach (var property in basetype.GetProperties())
            {
                var propertytype = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (propertytype.IsEnum ||
                    propertytype.Namespace == "System")
                {
                    var value = type.GetProperty(property.Name)
                        .GetGetMethod().Invoke(efobject, null);
                    //.GetValue(efobject);

                    property.SetValue(returnobject, value);
                }
            }
            return returnobject;
        }

        return efobject;
    }
}