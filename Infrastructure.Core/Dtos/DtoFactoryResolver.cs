using System.Reflection;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

[ScopedDependency]
public class DtoFactoryResolver
{
    [SingletonDependency]
    public class DtoFactoryTypeStore
    {
        private static Dictionary<Tuple<Type, Type>, Type> _dtoFactoryTypes;

        public DtoFactoryTypeStore()
        {
            _dtoFactoryTypes = new Dictionary<Tuple<Type, Type>, Type>();

            var factoryTypes =
                AssemblyUtils.AllLoadedTypes()
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

        public bool Contains(Type dtoType, Type entityType)
        {
            var tuple = new Tuple<Type, Type>(dtoType, entityType);
            return _dtoFactoryTypes.ContainsKey(tuple);
        }

        public Type GetFactoryType(Type dtoType, Type entityType)
        {
            var tuple = new Tuple<Type, Type>(dtoType, entityType);

            var factoryType = _dtoFactoryTypes[tuple];

            return factoryType;
        }
    }

    protected readonly IServiceProvider _serviceProvider;
    protected readonly DtoFactoryTypeStore _dtoFactoryTypeStore;
    protected readonly IMemoryCache _memoryCache;
    protected readonly IDbContext _dbContext;

    public DtoFactoryResolver(IServiceProvider serviceProvider, DtoFactoryTypeStore dtoFactoryResolver, IMemoryCache memoryCache, IDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dtoFactoryTypeStore = dtoFactoryResolver;
        _memoryCache = memoryCache;
        _dbContext = dbContext;
    }

    private Tuple<Type, Type> FindMatchingTuble(Type dtoType, Type entityType)
    {
        var tuple = new Tuple<Type, Type>(dtoType, entityType);

        if (_dtoFactoryTypeStore.Contains(dtoType, entityType) == true)
            return tuple;

        if (tuple.Item1.BaseType == null || tuple.Item1.BaseType == typeof(object))
            return null;

        var resultTuple = FindMatchingTuble(tuple.Item1.BaseType, tuple.Item2);

        if (resultTuple != null)
            return resultTuple;

        if (tuple.Item2.BaseType == null || tuple.Item2.BaseType == typeof(object))
            return null;

        resultTuple = FindMatchingTuble(tuple.Item1, tuple.Item2.BaseType);

        if (resultTuple != null)
            return resultTuple;

        return FindMatchingTuble(tuple.Item1.BaseType, tuple.Item2.BaseType);
    }

    private static MethodInfo resolveMethod = typeof(DtoFactoryResolver)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(m =>
            m.Name == nameof(Resolve) &&
            m.IsGenericMethodDefinition &&
            m.GetGenericArguments().Length == 2 &&
            m.GetParameters().Length == 0);

    private IDtoFactory Resolve(IDto dto, IEntity entity)
    {
        return Resolve(dto.GetType(), entity.GetType());
    }

    private IDtoFactory Resolve(Type dtoType, Type entityType)
    {
        return (IDtoFactory)resolveMethod
            .MakeGenericMethod([dtoType, entityType.UnProxy()])
            .Invoke(this, null);
    }

    private IDtoFactory<TDto, TEntity> Resolve<TDto, TEntity>()
        where TDto : IDto
        where TEntity : IEntity
    {
        var cacheKey = $"{nameof(DtoFactoryResolver)}_{nameof(Resolve)}_{typeof(TDto).FullName}_{typeof(TEntity).FullName}";

        if (!_memoryCache.TryGetValue(cacheKey, out Type cachedfactoryType))
        {
            var tuple = FindMatchingTuble(typeof(TDto), typeof(TEntity));

            if (tuple == null)
            {
                cachedfactoryType = typeof(DefaultDtoFactory<TDto, TEntity>);
            }
            else
            {
                var factoryType = _dtoFactoryTypeStore.GetFactoryType(tuple.Item1, tuple.Item2);

                if (factoryType.ContainsGenericParameters)
                    factoryType = factoryType.MakeGenericType(new[] { typeof(TDto), typeof(TEntity) });

                cachedfactoryType = factoryType;
            }

            _memoryCache.Set(cacheKey, cachedfactoryType);
        }

        var factory = _serviceProvider.GetRequiredService(cachedfactoryType);
        return (IDtoFactory<TDto, TEntity>)factory;
    }

    private static MethodInfo convertToDtosMethod = typeof(DtoFactoryResolver)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m =>
                m.Name == nameof(ConvertToDtos) &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1);

