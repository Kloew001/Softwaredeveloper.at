using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class EntityService<TEntity> : IScopedService
        where TEntity : BaseEntity, new()
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

        protected Task<TEntity> GetSingleByIdInternalAsync(Guid id)
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

        protected async Task<TEntity> GetSingleInternalAsync(Expression<Func<TEntity, bool>> whereClause = null)
        {
            var entity = await _context
                .Set<TEntity>()
                .Where(whereClause)
                .SingleOrDefaultAsync();

            if (_accessService.CanRead(entity) == false)
                throw new UnauthorizedAccessException();

            return entity;
        }

        public async Task<IEnumerable<TDto>> GetCollectionAsync<TDto>(Expression<Func<TEntity, bool>> whereClause = null)
            where TDto : DtoBase, new()
        {
            var dtos = (await GetCollectionInternal(whereClause).ToListAsync())
                .ConvertToDtos<TDto>();

            return dtos;
        }

        protected IQueryable<TEntity> GetCollectionInternal(Expression<Func<TEntity, bool>> whereClause = null)
        {
            var query = _context
                .Set<TEntity>()
                //.Where(_=> _accessService.CanReadQuery(_))
                .AsQueryable<TEntity>();
            
            if(whereClause != null)
                query = query.Where(whereClause);
            
            return query;
        }

        public async Task<Guid> CreateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = new TEntity();
            await _context.AddAsync(entity);

            dto.ConvertToEntity(entity);

            await CreateInternalAsync(dto, entity);

            if (_accessService.CanCreate(entity) == false)
                throw new UnauthorizedAccessException();

            await _context.SaveChangesAsync();

            return entity.Id;
        }

        protected virtual Task CreateInternalAsync<TDto>(TDto dto,TEntity entity)
            where TDto : DtoBase, new()
        {
            return Task.CompletedTask;
        }

        public async Task UpdateAsync<TDto>(TDto dto)
            where TDto : DtoBase, new()
        {
            var entity = await GetSingleByIdInternalAsync(dto.Id.Value);

            dto.ConvertToEntity(entity);

            await UpdateInternalAsync(dto, entity);

            if (_accessService.CanUpdate(entity) == false)
                throw new UnauthorizedAccessException();

            await _context.SaveChangesAsync();
        }

        protected virtual Task UpdateInternalAsync<TDto>(TDto dto, TEntity entity)
            where TDto : DtoBase, new()
        {
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetSingleByIdInternalAsync(id);

            _context.Remove(entity);

            if (_accessService.CanDelete(entity) == false)
                throw new UnauthorizedAccessException();

            await _context.SaveChangesAsync();
        }
    }
}
