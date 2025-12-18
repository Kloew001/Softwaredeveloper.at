using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage;

public interface IEMailSendHandler
{
    Task<List<Guid>> GetIdsAsync(DateTime sendAt, int batchSize, CancellationToken ct);
    Task HandleMessageAsync(EmailMessage emailMessage, CancellationToken ct);
}

public class EMailSendHandler : IEMailSendHandler
{
    protected readonly ILogger<EMailSendHandler> _logger;
    protected readonly IDbContext _context;
    protected readonly IEMailSender _emailSender;
    protected readonly IDateTimeService _dateTimeService;

    public EMailSendHandler(
        ILogger<EMailSendHandler> logger,
        IDbContext context,
        IEMailSender emailSender,
        IDateTimeService dateTimeService)
    {
        _logger = logger;
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

            _logger.LogError(ex, "Error sending email message with id {EmailMessageId}: {ErrorMessage}", emailMessage.Id, ex.Message);
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
        var baseQuery = _context.Set<EmailMessage>()
            .Where(_ => _.Status == EmailMessageStatusType.Created &&
                        _.SendAt < sendAt);

        if (batchSize == 1)
        {
            return await baseQuery
                .OrderBy(_ => _.DateCreated)
                .Select(_ => _.Id)
                .Take(batchSize)
                .ToListAsync(ct);
        }

        var lifoRatio = 0.2;
        var lifoCount = Math.Max(1, (int)Math.Floor(batchSize * lifoRatio));
        var fifoCount = batchSize - lifoCount;

        var fifoIds = await baseQuery
            .OrderBy(_ => _.DateCreated)
            .Select(_ => _.Id)
            .Take(fifoCount)
                .ToListAsync(ct);

        var lifoIds = await baseQuery
            .OrderByDescending(_ => _.DateCreated)
            .Select(_ => _.Id)
            .Take(lifoCount)
                .ToListAsync(ct);

        var mailMessagesIds = fifoIds.Union(lifoIds).ToList();

        return mailMessagesIds;
    }
}

public class EMailHostedService : HandleBatchTimeHostedService, IBackgroundTriggerable<EmailMessage>
{
    public EMailHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EMailHostedService> logger,
        IApplicationSettings settings,
        IHostApplicationLifetime appLifetime)
        : base(serviceScopeFactory, appLifetime, logger, settings)
    {
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();

        config.BatchSize ??= 10;
        config.ExecuteMode ??= HostedServicesExecuteModeType.Trigger;

        if (config.ExecuteMode == HostedServicesExecuteModeType.Interval)
        {
            config.Interval ??= TimeSpan.FromSeconds(5);
        }
        else if (config.ExecuteMode == HostedServicesExecuteModeType.Trigger)
        {
            config.TriggerExecuteWaitTimeout ??= TimeSpan.FromMinutes(1);
        }

        return config;
    }

    protected override async Task<List<Guid>> GetIdsAsync(IServiceScope scope, CancellationToken ct)
    {
        var sendHandler = scope.ServiceProvider.GetService<IEMailSendHandler>();

        var now = _dateTimeService.Now();

        return await sendHandler.GetIdsAsync(now, _configuration.BatchSize.Value, ct);
    }

    protected override async Task HandleIdAsync(IServiceScope scope, Guid id, CancellationToken ct)
    {
        try
        {
            scope.ServiceProvider.GetService<ICurrentUserService>()
                .SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var eMailSendHandler = scope.ServiceProvider.GetService<IEMailSendHandler>();
            var context = scope.ServiceProvider.GetService<IDbContext>();

            var mailMessage = await context.Set<EmailMessage>().SingleAsync(_ => _.Id == id);

            await eMailSendHandler.HandleMessageAsync(mailMessage, ct);

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}