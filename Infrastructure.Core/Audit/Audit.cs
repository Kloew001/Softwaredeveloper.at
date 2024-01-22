using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}
