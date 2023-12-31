using System.ComponentModel.DataAnnotations.Schema;

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
}
