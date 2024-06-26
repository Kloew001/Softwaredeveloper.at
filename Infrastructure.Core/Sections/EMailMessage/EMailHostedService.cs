﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
    public interface IEMailSendHandler
    {
        Task<List<Guid>> GetIdsAsync(DateTime sendAt, int batchSize);
        Task HandleMessageAsync(EmailMessage emailMessage);
    }

    public class EMailSendHandler : IEMailSendHandler
    {
        protected readonly IDbContext _context;
        private readonly IEMailSender _emailSender;

        public EMailSendHandler(IDbContext context, IEMailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task HandleMessageAsync(EmailMessage emailMessage)
        {
            try
            {
                await _emailSender.SendAsync(emailMessage);

                emailMessage.ErrorMessage = null;
                emailMessage.SentAt = DateTime.Now;
                emailMessage.Status = EmailMessageStatusType.Sent;

                OnSent(emailMessage);
            }
            catch (Exception ex)
            {
                emailMessage.ErrorMessage = ex.ToString();
                emailMessage.Status = EmailMessageStatusType.Error;

                OnError(emailMessage);
            }
        }

        protected virtual void OnError(EmailMessage emailMessage)
        {
        }

        protected virtual void OnSent(EmailMessage emailMessage)
        {
        }

        public async Task<List<Guid>> GetIdsAsync(DateTime sendAt, int batchSize)
        {
            var mailMessagesIds =
                await _context.Set<EmailMessage>()
                    .Where(_ => _.Status == EmailMessageStatusType.Created &&
                                _.SendAt < sendAt)
                    .OrderByDescending(_ => _.SendAt)
                    .Select(_ => _.Id)
                    .Take(batchSize)
                    .ToListAsync();

            return mailMessagesIds;

        }
    }

    public class EMailHostedService : TimerHostedService
    {
        public EMailHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EMailHostedService> logger,
            IApplicationSettings settings,
            IHostApplicationLifetime appLifetime)
            : base(serviceScopeFactory, appLifetime, logger, settings)
        {
        }

        protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
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

                    var eMailSendHandler = scope.ServiceProvider.GetService<IEMailSendHandler>();
                    var context = scope.ServiceProvider.GetService<IDbContext>();

                    var mailMessage = await context.Set<EmailMessage>().SingleAsync(_ => _.Id == id);

                    await eMailSendHandler.HandleMessageAsync(mailMessage);

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
                var sendHandler = scope.ServiceProvider.GetService<IEMailSendHandler>();

                var date = DateTime.Now.Subtract(_hostedServicesConfiguration.InitialDelay);

                return await sendHandler.GetIdsAsync(date, _hostedServicesConfiguration.BatchSize);
            }
        }
    }
}
