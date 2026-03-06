using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

[ScopedDependency]
public class DtoFactoryResolver
{
    [SingletonDependency]
    public class DtoFactoryTypeStore
    {
        public readonly struct DtoFactoryStoreItem(Type dtoType, Type entityType, Type dtoFactoryType)
        {
            public Type DtoType { get; } = dtoType;
            public Type EntityType { get; } = entityType;
            public Type DtoFactoryType { get; } = dtoFactoryType;

            public override string ToString()
            {
                return $"[{DtoType.Name}, {EntityType.Name}, {DtoFactoryType.Name}]";
            }
        }

        private readonly DtoFactoryStoreItem[] _store = [];

        public DtoFactoryTypeStore()
        {
            var factoryTypes = AssemblyUtils.GetDerivedConcretClasses<IDtoFactory>();

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

                        if(Exists(dtoTypeGeneric, entityTypeGeneric))
                        {
                            throw new InvalidOperationException($"Multiple DTO factories found for DTO type '{dtoTypeGeneric.FullName}' and entity type '{entityTypeGeneric.FullName}'.");
                        }

                        _store = [.._store, new (dtoTypeGeneric, entityTypeGeneric, factoryTyp)];
                    }
                }
            }
        }

        public IEnumerable<DtoFactoryStoreItem> GetAllFactoryStoreItems()
        {
            return _store;
        }

        public bool Exists(Type dtoType, Type entityType)
        {
            return _store.Any(_ => _.DtoType == dtoType && _.EntityType == entityType);
        }

        public Type GetDtoType(IDtoFactory dtoFactory, Type entityType)
        {
            return GetDtoType(dtoFactory.GetType(), entityType);
        }

        public Type GetDtoType(Type dtoFactoryType, Type entityType)
        {
            var dtoInterface = dtoFactoryType
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IDtoFactory<,>) &&
                    i.GetGenericArguments()[1].IsAssignableFrom(entityType));

            var dtoType = dtoInterface.GetGenericArguments()[0];

            return dtoType;
        }

        public Type GetFactoryType(Type dtoType, Type entityType)
        {
            return _store
                .Where(_ => _.DtoType == dtoType && _.EntityType == entityType)
                .Select(_ => _.DtoFactoryType)
                .SingleOrDefault();
        }
    }

    protected readonly IServiceProvider _serviceProvider;
    protected readonly DtoFactoryTypeStore _dtoFactoryTypeStore;
    protected readonly IMemoryCache _memoryCache;
    protected readonly IDbContext _dbContext;
    protected readonly ILogger<DtoFactoryResolver> _logger;

    public DtoFactoryResolver(
        IServiceProvider serviceProvider, 
        DtoFactoryTypeStore dtoFactoryResolver, 
        IMemoryCache memoryCache, 
        IDbContext dbContext,
        ILogger<DtoFactoryResolver> logger)
    {
        _serviceProvider = serviceProvider;
        _dtoFactoryTypeStore = dtoFactoryResolver;
        _memoryCache = memoryCache;
        _dbContext = dbContext;
        _logger = logger;
    }

    private Type FindMatchingFactoryType(Type dtoType, Type entityType)
    {
        if (_dtoFactoryTypeStore.Exists(dtoType, entityType))
            return _dtoFactoryTypeStore.GetFactoryType(dtoType, entityType);

        var storeItems = _dtoFactoryTypeStore
        .GetAllFactoryStoreItems()
        .Where(_ =>
            (dtoType.IsAssignableFrom(_.DtoType) || _.DtoType.IsAssignableFrom(dtoType)) && // Dto kann in beiden Richtungen matchen, für Vererbung
            _.EntityType.IsAssignableFrom(entityType)); //entityType ist immer konkret

        var candidates = storeItems
            .Select(_ => new
            {
                _.DtoFactoryType,
                DtoDistance = GetInheritanceDistance(dtoType, _.DtoType),
                EntityDistance = GetInheritanceDistance(entityType, _.EntityType)
            })
            .Where(_ => _.DtoDistance.HasValue && _.EntityDistance.HasValue)
            .OrderBy(c => c.DtoDistance)
            .ThenBy(c => c.EntityDistance);

        var bestSpecificCandidate = candidates.FirstOrDefault();

        if (bestSpecificCandidate != null)
            return bestSpecificCandidate.DtoFactoryType;

        return null;
    }

    private static int? GetInheritanceDistance(Type requestedType, Type candidateType)
    {
        // exakter Treffer
        // GetInheritanceDistance(typeof(CarDto), typeof(CarDto))          // => 0

        // Kandidat ist spezieller (abgeleitet von requested)
        //GetInheritanceDistance(typeof(CarDto), typeof(ElectroCarDto))   // => -1
        //GetInheritanceDistance(typeof(DtoBase), typeof(CarDto))         // => -1
        //GetInheritanceDistance(typeof(DtoBase), typeof(ElectroCarDto))  // => -2

        // Kandidat ist allgemeiner (Basistyp von requested)
        //GetInheritanceDistance(typeof(CarDto), typeof(DtoBase))         // => 1
        //GetInheritanceDistance(typeof(ElectroCarDto), typeof(CarDto))   // => 1
        //GetInheritanceDistance(typeof(ElectroCarDto), typeof(DtoBase))  // => 2

        // Nicht kompatibel
        //GetInheritanceDistance(typeof(string), typeof(CarDto))          // => null
        
        if (requestedType == candidateType)
            return 0;

        // Wenn spezieller als der angefragte Typ, bevorzugen
        if (requestedType.IsAssignableFrom(candidateType))
        {
            var downDistance = GetDistanceUp(candidateType, requestedType, 0);
            return downDistance.HasValue ? -downDistance : null;
        }

        // Wenn allgemeiner
        if (candidateType.IsAssignableFrom(requestedType))
        {
            var upDistance = GetDistanceUp(requestedType, candidateType, 0);
            return upDistance;
        }

        return null;
    }

    private static int? GetDistanceUp(Type current, Type target, int distance)
    {
        if (current == null || current == typeof(object))
            return null;

        if (current == target)
            return distance;

        // Rekursiv nach oben in der Vererbungshierarchie wandern
        return GetDistanceUp(current.BaseType, target, distance + 1);
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

    private IDtoFactory Resolve<TDto, TEntity>()
        where TDto : IDto
        where TEntity : IEntity
    {
        var cacheKey = $"{nameof(DtoFactoryResolver)}_{nameof(Resolve)}_{typeof(TDto).FullName}_{typeof(TEntity).FullName}";

        if (!_memoryCache.TryGetValue(cacheKey, out Type cachedfactoryType))
        {
            var factoryType = FindMatchingFactoryType(typeof(TDto), typeof(TEntity));

            if (factoryType == null)
            {
                cachedfactoryType = typeof(DefaultDtoFactory<TDto, TEntity>);
            }
            else
            {
                if (factoryType.ContainsGenericParameters)
                    factoryType = factoryType.MakeGenericType([typeof(TDto), typeof(TEntity)]);

                cachedfactoryType = factoryType;
            }

            _memoryCache.Set(cacheKey, cachedfactoryType);
        }

        var factory = _serviceProvider.GetRequiredService(cachedfactoryType);
        return (IDtoFactory)factory;
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
            .Invoke(this, [entities]);
    }

    public IEnumerable<TDto> ConvertToDtos<TDto>(IEnumerable<IEntity> entities)
        where TDto : IDto
    {
        if (entities == null)
            return null;
        
        var stopwatch = Stopwatch.StartNew();

        var dtos = entities
            .Select(entity =>
            {
                return ConvertToDto<TDto>(entity);
            })
            .ToList();

        if(stopwatch.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning("Converted {Count} entities to dtos in {ElapsedMilliseconds} ms, which is longer than the threshold of 1000 ms. Consider optimizing the DTO factories or caching the results.", entities.Count(), stopwatch.ElapsedMilliseconds);
        }

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

        var stopwatch = Stopwatch.StartNew();

        var dtoFactory = Resolve(typeof(TDto), entity.GetType());

        if (dto == null)
        {
            dto = CreateDto<TDto>(dtoFactory, entity);
        }

        dto = (TDto)dtoFactory.ConvertToDto(entity, dto);

        if (stopwatch.ElapsedMilliseconds > 100)
        {
            _logger.LogWarning("Converted entity of type {EntityType} to DTO of type {DtoType} in {ElapsedMilliseconds} ms, which is longer than the threshold of 500 ms. Consider optimizing the DTO factory or caching the results.", entity.GetType().FullName, typeof(TDto).FullName, stopwatch.ElapsedMilliseconds);
        }

        return dto;
    }

    private static readonly ConcurrentDictionary<(Type factoryType, Type entityType), Func<object>> _dtoActivatorCache
        = new();

    private TDto CreateDto<TDto>(IDtoFactory dtoFactory, IEntity entity)
        where TDto : IDto
    {
        var key = (dtoFactory.GetType(), entity.GetType());

        var activator = _dtoActivatorCache.GetOrAdd(key, k =>
        {
            var dtoType = _dtoFactoryTypeStore.GetDtoType(dtoFactory, entity.GetType());

            var ctor = dtoType.GetConstructor(Type.EmptyTypes)
                      ?? throw new InvalidOperationException($"Type {dtoType.FullName} needs a parameterless constructor.");

            return () => ctor.Invoke(null);
        });

        return (TDto)activator();
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