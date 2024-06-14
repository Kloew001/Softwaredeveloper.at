using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections
{
    public abstract class IReferencedToEntityAccessCondition<TReferencedToEntityType> : IAccessCondition<TReferencedToEntityType>
        where TReferencedToEntityType : IReferencedToEntity
    {
        private readonly AccessService _accessService;
        private readonly IDbContext _dbContext;

        public IReferencedToEntityAccessCondition(AccessService accessService, IDbContext dbContext)
        {
            _accessService = accessService;
            _dbContext = dbContext;
        }

        public virtual async ValueTask<bool> CanCreateAsync(TReferencedToEntityType entity)
        {
            var referencedEntity = await entity.GetReferencedEntityAsync(_dbContext);
            
            var result = await _accessService.EvaluateAsync(referencedEntity, (ac, se) => 
                ac.CanCreateAsync(se));
        
            return result;
        }

        public virtual async ValueTask<bool> CanDeleteAsync(TReferencedToEntityType entity)
        {
            var referencedEntity = await entity.GetReferencedEntityAsync(_dbContext);

            var result = await _accessService.EvaluateAsync(referencedEntity, (ac, se) =>
                ac.CanDeleteAsync(se));

            return result;
        }

        public virtual async ValueTask<bool> CanReadAsync(TReferencedToEntityType entity)
        {
            var referencedEntity = await entity.GetReferencedEntityAsync(_dbContext);

            var result = await _accessService.EvaluateAsync(referencedEntity, (ac, se) =>
                ac.CanReadAsync(se));

            return result;
        }

        public ValueTask<IQueryable<TReferencedToEntityType>> CanReadQueryAsync(IQueryable<TReferencedToEntityType> query)
        {
            return ValueTask.FromResult(query);
        }

        public virtual async ValueTask<bool> CanSaveAsync(TReferencedToEntityType entity)
        {
            var referencedEntity = await entity.GetReferencedEntityAsync(_dbContext);

            var result = await _accessService.EvaluateAsync(referencedEntity, (ac, se) =>
                ac.CanSaveAsync(se));

            return result;
        }

        public virtual async ValueTask<bool> CanUpdateAsync(TReferencedToEntityType entity)
        {
            var referencedEntity = await entity.GetReferencedEntityAsync(_dbContext);

            var result = await _accessService.EvaluateAsync(referencedEntity, (ac, se) =>
                ac.CanUpdateAsync(se));

            return result;
        }
    }
}
