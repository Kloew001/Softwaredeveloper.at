using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ISelfRegisterDependency
    {
    }

    public interface ITransientDependency : ISelfRegisterDependency
    {
    }
    public interface IScopedDependency : ISelfRegisterDependency
    {
    }
    public interface ISingletonDependency : ISelfRegisterDependency
    {
    }

    public interface ITypedDependency<TTypeRegisterFor> : ISelfRegisterDependency
    {
    }
    public interface ITypedTransientDependency<TTypeRegisterFor> : ITransientDependency, ITypedDependency<TTypeRegisterFor>
    {
    }
    public interface ITypedScopedDependency<TTypeRegisterFor> : IScopedDependency, ITypedDependency<TTypeRegisterFor>
    {
    }
    public interface ITypedSingletonDependency<TTypeRegisterFor> : ISingletonDependency, ITypedDependency<TTypeRegisterFor>
    {
    }

    public static class ServiceCollectionExtensions
    {
        public static void RegisterSelfRegisterDependencies(this IServiceCollection services)
        {
            var serviceTypes = AssemblyUtils.AllLoadedTypes()
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(p => typeof(ISelfRegisterDependency).IsAssignableFrom(p))
               .ToList();

            foreach (var serviceType in serviceTypes)
            {
                if (typeof(ITransientDependency).IsAssignableFrom(serviceType))
                    services.AddTransient(serviceType);

                if (typeof(IScopedDependency).IsAssignableFrom(serviceType))
                    services.AddScoped(serviceType);

                if (typeof(ISingletonDependency).IsAssignableFrom(serviceType))
                    services.AddSingleton(serviceType);

                //if (typeof(ITypedService<>).IsAssignableFrom(serviceType))
                var genericArguments = serviceType.GetInterfaces()
                    .Where(_ => _.IsGenericType &&
                                _.GetGenericTypeDefinition() == typeof(ITypedDependency<>))
                    .SelectMany(_ => _.GetGenericArguments())
                    .ToList();

                foreach (var genericArgument in genericArguments)
                {
                    if (typeof(ITransientDependency).IsAssignableFrom(serviceType))
                    {
                        //services.AddTransient(genericArgument, serviceType);
                        services.AddTransient(genericArgument, (sp) => sp.GetRequiredService(serviceType));
                    }

                    if (typeof(IScopedDependency).IsAssignableFrom(serviceType))
                    {
                        //services.AddScoped(genericArgument, serviceType);
                        services.AddScoped(genericArgument, (sp) => sp.GetRequiredService(serviceType));
                    }

                    if (typeof(ISingletonDependency).IsAssignableFrom(serviceType))
                    {
                        //services.AddSingleton(genericArgument, serviceType);
                        services.AddSingleton(genericArgument, (sp) => sp.GetRequiredService(serviceType));
                    }
                }
            }
        }

        public static void RegisterAllHostedService(this IServiceCollection services)
        {
            var serviceTypes = AssemblyUtils.AllLoadedTypes()
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
