namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

public abstract class SaveUseCase<TEntity, TDto> : UseCase<TDto, Guid>
     where TEntity : Entity
    where TDto : Dto, new()
{
    private readonly EntityService<TEntity> _service;

    public SaveUseCase(EntityService<TEntity> service)
    {
        _service = service;
    }

    public override async ValueTask<bool> IsAvailableAsync(TDto dto)
    {
        if (await base.IsAvailableAsync(dto) == false)
            return false;

        if (ShouldCreate(dto))
        {
            return await _service.CanCreateAsync(dto);
        }
        else
        {
            return await _service.CanUpdateAsync(dto.Id.Value);
        }
    }

    protected override async Task<Guid> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        if (ShouldCreate(dto))
        {
            return await _service.CreateAsync(dto);
        }
        else
        {
            await _service.UpdateAsync(dto);
            return dto.Id.Value;
        }
    }

    protected virtual bool ShouldCreate(TDto dto)
    {
        return dto?.Id == null;
    }
}