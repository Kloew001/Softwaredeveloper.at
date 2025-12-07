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

    public async ValueTask<IEnumerable<UseCaseInfo>> EvaluateAsync(IEnumerable<string> useCaseIdentifiers, Dictionary<string, object> parameter)
    {
        var useCaseInfos = new List<UseCaseInfo>();

        foreach (var useCaseIdentifier in useCaseIdentifiers)
        {
            var useCaseType = _useCaseServiceResolver.UseCases[useCaseIdentifier];

            var useCase = _serviceProvider.GetService(useCaseType) as IUseCase;

            var useCaseInfo = await EvaluateAsync(useCase, parameter);
            useCaseInfos.Add(useCaseInfo);
        }

        return useCaseInfos;
    }

    public async Task<object> ExecuteAsync(string useCaseIdentifier, Dictionary<string, object> parameter)
    {
        var useCaseType = _useCaseServiceResolver.UseCases[useCaseIdentifier];
        var useCase = _serviceProvider.GetService(useCaseType) as IUseCase;

        return await useCase.ExecuteAsync(parameter);
    }

    public async Task<object> ExecuteAsync<TUseCase>(Dictionary<string, object> paramter)
        where TUseCase : IUseCase
    {
        var useCase = _serviceProvider.GetService<TUseCase>();

        return await useCase.ExecuteAsync(paramter);
    }

    public async ValueTask<UseCaseInfo> EvaluateAsync<TUseCase>(Dictionary<string, object> paramter)
        where TUseCase : IUseCase
    {
        var useCase = _serviceProvider.GetService<TUseCase>();

        return await EvaluateAsync(useCase, paramter);
    }

    private async ValueTask<UseCaseInfo> EvaluateAsync(IUseCase useCase, Dictionary<string, object> paramter)
    {
        var useCaseInfo = new UseCaseInfo()
        {
            UseCaseIdentifier = useCase.UseCaseIdentifier
        };

        useCaseInfo.IsAvailable =
                    await useCase.IsAvailableAsync() &&
                    await useCase.IsAvailableAsync(paramter);

        useCaseInfo.CanExecute =
                 useCaseInfo.IsAvailable &&
                 await useCase.CanExecuteAsync() &&
                 await useCase.CanExecuteAsync(paramter);

        return useCaseInfo;
    }
}