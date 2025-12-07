namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;

public static class ISoftDeleteEntityServiceExtensions
{
    public static async Task SoftDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
        where TEntity : Entity, ISoftDelete
    {
        var entity = await service.GetSingleByIdAsync(id);

        if (await service.CanDeleteAsync(entity) == false)
            throw new UnauthorizedAccessException();

        entity.IsDeleted = true;

        await service.SaveAsync(entity);
    }

    public static async Task HardDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
        where TEntity : Entity, ISoftDelete
    {
        await service.DeleteAsync(id);
    }
}