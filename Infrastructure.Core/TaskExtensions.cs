using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
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
    }
}
