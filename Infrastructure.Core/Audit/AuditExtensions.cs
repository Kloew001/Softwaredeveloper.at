using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

public static class AuditExtensions
{
    public static void CreateEntityAudit(this IAuditableEntity auditableEntity, IDbContext context, AuditActionType auditAction, DateTime now, Guid transactionId)
    {
        var entityAuditType = auditableEntity.GetEntityAuditType();

        var entityType = context.Model.FindEntityType(entityAuditType);

        if (entityType == null)
            return;

        var entityAudit = context.As<IDbContext>()
                .GetType()
                .GetMethod(nameof(IDbContext.CreateEntity))
                .MakeGenericMethod(entityAuditType)
                .Invoke(context, null)
                .As<IEntityAudit>();

        auditableEntity.CopyPropertiesTo(entityAudit);

        entityAudit.Id = Guid.NewGuid();

        entityAudit.AuditId = auditableEntity.Id;
        entityAudit.Audit = auditableEntity;

        entityAudit.ValidFrom = now;

        var lastAudit = context.GetDbSetByType<IEntityAudit>(entityAuditType)
            .Where(_ => _.AuditId == auditableEntity.Id)
            .OrderBy(_ => _.ValidFrom)
            .LastOrDefault();

        if (lastAudit != null)
            lastAudit.ValidTo = now;

        entityAudit.AuditAction = auditAction;

        //var entityFrameworkEvent = auditEvent?.GetEntityFrameworkEvent();
        entityAudit.TransactionId = transactionId.ToString();

        //entityAudit.CallingMethod = auditEvent.Environment?.CallingMethodName;
        //entityAudit.MachineName = auditEvent.Environment?.MachineName;
    }

    public static Type GetEntityAuditType(this IAuditableEntity entity)
    {
        var interfac = entity
            .GetType()
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IAuditableEntity<>));

        var entityAuditType = interfac.GenericTypeArguments.Single();

        return entityAuditType;
    }

    public static AuditActionType GetAuditActionType(this EntityEntry entityEntry)
    {
        switch (entityEntry.State)
        {
            case EntityState.Added:
                return AuditActionType.Created;
            case EntityState.Modified:
                return AuditActionType.Modified;
            case EntityState.Deleted:
                return AuditActionType.Deleted;
            default:
                throw new InvalidOperationException($"State {entityEntry.State} not supported");
        }
    }
}