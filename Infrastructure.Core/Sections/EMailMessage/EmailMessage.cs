using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Activateable;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportDefault;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate;
using DocumentFormat.OpenXml.Drawing;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public class EmailMessageIgnoreSection : Section
    {
        public bool IgnoreCreate { get; set; } = true;
    }

    [Table(nameof(EmailMessage), Schema = "core")]
    public class EmailMessage : Entity, IReferencedToEntity
    {
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime SendAt { get; set; } = DateTime.Now;
        public DateTime? SentAt { get; set; } = null;

        public EmailMessageStatusType Status { get; set; } = EmailMessageStatusType.Created;

        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        [NotMapped]
        public virtual Entity Reference { get; set; }

        public Guid? CultureId { get; set; }
        public virtual MultilingualCulture Culture { get; set; }

        public Guid? TemplateId { get; set; }
        public virtual EMailMessageTemplate Template { get; set; }

        public string AnAdress { get; set; }
        public string CcAdress { get; set; }
        public string BccAdress { get; set; }

        public string Subject { get; set; }
        public string HtmlContent { get; set; }

        public string ErrorMessage { get; set; }

        public virtual ICollection<EMailMessageAttachment> Attachments { get; set; }
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

            builder.Navigation(_ => _.Attachments).AutoInclude();
        }
    }


    [Table(nameof(EMailMessageAttachment), Schema = "core")]
    public class EMailMessageAttachment : Entity
    {
        public Guid EMailMessageId { get; set; }
        public virtual EmailMessage EMailMessage { get; set; }

        public Guid BinaryContentId { get; set; }
        public virtual BinaryContent BinaryContent { get; set; }
    }

    [Table(nameof(EMailMessageConfiguration), Schema = "core")]
    public class EMailMessageConfiguration : Entity, ISupportDefault, IActivateable, ISupportValidDateRange, IMultiLingualEntity<EMailMessageConfigurationTranslation>
    {
        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public string Name { get; set; }

        public virtual ICollection<EMailMessageConfigurationTranslation> Translations { get; set; }
    }

    [Table(nameof(EMailMessageConfigurationTranslation), Schema = "core")]
    public class EMailMessageConfigurationTranslation : EntityTranslation<EMailMessageConfiguration>
    {
        public string Style { get; set; }
        public string Signature { get; set; }
        public string HtmlContent { get; set; }
    }

    [Table(nameof(EMailMessageTemplate), Schema = "core")]
    public class EMailMessageTemplate : Entity, IActivateable, ISupportValidDateRange, IMultiLingualEntity<EMailMessageTemplateTranslation>
    {
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public string Name { get; set; }

        public Guid? ConfigurationID { get; set; }
        public virtual EMailMessageConfiguration Configuration { get; set; }

        public virtual ICollection<EMailMessageTemplateTranslation> Translations { get; set; }
    }

    public class EMailMessageTemplateConfiguration : IEntityTypeConfiguration<EMailMessageTemplate>
    {
        public void Configure(EntityTypeBuilder<EMailMessageTemplate> builder)
        {
            builder.Navigation(_ => _.Configuration).AutoInclude();
        }
    }

    [Table(nameof(EMailMessageTemplateTranslation), Schema = "core")]
    public class EMailMessageTemplateTranslation : EntityTranslation<EMailMessageTemplate>
    {
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
    }

    public enum EmailMessageStatusType
    {
        Created = 0,
        Sent = 1,
        Error = 2,
    }
}
