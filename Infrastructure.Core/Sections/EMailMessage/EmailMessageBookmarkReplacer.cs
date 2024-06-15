using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public class EmailMessageBookmarkSection : Section
    {
        public List<IEmailMessageBookmark> BookmarkExtensions = [];

        protected override void OnCreateScope()
        {
            base.OnCreateScope();

            BookmarkExtensions = [];
        }
    }

    public interface IEmailMessageBookmark
    {
        string Key { get; set; }
        Func<EmailMessage, IEntity, string> ValueResolver { get; }
        Type TypeOfRoot { get; }
    }

    public class EmailMessageBookmark<TRoot> : IEmailMessageBookmark
    {
        public string Key { get; set; }
        public Func<EmailMessage, TRoot, string> ValueResolver { get; set; }
        public Type TypeOfRoot { get => typeof(TRoot); }

        Func<EmailMessage, IEntity, string> IEmailMessageBookmark.ValueResolver => (e, o) => { return ValueResolver(e, (TRoot)o); };

        public EmailMessageBookmark(string key, Func<EmailMessage, TRoot, string> valueFunc)
        {
            Key = key;
            ValueResolver = valueFunc;
        }
    }

    public interface IEmailMessageGlobalBookmark
    {
        List<IEmailMessageBookmark> Bookmarks { get; }
    }

    public class EmailMessageGlobalBookmark : IEmailMessageGlobalBookmark
    {
        public const string STYLE = "$$STYLE$$";
        public const string SIGNATURE = "$$SIGNATURE$$";
        public const string BODY = "$$BODY$$";
        public const string BaseUrl = "{{BaseUrl}}";
        public const string CurrentYear = "{{CurrentYear}}";

        protected readonly ILogger<EmailMessageGlobalBookmark> _logger;
        protected readonly IApplicationSettings _applicationSettings;

        public List<IEmailMessageBookmark> Bookmarks { get; set; } = [];

        public EmailMessageGlobalBookmark(ILogger<EmailMessageGlobalBookmark> logger, IApplicationSettings applicationSettings)
        {
            _logger = logger;
            _applicationSettings = applicationSettings;

            Bookmarks.Add(new EmailMessageBookmark<IEntity>(STYLE, GetStyle));
            Bookmarks.Add(new EmailMessageBookmark<IEntity>(SIGNATURE, GetSignature));
            Bookmarks.Add(new EmailMessageBookmark<IEntity>(BaseUrl, (e, o) => _applicationSettings.Url.BaseUrl));
            Bookmarks.Add(new EmailMessageBookmark<IEntity>(CurrentYear, (e, o) => DateTime.Now.Year.ToString()));
        }

        protected virtual string GetStyle(EmailMessage email, IEntity referenceEntity)
        {
            return GetEmailConfiguration(email, referenceEntity)?.Style;
        }

        protected virtual string GetSignature(EmailMessage email, IEntity referenceEntity)
        {
            return GetEmailConfiguration(email, referenceEntity)?.Signature;
        }

        protected virtual EMailMessageConfigurationTranslation GetEmailConfiguration(EmailMessage email, IEntity referenceEntity)
        {
            if (email.Template?.Configuration == null)
                return null;

            var configuration = email.Template.Configuration.GetCultureTranslationOrDefault(email.CultureId);

            if (configuration == null)
            {
                _logger.LogWarning("No configuration found for email template {0} and culture {1}", email.TemplateId, email.CultureId);
                throw new InvalidOperationException($"No configuration found for email template {email.TemplateId} and culture {email.CultureId}");
            }

            return configuration;
        }

    }

    public interface IEmailMessageBookmarkReplacer
    {
        void ReplaceAllBookmarks(EmailMessage emailMessage, IEntity referenceEntity);
    }

    public class EmailMessageBookmarkReplacer : IEmailMessageBookmarkReplacer
    {
        private IEmailMessageGlobalBookmark _globalBookmarks;
        private EmailMessageBookmarkSection _bookmarkReplaceSection;

        public EmailMessageBookmarkReplacer(
            IEmailMessageGlobalBookmark globalBookmarks,
            EmailMessageBookmarkSection bookmarkReplaceSection)
        {
            _globalBookmarks = globalBookmarks;
            _bookmarkReplaceSection = bookmarkReplaceSection;
        }

        public void ReplaceAllBookmarks(EmailMessage emailMessage, IEntity referenceEntity)
        {
            emailMessage.Subject = ReplaceAllBookmarks(emailMessage, emailMessage.Subject, referenceEntity);
            emailMessage.HtmlContent = ReplaceAllBookmarks(emailMessage, emailMessage.HtmlContent, referenceEntity);
        }

        private string ReplaceAllBookmarks(EmailMessage emailMessage, string content, IEntity referenceEntity)
        {
            foreach (var bookmark in _globalBookmarks.Bookmarks)
            {
                if (_bookmarkReplaceSection.IsActive &&
                    _bookmarkReplaceSection.BookmarkExtensions.Any(e => e.Key == bookmark.Key))
                    continue;

                content = HandleBookmark(emailMessage, bookmark, content, referenceEntity);
            }

            if (_bookmarkReplaceSection.IsActive)
            {
                foreach (var bookmark in _bookmarkReplaceSection.BookmarkExtensions)
                {
                    content = HandleBookmark(emailMessage, bookmark, content, referenceEntity);
                }
            }

            return content;
        }

        private string HandleBookmark(EmailMessage email, IEmailMessageBookmark bookmark, string content, IEntity referenceEntity)
        {
            if (!bookmark.TypeOfRoot.IsAssignableFrom(referenceEntity.GetType()))
                return content;

            if (!ContainsBookmark(bookmark.Key, content))
                return content;

            var value = bookmark.ValueResolver(email, referenceEntity);
            content = ReplaceBookmark(bookmark.Key, content, value);

            return content;
        }

        private string ReplaceBookmark(string bookmarkKey, string content, string value)
        {
            content = content.Replace(bookmarkKey, value);
            return content;
        }

        private bool ContainsBookmark(string bookmarkKey, string content)
        {
            return content.Contains(bookmarkKey);
        }
    }
}
