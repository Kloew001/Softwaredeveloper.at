using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    [Table(nameof(EmailMessage))]
    public class EmailMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; }

        public EmailMessageStatusType Status { get; set; } = EmailMessageStatusType.Created;

        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }

        public string AnAdress { get; set; }
        public string CcAdress { get; set; }
        public string BccAdress { get; set; }

        public string Subject { get; set; }
        public string HtmlContent { get; set; }

        public string ErrorMessage { get; set; }

        public string Attachment1Name { get; set; }
        public byte[] Attachment1 { get; set; }

        public string Attachment2Name { get; set; }
        public byte[] Attachment2 { get; set; }
    }

    public enum EmailMessageStatusType
    {
        Created = 0,
        Sent = 1,
        Error = 2,
    }
}
