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
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using static Infrastructure.Core.Web.Middleware.ValidationExceptionHandler;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Core.Web
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddDefaultServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddLogging(options =>
            {
            });
            builder.Logging.AddLog4Net("log4net.config");

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddMemoryCache();

            builder.Services.AddControllers(options =>
            {
                //options.ModelBinderProviders.Remove
                //(options.ModelBinderProviders.Single(_ => _.GetType() == typeof(Microsoft.AspNetCore.Mvc.ModelBinding.Binders.DateTimeModelBinderProvider)));

                //options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
            })
            .AddNewtonsoftJson(options =>
            {
                //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

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
                options.BearerTokenExpiration = TimeSpan.FromMinutes(500); //TODO
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

        public const string _allowSpecificOrigins = "allowSpecificOrigins";

        public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: _allowSpecificOrigins,
                  policy =>
                  {
                      policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                  });
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

        public static void UseAuditMiddleware(this IApplicationBuilder app)
        {
            //app.UseAuditMiddleware(_ => _
            //    .FilterByRequest(rq => !rq.Path.Value.EndsWith("favicon.ico"))
            //    .WithEventType("{verb}:{url}")
            //    .IncludeHeaders()
            //    .IncludeResponseHeaders()
            //    .IncludeRequestBody()
            //    .IncludeResponseBody());

            //app.ApplicationServices.GetService<IAuditHandler>()
            //    .RegisterProvider();
        }
    }
}
