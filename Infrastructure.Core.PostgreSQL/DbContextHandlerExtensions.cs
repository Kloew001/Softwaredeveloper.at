using Audit.EntityFramework;
using Audit.EntityFramework.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class DbContextHandlerExtensions
    {
        //public class ChangeTrackedEntitySaveChangesInterceptor : SaveChangesInterceptor, IScopedService
        //{
        //    private readonly ICurrentUserService _currentUserService;
        //    public ChangeTrackedEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
        //    {
        //        _currentUserService = currentUserService;
        //    }

        //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        //        DbContextEventData eventData,
        //        InterceptionResult<int> result,
        //        CancellationToken cancellationToken = default)
        //    {
        //        if (eventData.Context is not null)
        //        {
        //            UpdateChangeTrackedEntity(eventData.Context);
        //        }

        //        return base.SavingChangesAsync(eventData, result, cancellationToken);
        //    }

        //}
    }
}