using DocumentFormat.OpenXml.Office2010.Excel;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

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
                        var useCaseId = Guid.Parse(useCaseAttribute.Id);
                        UseCases.Add(useCaseId, useCaseType);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
