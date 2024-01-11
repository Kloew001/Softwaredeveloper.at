using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class EMailHostedService : TimerHostedService
    {

        public EMailHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EMailHostedService> logger,
            IApplicationSettings optionsAccessor,
            IHostApplicationLifetime appLifetime)
            : base(serviceScopeFactory, appLifetime, logger, optionsAccessor)
        {
        }
        protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            var ids = await GetIdsAsync();

            if (ids.Any() == false)
                return;

            foreach (var id in ids)
            {
                await HandleMessageAsync(id);
            }
        }

        private async Task HandleMessageAsync(Guid id)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    scope.ServiceProvider.GetService<ICurrentUserService>()
                        .SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

                    var emailSender = scope.ServiceProvider.GetService<IEMailSender>();
                    var context = scope.ServiceProvider.GetService<IDbContext>();

                    var mailMessage = await context.Set<EmailMessage>().SingleAsync(_ => _.Id == id);

                    try
                    {
                        emailSender.Send(mailMessage);
                        mailMessage.Status = EmailMessageStatusType.Sent;
                    }
                    catch (Exception ex)
                    {
                        mailMessage.ErrorMessage = ex.Message;
                        mailMessage.Status = EmailMessageStatusType.Error;

                        _logger.LogError(ex, ex.Message);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task<List<Guid>> GetIdsAsync()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var date = DateTime.Now.AddSeconds(-1 * _hostedServicesConfiguration.InitialDelayInSeconds);

                var mailMessagesIds =
                    await context.Set<EmailMessage>()
                        .Where(_ => _.Status == EmailMessageStatusType.Created &&
                                    _.DateCreated < date)
                        .OrderByDescending(_ => _.DateCreated)
                        .Select(_ => _.Id)
                        .Take(_hostedServicesConfiguration.BatchSize)
                        .ToListAsync();

                return mailMessagesIds;
            }
        }
    }

    public class EMailConfiguration
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }

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

    public class SmtpEMailSender : IEMailSender, ITypedScopedService<IEMailSender>
    {
        private EMailConfiguration _config { get; set; }

        public SmtpEMailSender(IApplicationSettings applicationSettings)
        {
            _config = applicationSettings.EMailConfiguration;
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
