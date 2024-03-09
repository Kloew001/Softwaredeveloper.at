using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public static class TaskExtension
    {
        public static Task StartNewWithScope(IServiceProvider serviceProvider, CancellationToken cancellationToken, Func<IServiceScope, CancellationToken, Task> action)
        {
            return Task.Run(async () => 
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    await action(serviceScope, cancellationToken);
                }
            });
        }

        public static Task StartNewWithCurrentUser(IServiceProvider serviceProvider, CancellationToken cancellationToken, Func<IServiceScope, CancellationToken, Task> action)
        {
            var currentUserService = serviceProvider.GetService<ICurrentUserService>();
            var currentUserId = currentUserService.GetCurrentUserId();

            return StartNewWithScope(serviceProvider, cancellationToken, async (serviceScope, ct) => {
                var currentUserService = serviceScope.ServiceProvider.GetService<ICurrentUserService>();
                currentUserService.SetCurrentUserId(currentUserId);
                await action(serviceScope, ct);
            });
        }
    }
}
