using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class EntityService<TEntity> : IScopedService
        where TEntity : BaseEntity
    {
        protected readonly IDbContext _context;
        protected readonly AccessService _accessService;

        public EntityService(IDbContext context, AccessService accessService)
        {
            _context = context;
            _accessService = accessService;
        }

        public async Task<TDto> GetSingleByIdAsync<TDto>(Guid id)
            where TDto : DtoBase, new()
        {
            var dto = (await GetSingleByIdInternalAsync(id))
                   .ConvertToDto<TDto>();

            return dto;
        }

        public Task<TEntity> GetSingleByIdInternalAsync(Guid id)
        {
            return GetSingleInternalAsync((_) => _.Id == id);
        }

        public async Task<TDto> GetSingleAsync<TDto>(Expression<Func<TEntity, bool>> whereClause = null)
            where TDto : DtoBase, new()
        {
            var dto = (await GetSingleInternalAsync(whereClause))
                   .ConvertToDto<TDto>();

            return dto;
        }

        public async Task<TEntity> GetSingleInternalAsync(Expression<Func<TEntity, bool>> whereClause = null)
        {
            var entity = await _context
                .Set<TEntity>()
                .Where(whereClause)
                .SingleOrDefaultAsync();

            if (entity == null)
                return null;

            if (await _accessService.CanReadAsync(entity) == false)
                throw new UnauthorizedAccessException();

            return entity;
        }

        public async Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(Expression<Func<TEntity, bool>> whereClause = null)
            where TDto : DtoBase, new()
        {
            var query = GetCollectionInternal(whereClause);
            var entities = await query.ToListAsync();

            var dtos = entities.ConvertToDtos<TDto>();

            return dtos;
        }

        public IQueryable<TEntity> GetCollectionInternal(Expression<Func<TEntity, bool>> whereClause = null)
        {
            var query = _context
                .Set<TEntity>()
                //.Where(_=> _accessService.CanReadQuery(_))
                .AsQueryable<TEntity>();

            if (whereClause != null)
                query = query.Where(whereClause);

            query = AppendOrderBy(query);

            return query;
        }

        protected virtual IQueryable<TEntity> AppendOrderBy(IQueryable<TEntity> query)
        {
            if (typeof(ISupportIndex).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Cast<ISupportIndex>().OrderByIndex().Cast<TEntity>();
            }
            else if (typeof(ISupportDisplayName).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Cast<ISupportDisplayName>().OrderByDisplayName().Cast<TEntity>();
            }
            else if (typeof(ChangeTrackedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Cast<ChangeTrackedEntity>().OrderBy(_ => _.DateModified).Cast<TEntity>();
            }
            else
            {
                query = query.OrderBy(_ => _.Id);
            }

            return query;
        }

        public async Task<Guid> CreateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = await CreateInternalAsync(dto);

            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public virtual async Task<TEntity> CreateInternalAsync<TDto>(TDto dto, TEntity entity = null)
            where TDto : DtoBase, new()
        {
            if (entity == null)
            {
                entity = _context.Set<TEntity>().CreateProxy();
                await _context.AddAsync(entity);
            }

            dto.ConvertToEntity(entity);

            await OnCreateInternalAsync(dto, entity);

            if (await _accessService.CanCreateAsync(entity) == false)
                throw new UnauthorizedAccessException();

            return entity;
        }

        protected virtual Task OnCreateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : DtoBase, new()
        {
            return Task.CompletedTask;
        }

        public async Task UpdateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = await GetSingleByIdInternalAsync(dto.Id.Value);

            entity = await UpdateInternalAsync(dto, entity);

            await _context.SaveChangesAsync();
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

        public async Task DeleteAsync(Guid id)
        {
            await DeleteInternalAsync(id);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteInternalAsync(Guid id)
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
