using System.Reflection;

using DocumentFormat.OpenXml.InkML;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class TypeUtility
    {
        public static IEnumerable<Type> GetAllTypesWithAttribute(Type attributeType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => p.IsAbstract == false &&
                            p.IsInterface == false &&
                            p.GetCustomAttributes(attributeType, true).Length > 0)
                .ToList();
        }

        public static Attribute GetAttribute(this Type objType, Type attributeType)
        {
            return objType.GetCustomAttributes(attributeType, true)
                .OfType<Attribute>()
                .SingleOrDefault();
        }
        public static TAttrbitute GetAttribute<TAttrbitute>(this Type objType)
            where TAttrbitute : Attribute
        {
            return GetAttributes<TAttrbitute>(objType)
                .SingleOrDefault();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type objType)
            where TAttribute : Attribute
        {
            return objType.GetCustomAttributes(typeof(TAttribute), true)
                .OfType<TAttribute>()
                .ToList();
        }
        public static Type UnProxy(this Type entityType)
        {
            if(entityType == null) 
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
}
