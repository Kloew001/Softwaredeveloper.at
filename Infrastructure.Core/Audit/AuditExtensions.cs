using AuditCoreEntityFramework = Audit.EntityFramework;

using Microsoft.Extensions.Hosting;

using AuditCore = Audit.Core;
using Audit.Core;
using Audit.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit
{
    public static class AuditExtensions
    {
        public const string auditPostfix = "Audit";
        public static void UseAudit(this IHost host)
        {
            AuditCoreEntityFramework.Configuration.Setup()
                .ForAnyContext()
                .UseOptIn();

            AuditCore.Configuration.Setup()
                .UseEntityFramework(_ => _
                    .AuditTypeNameMapper(typeName => typeName + auditPostfix)
                    .AuditEntityAction<IAudit>((auditEvent, entry, auditEntity) =>
                        {
                            auditEntity.Id = Guid.NewGuid();

                            if (entry.ColumnValues.TryGetValue("Id", out object id) &&
                                Guid.TryParse(id.ToString(), out Guid idGuid))
                            {
                                auditEntity.AuditId = idGuid;
                            }

                            auditEntity.AuditDate = DateTime.Now;
                            auditEntity.AuditAction = entry.Action;

                            var entityFrameworkEvent = auditEvent?.GetEntityFrameworkEvent();
                            auditEntity.TransactionId = entityFrameworkEvent?.TransactionId;

                            auditEntity.CallingMethod = auditEvent.Environment?.CallingMethodName;
                            auditEntity.MachineName = auditEvent.Environment?.MachineName;
                        }));
        }
    }
}
