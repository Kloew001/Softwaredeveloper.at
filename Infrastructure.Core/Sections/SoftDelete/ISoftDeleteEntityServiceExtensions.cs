namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete
{
    public static class ISoftDeleteEntityServiceExtensions
    {
        public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            var accessService = service.EntityServiceDependency.AccessService;

            if (await accessService.EvaluateAsync(entity, (accessCondition, securityEntity) =>
                        accessCondition.CanDeleteAsync(securityEntity)) == false)
                throw new UnauthorizedAccessException();

            entity.IsDeleted = true;

            await service.SaveChangesAsync(entity);
        }

        public static async Task HardDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            await service.DeleteAsync(id);
        }
    }
}
