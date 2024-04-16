using Microsoft.Extensions.Caching.Memory;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos
{
    public class DefaultDtoFactory<TDto, TEntity> : IDtoFactory<TDto, TEntity>
        where TDto : IDto
        where TEntity : IEntity
    {
        private IMemoryCache _memoryCache;

        public bool AutoSubPropertyMapping { get; set; } = true;

        public DefaultDtoFactory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public TDto ConvertToDto(TEntity entity, TDto dto, IServiceProvider serviceProvider)
        {
            return (TDto)SimpleNameMapping(entity, dto, serviceProvider);
        }

        public TEntity ConvertToEntity(TDto dto, TEntity entity, IServiceProvider serviceProvider)
        {
            return (TEntity)SimpleNameMapping(dto, entity, serviceProvider);
        }

        protected virtual object SimpleNameMapping(object source, object target, IServiceProvider serviceProvider)
        {
            if (source == null || target == null)
                return null;

            var sourceType = source.GetType();
            var targetType = target.GetType();

            var cacheKey = $"{nameof(SimpleNameMapping)}_{nameof(sourceType.FullName)}_{targetType.FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<PropertyMap> propertyMaps))
            {
                var isDtoToEntity =
                         typeof(Dto).IsAssignableFrom(sourceType) &&
                         typeof(Entity).IsAssignableFrom(targetType);

                var isEntityToDto =
                         typeof(Entity).IsAssignableFrom(sourceType) &&
                         typeof(Dto).IsAssignableFrom(targetType);

                propertyMaps = new List<PropertyMap>();

                var targetProperties = targetType.GetProperties().ToList();
                var sourceProperties = sourceType.GetProperties().ToList();

                targetProperties
                .ForEach(targetProperty =>
                {
                    if (isDtoToEntity && targetProperty.Name == "Id")
                        return;

                    if (targetProperty.Name == "Id" &&
                        targetType.GetAttribute<DtoFactoryAttribute>()?.IgnoreId == true)
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

            foreach (var propertyMap in propertyMaps)
            {
                object sourceValue = ResolveSourceValue(source, propertyMap);

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
                        .SetValue(target, sourceValue);
                }
                else if (typeof(Entity).IsAssignableFrom(sourceValueType) &&
                         typeof(Dto).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
                {
                    var sourceEntity = sourceValue as Entity;
                    if (sourceEntity == null)
                    {
                        propertyMap.TargetProperty.SetValue(target, null);
                        continue;
                    }

                    var dto =
                        typeof(DtoFactoryExtensions)
                        .GetMethod(nameof(DtoFactoryExtensions.ConvertToDto))
                        .MakeGenericMethod(propertyMap.TargetProperty.PropertyType)
                        .Invoke(null, new object[] { sourceEntity, null, serviceProvider });

                    propertyMap.TargetProperty
                        .SetValue(target, dto);
                }
                else if (typeof(IEnumerable<Entity>).IsAssignableFrom(sourceValueType) &&
                         typeof(IEnumerable<Dto>).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
                {
                    var sourceEntity = sourceValue as IEnumerable<Entity>;
                    if (sourceEntity == null)
                    {
                        propertyMap.TargetProperty.SetValue(target, null);
                        continue;
                    }

                    var dtoType = propertyMap.TargetProperty.PropertyType.GenericTypeArguments.First();

                    var dtos =
                        typeof(DtoFactoryExtensions)
                        .GetMethod(nameof(DtoFactoryExtensions.ConvertToDtos))
                        .MakeGenericMethod(dtoType)
                        .Invoke(null, new object[] { sourceEntity, null, serviceProvider });

                    propertyMap.TargetProperty
                        .SetValue(target, dtos);
                }
                else if (typeof(Dto).IsAssignableFrom(sourceValueType) &&
                         typeof(Entity).IsAssignableFrom(propertyMap.TargetProperty.PropertyType))
                {
                    var sourceDto = sourceValue as Dto;
                    if (sourceDto == null)
                    {
                        propertyMap.TargetProperty.SetValue(target, null);
                        continue;
                    }

                    var targetValue = propertyMap.TargetProperty.GetValue(target);

                    var entity =
                       typeof(DtoFactoryExtensions)
                       .GetMethod(nameof(DtoFactoryExtensions.ConvertToEntity))
                       .MakeGenericMethod(propertyMap.TargetProperty.PropertyType)
                       .Invoke(null, new object[] { sourceDto, targetValue, null, serviceProvider });

                    propertyMap.TargetProperty
                        .SetValue(target, entity);

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
            }

            return target;
        }

        private static object ResolveSourceValue(object source, PropertyMap propertyMap)
        {
            object sourceValue = source;
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
}
