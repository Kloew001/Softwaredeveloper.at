﻿using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using DocumentFormat.OpenXml.Vml.Office;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class SuppressSaveChangesSection : Section
    {
    }

    public class EntityService<TEntity> : IScopedService
        where TEntity : BaseEntity
    {
        protected readonly IDbContext _context;
        protected readonly AccessService _accessService;
        private readonly SectionManager _sectionManager;

        public EntityService(IDbContext context, AccessService accessService, SectionManager sectionManager)
        {
            _context = context;
            _accessService = accessService;
            _sectionManager = sectionManager;
        }

        public virtual async Task<TDto> GetSingleByIdAsync<TDto>(Guid id)
            where TDto : DtoBase, new()
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
            where TDto : DtoBase, new()
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
            where TDto : DtoBase, new()
        {
            var entities = await query.ToListAsync();

            var dtos = entities.ConvertToDtos<TDto>();

            return dtos;
        }

        public virtual Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryExtension = null)
            where TDto : DtoBase, new()
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

            typeof(TEntity).GetProperties()
                .Where(_ => _.GetCustomAttributes(typeof(AutoQueryIncludeAttribute), true)?.Any() == true)
                .ToList()
                .ForEach(_ => query = query.Include(_.Name));

            if (queryExtension != null)
                query = queryExtension(query);

            query = AppendOrderBy(query);

            return query;
        }

        protected virtual IQueryable<TEntity> AppendOrderBy(IQueryable<TEntity> query)
        {
            if(query is IOrderedQueryable<TEntity>)
                return query;

            var sortOrders =
                typeof(TEntity).GetProperties()
                    .Select(_ => new { Property = _, Attribute = _.GetCustomAttributes(typeof(OrderByAttribute), false).SingleOrDefault() as OrderByAttribute })
                    .Where(_ => _.Attribute != null)
                    .ToList();

            if (sortOrders.Any())
            {
                var isFirst = true;
                foreach (var sortOrder in sortOrders.OrderBy(_=>_.Attribute.Order))
                {
                    var direction = sortOrder.Attribute.Direction;
                    var order = sortOrder.Attribute.Order;
                    if (isFirst)
                    {
                        if (direction == OrderByAttribute.SortDirection.Ascending)
                            query = query.OrderByPropertyName(sortOrder.Property.Name);
                        else
                            query = query.OrderByPropertyNameDescending(sortOrder.Property.Name);
                    }
                    else
                    {
                        if (direction == OrderByAttribute.SortDirection.Ascending)
                            query = query.ThenByPropertyName(sortOrder.Property.Name);
                        else
                            query = query.ThenByPropertyNameDescending(sortOrder.Property.Name);

                    }

                    isFirst = false;
                }

            }
            else if (typeof(ISupportIndex).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyName(nameof(ISupportIndex.Index));
            }
            else if (typeof(ISupportDisplayName).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyName(nameof(ISupportDisplayName.DisplayName));
            }
            else if (typeof(ChangeTrackedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyNameDescending(nameof(ChangeTrackedEntity.DateModified));
            }
            else
            {
                query = query.OrderBy(_ => _.Id);
            }

            return query;
        }

        public virtual async Task<TDtoOut> CreateVirtualAsync<TDto, TDtoOut>(TDto dto)
            where TDto : DtoBase, new()
            where TDtoOut : DtoBase, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressSaveChangesSection>())
            {
                var entity = await CreateInternalAsync<TDto>(dto);

                return entity.ConvertToDto<TDtoOut>();
            }
        }

        public virtual async Task<Guid> CreateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = await CreateInternalAsync<TDto>(dto);

            if (!_sectionManager.IsActive<SuppressSaveChangesSection>())
            {
                if (await _accessService.CanSaveAsync(entity) == false)
                    throw new UnauthorizedAccessException();

                await _context.SaveChangesAsync();
            }
            return entity.Id;
        }

        public virtual async Task<TEntity> CreateInternalAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
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

            return entity;
        }

        protected virtual Task OnCreateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : DtoBase, new()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnCreateInternalAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public virtual async Task UpdateVirtualAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressSaveChangesSection>())
            {
                await UpdateAsync(dto);
            }
        }

        public virtual async Task<TDtoOut> UpdateVirtualAsync<TDto, TDtoOut>(TDto dto)
            where TDto : DtoBase, new()
            where TDtoOut : DtoBase, new()
        {
            using (_sectionManager.CreateSectionScope<SuppressSaveChangesSection>())
            {
                var entity = await GetSingleByIdInternalAsync(dto.Id.Value);
                entity = await UpdateInternalAsync<TDto>(dto, entity);

                return entity.ConvertToDto<TDtoOut>();
            }
        }

        public virtual async Task UpdateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = await GetSingleByIdInternalAsync(dto.Id.Value);

            entity = await UpdateInternalAsync(dto, entity);

            if (!_sectionManager.IsActive<SuppressSaveChangesSection>())
            {
                if (await _accessService.CanSaveAsync(entity) == false)
                    throw new UnauthorizedAccessException();

                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<TEntity> UpdateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : DtoBase, new()
        {
            dto.ConvertToEntity(entity);

            await OnUpdateInternalAsync(dto, entity);

            if (await _accessService.CanUpdateAsync(entity) == false)
                throw new UnauthorizedAccessException();

            return entity;
        }

        protected virtual Task OnUpdateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : DtoBase, new()
        {
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await DeleteInternalAsync(id);

            if (!_sectionManager.IsActive<SuppressSaveChangesSection>())
            {
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task DeleteInternalAsync(Guid id)
        {
            var entity = await GetSingleByIdInternalAsync(id);

            _context.Remove(entity);

            await OnDeleteInternalAsync(entity);

            if (await _accessService.CanDeleteAsync(entity) == false)
                throw new UnauthorizedAccessException();
        }

        protected virtual Task OnDeleteInternalAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

    }
}
