using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UseCaseAttribute : Attribute
    {
        public string Id { get; set; }

    }

    public interface IUseCase
    {
        Guid UseCaseId { get; }

        ValueTask<bool> IsAvailableAsync();

        ValueTask<bool> IsAvailableAsync(object paramter);

        ValueTask<bool> CanExecuteAsync();

        ValueTask<bool> CanExecuteAsync(object paramter);

        Task<object> ExecuteAsync(object paramter);
    }

    public interface IUseCase<TParamter, TResult> : IUseCase
        where TParamter : new()
    {
        ValueTask<bool> IsAvailableAsync(TParamter paramter);

        ValueTask<bool> CanExecuteAsync(TParamter paramter);
        Task<TResult> ExecuteAsync(TParamter paramter);
    }

    public abstract class UseCase<TEntity, TParamter, TResult> :
        IUseCase<TParamter, TResult>,
        IScopedDependency,
        ITypedScopedDependency<IUseCase>
        where TEntity : Entity
        where TParamter : new()
    {
        public Guid UseCaseId => Guid.Parse(GetType().GetCustomAttribute<UseCaseAttribute>().Id);

        protected readonly EntityService<TEntity> _service;

        public UseCase(EntityService<TEntity> service)
        {
            _service= service;
        }

        public virtual ValueTask<bool> IsAvailableAsync() => ValueTask.FromResult(true);

        public virtual ValueTask<bool> IsAvailableAsync(TParamter paramter) => ValueTask.FromResult(true);

        public virtual ValueTask<bool> CanExecuteAsync() => ValueTask.FromResult(true);

        public virtual ValueTask<bool> CanExecuteAsync(TParamter paramter) => ValueTask.FromResult(true);

        ValueTask<bool> IUseCase.IsAvailableAsync(object paramter) => IsAvailableAsync(CreateParamter(paramter));
        ValueTask<bool> IUseCase.CanExecuteAsync(object paramter) => CanExecuteAsync(CreateParamter(paramter));

        async Task<object> IUseCase.ExecuteAsync(object paramter)
        {
            var result = await ExecuteAsync(CreateParamter(paramter));
            return result;
        }

        private TParamter CreateParamter(object paramter)
        {
            if (paramter == null)
                return default;

            if (paramter is TParamter paramterParamter)
                return paramterParamter;

            var newParamter = new TParamter();

            paramter.CopyPropertiesTo(newParamter);

            return newParamter;
        }

        public async Task<TResult> ExecuteAsync(TParamter paramter)
        {
            if (!await IsAvailableAsync())
                throw new InvalidOperationException();

            if (!await CanExecuteAsync())
                throw new InvalidOperationException();

            if (!await IsAvailableAsync(paramter))
                throw new InvalidOperationException();

            if (!await CanExecuteAsync(paramter))
                throw new InvalidOperationException();

            return await ExecuteInternal(paramter);
        }

        protected abstract Task<TResult> ExecuteInternal(TParamter paramter);

    }
}
