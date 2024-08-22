using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases
{
    public class UseCaseServiceResolver : ISingletonDependency, IAppStatupInit
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Dictionary<Guid, Type> UseCases { get; set; } = new Dictionary<Guid, Type>();

        public UseCaseServiceResolver(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task Init()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var allUseCases = scope.ServiceProvider.GetServices<IUseCase>();

                foreach (var useCase in allUseCases)
                {
                    UseCases.Add(useCase.UseCaseId, useCase.GetType());
                }
            }

            return Task.CompletedTask;
        }
    }
}
