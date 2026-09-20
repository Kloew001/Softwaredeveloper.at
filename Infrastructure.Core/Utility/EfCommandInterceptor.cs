using System.Data;
using System.Data.Common;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

[SingletonDependency]
public sealed partial class EfCommandInterceptor : DbCommandInterceptor
{
    private readonly ILogger<EfCommandInterceptor> _logger;
    private readonly IOptionsMonitor<AppLoggingConfiguration> _settings;

    public EfCommandInterceptor(
        ILogger<EfCommandInterceptor> logger,
        IOptionsMonitor<AppLoggingConfiguration> settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogCommand(command, eventData, eventData.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, eventData, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    public override object ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object result)
    {
        LogCommand(command, eventData, eventData.Duration);
        return result;
    }

    public override ValueTask<object> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object result,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, eventData, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogCommand(command, eventData, eventData.Duration);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, eventData, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        LogCommand(command, eventData, eventData.Duration, LogLevel.Error, "Failed");
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, eventData, eventData.Duration, LogLevel.Error, "Failed");
        return Task.CompletedTask;
    }

    public override void CommandCanceled(DbCommand command, CommandEndEventData eventData)
    {
        LogCommand(command, eventData, eventData.Duration, LogLevel.Debug, "Canceled");
    }

    public override Task CommandCanceledAsync(
        DbCommand command,
        CommandEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, eventData, eventData.Duration, LogLevel.Debug, "Canceled");
        return Task.CompletedTask;
    }

    private void LogCommand(
        DbCommand command,
        CommandEventData eventData,
        TimeSpan duration,
        LogLevel? forcedLevel = null,
        string outcome = "Executed")
    {
        if (forcedLevel is { } requiredLevel)
        {
            if (!_logger.IsEnabled(requiredLevel))
                return;
        }
        else if (!_logger.IsEnabled(LogLevel.Warning) && !_logger.IsEnabled(LogLevel.Debug))
            return;

        var settings = _settings.CurrentValue.EntityFramework;
        var durationMilliseconds = duration.TotalMilliseconds;
        var level = forcedLevel
            ?? (settings?.CommandStackTraceWarningThresholdMilliseconds is { } threshold
                && durationMilliseconds > threshold
                    ? LogLevel.Warning
                    : LogLevel.Debug);

        if (!_logger.IsEnabled(level))
            return;

        var commandText = eventData.LogCommandText;
        var parameters = EfCommandLogFormatter.FormatParameters(
            command.Parameters,
            settings?.EnableSensitiveDataLogging == true);

        if (settings?.EnableCommandStackTraceLogging == true)
            LogWithTrace(
                _logger,
                level,
                eventData.CommandId,
                eventData.ExecuteMethod,
                durationMilliseconds,
                outcome,
                parameters,
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                commandText,
                CaptureStackTrace(eventData));
        else
            LogWithoutTrace(
                _logger,
                level,
                eventData.CommandId,
                eventData.ExecuteMethod,
                durationMilliseconds,
                outcome,
                parameters,
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                commandText);
    }

    [LoggerMessage(
        EventId = 1,
        Message = "{Outcome} DbCommand ({DurationMilliseconds}ms) " +
            "[Parameters=[{Parameters}], CommandType='{CommandType}', CommandTimeout='{CommandTimeout}', " +
            "CommandId='{CommandId}', ExecuteMethod='{ExecuteMethod}']{NewLine}{CommandText}",
        SkipEnabledCheck = true)]
    private static partial void LogWithoutTrace(
        ILogger logger,
        LogLevel level,
        Guid commandId,
        DbCommandMethod executeMethod,
        double durationMilliseconds,
        string outcome,
        string parameters,
        CommandType commandType,
        int commandTimeout,
        string newLine,
        string commandText);

    [LoggerMessage(
        EventId = 2,
        Message = "{Outcome} DbCommand ({DurationMilliseconds}ms) " +
            "[Parameters=[{Parameters}], CommandType='{CommandType}', CommandTimeout='{CommandTimeout}', " +
            "CommandId='{CommandId}', ExecuteMethod='{ExecuteMethod}']{NewLine}{CommandText}" +
            "{NewLine}C# completion stack:{NewLine}{EfCommandStackTrace}",
        SkipEnabledCheck = true)]
    private static partial void LogWithTrace(
        ILogger logger,
        LogLevel level,
        Guid commandId,
        DbCommandMethod executeMethod,
        double durationMilliseconds,
        string outcome,
        string parameters,
        CommandType commandType,
        int commandTimeout,
        string newLine,
        string commandText,
        string efCommandStackTrace);

    private static string CaptureStackTrace(CommandEventData eventData)
    {
        var prefix = Assembly.GetExecutingAssembly()
            .GetName().Name!
            .Split('.')[0];
        var applicationPrefix = eventData.Context?.GetType().Assembly
            .GetName().Name?.Split('.')[0];

        return StackTraceHelper.GetApplicationStack(prefix, applicationPrefix);
    }
}
