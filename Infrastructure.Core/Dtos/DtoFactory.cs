using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos
{
    public static class DtoFactoryExtensions
    {
        private static DtoFactory _factory;

        public static void UseDtoFactory(this IHost host)
        {
            _factory = host.Services.GetRequiredService<DtoFactory>();
        }

        //TODO Should Update DeepLevel References
        public static TDto ConvertToDto<TDto>(this IEntity entitiy, IDtoFactory dtoFactory = null)
            where TDto : IDto, new()
        {
            return _factory.ConvertToDto<TDto>(entitiy, default, dtoFactory);
        }

        public static IEnumerable<TDto> ConvertToDtos<TDto>(this IEnumerable<IEntity> entities, IDtoFactory dtoFactory = null)
            where TDto : IDto, new()
        {
            return _factory.ConvertToDtos<TDto>(entities, dtoFactory);
        }

        public static TEntity ConvertToEntity<TEntity>(this IDto dto, TEntity entity, IDtoFactory dtoFactory = null)
            where TEntity : IEntity
        {
            return _factory.ConvertToEntity<TEntity>(dto, entity, dtoFactory);
        }

        //TODO ShouldUpdateRecerences
        public static ICollection<TEntity> ConvertIdsToEntities<TEntity>(this IEnumerable<Guid> dtoIds, IEntity rootEntity, ICollection<TEntity> entities = null, IDtoFactory dtoFactory = null)
            where TEntity : class, IEntity
        {
            return _factory.ConvertIdsToEntities<TEntity>(rootEntity, dtoIds, entities, dtoFactory);
        }

        //TODO ShouldUpdateRecerences
        public static ICollection<TEntity> ConvertToEntities<TEntity>(this IEnumerable<IDto> dto, IEntity rootEntity, ICollection<TEntity> entities = null, IDtoFactory dtoFactory = null)
            where TEntity : class, IEntity
        {
            return _factory.ConvertToEntities<TEntity>(rootEntity, dto, entities, dtoFactory);
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
                    AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetExportedTypes())
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

            if (tuple.Item2.BaseType == null || tuple.Item2.BaseType == typeof(object))
                return null;

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
            where TDto : IDto
            where TEntity : IEntity
        {
            var cacheKey = $"{nameof(DtoFactory)}_{nameof(Resolve)}_{typeof(TDto).FullName}_{typeof(TEntity).FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out IDtoFactory<TDto, TEntity> cachedfactory))
            {
                var tuple = FindTuple(new Tuple<Type, Type>(typeof(TDto), typeof(TEntity)));

                if (tuple == null)
                    return _serviceProvider.GetService<DefaultDtoFactory<TDto, TEntity>>();

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

        public IEnumerable<TDto> ConvertToDtos<TDto>(IEnumerable<IEntity> entities, IDtoFactory dtoFactory = null)
            where TDto : IDto, new()
        {
            if (entities == null)
                return null;

            return entities
                .Select(_ => ConvertToDto(_, new TDto(), dtoFactory))
                .ToList();
        }

        public TDto ConvertToDto<TDto>(IEntity entity, TDto dto = default, IDtoFactory dtoFactory = null)
            where TDto : IDto, new()
        {
            if (entity == null)
                return default;

            if (dto == null)
                dto = new TDto();

            if (dtoFactory == null)
                dtoFactory = ResolveTyped(typeof(TDto), entity.GetType());

            var convertToDtoMethod = dtoFactory.GetType().GetMethod("ConvertToDto"); //IDtoFactory<TDto, TEntity>.ConvertToDto

            dto = (TDto)convertToDtoMethod.Invoke(dtoFactory, new object[] { entity, dto });

            return dto;
        }

        public TDto ConvertToDto<TFactory, TEntity, TDto>(TEntity entity, TDto dto)
            where TFactory : IDtoFactory<TDto, TEntity>
            where TDto : IDto, new()
            where TEntity : IEntity
        {
            var factory = _serviceProvider.GetService<TFactory>();

            if (factory == null)
                throw new MissingMethodException();

            return factory.ConvertToDto(entity, dto);
        }

        //TODO ShouldUpdateRecerences
        public ICollection<TEntity> ConvertIdsToEntities<TEntity>(IEntity rootEntity, IEnumerable<Guid> dtoIds, ICollection<TEntity> entities = null, IDtoFactory dtoFactory = null)
            where TEntity : class, IEntity
        {
            if (dtoIds == null)
                dtoIds = Enumerable.Empty<Guid>();

            var dbContext = rootEntity.ResolveDbContext();

            var itemsCollection = entities ?? new List<TEntity>();

            var entitiesToRemove = itemsCollection.Where(_ => dtoIds.Contains(_.Id) == false).ToList();
            entitiesToRemove
                .ForEach(_ => itemsCollection.Remove(_));

            var dtoEntityJoin =
                dtoIds
                .GroupJoin(itemsCollection, dtoId => dtoId, entity => entity.Id,
                (dtoId, entities) => new { dtoId, entity = entities.SingleOrDefault() })
                .ToList();

            dtoEntityJoin
                .ForEach(_ =>
                {
                    var entity = _.entity;

                    if (entity == null)
                    {
                        entity = dbContext.Set<TEntity>()
                            .Where(db => db.Id == _.dtoId)
                            .FirstOrDefault();

                        if (entity != null)
                            itemsCollection.Add(entity);
                    }

                    //if (entity == null)
                    //{
                    //    entity = dbContext.Set<TEntity>().CreateProxy();
                    //    dbContext.Add(entity);
                    //    itemsCollection.Add(entity);
                    //}
                });

            return itemsCollection;
        }

        //TODO ShouldUpdateRecerences
        public ICollection<TEntity> ConvertToEntities<TEntity>(IEntity rootEntity, IEnumerable<IDto> dtos, ICollection<TEntity> entities = null, IDtoFactory dtoFactory = null)
            where TEntity : class, IEntity
        {
            if (dtos == null)
                dtos = Enumerable.Empty<IDto>();

            var dbContext = rootEntity.ResolveDbContext();

            var itemsCollection = entities ?? new List<TEntity>();
            var dtoCollection = dtos;

            var dtoIds = dtos.Where(_ => _.Id.HasValue).Select(_ => _.Id.Value).ToList();

            var entitiesToRemove = itemsCollection.Where(_ => dtoIds.Contains(_.Id) == false).ToList();
            entitiesToRemove
                .ForEach(_ => itemsCollection.Remove(_));

            var dtoEntityJoin =
                dtoCollection
                .GroupJoin(itemsCollection, dto => dto.Id, entity => entity.Id,
                (dto, entities) => new { dto, entity = entities.SingleOrDefault() })
                .ToList();

            dtoEntityJoin
                .ForEach(_ =>
                {
                    var entity = _.entity;

                    if (_.dto.Id.HasValue && entity == null)
                    {
                        entity = dbContext.Set<TEntity>()
                            .Where(db => db.Id == _.dto.Id.Value)
                            .FirstOrDefault();

                        if (entity != null)
                            itemsCollection.Add(entity);
                    }

                    if (entity == null)
                    {
                        entity = dbContext.Set<TEntity>().CreateProxy();
                        dbContext.Add(entity);
                        itemsCollection.Add(entity);
                    }

                    _.dto.ConvertToEntity(entity, dtoFactory); //TODO ShouldUpdateRecerences paramter
                });

            return itemsCollection;
        }

        public TEntity ConvertToEntity<TEntity>(IDto dto, TEntity entity, IDtoFactory dtoFactory = null)
            where TEntity : IEntity
        {
            if (dto == null)
                return default(TEntity);

            if (dtoFactory == null)
            {
                dtoFactory = ResolveTyped(dto.GetType(), typeof(TEntity));
            }

            var convertToEntityMethod = dtoFactory.GetType()
                .GetMethod("ConvertToEntity"); //IDtoFactory<TDto, TEntity>.ConvertToEntity

            entity = (TEntity)convertToEntityMethod.Invoke(dtoFactory, new object[] { dto, entity });

            return entity;
        }

        public TEntity ConvertToEntity<TDto, TEntity>(TDto dto, TEntity entity)
            where TDto : IDto
            where TEntity : IEntity
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
        where TDto : IDto
        where TEntity : IEntity
    {
        TDto ConvertToDto(TEntity entity, TDto dto);
        TEntity ConvertToEntity(TDto dto, TEntity entity);
    }
}
