using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

[ScopedDependency]
public class UseCaseService
{
    public class UseCaseInfo
    {
        public string UseCaseIdentifier { get; set; }
        public bool IsAvailable { get; set; }
        public bool CanExecute { get; set; }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly UseCaseServiceResolver _useCaseServiceResolver;

    public UseCaseService(IServiceProvider serviceProvider, UseCaseServiceResolver useCaseServiceResolver)
    {
        _serviceProvider = serviceProvider;
        _useCaseServiceResolver = useCaseServiceResolver;
    }

    public async Task<object> ExecuteAsync(string useCaseIdentifier, Dictionary<string, object> parameter, CancellationToken cancellationToken = default)
    {
        var useCase = GetUseCase(useCaseIdentifier);

        return await useCase.ExecuteAsync(parameter, cancellationToken);
    }

    public async Task<TResult> ExecuteAsync<TUseCase, TResult, TParameter>(TParameter paramter, CancellationToken cancellationToken = default)
        where TUseCase : IUseCase<TParameter, TResult>
        where TParameter : new()
    {
        var useCase = _serviceProvider.GetService<TUseCase>();

        return await useCase.ExecuteAsync(paramter, cancellationToken);
    }

    public async Task<object> ExecuteAsync<TUseCase>(Dictionary<string, object> paramter, CancellationToken cancellationToken = default)
        where TUseCase : IUseCase
    {
        var useCase = _serviceProvider.GetService<TUseCase>();

        return await useCase.ExecuteAsync(paramter, cancellationToken);
    }

    public async ValueTask<IEnumerable<UseCaseInfo>> EvaluateAsync(IEnumerable<string> useCaseIdentifiers, Dictionary<string, object> parameter, CancellationToken cancellationToken = default)
    {
        var evaluationTasks = useCaseIdentifiers.Select(async useCaseIdentifier =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var childScope = _serviceProvider.CreateChildScope();

            var useCaseService = childScope.ServiceProvider.GetRequiredService<UseCaseService>();

            return await useCaseService.EvaluateAsync(useCaseIdentifier, parameter, cancellationToken);
        });

        var useCaseInfos = await Task.WhenAll(evaluationTasks);
        return useCaseInfos;
    }

    public async ValueTask<UseCaseInfo> EvaluateAsync<TUseCase>(Dictionary<string, object> paramter, CancellationToken cancellationToken = default)
        where TUseCase : IUseCase
    {
        var useCase = _serviceProvider.GetService<TUseCase>();

        return await EvaluateAsync(useCase, paramter, cancellationToken);
    }

    public async ValueTask<UseCaseInfo> EvaluateAsync(string useCaseIdentifier, Dictionary<string, object> paramter, CancellationToken cancellationToken = default)
    {
        var useCase = GetUseCase(useCaseIdentifier);

        return await EvaluateAsync(useCase, paramter, cancellationToken);
    }

    private IUseCase GetUseCase(string useCaseIdentifier)
    {
        if (!_useCaseServiceResolver.UseCases.TryGetValue(useCaseIdentifier, out var useCaseType))
            throw new InvalidOperationException($"Unknown use case identifier '{useCaseIdentifier}'.");

        var useCase = _serviceProvider.GetService(useCaseType) as IUseCase
                      ?? throw new InvalidOperationException($"Use case '{useCaseIdentifier}' not registered in DI.");
        return useCase;
    }

    public async ValueTask<UseCaseInfo> EvaluateAsync(IUseCase useCase, Dictionary<string, object> paramter, CancellationToken cancellationToken = default)
    {
        var useCaseInfo = new UseCaseInfo
        {
            UseCaseIdentifier = useCase.UseCaseIdentifier,
            IsAvailable =
                        await useCase.IsAvailableAsync() &&
                        await useCase.IsAvailableAsync(paramter)
        };

        useCaseInfo.CanExecute =
                 useCaseInfo.IsAvailable &&
                 await useCase.CanExecuteAsync() &&
                 await useCase.CanExecuteAsync(paramter);

        return useCaseInfo;
    }
}