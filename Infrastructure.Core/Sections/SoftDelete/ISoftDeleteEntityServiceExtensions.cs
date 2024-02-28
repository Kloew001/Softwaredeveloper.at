using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete
{
    public static class ISoftDeleteEntityServiceExtensions
    {
        public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            entity.IsDeleted = true;

            await service.SaveChangesAsync(entity);
        }
    }
}
