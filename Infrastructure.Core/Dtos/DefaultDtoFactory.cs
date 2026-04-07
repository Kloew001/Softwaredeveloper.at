using System.Reflection;

using ExtendableEnums;

using Microsoft.Extensions.Caching.Memory;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

public class DefaultDtoFactory<TDto, TEntity> : IDtoFactory<TDto, TEntity>
    where TDto : IDto
    where TEntity : IEntity
{
    private readonly IMemoryCache _memoryCache;
    protected readonly DtoFactoryResolver _factoryResolver;

    public bool AutoSubPropertyMapping { get; set; } = true;

    public DefaultDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    {
        _memoryCache = memoryCache;
        _factoryResolver = factoryResolver;
    }

    public virtual TDto ConvertToDto(TEntity entity, TDto dto)
    {
        return (TDto)SimpleNameMapping(entity, dto);
    }

    public virtual TEntity ConvertToEntity(TDto dto, TEntity entity)
    {
        return (TEntity)SimpleNameMapping(dto, entity);
    }

    protected virtual object SimpleNameMapping(object source, object target)
    {
        if (source == null || target == null)
            return null;

        var sourceType = source.GetType();
        var targetType = target.GetType();

        var propertyMaps = ResolvePropertyMaps(sourceType, targetType);

        foreach (var propertyMap in propertyMaps)
        {
            var sourceValue = ResolveSourceValue(source, propertyMap);

            if (sourceValue == null)
            {
                propertyMap.TargetProperty.SetValue(target, null);

                continue;
            }

            var sourceValueType = sourceValue?.GetType();

            if (propertyMap.TargetProperty.PropertyType.IsValueType ||
                propertyMap.TargetProperty.PropertyType == typeof(string))
            {
                propertyMap.TargetProperty
                    .SetValue(target, GetSimpleValue( propertyMap.TargetProperty, sourceValue));
            }
            else if (propertyMap.TargetProperty.PropertyType.IsExtendableEnum())
            {
                propertyMap.TargetProperty
                    .SetValue(target, sourceValue);
            }
            else if (propertyMap.TargetProperty.PropertyType == typeof(byte[]))
            {
                propertyMap.TargetProperty
                    .SetValue(target, sourceValue);
            }
            else if (typeof(IEntity).IsAssignableFrom(sourceValueType) &&
                     typeof(IDto).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
            {
                var sourceEntity = sourceValue as IEntity;
                if (sourceEntity == null)
                {
                    propertyMap.TargetProperty.SetValue(target, null);
                    continue;
                }

                var dto = _factoryResolver.ConvertToDto(sourceEntity, propertyMap.TargetProperty.PropertyType);

                propertyMap.TargetProperty
                    .SetValue(target, dto);
            }
            else if (typeof(IEnumerable<IEntity>).IsAssignableFrom(sourceValueType) &&
                     typeof(IEnumerable<IDto>).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
            {
                var sourceEntity = sourceValue as IEnumerable<IEntity>;
                if (sourceEntity == null)
                {
                    propertyMap.TargetProperty.SetValue(target, null);
                    continue;
                }

                var dtoType = propertyMap.TargetProperty.PropertyType.GenericTypeArguments.First();

                var dtos = _factoryResolver.ConvertToDtos(sourceEntity, dtoType);

                propertyMap.TargetProperty
                    .SetValue(target, dtos);
            }
            else if (typeof(IDto).IsAssignableFrom(sourceValueType) &&
                     typeof(IEntity).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
            {
                var sourceDto = sourceValue as IDto;
                if (sourceDto == null)
                {
                    propertyMap.TargetProperty.SetValue(target, null);
                    continue;
                }

                var targetValue = propertyMap.TargetProperty.GetValue(target) as IEntity;

                var entity = _factoryResolver.ConvertToEntity(sourceDto, targetValue);

                propertyMap.TargetProperty.SetValue(target, entity);

                if (AutoSubPropertyMapping)
                {
                    var targetIdProperty = target.GetType().GetProperty(propertyMap.TargetProperty.Name + "Id");

                    if (targetIdProperty != null)
                    {
                        var id = entity.GetType().GetProperty("Id").GetValue(entity);
                        targetIdProperty.SetValue(target, id);
                    }
                }
            }
            else if (typeof(IEnumerable<IDto>).IsAssignableFrom(sourceValueType) &&
                     typeof(IEnumerable<IEntity>).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
            {
                if (sourceValue == null)
                {
                    propertyMap.TargetProperty.SetValue(target, null);
                    continue;
                }

                var entityType = propertyMap.TargetProperty.PropertyType.GenericTypeArguments.First();

                var targetValue = propertyMap.TargetProperty.GetValue(target);

                var iCollectionType = typeof(ICollection<>).MakeGenericType(entityType);
                if (!iCollectionType.IsAssignableFrom(targetValue.GetType()))
                {
                    throw new Exception($"Target property {propertyMap.TargetProperty.Name} must implement ICollection<{entityType.Name}>");
                }

                var dtos = sourceValue as IEnumerable<IDto>;

                var entities = _factoryResolver.ConvertToEntities(dtos, targetValue, entityType);

                propertyMap.TargetProperty
                    .SetValue(target, entities);
            }
        }

        return target;
    }

    private object GetSimpleValue(PropertyInfo targetProperty, object sourceValue)
    {
        if(targetProperty.PropertyType == sourceValue.GetType())
        {
            return sourceValue;
        }

        var targetType = Nullable.GetUnderlyingType(targetProperty.PropertyType) ?? targetProperty.PropertyType;

        if (targetType.IsAssignableFrom(sourceValue.GetType()))
        {
            return sourceValue;
        }
        else if (targetType.IsEnum)
        {
            var enumValue = sourceValue is string s ? Enum.Parse(targetType, s, true) : Enum.ToObject(targetType, sourceValue);
            return enumValue;
        }
        else
        {
            return Convert.ChangeType(sourceValue, targetType);
        }
    }

    private List<PropertyMap> ResolvePropertyMaps(Type sourceType, Type targetType)
    {
        var cacheKey = $"{nameof(SimpleNameMapping)}_{nameof(sourceType.FullName)}_{targetType.FullName}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<PropertyMap> propertyMaps))
        {
            var isDtoToEntity =
                     typeof(IDto).IsAssignableFrom(sourceType) &&
                     typeof(IEntity).IsAssignableFrom(targetType);

            var isEntityToDto =
                     typeof(IEntity).IsAssignableFrom(sourceType) &&
                     typeof(IDto).IsAssignableFrom(targetType);

            propertyMaps = new List<PropertyMap>();

            var targetProperties = targetType.GetProperties().ToList();
            var sourceProperties = sourceType.GetProperties().ToList();

            targetProperties
            .ForEach(targetProperty =>
            {
                if (targetProperty.CanWrite == false)
                    return;

                if (targetProperty.GetCustomAttribute<DtoFactoryIgnoreAttribute>() != null)
                    return;

                var sourceProperty = sourceType.GetProperty(targetProperty.Name);

                if (sourceProperty != null)
                {
                    propertyMaps.Add(new PropertyMap
                    {
                        TargetProperty = targetProperty,
                        SourceProperties = new[] { sourceProperty.Name }
                    });
                }

                if (AutoSubPropertyMapping &&
                    sourceProperty == null && isEntityToDto)
                {
                    //PersonName -> Person.Name
                    var sourcePropertyFirstLevel = sourceProperties
                        .FirstOrDefault(_ => targetProperty.Name.StartsWith(_.Name));

                    if (sourcePropertyFirstLevel != null)
                    {
                        var secondLevelPropertyName = targetProperty.Name.Substring(sourcePropertyFirstLevel.Name.Length);

                        var sourcePropertySecoundLevel = sourcePropertyFirstLevel.PropertyType.GetProperty(secondLevelPropertyName);

                        if (targetProperty.PropertyType == sourcePropertySecoundLevel?.PropertyType ||
                            targetProperty.Name == "Id")
                        {
                            propertyMaps.Add(new PropertyMap
                            {
                                TargetProperty = targetProperty,
                                SourceProperties = new[]
                                {
                                    sourcePropertyFirstLevel.Name,
                                    sourcePropertySecoundLevel.Name
                                }
                            });
                        }
                        else
                        {

                        }
                    }
                }
            });

            //if (typeof(IMultiLingualEntity<>).IsAssignableFrom(sourceType))
            //{
            //    var translationType =
            //        sourceType
            //        .GetInterfaces()
            //        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMultiLingualEntity<>))
            //        .SelectMany(i => i.GetGenericArguments())
            //        .Single();

            //    targetProperties
            //        .ForEach(targetProperty =>
            //        {
            //            var sourceProperty = translationType.GetProperty(targetProperty.Name);

            //            if (sourceProperty != null)
            //            {
            //                propertyMaps.Add(new PropertyMap
            //                {
            //                    TargetProperty = targetProperty,
            //                    SourceProperties = new[] { sourceProperty.Name }
            //                });
            //            }
            //        });
            //}

            _memoryCache.Set(cacheKey, propertyMaps);
        }

        return propertyMaps;
    }

    private static object ResolveSourceValue(object source, PropertyMap propertyMap)
    {
        var sourceValue = source;
        foreach (var sourceProperty in propertyMap.SourceProperties)
        {
            var sourceValue2 = sourceValue.GetType()
                .GetProperty(sourceProperty)
                .GetValue(sourceValue);

            sourceValue = sourceValue2;
        }

        return sourceValue;
    }

    public class PropertyMap
    {
        public PropertyInfo TargetProperty { get; set; }

        public IEnumerable<string> SourceProperties { get; set; }
    }
}