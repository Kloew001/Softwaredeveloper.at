namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

public class SaveUseCase<TEntity, TDto> : UseCase<TEntity, TDto, TDto>
     where TEntity : Entity
    where TDto : Dto, new()
{
    public SaveUseCase(EntityService<TEntity> service)
        : base(service)
    {
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

    protected override async Task<TDto> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        if (ShouldCreate(dto))
        {
            return await _service.CreateAsync(dto);
        }
        else
        {
            return await _service.UpdateAsync(dto);
        }
    }

    protected virtual bool ShouldCreate(TDto dto)
    {
        return dto?.Id == null;
    }
}
