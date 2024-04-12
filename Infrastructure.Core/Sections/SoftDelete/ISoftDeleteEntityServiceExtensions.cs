using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete
{
    public static class ISoftDeleteEntityServiceExtensions
    {
        public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            var accessService = service.EntityServiceDependency.AccessService;
            var accessConditionInfo = accessService.ResolveAccessConditionInfo(entity);
            var accessCondition = accessConditionInfo.AccessCondition;

            if (!(await accessCondition.CanDeleteAsync(accessConditionInfo.SecurityEntity)))
                throw new UnauthorizedAccessException();

            entity.IsDeleted = true;

            await service.SaveChangesAsync(entity);
        }
    }
}
