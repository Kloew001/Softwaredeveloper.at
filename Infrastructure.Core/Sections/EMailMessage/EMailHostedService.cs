﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage
{
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

                    var emailSender = scope.ServiceProvider.GetService<IEMailSender>();
                    var context = scope.ServiceProvider.GetService<IDbContext>();

                    var mailMessage = await context.Set<EmailMessage>().SingleAsync(_ => _.Id == id);

                    try
                    {
                        await emailSender.SendAsync(mailMessage);

                        mailMessage.ErrorMessage = null;
                        mailMessage.SentAt = DateTime.Now;
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

                var date = DateTime.Now.Subtract(_hostedServicesConfiguration
                    .InitialDelay);

                var mailMessagesIds =
                    await context.Set<EmailMessage>()
                        .Where(_ => _.Status == EmailMessageStatusType.Created &&
                                    _.SendAt < date)
                        .OrderByDescending(_ => _.SendAt)
                        .Select(_ => _.Id)
                        .Take(_hostedServicesConfiguration.BatchSize)
                        .ToListAsync();

                return mailMessagesIds;
            }
        }
    }
}
