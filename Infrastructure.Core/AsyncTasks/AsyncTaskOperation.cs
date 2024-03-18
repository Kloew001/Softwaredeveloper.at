using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks
{
    public class AsyncTaskOperation : Entity
    {
        public AsyncTaskOperation()
        {

        }

        public Guid OperationHandlerId { get; set; }
        public string OperationKey { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ParameterSerialized { get; set; }

        public Guid? ExecuteById { get; set; }
        public virtual ApplicationUser ExecuteBy { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExecuteAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public AsyncTaskOperationStatus Status { get; set; }
        public int SortIndex { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
    }

    public class AsyncTaskOperationConfiguration : IEntityTypeConfiguration<AsyncTaskOperation>
    {
        public void Configure(EntityTypeBuilder<AsyncTaskOperation> builder)
        {
            builder
                .HasIndex(c => new
                {
                    c.ReferenceId,
                    c.Status
                });

            builder
                .HasIndex(c => new
                {
                    c.ExecuteAt,
                    c.Status
                });

            builder
                .HasIndex(c => new
                {
                    c.OperationKey
                });
        }
    }
}
