using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases
{
    public class UseCaseService : IScopedDependency
    {
        public class UseCaseInfo
        {
            public Guid UseCaseId { get; set; }
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

        public async Task<IEnumerable<UseCaseInfo>> EvaluateAsync(IEnumerable<Guid> useCaseIds, Dictionary<string, object> parameter)
        {
            var useCaseInfos = new List<UseCaseInfo>();

            foreach (var useCaseId in useCaseIds)
            {
                var useCaseType = _useCaseServiceResolver.UseCases[useCaseId];

                var useCase = _serviceProvider.GetService(useCaseType) as IUseCase;

                var useCaseInfo = await EvaluateAsync(useCase, parameter);
                useCaseInfos.Add(useCaseInfo);
            }

            return useCaseInfos;
        }

        public async Task ExecuteAsync(Guid useCaseId, Dictionary<string, object> parameter)
        {
            var useCaseType = _useCaseServiceResolver.UseCases[useCaseId];
            var useCase = _serviceProvider.GetService(useCaseType) as IUseCase;

            await useCase.ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync<TUseCase>(Dictionary<string, object> paramter)
            where TUseCase : IUseCase
        {
            var useCase = _serviceProvider.GetService<TUseCase>();

            await useCase.ExecuteAsync(paramter);
        }

        public async Task<UseCaseInfo> EvaluateAsync<TUseCase>(Dictionary<string, object> paramter)
            where TUseCase : IUseCase
        {
            var useCase = _serviceProvider.GetService<TUseCase>();

            return await EvaluateAsync(useCase, paramter);
        }

        private async Task<UseCaseInfo> EvaluateAsync(IUseCase useCase, Dictionary<string, object> paramter)
        {
            return new UseCaseInfo()
            {
                UseCaseId = useCase.UseCaseId,
                IsAvailable =
                        await useCase.IsAvailableAsync() &&
                        await useCase.IsAvailableAsync(paramter),

                CanExecute =
                        await useCase.CanExecuteAsync() &&
                        await useCase.CanExecuteAsync(paramter)
            };
        }
    }
}
