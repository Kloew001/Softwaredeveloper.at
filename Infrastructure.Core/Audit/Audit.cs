using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuditCoreEntityFramework = Audit.EntityFramework;

using Microsoft.Extensions.Hosting;

using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

using AuditCore = Audit.Core;
using Audit.Core;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit
{
    public interface IAuditAttribute
    {
        public Type AuditType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AuditAttribute<TAuditEntity> : Attribute, IAuditAttribute
        where TAuditEntity : IAudit
    {
        public Type AuditType { get; set; }

        public AuditAttribute()
        {
            AuditType = typeof(TAuditEntity);
        }
    }

    public interface IAudit
    {
        public Guid Id { get; set; }
        
        public Guid AuditId { get; set; }

        string TransactionId { get; set; }
        DateTime AuditDate { get; set; }
        public Guid ModifiedById { get; set; }
        string AuditAction { get; set; }
        string CallingMethod { get; set; }
        string MachineName { get; set; }
    }

    [Index(nameof(AuditId))]
    [Index(nameof(TransactionId))]
    public abstract class BaseAudit : IAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }
        public Guid AuditId { get; set; }
        public string TransactionId { get; set; }
        public DateTime AuditDate { get; set; }
        public Guid ModifiedById { get; set; }
        public string AuditAction { get; set; }
        public string CallingMethod { get; set; }
        public string MachineName { get; set; }
    }

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

            //.AuditTypeExplicitMapper(_ =>
            //{
            //    var typesForAudit =
            //    TypeUtility.GetAllTypesWithAttribute(typeof(AuditAttribute<>));

            //    foreach (var typeForAudit in typesForAudit)
            //    {
            //        var attribute = typeForAudit
            //        .GetAttribute(typeof(AuditAttribute<>)) as IAuditAttribute;

            //        var mapMethods = _.GetType().GetMethods()
            //        .Where(_ => _.Name == "Map" &&
            //                    _.IsGenericMethod &&
            //                    _.GetParameters().Count() == 1);

            //        var mapMethod =
            //        mapMethods.First()
            //        .MakeGenericMethod(typeForAudit, attribute.AuditType);

            //        //var actionMap = typeof(Action<,>).MakeGenericType(typeForAudit, attribute.AuditType);

            //        mapMethod.Invoke(_, new object[] { });
            //    }

            //    _.AuditEntityAction<IAudit>((auditEvent, entry, auditEntity) =>
            //    {
            //        //auditEntity.AuditId = entity.Id;
            //        //auditEntity.ModifiedById = entity.ModifiedById;

            //        auditEntity.Id = Guid.NewGuid();
            //        auditEntity.AuditDate = DateTime.Now;
            //        auditEntity.AuditAction = entry.Action;

            //        var entityFrameworkEvent = auditEvent?.GetEntityFrameworkEvent();
            //        auditEntity.TransactionId = entityFrameworkEvent?.TransactionId;
            //    });
            //}));
        }
    }
}
