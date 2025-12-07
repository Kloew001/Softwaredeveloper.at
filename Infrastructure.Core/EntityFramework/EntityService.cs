using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.PrePersistant;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public class SuppressSaveChangesSection : Section
{
}
public class SuppressValidationSection : Section
{
}

[TransientDependency]
public class EntityServiceDependency<TEntity>
    where TEntity : Entity
{
    public IServiceProvider ServiceProvider { get; private set; }
    public ILogger<EntityService<TEntity>> Logger { get; private set; }
    public IDbContext DbContext { get; private set; }
    public AccessService AccessService { get; private set; }
    public SectionManager SectionManager { get; private set; }
    public EntityQueryService<TEntity> EntityQueryService { get; private set; }
    public EntityValidator<TEntity> Validator { get; private set; }
    public ICacheService CacheService { get; private set; }
    public MultilingualService MultilingualService { get; private set; }
    public ICurrentUserService CurrentUserService { get; private set; }
    public IDateTimeService DateTimeService { get; private set; }

    public EntityServiceDependency(
        IServiceProvider serviceProvider,
        ILogger<EntityService<TEntity>> logger,
        IDbContext context,
        AccessService accessService,
        SectionManager sectionManager,
        EntityQueryService<TEntity> entityQueryService,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        MultilingualService multilingualService,
        IDateTimeService dateTimeService)
    {
        ServiceProvider = serviceProvider;

        Logger = logger;
        DbContext = context;
        AccessService = accessService;
        SectionManager = sectionManager;
        EntityQueryService = entityQueryService;
        CurrentUserService = currentUserService;
        CacheService = cacheService;
        MultilingualService = multilingualService;
        DateTimeService = dateTimeService;

        Validator = GetService<EntityValidator<TEntity>>();
    }

    public T GetService<T>()
    {
        return ServiceProvider.GetService<T>();
    }
}

