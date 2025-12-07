using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class TaskExtension
{
    public static Task CreateChildScope(
        this IServiceScope parentServiceScope,
        Func<IServiceProvider, CancellationToken, Task> innerAction, CancellationToken cancellationToken = default)
    {
        return CreateChildScope(parentServiceScope.ServiceProvider, innerAction, cancellationToken);
    }

    public static async Task CreateChildScope(
        this IServiceProvider parentServiceProvider,
        Func<IServiceProvider, CancellationToken, Task> innerAction, CancellationToken cancellationToken = default)
    {
        using (var childScope = parentServiceProvider.CreateChildScope())
        {
            await innerAction(childScope.ServiceProvider, cancellationToken);
        }
    }
    public static async Task CreateChildScope<T1>(
        this IServiceProvider parentServiceProvider,
        Func<IServiceProvider, T1, CancellationToken, Task> innerAction, CancellationToken cancellationToken = default)
    {
        using (var childScope = parentServiceProvider.CreateChildScope())
        {
            var t1 = childScope.ServiceProvider.GetService<T1>();

            await innerAction(childScope.ServiceProvider, t1, cancellationToken);
        }
    }
    public static async Task CreateChildScope<T1, T2>(
        this IServiceProvider parentServiceProvider,
        Func<IServiceProvider, T1, T2, CancellationToken, Task> innerAction, CancellationToken cancellationToken = default)
    {
        using (var childScope = parentServiceProvider.CreateChildScope())
        {
            var t1 = childScope.ServiceProvider.GetService<T1>();
            var t2 = childScope.ServiceProvider.GetService<T2>();

            await innerAction(childScope.ServiceProvider, t1, t2, cancellationToken);
        }
    }
    public static async Task CreateChildScope<T1, T2, T3>(
        this IServiceProvider parentServiceProvider,
        Func<IServiceProvider, T1, T2, T3, CancellationToken, Task> innerAction, CancellationToken cancellationToken = default)
    {
        using (var childScope = parentServiceProvider.CreateChildScope())
        {
            var t1 = childScope.ServiceProvider.GetService<T1>();
            var t2 = childScope.ServiceProvider.GetService<T2>();
            var t3 = childScope.ServiceProvider.GetService<T3>();

            await innerAction(childScope.ServiceProvider, t1, t2, t3, cancellationToken);
        }
    }

    public static IServiceScope CreateChildScope(
        this IServiceScope parentServiceScope,
        bool restoreParentSettings = true)
    {
        return CreateChildScope(parentServiceScope.ServiceProvider, restoreParentSettings);
    }

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