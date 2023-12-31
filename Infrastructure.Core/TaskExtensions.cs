using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public static class TaskFactoryExtensions
    {

        public static Task StartNewWithScope(this TaskFactory taskFactory, IServiceProvider serviceProvider, Func<IServiceScope, Task> action)
        {
            return taskFactory.StartNew(async () =>
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    await action(serviceScope);
                }
            });
        }

        public static Task StartNewWithCurrentUser(this TaskFactory taskFactory, IServiceProvider serviceProvider, Func<IServiceScope, Task> action)
        {
            var currentUserService = serviceProvider.GetService<ICurrentUserService>();
            var currentUserId = currentUserService.GetCurrentUserId();

            return StartNewWithScope(taskFactory, serviceProvider, async (serviceScope) => {
                var currentUserService = serviceScope.ServiceProvider.GetService<ICurrentUserService>();
                currentUserService.SetCurrentUserId(currentUserId);
                await action(serviceScope);
            });
        }
    }
}
