namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EmailMessaga
{
    public class EmailMessageService : IScopedDependency
    {
        private readonly IDbContext _context;
        private readonly MultilingualService _multilingualService;

        public EmailMessageService(IDbContext context, MultilingualService multilingualService)
        {
            _context = context;
            _multilingualService = multilingualService;
        }

        public async Task<EmailMessage> Create()
        {
            var email = await _context.CreateEntity<EmailMessage>();

            return email;
        }

        public async Task<EmailMessage> Create(string multilingualTextKey)
        {
            var email = await Create();

            email.Subject = await _multilingualService.GetTextAsync($"{multilingualTextKey}.Subject");

            email.HtmlContent = await _multilingualService.GetTextAsync($"{multilingualTextKey}.HtmlContent");

            return email;   
        }

    }
}
