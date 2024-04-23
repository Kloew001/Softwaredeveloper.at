using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public class EMailIgnoreSection : Section
    {
    }

    [Table(nameof(EmailMessage))]
    public class EmailMessage : IReferencedToEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime SendAt { get; set; } = DateTime.Now;
        public DateTime? SentAt { get; set; } = null;

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

    public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
    {
        public void Configure(EntityTypeBuilder<EmailMessage> builder)
        {
            builder.HasIndex(_ => new
            {
                _.Status,
                _.SendAt
            });
        }
    }

    public enum EmailMessageStatusType
    {
        Created = 0,
        Sent = 1,
        Error = 2,
    }
}
