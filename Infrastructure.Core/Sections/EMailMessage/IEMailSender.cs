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

    public class EMailServerConfiguration
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
        private EMailServerConfiguration _config { get; set; }

        public SmtpEMailSender(IApplicationSettings applicationSettings)
        {
            _config = applicationSettings.EMailServer;
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
