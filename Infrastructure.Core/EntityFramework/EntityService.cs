using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using SoftwaredeveloperDotAt.Infrastructure.Core.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class SuppressSaveChangesSection : Section
    {
    }
    public class SuppressValidationSection : Section
    {
    }

    public class EntityServiceDependency<TEntity> : IScopedDependency
        where TEntity : Entity
    {
        public ILogger<EntityService<TEntity>> Logger { get; private set; }
        public IDbContext DbContext { get; private set; }
        public AccessService AccessService { get; private set; }
        public SectionManager SectionManager { get; private set; }
        public EntityQueryService<TEntity> EntityQueryService { get; private set; }
        public EntityValidator<TEntity> Validator { get; private set; }
        public IMemoryCache MemoryCache { get; private set; }
        public ICurrentUserService CurrentUserService { get; private set; }
     

        public EntityServiceDependency(
            ILogger<EntityService<TEntity>> logger,
            IDbContext context,
            AccessService accessService,
            SectionManager sectionManager,
            EntityQueryService<TEntity> entityQueryService,
            IMemoryCache memoryCache,
            ICurrentUserService currentUserService,
            EntityValidator<TEntity> validator = null)
        {
            Logger = logger;
            DbContext = context;
            AccessService = accessService;
            SectionManager = sectionManager;
            EntityQueryService = entityQueryService;
            CurrentUserService = currentUserService;
            MemoryCache = memoryCache;
            Validator = validator;
        }
    }

    public class EntityService<TEntity> : IScopedDependency
        where TEntity : Entity
    {
        protected readonly ILogger<EntityService<TEntity>> _logger;
        protected readonly IDbContext _context;
        protected readonly AccessService _accessService;
        protected readonly SectionManager _sectionManager;
        protected readonly EntityQueryService<TEntity> _entityQueryService;
        protected readonly ICurrentUserService _currentUserService;
        protected readonly EntityValidator<TEntity> _validator;

        protected readonly IMemoryCache _memoryCache;

        public EntityServiceDependency<TEntity> EntityServiceDependency { get; private set; }

        public EntityService(EntityServiceDependency<TEntity> entityServiceDependency)
        {
            EntityServiceDependency = entityServiceDependency;

            _logger = entityServiceDependency.Logger;
            _context = entityServiceDependency.DbContext;
            _accessService = entityServiceDependency.AccessService;
            _sectionManager = entityServiceDependency.SectionManager;
            _entityQueryService = entityServiceDependency.EntityQueryService;
            _currentUserService = entityServiceDependency.CurrentUserService;
            _memoryCache = entityServiceDependency.MemoryCache;
            _validator = entityServiceDependency.Validator;
        }

        public virtual async Task<TDto> GetSingleByIdAsync<TDto>(Guid id)
            where TDto : Dto, new()
        {
            var entity = await GetSingleByIdInternalAsync(id);

            var dto = entity.ConvertToDto<TDto>();

            return dto;
        }

        public virtual Task<TEntity> GetSingleByIdInternalAsync(Guid id)
        {
            return GetSingleInternalAsync((query) => query.Where(_ => _.Id == id));
        }

        public virtual async Task<TDto> GetSingleAsync<TDto>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
            where TDto : Dto, new()
        {
            var entity = await GetSingleInternalAsync(queryExtension);

            var dto = entity.ConvertToDto<TDto>();

            return dto;
        }

        public virtual async Task<TEntity> GetSingleInternalAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
        {
            var query = GetCollectionQueryInternal(queryExtension);

            var entity = await query.SingleOrDefaultAsync();

            if (entity == null)
                return null;

            if (await _accessService.CanReadAsync(entity) == false)
                throw new UnauthorizedAccessException();

            return entity;
        }

        public virtual async Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(IQueryable<TEntity> query)
            where TDto : Dto, new()
        {
            var entities = await query.ToListAsync();

            var dtos = entities.ConvertToDtos<TDto>();

            return dtos;
        }

        public virtual Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
            where TDto : Dto, new()
        {
            var query = GetCollectionQueryInternal(queryExtension);

            return GetCollectionAsync<TDto>(query);
        }

        public virtual IQueryable<TEntity> GetCollectionQueryInternal(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
        {
            var query = _context
                .Set<TEntity>()
                //.Where(_=> _accessService.CanReadQuery(_))
                .AsQueryable();

            query = IncludeAutoQueryProperties(query);

            if (queryExtension != null)
                query = queryExtension(query);

            query = AppendOrderBy(query);

            return query;
        }

        protected virtual IQueryable<TEntity> IncludeAutoQueryProperties(IQueryable<TEntity> query)
        {
            return _entityQueryService.IncludeAutoQueryProperties(query);
        }

        protected virtual IQueryable<TEntity> AppendOrderBy(IQueryable<TEntity> query)
        {
            return _entityQueryService.AppendDefaultOrderBy(query);
        }

        public virtual async Task<Guid> CreateAsync<TDto>(TDto dto)
            where TDto : Dto, new()
        {
            var entity = await CreateInternalAsync<TDto>(dto);

            await SaveChangesAsync(entity);

            return entity.Id;
        }

        public virtual async Task<TEntity> CreateInternalAsync<TDto>(TDto dto)
            where TDto : Dto, new()
        {
            var entity = await CreateInternalAsync(async (e) =>
            {
                dto.ConvertToEntity(e);

                await OnCreateInternalAsync(dto, e);
            });

            return entity;
        }

        public virtual async Task<TEntity> CreateInternalAsync(Action<TEntity> modifyEntity = null)
        {
            var entity = _context.Set<TEntity>().CreateProxy();
            await _context.AddAsync(entity);

            if (modifyEntity != null)
                modifyEntity(entity);

            await OnCreateInternalAsync(entity);

            if (await _accessService.CanCreateAsync(entity) == false)
                throw new UnauthorizedAccessException();

            if (!_sectionManager.IsActive<SuppressValidationSection>())
                await ValidateAndThrowInternalAsync(entity);

            return entity;
        }

        protected virtual Task OnCreateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : Dto, new()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnCreateInternalAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public virtual async Task<Guid> QuickCreateAsync<TDto>(TDto dto)
            where TDto : Dto, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
            {
                var id = await CreateAsync<TDto>(dto);
                return id;
            }
        }

        public virtual async Task<TDto> QuickUpdateAsync<TDto>(TDto dto)
            where TDto : Dto, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
            {
                var updatedDto = await UpdateAsync(dto);
                return updatedDto;
            }
        }

        public virtual async Task<TDto> UpdateAsync<TDto>(TDto dto)
            where TDto : Dto, new()
        {
            var entity = await GetSingleByIdInternalAsync(dto.Id.Value);

            entity = await UpdateInternalAsync(dto, entity);

            await SaveChangesAsync(entity);

            return entity.ConvertToDto<TDto>();
        }

        public virtual async Task<TEntity> UpdateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : Dto, new()
        {
            dto.ConvertToEntity(entity);

            await OnUpdateInternalAsync(dto, entity);

            if (await _accessService.CanUpdateAsync(entity) == false)
                throw new UnauthorizedAccessException();

            if (!_sectionManager.IsActive<SuppressValidationSection>())
                await ValidateAndThrowInternalAsync(entity);

            return entity;
        }

        protected virtual Task OnUpdateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : Dto, new()
        {
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetSingleByIdInternalAsync(id);

            await DeleteInternalAsync(entity);

            await SaveChangesAsync(entity);
        }

        public async Task SaveChangesAsync(TEntity entity)
        {
            if (!_sectionManager.IsActive<SuppressSaveChangesSection>())
            {
                if (await _accessService.CanSaveAsync(entity) == false)
                    throw new UnauthorizedAccessException();

                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task DeleteInternalAsync(TEntity entity)
        {
            _context.Remove(entity);

            await OnDeleteInternalAsync(entity);

            if (await _accessService.CanDeleteAsync(entity) == false)
                throw new UnauthorizedAccessException();
        }

        protected virtual Task OnDeleteInternalAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ValidateAndThrowAsync<TDto>(TDto dto)
          where TDto : Dto, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressSaveChangesSection>())
            {
                var entity = await GetSingleByIdInternalAsync(dto.Id.Value);

                if (entity == null)
                    entity = await CreateInternalAsync(dto);
                else
                    entity = await UpdateInternalAsync<TDto>(dto, entity);
            }
        }

        protected async Task ValidateAndThrowInternalAsync(TEntity entity)
        {
            if (_sectionManager.IsActive<SuppressValidationSection>())
                return;

            if (_validator == null)
                return;

            var validationResult = await _validator?.ValidateAsync(entity);

            if (validationResult.IsValid == false)
                throw validationResult.ToValidationException();
        }
    }
}
