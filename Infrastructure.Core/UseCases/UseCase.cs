using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

[AttributeUsage(AttributeTargets.Class)]
public class UseCaseAttribute : Attribute
{
    public string UniqueIdentifier { get; set; }
}

[ScopedDependency<IUseCase>]
public interface IUseCase
{
    string UseCaseIdentifier { get; }

    ValueTask<bool> IsAvailableAsync();

    ValueTask<bool> IsAvailableAsync(object paramter);

    ValueTask<bool> CanExecuteAsync();

    ValueTask<bool> CanExecuteAsync(object paramter);

    Task<object> ExecuteAsync(object paramter, CancellationToken cancellationToken = default);
}

public interface IUseCase<TParamter, TResult> : IUseCase
    where TParamter : new()
{
    ValueTask<bool> IsAvailableAsync(TParamter paramter);

    ValueTask<bool> CanExecuteAsync(TParamter paramter);
    Task<TResult> ExecuteAsync(TParamter paramter, CancellationToken cancellationToken = default);
}

public abstract class UseCase<TParamter, TResult> :
    IUseCase<TParamter, TResult>
    where TParamter : new()
{
    public string UseCaseIdentifier =>
        GetType().GetCustomAttribute<UseCaseAttribute>()?.UniqueIdentifier ??
        GetType().Name;

    public virtual ValueTask<bool> IsAvailableAsync() => ValueTask.FromResult(true);

    public virtual ValueTask<bool> IsAvailableAsync(TParamter paramter) => ValueTask.FromResult(true);

    public virtual ValueTask<bool> CanExecuteAsync() => ValueTask.FromResult(true);

    public virtual ValueTask<bool> CanExecuteAsync(TParamter paramter) => ValueTask.FromResult(true);

    ValueTask<bool> IUseCase.IsAvailableAsync(object paramter) => IsAvailableAsync(CreateParamter(paramter));
    ValueTask<bool> IUseCase.CanExecuteAsync(object paramter) => CanExecuteAsync(CreateParamter(paramter));

    async Task<object> IUseCase.ExecuteAsync(object paramter, CancellationToken cancellationToken)
    {
        TResult result = await ExecuteAsync(CreateParamter(paramter), cancellationToken);
        return result;
    }

    private TParamter CreateParamter(object paramter)
    {
        if (paramter == null)
            return default;

        if (paramter is TParamter paramterParamter)
            return paramterParamter;

        var paramterJson = JsonConvert.SerializeObject(paramter);
        var obj = JsonConvert.DeserializeObject<TParamter>(paramterJson);

#if (DEBUG)

        TParamter newParamter = new TParamter();
        paramter.CopyPropertiesTo(newParamter);

        var newParamterJson = JsonConvert.SerializeObject(newParamter);
        var objJson = JsonConvert.SerializeObject(newParamter);
        if (newParamterJson != objJson)
        {
            Debugger.Break();
        }
#endif

        return obj;
    }

    public async Task<TResult> ExecuteAsync(TParamter paramter, CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync())
            throw new InvalidOperationException();

        if (!await CanExecuteAsync())
            throw new InvalidOperationException();

        if (!await IsAvailableAsync(paramter))
            throw new InvalidOperationException();

        if (!await CanExecuteAsync(paramter))
            throw new InvalidOperationException();

        return await OnExecute(paramter, cancellationToken);
    }

    protected abstract Task<TResult> OnExecute(TParamter paramter, CancellationToken cancellationToken);

}
