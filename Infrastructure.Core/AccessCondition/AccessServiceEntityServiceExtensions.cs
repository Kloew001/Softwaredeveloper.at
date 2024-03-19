namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete
{
    public static class AccessServiceEntityServiceExtensions
    {
        public static async Task<bool> CanCreateAsync<TEntity>(this EntityService<TEntity> service)
            where TEntity : Entity, ISoftDelete
        {
            try
            {
                using (var tran = await service.EntityServiceDependency.DbContext.Database.BeginTransactionAsync())
                {
                    await service.QuickCreateInternalAsync();
                    await tran.RollbackAsync();
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static async Task<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await service.EntityServiceDependency.AccessService.CanUpdateAsync(entity);
        }

        public static async Task<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await service.EntityServiceDependency.AccessService.CanDeleteAsync(entity);
        }
    }
}
