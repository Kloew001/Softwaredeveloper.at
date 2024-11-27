namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

public class DeleteUseCase<TEntity, TDto> : UseCase<TDto, bool>
    where TEntity : Entity
   where TDto : Dto, new()
{
    private readonly EntityService<TEntity> _service;

    public DeleteUseCase(EntityService<TEntity> service)
    {
        _service = service;
    }

    public override async ValueTask<bool> IsAvailableAsync(TDto dto)
    {
        if (await base.IsAvailableAsync(dto) == false)
            return false;

        return await _service.CanDeleteAsync(dto.Id.Value);
    }

    protected override async Task<bool> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        TEntity entity = await _service.GetSingleByIdAsync(dto.Id.Value);

        if (entity is ISoftDelete softDelete)
        {
            System.Reflection.MethodInfo method = typeof(ISoftDeleteEntityServiceExtensions)
                           .GetMethod(nameof(ISoftDeleteEntityServiceExtensions.SoftDeleteAsync))
                           ?.MakeGenericMethod(typeof(TEntity));

            Task task = (Task)method.Invoke(null, new object[] { _service, dto.Id.Value });
            await task;
        }
        else
        {
            await _service.DeleteAsync(dto.Id.Value);
        }

        return true;
    }
}
