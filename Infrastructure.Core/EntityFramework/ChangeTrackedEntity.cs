﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public abstract class ChangeTrackedEntity : BaseEntity
    {
        [Column(nameof(CreatedById))]
        public Guid CreatedById { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        [Column(nameof(ModifiedById))]
        public Guid ModifiedById { get; set; }

        [ForeignKey(nameof(ModifiedById))]
        public virtual ApplicationUser ModifiedBy { get; set; }
    }

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
