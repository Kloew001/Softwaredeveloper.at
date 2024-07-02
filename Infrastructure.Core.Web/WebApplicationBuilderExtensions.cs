using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

using System.Threading.RateLimiting;
using Infrastructure.Core.Web.Middleware;
using Infrastructure.Core.Web.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Core.Web
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddDefaultServices(this WebApplicationBuilder builder)
        {
            builder.ConfigureHosting();

            builder.Services.AddLogging(options =>
            {
            });
            builder.Logging.AddLog4Net("log4net.config");

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers(options =>
            {
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

            builder.AddRateLimiter();

            builder.AddDefaultHsts();

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
            builder.Services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder, Action<AuthorizationBuilder> authorizationOptions = null)
        {
            return builder;
        }

        public static WebApplicationBuilder AddBearerAuthentication(this WebApplicationBuilder builder, Action<AuthorizationBuilder> authorizationOptions = null)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
                options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
                options.DefaultSignInScheme = IdentityConstants.BearerScheme;
            })
            .AddBearerToken(IdentityConstants.BearerScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
            });

            var authorizationBuilder = builder.Services.AddAuthorizationBuilder();


            if (authorizationOptions != null)
            {
                authorizationOptions(authorizationBuilder);
            }
            else
            {
                authorizationBuilder.AddPolicy("api", p =>
                {
                    p.RequireAuthenticatedUser();
                    p.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
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
                          policy.WithOrigins(allowedOrigins)
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

            return builder;
        }

        public static WebApplicationBuilder AddDefaultHsts(this WebApplicationBuilder builder)
        {
            builder.Services.AddHsts(options =>
            {
                options.Preload = false;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
                
                options.ExcludedHosts.Clear();
            });

            return builder;
        }

        public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder, Action<RateLimiterOptions> configureOptions = null)
        {
            //https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var ipAddress = httpContext.ResolveIpAddress();
                        if (ipAddress == null)
                            return RateLimitPartition.GetNoLimiter("none");

                        return RateLimitPartition.GetTokenBucketLimiter(ipAddress, (key) =>
                        {
                            return new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 100,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                                TokensPerPeriod = 1,
                                AutoReplenishment = true,
                                //QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                                //QueueLimit = 3
                            };
                        });
                    });

                options.AddPolicy("largeFileUpload", httpContext =>
                {
                    var ipAddress = httpContext.ResolveIpAddress();
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, (key) =>
                    {
                        return new FixedWindowRateLimiterOptions()
                        {
                            AutoReplenishment = true,
                            PermitLimit = 2,
                            QueueLimit = 1,
                            Window = TimeSpan.FromSeconds(15)
                        };
                    });
                });

                //PartitionedRateLimiter.CreateChained(
                //   PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //   {
                //       var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                //       httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor);

                //       var ipAddress = forwardedFor.FirstOrDefault() ?? httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString();

                //       if (ipAddress == null)
                //           return RateLimitPartition.GetNoLimiter("none");

                //       return RateLimitPartition.GetFixedWindowLimiter
                //       (userAgent, _ =>
                //           new FixedWindowRateLimiterOptions
                //           {
                //               AutoReplenishment = true,
                //               PermitLimit = 10,
                //               Window = TimeSpan.FromSeconds(2)
                //           });
                //       }),
                //       PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //       {
                //           var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                //           return RateLimitPartition.GetFixedWindowLimiter
                //           (userAgent, _ =>
                //               new FixedWindowRateLimiterOptions
                //               {
                //                   AutoReplenishment = true,
                //                   PermitLimit = 30,
                //                   Window = TimeSpan.FromSeconds(30)
                //               });
                //       }),
                //       PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //       {
                //           var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                //           return RateLimitPartition.GetFixedWindowLimiter
                //           (userAgent, _ =>
                //               new FixedWindowRateLimiterOptions
                //               {
                //                   AutoReplenishment = true,
                //                   PermitLimit = 40,
                //                   Window = TimeSpan.FromSeconds(60)
                //               });
                //       }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests; ;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"You have reached the request limit. Try again in {{retryAfter.TotalMinutes}} minutes.", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "You have reached the request limit. Try again later.", cancellationToken: token);
                    }
                };

                configureOptions?.Invoke(options);
            });

            return builder;
        }
    }
}
