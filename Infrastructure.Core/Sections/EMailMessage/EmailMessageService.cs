using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Activateable;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public static class EmailMessageServiceExtensions
    {
        public static async Task<EMailMessageTemplate> GetEMailMessageTemplate<TEntity>(this EntityService<TEntity> service, Guid templateId)
            where TEntity : Entity
        {
            var template = await service.EntityServiceDependency.DbContext
                .Set<EMailMessageTemplate>()
                .IsActive()
                .IsValidDateIncluded(DateTime.Now)
                .SingleOrDefaultAsync(_ => _.Id == templateId);

            return template;
        }

        public static async Task<EMailMessageTemplate> GetEMailMessageTemplate<TEntity>(this EntityService<TEntity> service, string templateName)
            where TEntity : Entity
        {
            var template = await service.EntityServiceDependency.DbContext
                .Set<EMailMessageTemplate>()
                .IsActive()
                .IsValidDateIncluded(DateTime.Now)
                .SingleOrDefaultAsync(_ => _.Name == templateName);

            if (template == null)
                throw new InvalidOperationException($"Template {template} not found");

            return template;
        }

        public static async Task<EmailMessage> CreateEMailMessageByTemplateAsync<TEntity>(
            this EntityService<TEntity> service,
            TEntity referenceEntity,
            string templateName,
            Guid? cultureId = null,
            Func<EmailMessage, ValueTask> modifyEmailMessage = null)
            where TEntity : Entity
        {
            var template = await GetEMailMessageTemplate(service, templateName);

            var emailMessageService =
             service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<EmailMessageService>();

            return await emailMessageService.CreateByTemplateAsync(referenceEntity, template, cultureId, modifyEmailMessage);
        }

        public static async Task<EmailMessage> CreateEMailMessageByTemplateAsync<TEntity>(
            this EntityService<TEntity> service,
            TEntity referenceEntity,
            Guid templateId,
            Guid? cultureId = null,
            Func<EmailMessage, ValueTask> modifyEmailMessage = null)
            where TEntity : Entity
        {
            var template = await GetEMailMessageTemplate(service, templateId);

            var emailMessageService =
             service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<EmailMessageService>();

            return await emailMessageService.CreateByTemplateAsync(referenceEntity, template, cultureId, modifyEmailMessage);
        }

        public static Task<EmailMessage> CreateEMailMessageByTemplateAsync<TEntity>(
            this EntityService<TEntity> service,
            TEntity referenceEntity,
            EMailMessageTemplate template,
            Guid? cultureId = null,
            Func<EmailMessage, ValueTask> modifyEmailMessage = null)
            where TEntity : Entity
        {
            var emailMessageService =
             service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<EmailMessageService>();

            if (template == null)
                throw new InvalidOperationException($"Template {template} not found");

            return emailMessageService.CreateByTemplateAsync(referenceEntity, template, cultureId, modifyEmailMessage);
        }
    }

    public class EmailMessageService : IScopedDependency
    {
        private readonly IDbContext _context;
        private readonly EmailMessageIgnoreSection _emailMessageIgnoreSection;
        private readonly IEmailMessageBookmarkReplacer _emailMessageBookmarkReplacer;
        private readonly ICurrentLanguageService _currentLanguageService;

        public EmailMessageService(
            IDbContext context,
            ICurrentLanguageService currentLanguageService,
            EmailMessageIgnoreSection emailMessageIgnoreSection,
            IEmailMessageBookmarkReplacer emailMessageBookmarkReplacer)
        {
            _context = context;
            _currentLanguageService = currentLanguageService;
            _emailMessageIgnoreSection = emailMessageIgnoreSection;
            _emailMessageBookmarkReplacer = emailMessageBookmarkReplacer;
        }

        public async Task<EmailMessage> CreateEmptyAsync(Entity referenceEntity)
        {
            if (_emailMessageIgnoreSection.IsActive &&
                _emailMessageIgnoreSection.IgnoreCreate == true)
                return null;

            var email = await _context.CreateEntityAync<EmailMessage>();

            email.Status = EmailMessageStatusType.Created;
            email.SetReference(referenceEntity);

            return email;
        }

        public async Task<EmailMessage> CreateByTemplateAsync(
            Entity referenceEntity, 
            EMailMessageTemplate template, 
            Guid? cultureId = null,
            Func<EmailMessage, ValueTask> modifyEmailMessage = null)
        {
            var email = await CreateEmptyAsync(referenceEntity);

            if (email == null)
                return null;

            email.CultureId = cultureId ?? _currentLanguageService.CurrentCultureId;

            email.Template = template;

            var templateTranslation = template.GetCultureTranslationOrDefault(email.CultureId);

            if (templateTranslation == null)
                throw new InvalidOperationException($"No translation found for template {template.Name}");

            email.Subject = templateTranslation.Subject;
            email.HtmlContent = GetBaseHtmlContent(email, template, templateTranslation);

            if (modifyEmailMessage != null)
                await modifyEmailMessage(email);

            _emailMessageBookmarkReplacer.ReplaceAllBookmarks(email, referenceEntity);

            return email;
        }

        private string GetBaseHtmlContent(EmailMessage email, EMailMessageTemplate template, EMailMessageTemplateTranslation templateTranslation)
        {
            if (template.Configuration != null)
            {
                var configurationTranslation = template.Configuration.GetCultureTranslationOrDefault(email.CultureId);

                if (configurationTranslation == null)
                    throw new InvalidOperationException($"No configuration found for email template {template.Id} and culture {email.CultureId}");

                var htmlContent = configurationTranslation.HtmlContent
                    .Replace(EmailMessageGlobalBookmark.BODY, templateTranslation.HtmlContent);

                return htmlContent;
            }
            else
            {
                return templateTranslation.HtmlContent;
            }
        }
    }
}
