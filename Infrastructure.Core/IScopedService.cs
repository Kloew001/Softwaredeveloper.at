using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ISelfRegisterService
    {
    }

    public interface ITransientService : ISelfRegisterService
    {
    }
    public interface IScopedService : ISelfRegisterService
    {
    }
    public interface ISingletonService : ISelfRegisterService
    {
    }

    public interface ITypedService<TTypeRegisterFor> : ISelfRegisterService
    {
    }
    public interface ITypedTransientService<TTypeRegisterFor> : ITransientService, ITypedService<TTypeRegisterFor>
    {
    }
    public interface ITypedScopedService<TTypeRegisterFor> : IScopedService, ITypedService<TTypeRegisterFor>
    {
    }
    public interface ITypedSingletonService<TTypeRegisterFor> : ISingletonService, ITypedService<TTypeRegisterFor>
    {
    }

    public static class ServiceCollectionExtensions
    {
        public static void RegisterSelfRegisterServices(this IServiceCollection services)
        {
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(a => a.GetExportedTypes())
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(p => typeof(ISelfRegisterService).IsAssignableFrom(p))
               .ToList();

            foreach (var serviceType in serviceTypes)
            {
                if (typeof(ITransientService).IsAssignableFrom(serviceType))
                    services.AddTransient(serviceType);

                if (typeof(IScopedService).IsAssignableFrom(serviceType))
                    services.AddScoped(serviceType);

                if (typeof(ISingletonService).IsAssignableFrom(serviceType))
                    services.AddSingleton(serviceType);

                //if (typeof(ITypedService<>).IsAssignableFrom(serviceType))
                var genericArguments = serviceType.GetInterfaces()
                    .Where(_ => _.IsGenericType &&
                                _.GetGenericTypeDefinition() == typeof(ITypedService<>))
                    .SelectMany(_ => _.GetGenericArguments())
                    .ToList();

                foreach (var genericArgument in genericArguments)
                {
                    if (typeof(ITransientService).IsAssignableFrom(serviceType))
                        services.AddTransient(genericArgument, serviceType);

                    if (typeof(IScopedService).IsAssignableFrom(serviceType))
                        services.AddScoped(genericArgument, serviceType);

                    if (typeof(ISingletonService).IsAssignableFrom(serviceType))
                        services.AddSingleton(genericArgument, serviceType);
                }
            }
        }

        public static void RegisterAllHostedService(this IServiceCollection services)
        {
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(a => a.GetExportedTypes())
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(p => typeof(BaseHostedService).IsAssignableFrom(p))
               .ToList();

            foreach (var serviceType in serviceTypes)
            {
                services.AddScoped(serviceType);

                typeof(ServiceCollectionHostedServiceExtensions)
                    .GetMethods()
                    .Where(m => m.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService) &&
                                m.GetParameters().Count() == 1)
                    .Single()
                    .MakeGenericMethod(serviceType)
                    .Invoke(null, new object[] { services });
            }
        }
    }

}
