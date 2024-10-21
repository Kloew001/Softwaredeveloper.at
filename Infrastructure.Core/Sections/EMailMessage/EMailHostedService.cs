using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage;

public interface IEMailSendHandler
{
    Task<List<Guid>> GetIdsAsync(DateTime sendAt, int batchSize, CancellationToken ct);
    Task HandleMessageAsync(EmailMessage emailMessage, CancellationToken ct);
}

public class EMailSendHandler : IEMailSendHandler
{
    protected readonly IDbContext _context;
    private readonly IEMailSender _emailSender;
    private readonly IDateTimeService _dateTimeService;

    public EMailSendHandler(IDbContext context, IEMailSender emailSender, IDateTimeService dateTimeService)
    {
        _context = context;
        _emailSender = emailSender;
        _dateTimeService = dateTimeService;
    }

    public async Task HandleMessageAsync(EmailMessage emailMessage, CancellationToken ct)
    {
        try
        {
            await _emailSender.SendAsync(emailMessage);

            emailMessage.ErrorMessage = null;
            emailMessage.SentAt = _dateTimeService.Now();
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

    public async Task<List<Guid>> GetIdsAsync(DateTime sendAt, int batchSize, CancellationToken ct)
    {
        var mailMessagesIds =
            await _context.Set<EmailMessage>()
                .Where(_ => _.Status == EmailMessageStatusType.Created &&
                            _.SendAt < sendAt)
                .OrderByDescending(_ => _.SendAt)
                .Select(_ => _.Id)
                .Take(batchSize)
                .ToListAsync(ct);

        return mailMessagesIds;

    }
}

public class EMailHostedService : HandleBatchTimeHostedService
{
    public EMailHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EMailHostedService> logger,
        IApplicationSettings settings,
        IHostApplicationLifetime appLifetime)
        : base(serviceScopeFactory, appLifetime, logger, settings)
    {
    }
    
    protected override async Task<List<Guid>> GetIdsAsync(CancellationToken ct)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var sendHandler = scope.ServiceProvider.GetService<IEMailSendHandler>();

            var now = DateTime.Now;
            var date = now.Subtract(_hostedServicesConfiguration.InitialDelay);

            return await sendHandler.GetIdsAsync(date, _hostedServicesConfiguration.BatchSize, ct);
        }
    }

    protected override async Task HandleIdAsync(Guid id, CancellationToken ct)
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

                await eMailSendHandler.HandleMessageAsync(mailMessage, ct);

                await context.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

}
