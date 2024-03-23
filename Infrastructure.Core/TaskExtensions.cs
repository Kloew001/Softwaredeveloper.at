using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public static class TaskExtension
    {
        public static Task StartNewWithScope(this IServiceProvider serviceProvider, Func<IServiceScope, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () => 
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    await action(serviceScope, cancellationToken);
                }
            });
        }

        public static Task StartNewWithCurrentUser(this IServiceProvider serviceProvider,  Func<IServiceScope, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            var currentUserService = serviceProvider.GetService<ICurrentUserService>();
            var currentUserId = currentUserService.GetCurrentUserId();

            return StartNewWithScope(serviceProvider, async (serviceScope, ct) => 
            {
                var currentUserService = serviceScope.ServiceProvider.GetService<ICurrentUserService>();
                currentUserService.SetCurrentUserId(currentUserId);
            
                await action(serviceScope, ct);
            }, cancellationToken);
        }
    }
}
