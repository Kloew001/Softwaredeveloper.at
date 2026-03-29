namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;

public static class ISoftDeleteEntityServiceExtensions
{
    public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
        where TEntity : Entity, ISoftDelete
    {
        var entity = await service.GetSingleByIdAsync(id);

        await service.SoftDeleteAsync(entity);

        await service.SaveAsync(entity);
    }

    public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
        where TEntity : Entity, ISoftDelete
    {
        if (await service.CanDeleteAsync(entity) == false)
            throw new UnauthorizedAccessException();

        entity.IsDeleted = true;
    }

    public static async Task HardDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
        where TEntity : Entity, ISoftDelete
    {
        await service.DeleteAsync(id);
    }

    public static async Task HardDeleteAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
        where TEntity : Entity, ISoftDelete
    {
        await service.DeleteAsync(entity);
    }
}