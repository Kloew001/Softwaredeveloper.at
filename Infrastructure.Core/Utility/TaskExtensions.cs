using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class TaskExtension
    {
        public static IServiceScope CreateChildScope(
            this IServiceProvider parentServiceProvider,
            bool restoreParentSettings = true)
        {
            var scopeInner = parentServiceProvider.CreateScope();
            var serivceProviderInner = scopeInner.ServiceProvider;

            if (restoreParentSettings)
            {
                var currentUserId =
                    parentServiceProvider.GetService<ICurrentUserService>()
                    .GetCurrentUserId();

                var activeSectionTypes =
                    parentServiceProvider.GetService<SectionManager>()
                    .GetAllActiveSectionTypes();

                serivceProviderInner
                    .GetService<ICurrentUserService>()
                        .SetCurrentUserId(currentUserId);

                serivceProviderInner.GetService<SectionManager>()
                    .CreateSectionScopes(activeSectionTypes);
            }

            return scopeInner;
        }

        public static Task<T> AsTaskResult<T>(this T result)
        {
            return Task.FromResult(result);
        }

        public static ValueTask<T> AsValueTaskResult<T>(this T result)
        {
            return ValueTask.FromResult(result);
        }
    }
}
