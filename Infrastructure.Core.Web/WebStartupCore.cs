using ExtendableEnums.Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;
using System.Diagnostics;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web;

public class WebStartupCore<TDomainStartup>
    where TDomainStartup : IDomainStartupCore, new()
{
    public WebApplicationBuilder Builder { get; }
    protected IConfiguration Configuration => Builder.Configuration;
    protected IHostEnvironment Environment => Builder.Environment;
    protected IServiceCollection Services => Builder.Services;
    public TDomainStartup DomainStartup { get; }

    public WebStartupCore(WebApplicationBuilder builder)
    {
        Builder = builder;
        DomainStartup = new TDomainStartup();
    }

    public void Run()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(GetLogFilePath(),
                rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web application");

            ConfigureServices();

            var app = Builder.Build();

            ConfigureApp(app);

            app.MapFallbackToFile("/index.html");

            app.Run();

            Log.Information("Web application finished");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Web application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    protected virtual void ConfigureServices()
    {
        DomainStartup.ConfigureServices(Services, Configuration, Environment);

        AddDefaultServices();

        Builder.AddSwaggerGenWithBearer();

        HandleAuthentication();

        Builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }

    protected virtual void AddDefaultServices()
    {
        Builder.ConfigureHosting();

        AddLogging();

        var environmentVariablesPrefix = Builder.Configuration.GetSection("EnvironmentVariablesPrefix").Get<string>() ?? string.Empty;
        Builder.Configuration.AddEnvironmentVariables(environmentVariablesPrefix);

        Builder.Services.AddHttpContextAccessor();

        Builder.AddForwardedHeaders();

        var rateLimitingConfiguration = Builder.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

        if (rateLimitingConfiguration.Enabled)
        {
            Builder.AddRateLimiter();
        }

        Builder.Services.AddControllers(options =>
        {
            options.UseExtendableEnumModelBinding();

            //options.ModelBinderProviders.Remove
            //(options.ModelBinderProviders.Single(_ => _.GetType() == typeof(Microsoft.AspNetCore.Mvc.ModelBinding.Binders.DateTimeModelBinderProvider)));

            //options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
        })
        //.AddApplicationPart(typeof(BaseApiController).Assembly)

        .AddNewtonsoftJson(options =>
        {
            //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
            options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
        });

        Builder.Services.AddEndpointsApiExplorer();

        Builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var problem = context.ProblemDetails;

                problem.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                problem.Type ??= $"https://httpstatuses.com/{problem.Status}";

                var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                problem.Extensions["traceId"] = traceId;
                problem.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                if (env.IsDevelopment() && context.Exception is not null)
                {
                    problem.Extensions["exception"] = new
                    {
                        type = context.Exception.GetType().Name,
                        message = context.Exception.Message,
                        stackTrace = context.Exception.StackTrace
                    };
                }
            };
        });

        Builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        Builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        Builder.AddResponseCompression();

        Builder.AddCors();

        Builder.AddDefaultHsts();

        Builder.Services.AddSingleton<ISecurityHeadersService, SecurityHeadersService>();
    }

    protected virtual void AddLogging()
    {
        Builder.Host.UseSerilog((ctx, services, cfg) =>
        {
            cfg
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)

            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()

            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
            
            .WriteTo.Async(a => a
                 .File(GetLogFilePath(),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 1000,
                    shared: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024,  // 10 MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({MachineName}/{ThreadId}) {SourceContext} | {Message:lj}{NewLine}{Exception}")
            );

            var serilogSection = Builder.Configuration.GetSection("Serilog");
            if (serilogSection.Exists())
            {
                cfg.ReadFrom.Configuration(Builder.Configuration);
            }
        });
    }

    protected virtual string GetLogFilePath()
    {
        return GetDefaultLogFilePath();
    }
    
    protected static string GetDefaultLogFilePath()
    {
        var projectName = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Name!.ToLowerInvariant();

        string logFolder;

        if (OperatingSystem.IsWindows())
        {
            var systemDrive = System.Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            logFolder = Path.Combine(systemDrive, "logs", projectName);
        }
        else if (OperatingSystem.IsLinux())
        {
            if (System.Environment.UserName == "root")
            {
                logFolder = Path.Combine("/var/log", projectName);
            }
            else
            {
                logFolder = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                    ".local", "share", projectName, "logs");
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            logFolder = Path.Combine("/usr/local/var/log", projectName);
        }
        else
        {
            logFolder = Path.Combine(AppContext.BaseDirectory, "logs");
        }

        Directory.CreateDirectory(logFolder);

        var logPath = Path.Combine(logFolder, "app-.log");
        return logPath;
    }

    protected virtual void HandleAuthentication()
    {
        Builder.AddJwtBearerAuthentication();
    }

    protected virtual void ConfigureApp(WebApplication app)
    {
        DomainStartup.ConfigureApp(app);

        app.UseForwardedHeaders();

        UseLogging(app);

        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseResponseCompression();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseCors();

        HandleRateLimiting(app);

        HandleAuthentication(app);
        HandleAuthorization(app);

        HandleCurrentCulture(app);

        app.UseSecurityHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }

        app.MapControllers();
    }

    protected virtual void UseLogging(WebApplication app)
    {
        app.UseFullRequestLogging();

        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = LogAdditionalInfo;
            opts.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.000} ms " +
                "[cid:{CorrelationId} acc:{AccountId} ip:{ClientIP}|{BucketIp}]";

            opts.GetLevel = (ctx, elapsed, ex) =>
            {
                var path = ctx.Request.Path;
                if (path.StartsWithSegments("/health") ||
                    path.StartsWithSegments("/swagger"))
                    return LogEventLevel.Verbose;

                if (ex != null) 
                    return LogEventLevel.Error;

                if (elapsed > 500)
                    return LogEventLevel.Warning; // > 500ms

                if (ctx.Response.StatusCode == 401) 
                    return LogEventLevel.Debug;
                
                if (ctx.Response.StatusCode == 403) 
                    return LogEventLevel.Debug;

                if (ctx.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (ctx.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });
    }

    protected virtual void LogAdditionalInfo(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("CorrelationId",
            httpContext.TraceIdentifier);

        diagnosticContext.Set("ClientIP",
            httpContext.ResolveIp());

        diagnosticContext.Set("BucketIp",
            httpContext.ResolveBucketIp());

        diagnosticContext.Set("AccountId",
            httpContext.ResolveAccountIdOrAnon());
    }

    protected virtual void HandleAuthorization(WebApplication app)
    {
        app.UseAuthorization();
    }

    protected virtual void HandleAuthentication(WebApplication app)
    {
        app.UseAuthentication();
    }

    protected virtual void HandleRateLimiting(WebApplication app)
    {
        var rateLimitingConfiguration = app.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

        if (rateLimitingConfiguration.Enabled)
        {
            app.UseRateLimiter();
        }
    }

    protected virtual void HandleCurrentCulture(WebApplication app)
    {
        app.UseCurrentCulture();
    }
}
