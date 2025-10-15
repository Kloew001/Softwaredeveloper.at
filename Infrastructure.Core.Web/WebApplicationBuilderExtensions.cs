﻿using ExtendableEnums.Microsoft.AspNetCore;
using Infrastructure.Core.Web.Middleware;
using Infrastructure.Core.Web.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;
using System.IO.Compression;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace Infrastructure.Core.Web;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddDefaultServices(this WebApplicationBuilder builder)
    {
        builder.ConfigureHosting();

        var environmentVariablesPrefix = builder.Configuration.GetSection("EnvironmentVariablesPrefix").Get<string>() ?? string.Empty;
        builder.Configuration.AddEnvironmentVariables(environmentVariablesPrefix);

        builder.Services.AddLogging(options =>
        {
        });
        builder.Logging.AddLog4Net("log4net.config");

        builder.Services.AddHttpContextAccessor();

        builder.AddForwardedHeaders();

        builder.AddRateLimiter();

        builder.Services.AddControllers(options =>
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

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
        {
            var problemCorrelationId = Guid.NewGuid().ToString();
            //log problemCorrelationId into logging system
            ctx.ProblemDetails.Instance = problemCorrelationId;
        });

        builder.AddResponseCompression();

        builder.AddCors();

        builder.AddDefaultHsts();

        builder.Services.AddSingleton<ISecurityHeadersService, SecurityHeadersService>();

        return builder;
    }

    private const int _defaultMxRequestBodySizeInMB = 5;

    public static WebApplicationBuilder ConfigureHosting(this WebApplicationBuilder builder)
    {
        var maxRequestBodySizeInMB = builder.Configuration.GetSection("MaxRequestBodySizeInMB").Get<int>();

        if (maxRequestBodySizeInMB == 0)
            maxRequestBodySizeInMB = _defaultMxRequestBodySizeInMB;

        builder.Services.Configure(delegate (IISServerOptions options)
        {
            options.MaxRequestBodySize = maxRequestBodySizeInMB * 1024 * 1024;
        });

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = maxRequestBodySizeInMB * 1024 * 1024;

            serverOptions.AddServerHeader = false;

        });

        return builder;
    }

    public static WebApplicationBuilder AddSwaggerGenWithBearer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddSwaggerGenWithNegotiate(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });
        });

        return builder;
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationMinutes { get; set; } = 60;
    }

    public static WebApplicationBuilder AddJwtBearerAuthentication(this WebApplicationBuilder builder, Action<AuthorizationBuilder> authorizationOptions = null)
    {
        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        //    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        //    options.DefaultSignInScheme = IdentityConstants.BearerScheme;
        //})
        //.AddBearerToken(IdentityConstants.BearerScheme, options =>
        //{
        //    options.BearerTokenExpiration = TimeSpan.FromMinutes(15);
        //    options.RefreshTokenExpiration = TimeSpan.FromMinutes(60);
        //});
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
            options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.UniqueName;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.EmailClaimType = JwtRegisteredClaimNames.Email;
            options.ClaimsIdentity.SecurityStampClaimType = TokenAuthenticateService.JwtSecurityStampClaim;
        });

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        builder.Services.AddSingleton<JwtSettings>(jwtSettings);
        builder.Services.AddTransient<ITokenService, JwtTokenService>();
        builder.Services.AddTransient<TokenAuthenticateService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {

            options.MapInboundClaims = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero,
                SaveSigninToken = false,

                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimsIdentity.DefaultRoleClaimType,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async (ctx) =>
                {
                    var signInManager = ctx.HttpContext.RequestServices
                        .GetRequiredService<SignInManager<ApplicationUser>>();

                    signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

                    var user = await signInManager.ValidateSecurityStampAsync(ctx.Principal);

                    if (user == null)
                    {
                        ctx.Fail("Invalid Security Stamp");
                    }
                    else
                    {
                        var claimsSecurityStamp = ctx.Principal.FindFirstValue(signInManager.Options.ClaimsIdentity.SecurityStampClaimType);
                        var securityStamp = await signInManager.UserManager.GetSecurityStampAsync(user);
                    }
                }
            };
        });

        var authorizationBuilder = builder.Services
            .AddAuthorizationBuilder();

        if (authorizationOptions != null)
        {
            authorizationOptions(authorizationBuilder);
        }
        else
        {
            authorizationBuilder.AddPolicy("api", p =>
            {
                p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                p.RequireAuthenticatedUser();
            });
        }

        return builder;
    }

    public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

        if (allowedOrigins != null && allowedOrigins.Any())
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                  {
                      policy
                          .WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                  });
            });
        }

        return builder;
    }

    public static WebApplicationBuilder AddResponseCompression(this WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });

        return builder;
    }

    public static WebApplicationBuilder AddIdentity<TUser, TRole, TContext>(this WebApplicationBuilder builder)
        where TUser : ApplicationUser
        where TRole : ApplicationRole
        where TContext : DbContext
    {
        var identityBuilder =
        builder.Services
            .AddIdentity<TUser, TRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoleManager<RoleManager<TRole>>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders()
            .AddUserConfirmation<UserConfirmation>();

        identityBuilder
            .AddTokenProvider(TokenAuthenticateService.TokenProviderName, typeof(DataProtectorTokenProvider<ApplicationUser>));

        return builder;
    }

    public static WebApplicationBuilder AddDefaultHsts(this WebApplicationBuilder builder)
    {
        //https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
        builder.Services.AddHsts(options =>
        {
            options.Preload = false;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(60);

            options.ExcludedHosts.Clear();
        });

        return builder;
    }

    public static void AddForwardedHeaders(this WebApplicationBuilder builder)
    {
        var knownProxies = builder.Configuration.GetSection("KnownProxies").Get<string[]>() ?? [];
        var knownNetworks = builder.Configuration.GetSection("KnownNetworks").Get<string[]>() ?? [];

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            if (knownProxies.Length > 0 || knownNetworks.Length > 0)
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                foreach (var proxy in knownProxies)
                    options.KnownProxies.Add(IPAddress.Parse(proxy));

                foreach (var network in knownNetworks)
                {
                    var parts = network.Split('/');
                    options.KnownNetworks.Add(
                        new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse(parts[0]), int.Parse(parts[1])));
                }
            }
            else
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
            }
        });
    }

    public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder, Action<RateLimiterOptions>? configureOptions = null)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    httpContext.ResolveIpOrAnon(), _ =>
                    new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed2per10Sec, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions()
                    {
                        AutoReplenishment = true,
                        PermitLimit = 2,
                        QueueLimit = 1,
                        Window = TimeSpan.FromSeconds(10)
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed5per10Sec, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions()
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        QueueLimit = 1,
                        Window = TimeSpan.FromSeconds(10)
                    });
            });

            options.AddPolicy(RateLimitPolicy.Sliding60per1Min, httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed10per1Min, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed5per1Hour, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed10per1Hour, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed10per24Hours, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromHours(24),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicy.Fixed100per24Hours, httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromHours(24),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicy.Sliding1per1SecAnd100per24Hours, httpContext =>
            {
                return RateLimitPartition.Get(
                    httpContext.ResolveAccountIdOrAnonBucketIdKey(),
                    _ =>
                    {
                        var perSecond = new SlidingWindowRateLimiter(
                            new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = 1,
                                Window = TimeSpan.FromSeconds(1),
                                SegmentsPerWindow = 5,
                                QueueLimit = 2,
                                AutoReplenishment = true
                            });

                        var perDay = new SlidingWindowRateLimiter(
                            new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = 10,
                                Window = TimeSpan.FromHours(24),
                                QueueLimit = 0,
                                SegmentsPerWindow = 1,
                                AutoReplenishment = true
                            });

                        return RateLimiterCombine.AndAll(perSecond, perDay);
                    });
            });

            configureOptions?.Invoke(options);
        });

        return builder;
    }
}

public static class RateLimitPolicy
{
    public const string Fixed2per10Sec = "Fixed2per10Sec";
    public const string Fixed5per10Sec = "Fixed5per10Sec";
    public const string Fixed10per1Min = "Fixed10per1Min";
    public const string Fixed5per1Hour = "Fixed5per1Hour";
    public const string Fixed10per1Hour = "Fixed10per1Hour";
    public const string Fixed10per24Hours = "Fixed10per24Hours";
    public const string Fixed100per24Hours = "Fixed100per24Hours";
    public const string Sliding1per1SecAnd100per24Hours = "Fixed1per1SecAnd100per24Hours";

    public const string Sliding60per1Min = "Sliding60per1Min";
}