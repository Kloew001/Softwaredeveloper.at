using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos
{
    public static class DtoFactoryExtensions
    {
        private static DtoFactory _factory;

        public static void Configure(DtoFactory factory)
        {
            _factory = factory;
        }

        public static IEnumerable<TDto> ConvertToDtos<TDto>(this IEnumerable<BaseEntity> entities)
            where TDto : DtoBase, new()
        {
            return _factory.ConvertToDtos<TDto>(entities);
        }

        public static TDto ConvertToDto<TDto>(this BaseEntity entitiy)
            where TDto : DtoBase, new()
        {
            return _factory.ConvertToDto<TDto>(entitiy);
        }


        public static TEntity ConvertToEntity<TEntity>(this DtoBase dto, TEntity entity)
            where TEntity : BaseEntity, new()
        {
            return _factory.ConvertToEntity<TEntity>(dto, entity);
        }
    }

    public class DtoFactory : ISingletonService
    {
        public class DtoFactoryTypeMappingResolver : ISingletonService
        {
            private static Dictionary<Tuple<Type, Type>, Type> _dtoFactoryTypes;

            public DtoFactoryTypeMappingResolver()
            {
                _dtoFactoryTypes = new Dictionary<Tuple<Type, Type>, Type>();

                var factoryTypes =
                    //typeof(DtoFactoryTypeMappingResolver).Assembly
                    AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                   .Where(p => p.IsAbstract == false &&
                               p.IsInterface == false &&
                               typeof(IDtoFactory).IsAssignableFrom(p))
                   .ToList();

                foreach (var factoryTyp in factoryTypes)
                {
                    var dtoFactoryInterfaces = factoryTyp.GetInterfaces()
                        .Where(_ => _.IsGenericType &&
                                    _.GetGenericTypeDefinition() == typeof(IDtoFactory<,>))
                        .ToList();

                    foreach (var typeinterface in dtoFactoryInterfaces)
                    {
                        var genericArguments = typeinterface.GetGenericArguments();

                        if (genericArguments.Count() == 2)
                        {
                            var dtoTypeGeneric = genericArguments[0];
                            var entityTypeGeneric = genericArguments[1];

                            if (dtoTypeGeneric.IsGenericParameter)
                                dtoTypeGeneric = dtoTypeGeneric.BaseType;

                            if (entityTypeGeneric.IsGenericParameter)
                                entityTypeGeneric = entityTypeGeneric.BaseType;

                            _dtoFactoryTypes.Add(new Tuple<Type, Type>(dtoTypeGeneric, entityTypeGeneric), factoryTyp);
                        }
                    }
                }
            }

            public bool Contains(Tuple<Type, Type> tuple)
            {
                return _dtoFactoryTypes.ContainsKey(tuple);
            }

            public Type Resolve(Tuple<Type, Type> tuple)
            {
                var factoryType = _dtoFactoryTypes[tuple];

                return factoryType;
            }
        }

        protected readonly IServiceProvider _serviceProvider;
        protected readonly DtoFactoryTypeMappingResolver _dtoFactoryResolver;
        protected readonly IMemoryCache _memoryCache;

        public DtoFactory(IServiceProvider serviceProvider, DtoFactoryTypeMappingResolver dtoFactoryResolver, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _dtoFactoryResolver = dtoFactoryResolver;
            _memoryCache = memoryCache;
        }

        private Tuple<Type, Type> FindTuple(Tuple<Type, Type> tuple)
        {
            if (_dtoFactoryResolver.Contains(tuple) == true)
                return tuple;

            if (tuple.Item1.BaseType == null || tuple.Item1.BaseType == typeof(object))
                return null;

            var firstBaseTuple = new Tuple<Type, Type>(tuple.Item1.BaseType, tuple.Item2);

            var resultTuple = FindTuple(firstBaseTuple);

            if (resultTuple != null)
                return resultTuple;

            var secondBaseTuple = new Tuple<Type, Type>(tuple.Item1, tuple.Item2.BaseType);

            resultTuple = FindTuple(secondBaseTuple);

            if (resultTuple != null)
                return resultTuple;

            var baseTuple = new Tuple<Type, Type>(tuple.Item1.BaseType, tuple.Item2.BaseType);

            return FindTuple(baseTuple);
        }

        private IDtoFactory ResolveTyped(Type dtoType, Type entityType)
        {
            var cacheKey = $"{nameof(DtoFactory)}_{nameof(ResolveTyped)}_{dtoType.FullName}_{entityType.FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out IDtoFactory cachedfactory))
            {
                cachedfactory = (IDtoFactory)
                    GetType().GetMethod(nameof(Resolve), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { dtoType, entityType.UnProxy() })
                    .Invoke(this, null);

                _memoryCache.Set(cacheKey, cachedfactory);
            }

            return cachedfactory;
        }

        private IDtoFactory<TDto, TEntity> Resolve<TDto, TEntity>()
            where TDto : DtoBase
            where TEntity : BaseEntity
        {
            var cacheKey = $"{nameof(DtoFactory)}_{nameof(Resolve)}_{typeof(TDto).FullName}_{typeof(TEntity).FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out IDtoFactory<TDto, TEntity> cachedfactory))
            {
                var tuple = FindTuple(new Tuple<Type, Type>(typeof(TDto), typeof(TEntity)));

                if (tuple == null)
                    throw new MissingMethodException();

                var factoryType = _dtoFactoryResolver.Resolve(tuple);

                if (factoryType.ContainsGenericParameters)
                    factoryType = factoryType.MakeGenericType(new[] { typeof(TDto), typeof(TEntity) });

                var factory = _serviceProvider.GetService(factoryType);

                if (factory == null)
                    throw new MissingMethodException();

                cachedfactory = (IDtoFactory<TDto, TEntity>)factory;

                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //    .SetSlidingExpiration(TimeSpan.FromHours(12));

                _memoryCache.Set(cacheKey, cachedfactory);
            }

            return cachedfactory;
        }

        public IEnumerable<TDto> ConvertToDtos<TDto>(IEnumerable<BaseEntity> entities)
            where TDto : DtoBase, new()
        {
            if (entities == null)
                return null;

            return entities
                .Select(_ => ConvertToDto(_, new TDto()))
                .ToList();
        }

        public TDto ConvertToDto<TDto>(BaseEntity entity, TDto dto = null)
            where TDto : DtoBase, new()
        {
            if (entity == null)
                return null;

            if (dto == null)
                dto = new TDto();

            var dtoFactory = ResolveTyped(typeof(TDto), entity.GetType());
            var convertToDtoMethod = dtoFactory.GetType().GetMethod("ConvertToDto"); //IDtoFactory<TDto, TEntity>.ConvertToDto

            dto = (TDto)convertToDtoMethod.Invoke(dtoFactory, new object[] { entity, dto });

            return dto;
        }

        public TDto ConvertToDto<TFactory, TEntity, TDto>(TEntity entity, TDto dto)
            where TFactory : IDtoFactory<TDto, TEntity>
            where TDto : DtoBase, new()
            where TEntity : BaseEntity
        {
            var factory = _serviceProvider.GetService<TFactory>();

            if (factory == null)
                throw new MissingMethodException();

            return factory.ConvertToDto(entity, dto);
        }

        public TEntity ConvertToEntity<TEntity>(DtoBase dto, TEntity entity)
            where TEntity : BaseEntity
        {
            if (dto == null)
                return null;

            var dtoFactory = ResolveTyped(dto.GetType(), typeof(TEntity));
            var convertToEntityMethod = dtoFactory.GetType().GetMethod("ConvertToEntity"); //IDtoFactory<TDto, TEntity>.ConvertToEntity

            entity = (TEntity)convertToEntityMethod.Invoke(dtoFactory, new object[] { dto, entity });

            return entity;
        }

        public TEntity ConvertToEntity<TDto, TEntity>(TDto dto, TEntity entity)
            where TDto : DtoBase
            where TEntity : BaseEntity
        {
            var factory = Resolve<TDto, TEntity>();

            if (factory == null)
                return entity;

            return factory.ConvertToEntity(dto, entity);
        }
    }

    public interface IDtoFactory : ISingletonService
    {
    }

    public interface IDtoFactory<TDto, TEntity> : IDtoFactory
        where TDto : DtoBase
        where TEntity : BaseEntity
    {
        TDto ConvertToDto(TEntity entity, TDto dto);
        TEntity ConvertToEntity(TDto dto, TEntity entity);
    }
}
