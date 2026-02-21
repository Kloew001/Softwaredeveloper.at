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
using Serilog.Formatting.Json;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web;

public class WebStartupCore<TDomainStartup>
    where TDomainStartup : IDomainStartupCore, new()
{
    public WebApplicationBuilder Builder { get; }
    protected IConfiguration Configuration => Builder.Configuration;
    protected IHostEnvironment Environment => Builder.Environment;
    protected IServiceCollection Services => Builder.Services;
    public TDomainStartup DomainStartup { get; }

    public WebStartupCore(string[] args)
    {
        Builder = WebApplication.CreateBuilder(args);
        DomainStartup = new TDomainStartup();

        ConfigureBuilder(Builder);
    }

    public void Run()
    {
        ConfigureBootstrapLogger();

        try
        {
            Log.Information("Starting web application");

            ConfigureServices();

            var app = Builder.Build();

            ConfigureApp(app);

            MapFallback(app);

            BeforeRun(app);

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

    protected virtual void ConfigureBuilder(WebApplicationBuilder builder)
    {
    }

    protected virtual void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(GetLogFilePath(),
                rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();
    }

    protected virtual void BeforeRun(WebApplication app)
    {
    }

    protected virtual void MapFallback(WebApplication app)
    {
        app.MapFallbackToFile("/index.html");
    }

    protected virtual void ConfigureServices()
    {
        DomainStartup.ConfigureServices(Services, Configuration, Environment);

        AddDefaultServices();

        AddSwaggerGen();

        AddAuthentication();

        AddCurrentUserService();
    }

    protected virtual void AddDefaultServices()
    {
        ConfigureHosting();

        AddLogging();

        AddEnvironmentVariables();

        AddHttpContextAccessor();

        AddForwardedHeaders();

        AddRateLimitingServices();

        ConfigureMvc();

        AddEndpointsApiExplorer();

        AddResponseCompression();

        AddCors();

        AddHsts();

        AddSecurityHeaders();

        AddProblemDetails();

        AddExceptionHandlers();
    }

    protected virtual void ConfigureApp(WebApplication app)
    {
        DomainStartup.ConfigureApp(app);

        UseForwardedHeaders(app);

        UseLogging(app);

        UseExceptionHandling(app);

        UseRequestBuffering(app);

        UseHttpsBehavior(app);

        UseResponseCompression(app);

        UseSpaStaticFiles(app);

        UseCors(app);

        UseRateLimiting(app);

        UseAuthentication(app);
        UseAuthorization(app);

        UseSerilogAccountContext(app);

        UseCurrentCulture(app);

        UseSecurityHeaders(app);

        UseSwagger(app);

        app.MapControllers();
    }

    protected virtual void ConfigureHosting()
    {
        Builder.ConfigureHosting();
    }

    protected virtual void AddCurrentUserService()
    {
        Builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }

    protected virtual void AddEnvironmentVariables()
    {
        var environmentVariablesPrefix = Builder.Configuration.GetSection("EnvironmentVariablesPrefix").Get<string>() ?? string.Empty;
        Builder.Configuration.AddEnvironmentVariables(environmentVariablesPrefix);
    }

    protected virtual void AddHttpContextAccessor()
    {
        Builder.Services.AddHttpContextAccessor();
    }

    protected virtual void AddForwardedHeaders()
    {
        Builder.AddForwardedHeaders();
    }

    protected virtual void AddRateLimitingServices()
    {
        var rateLimitingConfiguration = Builder.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

        if (rateLimitingConfiguration.Enabled)
        {
            Builder.AddRateLimiter();
        }
    }

    protected virtual void ConfigureMvc()
    {
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
    }

    protected virtual void AddEndpointsApiExplorer()
    {
        Builder.Services.AddEndpointsApiExplorer();
    }

    protected virtual void AddResponseCompression()
    {
        Builder.AddResponseCompression();
    }

    protected virtual void AddCors()
    {
        Builder.AddCors();
    }

    protected virtual void AddHsts()
    {
        Builder.AddDefaultHsts();
    }

    protected virtual void AddSecurityHeaders()
    {
        Builder.Services.AddSingleton<ISecurityHeadersService, SecurityHeadersService>();
    }

    protected virtual void AddProblemDetails()
    {
        Builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var problem = context.ProblemDetails;

                problem.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                problem.Type ??= $"https://httpstatuses.com/{problem.Status}";

                problem.Extensions["correlationId"] = context.HttpContext.ResolveCorrelationId();
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
    }

    protected virtual void AddExceptionHandlers()
    {
        Builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        Builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
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

            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")

            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(LoggingFilters.IsWebOrWorker)
                .WriteTo.Async(a => a.File(
                    path: GetLogFilePath("app-.jlog"),
                    formatter: new JsonFormatter(),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 100,
                    shared: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024,  // 10 MB
                    rollOnFileSizeLimit: true
                // outputTemplate: "[GEN {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
                ))
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(LoggingFilters.IsWeb)
                .WriteTo.Async(a => a.File(
                    formatter: new JsonFormatter(),
                    GetLogFilePath("web-.jlog"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 100,
                    shared: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024,  // 10 MB
                    rollOnFileSizeLimit: true
                // outputTemplate: "[WEB {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [cid:{CorrelationId} acc:{AccountId} ip:{ClientIP}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
                ))
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(LoggingFilters.IsWorker)
                .WriteTo.Async(a => a.File(
                    formatter: new JsonFormatter(),
                    path: GetLogFilePath("worker-.jlog"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 100,
                    shared: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024,  // 10 MB
                    rollOnFileSizeLimit: true
                // outputTemplate: "[WRK {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [cid:{CorrelationId}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
                ))
            );

            var serilogSection = Builder.Configuration.GetSection("Serilog");
            if (serilogSection.Exists())
            {
                cfg.ReadFrom.Configuration(Builder.Configuration);
            }
        });
    }

    protected virtual string GetLogFilePath(string fileName = "app-.log")
    {
        return GetDefaultLogFilePath(fileName);
    }

    protected static string GetDefaultLogFilePath(string fileName = "app-.log")
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

        var logPath = Path.Combine(logFolder, fileName);
        return logPath;
    }

    protected virtual void AddAuthentication()
    {
        Builder.AddJwtBearerAuthentication();
    }

    protected virtual void AddSwaggerGen()
    {
        Builder.AddSwaggerGenWithBearer();
    }

    protected virtual void UseForwardedHeaders(WebApplication app)
    {
        app.UseForwardedHeaders();
    }

    protected virtual void UseExceptionHandling(WebApplication app)
    {
        app.UseExceptionHandler();
    }

    protected virtual void UseRequestBuffering(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });
    }

    protected virtual void UseHttpsBehavior(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
    }

    protected virtual void UseResponseCompression(WebApplication app)
    {
        app.UseResponseCompression();
    }

    protected virtual void UseSpaStaticFiles(WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }

    protected virtual void UseCors(WebApplication app)
    {
        app.UseCors();
    }

    protected virtual void UseSerilogAccountContext(WebApplication app)
    {
        app.UseSerilogAccountContext();
    }

    protected virtual void UseSecurityHeaders(WebApplication app)
    {
        app.UseSecurityHeaders();
    }

    protected virtual void UseSwagger(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }
    }

    protected virtual void UseLogging(WebApplication app)
    {
        app.UseFullRequestLogging();

        app.UseSerilogAdditionalContext();

        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = LogAdditionalInfo;
            opts.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.000} ms";

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
        //diagnosticContext.Set("CorrelationId",
        //    httpContext.ResolveCorrelationId());
    }

    protected virtual void UseAuthorization(WebApplication app)
    {
        app.UseAuthorization();
    }

    protected virtual void UseAuthentication(WebApplication app)
    {
        app.UseAuthentication();
    }

    protected virtual void UseRateLimiting(WebApplication app)
    {
        var rateLimitingConfiguration = app.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

        if (rateLimitingConfiguration.Enabled)
        {
            app.UseRateLimiter();
        }
    }

    protected virtual void UseCurrentCulture(WebApplication app)
    {
        app.UseCurrentCulture();
    }
}