namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases
{
    public class DeleteUseCase<TEntity, TDto> : UseCase<TEntity, TDto, bool>
        where TEntity : Entity
       where TDto : Dto, new()
    {
        public DeleteUseCase(EntityService<TEntity> service)
            : base(service)
        {
        }

        public override async ValueTask<bool> IsAvailableAsync(TDto dto)
        {
            if (await base.IsAvailableAsync(dto) == false)
                return false;

            return await _service.CanDeleteAsync(dto.Id.Value);
        }

        protected override async Task<bool> OnExecute(TDto dto, CancellationToken cancellationToken)
        {
            var entity = await _service.GetSingleByIdAsync(dto.Id.Value);

            if (entity is ISoftDelete softDelete)
            {
                var method = typeof(ISoftDeleteEntityServiceExtensions)
                               .GetMethod(nameof(ISoftDeleteEntityServiceExtensions.SoftDeleteAsync))
                               ?.MakeGenericMethod(typeof(TEntity));

                var task = (Task)method.Invoke(null, new object[] { _service, dto.Id.Value });
                await task;
            }
            else
            {
                await _service.DeleteAsync(dto.Id.Value);
            }

            return true;
        }
    }
}
