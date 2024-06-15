using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public interface IEMailSender
    {
        Task SendAsync(EmailMessage message);
    }

    public class NoEmailSender : IEMailSender
    {
        public Task SendAsync(EmailMessage message)
        {
            return Task.CompletedTask;
        }
    }

    [ApplicationConfiguration]
    public class SmtpServerConfiguration
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }

    public class SmtpEMailSender : IEMailSender
    {
        private SmtpServerConfiguration _config { get; set; }

        public SmtpEMailSender(IApplicationSettings applicationSettings)
        {
            _config = applicationSettings.SmtpServer;
        }


        public async Task SendAsync(EmailMessage message)
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

                    foreach (var attachment in message.Attachments)
                    {
                        builder.Attachments.Add(Path.GetFileName(attachment.BinaryContent.Name), attachment.BinaryContent.Content);
                    }   

                    mailMessage.Body = builder.ToMessageBody();

                    await client.ConnectAsync(_config.SmtpServer, _config.Port);

                    if (!string.IsNullOrEmpty(_config.UserName))
                    {
                        await client.AuthenticateAsync(_config.UserName, _config.Password);
                    }

                    await client.SendAsync(mailMessage);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
