using Microsoft.Extensions.Options;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public interface IEMailSender
    {
        void Send(EmailMessage message);
    }

    public class DevelopmentEmailSender : IEMailSender
    {
        public void Send(EmailMessage message)
        {
        }
    }
}
