namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

public abstract class CreateUseCase<TEntity, TDto> : UseCase<TDto, Guid>
    where TEntity : Entity
    where TDto : Dto, new()
{
    protected readonly EntityService<TEntity> _service;

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

    protected override async Task<Guid> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(dto);
    }
}

public abstract class QuickCreateUseCase<TEntity, TDto> : CreateUseCase<TEntity, TDto>
    where TEntity : Entity
    where TDto : Dto, new()
{
    public QuickCreateUseCase(EntityService<TEntity> service)
        : base(service)
    {
    }

    protected override async Task<Guid> OnExecute(TDto dto, CancellationToken cancellationToken)
    {
        return await _service.QuickCreateAsync(dto);
    }
}