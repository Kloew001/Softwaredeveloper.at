using System.Collections;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class MappingUtility
{
    public static object CopyPropertiesTo(this object source, object target)
    {
        var destinationProperties = target.GetType().GetProperties();

        if (source is IDictionary dict)
        {
            var sourcePropertyKeys = dict.Keys;

            foreach (string sourcePropertyKey in sourcePropertyKeys)
            {
                var sourcePropertyValue = dict[sourcePropertyKey];

                var targetProperty = FindTargetProperty(destinationProperties, sourcePropertyKey);
                if (targetProperty == null)
                    continue;

                SetTargetPropertyValue(target, targetProperty, null, sourcePropertyValue);

            }
        }
        else
        {
            var sourceProperties = source.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = FindTargetProperty(destinationProperties, sourceProperty.Name);
                if (targetProperty == null)
                    continue;

                var sourcePropertyValue = sourceProperty.GetValue(source);

                SetTargetPropertyValue(target, targetProperty, sourceProperty, sourcePropertyValue);
            }
        }

        return target;
    }

    private static PropertyInfo FindTargetProperty(PropertyInfo[] destinationProperties, string sourcePropertyName)
    {
        return destinationProperties.FirstOrDefault(x => x.Name.ToUpper() == sourcePropertyName.ToUpper());
    }
    private static void SetTargetPropertyValue(object target, PropertyInfo targetProperty, PropertyInfo sourceProperty, object sourcePropertyValue)
    {
        if (sourcePropertyValue == null)
        {
            if (IsNullableType(targetProperty))
            {
                targetProperty.SetValue(target, null);
            }

            return;
        }

        var sourceType = sourceProperty?.PropertyType ?? sourcePropertyValue.GetType();
        var targetType = targetProperty.PropertyType;

        if (targetType == sourceType)
        {
            targetProperty.SetValue(target, sourcePropertyValue);
            return;
        }

        var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var underlyingSourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

        if (underlyingTargetType == underlyingSourceType)
        {
            targetProperty.SetValue(target, Convert.ChangeType(sourcePropertyValue, underlyingTargetType));
            return;
        }

        if (sourceType == typeof(string) && underlyingTargetType == typeof(Guid))
        {
            if (Guid.TryParse(sourcePropertyValue.ToString(), out var guidValue))
            {
                targetProperty.SetValue(target, guidValue);
                return;
            }
        }

        if (sourceType == typeof(string) && targetType == typeof(Guid?))
        {
            if (Guid.TryParse(sourcePropertyValue.ToString(), out var nullableGuidValue))
            {
                targetProperty.SetValue(target, (Guid?)nullableGuidValue);
                return;
            }
        }

        if (targetType.IsEnum)
        {
            if (sourceType == typeof(int) || sourceType == typeof(long))
            {
                targetProperty.SetValue(target, Enum.ToObject(targetType, sourcePropertyValue));
                return;
            }
            else if (sourceType == typeof(string))
            {
                if (Enum.TryParse(targetType, sourcePropertyValue.ToString(), out var enumValue))
                {
                    targetProperty.SetValue(target, enumValue);
                    return;
                }
            }
        }

        if (sourceType.IsEnum)
        {
            if (targetType == typeof(int) || targetType == typeof(long))
            {
                targetProperty.SetValue(target, sourcePropertyValue);
                return;
            }
        }
    }

    private static bool IsNullableType(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        return !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;
    }
}
