using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

public interface IAuditableEntity : IEntity
{
    IEnumerable<IEntityAudit> Audits { get; }
}

public interface IAuditableEntity<TEntityAudit> : IAuditableEntity
    where TEntityAudit : IEntityAudit
{
    new ICollection<TEntityAudit> Audits { get; set; }

    IEnumerable<IEntityAudit> IAuditableEntity.Audits => Audits.OfType<IEntityAudit>();
}

public interface IEntityAudit: ISupportValidDateRange
{
    public Guid Id { get; set; }

    public Guid AuditId { get; set; }
    IEntity Audit { get; set; }

    string TransactionId { get; set; }
    public Guid ModifiedById { get; set; }
    AuditActionType AuditAction { get; set; }
    string CallingMethod { get; set; }
    string MachineName { get; set; }
}

public enum AuditActionType
{
    [EnumExtension(DisplayName = "Created")]
    Created,

    [EnumExtension(DisplayName = "Modified")]
    Modified,

    [EnumExtension(DisplayName = "Deleted")]
    Deleted, 
}

public interface IEntityAudit<TEntity> : IEntityAudit
    where TEntity : IEntity
{
    new TEntity Audit { get; set; }

    IEntity IEntityAudit.Audit
    {
        get => Audit;
        set => Audit = (TEntity)value;
    }
}

[Index(nameof(AuditId))]
[Index(nameof(TransactionId))]
public abstract class EntityAudit<TEntity> : ChangeTrackedEntity, IEntityAudit<TEntity>
     where TEntity : Entity
{
    public Guid AuditId { get; set; }
    public virtual TEntity Audit { get; set; }

    public string TransactionId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public AuditActionType AuditAction { get; set; }
    public string CallingMethod { get; set; }
    public string MachineName { get; set; }
}
