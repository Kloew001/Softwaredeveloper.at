using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

[SingletonDependency]
public class UseCaseServiceResolver(IServiceScopeFactory serviceScopeFactory) : IAppStatupInit
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public Dictionary<string, Type> UseCases { get; set; } = [];

    public Task Init()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var useCaseTypes = AssemblyUtils.GetDerivedConcretClasses<IUseCase>();

            foreach (var useCaseType in useCaseTypes)
            {
                var useCaseAttribute =
                    useCaseType.GetCustomAttribute<UseCaseAttribute>();

                if (useCaseAttribute != null)
                {
                    UseCases.Add(useCaseAttribute.UniqueIdentifier, useCaseType);
                }
                else
                {
                    UseCases.Add(useCaseType.Name, useCaseType);
                }
            }
        }

        return Task.CompletedTask;
    }
}