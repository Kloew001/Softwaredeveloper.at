using System.Collections;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class MappingUtility
{
    public static void CopyPropertiesTo(this object source, object destination)
    {
        var destinationProperties = destination.GetType().GetProperties();

        if (source is IDictionary dict)
        {
            var sourcePropertyKeys = dict.Keys;

            foreach (string sourcePropertyKey in sourcePropertyKeys)
            {
                var sourcePropertyValue = dict[sourcePropertyKey];

                var destinationProperty = FindDestinationProperty(destinationProperties, sourcePropertyKey);
                if (destinationProperty == null)
                    continue;

                SetDestinationPropertyValue(destination, destinationProperty, null, sourcePropertyValue);

            }
        }
        else
        {
            var sourceProperties = source.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = FindDestinationProperty(destinationProperties, sourceProperty.Name);
                if (destinationProperty == null)
                    continue;

                var sourcePropertyValue = sourceProperty.GetValue(source);

                SetDestinationPropertyValue(destination, destinationProperty, sourceProperty, sourcePropertyValue);
            }
        }
    }

    private static PropertyInfo FindDestinationProperty(PropertyInfo[] destinationProperties, string sourcePropertyName)
    {
        return destinationProperties.FirstOrDefault(x => x.Name.ToUpper() == sourcePropertyName.ToUpper());
    }

    private static void SetDestinationPropertyValue(object destination, PropertyInfo destinationProperty, PropertyInfo sourceProperty, object sourcePropertyValue)
    {
        if (sourcePropertyValue == null)
        {
            destinationProperty.SetValue(destination, null);
            return;
        }
        
        var sourceType = sourceProperty?.PropertyType ?? sourcePropertyValue.GetType();

        if (destinationProperty.PropertyType == sourceType)
        {
            destinationProperty.SetValue(destination, sourcePropertyValue);
        }
        else if (sourceType == typeof(string) &&
                destinationProperty.PropertyType == typeof(Guid))
        {
            if (sourcePropertyValue == null)
            {
                destinationProperty.SetValue(destination, Guid.Empty);
            }
            else
            {
                destinationProperty.SetValue(destination, Guid.Parse((string)sourcePropertyValue));
            }
        }
        else if (sourceType == typeof(string) &&
                destinationProperty.PropertyType == typeof(Nullable<Guid>))
        {
            if (sourcePropertyValue == null)
            {
                destinationProperty.SetValue(destination, Guid.Empty);
            }
            else
            {
                if (sourcePropertyValue == null)
                {
                    destinationProperty.SetValue(destination, null);
                }
                else
                {
                    destinationProperty.SetValue(destination, Guid.Parse((string)sourcePropertyValue));
                }
            }
        }
    }
}
