namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

public class CreateUseCase<TEntity, TDto> : UseCase<TDto, TDto>
    where TEntity : Entity
    where TDto : Dto, new()
{
    private readonly EntityService<TEntity> _service;

    public CreateUseCase(EntityService<TEntity> service)
    {
        _service = service;
    }

    public override async ValueTask<bool> IsAvailableAsync(TDto dto)
    {
        if (await base.IsAvailableAsync(dto) == false)
            return false;

        return await _service.CanCreateAsync(dto);
    }

    protected override async Task<TDto> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(dto);
    }
}
