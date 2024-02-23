using MimeKit;
using MailKit.Net.Smtp;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public class SmtpEMailSender : IEMailSender, ITypedScopedDependency<IEMailSender>
    {
        private EMailServerConfiguration _config { get; set; }

        public SmtpEMailSender(IApplicationSettings applicationSettings)
        {
            _config = applicationSettings.EMailServer;
        }


        public void Send(EmailMessage message)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));

                    mailMessage.To.Add(MailboxAddress.Parse(message.AnAdress));

                    if (message.CcAdress.IsNullOrEmpty() == false)
                    {
                        foreach (var ccAdress in message.CcAdress.Split(';'))
                        {
                            mailMessage.Cc.Add(MailboxAddress.Parse(ccAdress));
                        }
                    }

                    if (message.BccAdress.IsNullOrEmpty() == false)
                    {
                        foreach (var bcAdress in message.CcAdress.Split(';'))
                        {
                            mailMessage.Bcc.Add(MailboxAddress.Parse(bcAdress));
                        }
                    }

                    mailMessage.Subject = message.Subject;

                    var builder = new BodyBuilder();
                    builder.HtmlBody = message.HtmlContent;


                    if (message.Attachment1 != null)
                    {
                        builder.Attachments.Add(
                       Path.GetFileName(message.Attachment1Name),
                       message.Attachment1);
                    }
                    if (message.Attachment2 != null)
                    {
                        builder.Attachments.Add(
                           Path.GetFileName(message.Attachment2Name),
                           message.Attachment2);
                    }

                    mailMessage.Body = builder.ToMessageBody();

                    client.Connect(_config.SmtpServer, _config.Port);

                    if (!string.IsNullOrEmpty(_config.UserName))
                    {
                        client.Authenticate(_config.UserName, _config.Password);
                    }

                    client.Send(mailMessage);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
