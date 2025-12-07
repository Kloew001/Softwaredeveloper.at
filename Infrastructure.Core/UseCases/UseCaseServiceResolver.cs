using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

[SingletonDependency]
public class UseCaseServiceResolver : IAppStatupInit
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Dictionary<string, Type> UseCases { get; set; } = new Dictionary<string, Type>();

    public UseCaseServiceResolver(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Init()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var useCaseTypes = AssemblyUtils.AllLoadedTypes()
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(p => typeof(IUseCase).IsAssignableFrom(p))
               .ToList();

            foreach (var useCaseType in useCaseTypes)
            {
                var useCaseAttribute =
                    useCaseType.GetCustomAttribute<UseCaseAttribute>();

                if (useCaseAttribute != null)
                {
                    var uniqueIdentifier = useCaseAttribute.UniqueIdentifier;
                    UseCases.Add(uniqueIdentifier, useCaseType);
                }
            }
        }

        return Task.CompletedTask;
    }
}