[ScopedDependency]
public class EntityService<TEntity>
    where TEntity : Entity
{
    protected readonly ILogger<EntityService<TEntity>> _logger;
    protected readonly IDbContext _context;
    protected readonly AccessService _accessService;
    protected readonly SectionManager _sectionManager;
    protected readonly EntityQueryService<TEntity> _entityQueryService;
    protected readonly ICurrentUserService _currentUserService;
    protected readonly EntityValidator<TEntity> _validator;
    protected readonly ICacheService _cacheService;
    protected readonly MultilingualService _multilingualService;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IDateTimeService _dateTimeService;

    public EntityServiceDependency<TEntity> EntityServiceDependency { get; private set; }

    public EntityService(EntityServiceDependency<TEntity> entityServiceDependency)
    {
        EntityServiceDependency = entityServiceDependency;

        _serviceProvider = entityServiceDependency.ServiceProvider;
        _logger = entityServiceDependency.Logger;
        _context = entityServiceDependency.DbContext;
        _accessService = entityServiceDependency.AccessService;
        _sectionManager = entityServiceDependency.SectionManager;
        _entityQueryService = entityServiceDependency.EntityQueryService;
        _currentUserService = entityServiceDependency.CurrentUserService;
        _cacheService = entityServiceDependency.CacheService;
        _multilingualService = entityServiceDependency.MultilingualService;
        _dateTimeService = entityServiceDependency.DateTimeService;

        _validator = entityServiceDependency.Validator;
    }

    public virtual async Task<TDto> GetSingleByIdAsync<TDto>(Guid id)
        where TDto : Dto
    {
        var entity = await GetSingleByIdAsync(id);

        var dto = entity.ConvertToDto<TDto>(serviceProvider: _serviceProvider);

        return dto;
    }

    public virtual Task<TEntity> GetSingleByIdAsync(Guid id)
    {
        return GetSingleAsync((query) => query.Where(_ => _.Id == id));
    }

    public virtual async Task<TDto> GetSingleAsync<TDto>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
        where TDto : Dto
    {
        var entity = await GetSingleAsync(queryExtension);

        var dto = entity.ConvertToDto<TDto>(serviceProvider: _serviceProvider);

        return dto;
    }
    public virtual async Task<TEntity> GetSingleAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
    {
        var query = await GetQueryAsync(queryExtension);

        var entity = await query.SingleOrDefaultAsync();

        if (entity == null)
            return null;

        if (await this.CanReadAsync(entity) == false)
            throw new UnauthorizedAccessException();

        return entity;
    }

    public virtual async Task<TEntity> GetFirstAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
    {
        var query = await GetQueryAsync(queryExtension);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
            return null;

        if (await this.CanReadAsync(entity) == false)
            throw new UnauthorizedAccessException();

        return entity;
    }

    public virtual async Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
        where TDto : Dto
    {
        var entities = await GetCollectionAsync(queryExtension);

        var dtos = entities.ConvertToDtos<TDto>(serviceProvider: _serviceProvider);

        return dtos;
    }

    public virtual async Task<IEnumerable<TEntity>> GetCollectionAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
    {
        var entities = await (await GetQueryAsync(queryExtension)).ToListAsync();

        return entities;
    }

    public virtual async Task<PageResult<TDto>> GetPagedCollectionAsync<TDto>(PageFilter pageFilter, Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
        where TDto : Dto
    {
        var entityPageResult = await GetPagedCollectionAsync(pageFilter, queryExtension);

        var dtoPageResult = new PageResult<TDto>
        {
            Page = entityPageResult.Page,
            PageSize = entityPageResult.PageSize,
            TotalCount = entityPageResult.TotalCount,

            PageItems = entityPageResult.PageItems.ConvertToDtos<TDto>(serviceProvider: _serviceProvider)
        };

        return dtoPageResult;
    }

    public virtual async Task<PageResult<TEntity>> GetPagedCollectionAsync(PageFilter pageFilter, Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
    {
        var query = await GetQueryAsync(queryExtension);

        var entityPageResult = await _entityQueryService.GetPageResultAsync(query, pageFilter);

        return entityPageResult;
    }

    public virtual async ValueTask<IQueryable<TEntity>> GetQueryAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
    {
        var query = _context
            .Set<TEntity>()
            .AsQueryable();

        var accessConditionInfo = _accessService.ResolveAccessConditionInfo<TEntity>();
        var accessCondition = accessConditionInfo.AccessCondition as IAccessCondition<TEntity>;

        if (accessCondition != null)
            query = await accessCondition.CanReadQueryAsync(query);
        else
        {

        }

        if (queryExtension != null)
            query = queryExtension(query);

        query = AppendOrderBy(query);

        return query;
    }

    protected virtual IQueryable<TEntity> AppendOrderBy(IQueryable<TEntity> query)
    {
        return _entityQueryService.AppendDefaultOrderBy(query);
    }

    public virtual async Task<Guid> QuickCreateAsync<TDto>(TDto dto)
        where TDto : Dto
    {
        using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
        {
            var entity = await QuickCreateAsync((e) =>
            {
                if (dto != null)
                    dto.ConvertToEntity(e, serviceProvider: _serviceProvider);

                return ValueTask.CompletedTask;
            });

            await SaveAsync(entity);

            return entity.Id;
        }
    }

    public virtual async Task<TEntity> QuickCreateAsync(Func<TEntity, ValueTask> modifyEntity = null)
    {
        using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
        {
            var entity = await CreateAsync(modifyEntity);
            return entity;
        }
    }

    public virtual async Task<TDto> CreateAsync<TDto>(TDto dto)
        where TDto : Dto
    {
        var entity = await CreateAsync((e) =>
        {
            if (dto != null)
                dto.ConvertToEntity(e, serviceProvider: _serviceProvider);

            return ValueTask.CompletedTask;
        });

        await SaveAsync(entity);

        return entity.ConvertToDto<TDto>(serviceProvider: _serviceProvider);
    }

    public virtual async Task<TEntity> CreateAsync(Func<TEntity, ValueTask> modifyEntity = null)
    {
        var entity = _context.Set<TEntity>().CreateProxy();
        await _context.AddAsync(entity);

        if (modifyEntity != null)
            await modifyEntity(entity);

        await OnCreateAsync(entity);

        if (await this.CanCreateAsync(entity) == false)
            throw new UnauthorizedAccessException();

        if (!_sectionManager.IsActive<SuppressValidationSection>())
            await ValidateAndThrowAsync(entity);

        return entity;
    }

    protected virtual Task OnCreateAsync(TEntity entity)
    {
        if (entity is ISupportPrePersistent supportPrePersistent)
        {
            supportPrePersistent.PrePersitent = true;
        }

        return Task.CompletedTask;
    }

    public virtual async Task<TDto> QuickUpdateAsync<TDto>(TDto dto)
        where TDto : Dto
    {
        using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
        {
            var updatedDto = await UpdateAsync(dto);
            return updatedDto;
        }
    }

    public virtual async Task<TEntity> QuickUpdateAsync(TEntity entity)
    {
        using (_sectionManager.CreateSectionScope<SuppressValidationSection>())
        {
            var updatedDto = await UpdateAsync(entity);
            return updatedDto;
        }
    }

    public virtual async Task<TDto> UpdateAsync<TDto>(TDto dto)
        where TDto : Dto
    {
        var entity = await GetSingleByIdAsync(dto.Id.Value);

        dto.ConvertToEntity(entity, serviceProvider: _serviceProvider);

        entity = await UpdateAsync(entity);

        await SaveAsync(entity);

        return entity.ConvertToDto<TDto>(serviceProvider: _serviceProvider);
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        await OnUpdateAsync(entity);

        if (await this.CanUpdateAsync(entity) == false)
            throw new UnauthorizedAccessException();

        if (!_sectionManager.IsActive<SuppressValidationSection>())
            await ValidateAndThrowAsync(entity);

        return entity;
    }

    protected virtual Task OnUpdateAsync(TEntity entity)
    {
        if (entity is ISupportPrePersistent supportPrePersistent)
        {
            supportPrePersistent.PrePersitent = false;
        }

        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetSingleByIdAsync(id);

        await DeleteAsync(entity);

        await SaveAsync(entity);
    }

    public async Task SaveAsync(TEntity entity)
    {
        if (!_sectionManager.IsActive<SuppressSaveChangesSection>())
        {
            if (await this.CanSaveAsync(entity) == false)
                throw new UnauthorizedAccessException();

            await OnSaveAsync(entity);

            await _context.SaveChangesAsync();
        }
    }

    protected virtual Task OnSaveAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        if (await this.CanDeleteAsync(entity) == false)
            throw new UnauthorizedAccessException();

        await OnDeleteAsync(entity);

        _context.Remove(entity);
    }

    protected virtual Task OnDeleteAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    public virtual async Task ValidateAndThrowAsync<TDto>(TDto dto)
      where TDto : Dto
    {
        using (_sectionManager.CreateSectionScope<SuppressSaveChangesSection>())
        {
            var entity = await GetSingleByIdAsync(dto.Id.Value);

            if (entity == null)
                await CreateAsync(dto);
            else
                await UpdateAsync(dto);
        }
    }

    public async Task ValidateAndThrowAsync(TEntity entity)
    {
        var validationResult = await ValidateAsync(entity);

        if (validationResult == null)
            return;

        if (validationResult.IsValid == false)
            throw ToValidationException(validationResult);
    }

    private ValidationException ToValidationException(FluentValidation.Results.ValidationResult validationResult)
    {
        return new ValidationException(this.GetText("ValidationError.Message"),
            validationResult.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage
            }));
    }

    public async Task<FluentValidation.Results.ValidationResult> ValidateAsync(TEntity entity)
    {
        if (_sectionManager.IsActive<SuppressValidationSection>())
            return null;

        if (_validator == null)
            return null;

        var validationResult = await _validator?.ValidateAsync(entity);

        return validationResult;
    }
}