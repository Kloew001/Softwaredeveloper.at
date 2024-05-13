using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EmailMessaga
{
    public static class EmailMessageServiceExtensions
    {
        public static Task<EmailMessage> CreateEMailMessageAsync<TEntity>(this EntityService<TEntity> service, TEntity referenceEntity, string multilingualTextKey, Guid? cultureId = null)
            where TEntity : Entity
        {
            var emailMessageService =
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<EmailMessageService>();

            return emailMessageService.Create(referenceEntity, multilingualTextKey, cultureId);
        }
    }

    public class EmailMessageService : IScopedDependency
    {
        private readonly IDbContext _context;
        private readonly MultilingualService _multilingualService;

        public EmailMessageService(IDbContext context, MultilingualService multilingualService)
        {
            _context = context;
            _multilingualService = multilingualService;
        }

        public async Task<EmailMessage> Create(Entity referenceEntity)
        {
            var email = await _context.CreateEntity<EmailMessage>();
            
            email.Status = EmailMessageStatusType.Created;
            email.SetReference(referenceEntity);

            return email;
        }

        public async Task<EmailMessage> Create(Entity referenceEntity, string multilingualTextKey, Guid? cultureId = null)
        {
            var email = await Create(referenceEntity);

            if (multilingualTextKey.IsNotNullOrEmpty())
            {
                email.Subject = _multilingualService.GetText($"{multilingualTextKey}.Subject", cultureId);

                email.HtmlContent = _multilingualService.GetText($"{multilingualTextKey}.HtmlContent", cultureId);
            }

            return email;
        }

    }
}
