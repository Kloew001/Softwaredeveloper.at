using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public static class TaskExtension
    {
        public static Task StartNewWithScope(IServiceProvider serviceProvider, Func<IServiceScope, Task> action)
        {
            return Task.Run(async () => 
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    await action(serviceScope);
                }
            });
        }

        public static Task StartNewWithCurrentUser(IServiceProvider serviceProvider, Func<IServiceScope, Task> action)
        {
            var currentUserService = serviceProvider.GetService<ICurrentUserService>();
            var currentUserId = currentUserService.GetCurrentUserId();

            return StartNewWithScope(serviceProvider, async (serviceScope) => {
                var currentUserService = serviceScope.ServiceProvider.GetService<ICurrentUserService>();
                currentUserService.SetCurrentUserId(currentUserId);
                await action(serviceScope);
            });
        }
    }
}