    public IEnumerable<IDto> ConvertToDtos<TEntity>(IEnumerable<TEntity> entities, Type dtoType)
        where TEntity : IEntity
    {
        return (IEnumerable<IDto>)convertToDtosMethod
            .MakeGenericMethod([dtoType])
            .Invoke(this, [entities, null]);
    }

    public IEnumerable<TDto> ConvertToDtos<TDto>(IEnumerable<IEntity> entities)
        where TDto : IDto
    {
        if (entities == null)
            return null;

        var dtos = entities
            .Select(entity =>
            {
                var dto = (TDto)Activator.CreateInstance(typeof(TDto));
                return ConvertToDto(entity, dto);
            })
            .ToList();

        return dtos;
    }

    private static MethodInfo convertToDtoMethod = typeof(DtoFactoryResolver)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m =>
                m.Name == nameof(ConvertToDto) &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 2);

    public IDto ConvertToDto(IEntity entity, Type dtoType)
    {
        return (IDto)convertToDtoMethod
            .MakeGenericMethod([dtoType])
            .Invoke(this, [entity, null]);
    }

    public TDto ConvertToDto<TDto>(IEntity entity, TDto dto = default)
        where TDto : IDto
    {
        if (entity == null)
            return default;

        if (dto == null)
            dto = (TDto)Activator.CreateInstance(typeof(TDto));

        var dtoFactory = Resolve(typeof(TDto), entity.GetType());

        return (TDto)dtoFactory.ConvertToDto(entity, dto);
    }

    public ICollection<TEntity> ConvertIdsToEntities<TEntity>(IEnumerable<Guid> dtoIds, ICollection<TEntity> entities = null)
        where TEntity : class, IEntity
    {
        if (dtoIds == null)
            dtoIds = Enumerable.Empty<Guid>();

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
                    entity = _dbContext.Set<TEntity>()
                        .Where(db => db.Id == _.dtoId)
                        .FirstOrDefault();

                    if (entity != null)
                        itemsCollection.Add(entity);
                }

                if (entity == null)
                {
                    entity = _dbContext.CreateEntityAync<TEntity>().GetAwaiter().GetResult();
                    entity.Id = _.dtoId;

                    itemsCollection.Add(entity);
                }
            });

        return itemsCollection;
    }

    public ICollection<TEntity> ConvertToEntities<TEntity>(IEnumerable<IDto> dtos, ICollection<TEntity> entities = null)
        where TEntity : class, IEntity
    {
        if (dtos == null)
            dtos = Enumerable.Empty<IDto>();

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
                    entity = _dbContext.Set<TEntity>()
                        .Where(db => db.Id == _.dto.Id.Value)
                        .FirstOrDefault();

                    if (entity != null)
                        itemsCollection.Add(entity);
                }

                if (_.dto.Id.HasValue && entity == null)
                {
                    entity = _dbContext.CreateEntityAync<TEntity>().GetAwaiter().GetResult();
                    entity.Id = _.dto.Id.Value;
                    itemsCollection.Add(entity);
                }

                ConvertToEntity(_.dto, entity);
            });

        return itemsCollection;
    }

    public TEntity ConvertToEntity<TEntity>(IDto dto, TEntity entity = null)
        where TEntity : class, IEntity
    {
        if (dto == null)
            return null;

        var factory = Resolve(dto.GetType(), typeof(TEntity));

        if (entity == null)
        {
            entity = _dbContext.CreateEntityAync<TEntity>().GetAwaiter().GetResult();
        }

        return (TEntity)factory.ConvertToEntity(dto, entity);
    }
}

[ScopedDependency]
public interface IDtoFactory
{
    IDto ConvertToDto(IEntity entity, IDto dto);
    IEntity ConvertToEntity(IDto dto, IEntity entity);
}

public interface IDtoFactory<TDto, TEntity> : IDtoFactory
    where TDto : IDto
    where TEntity : IEntity
{
    TDto ConvertToDto(TEntity entity, TDto dto);
    IDto IDtoFactory.ConvertToDto(IEntity entity, IDto dto) => ConvertToDto((TEntity)entity, (TDto)dto);

    TEntity ConvertToEntity(TDto dto, TEntity entity);
    IEntity IDtoFactory.ConvertToEntity(IDto dto, IEntity entity) => ConvertToEntity((TDto)dto, (TEntity)entity);
}