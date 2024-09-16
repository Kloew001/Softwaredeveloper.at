using Microsoft.Extensions.DependencyInjection;

using System.Linq;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IAppStatupInit
    {
        Task Init();
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public abstract class SelfRegisterDependencyAttribute : Attribute
    {
        public string Key { get; set; }
    }

    public class TransientDependencyAttribute : SelfRegisterDependencyAttribute
    { }

    public class ScopedDependencyAttribute : SelfRegisterDependencyAttribute
    { }

    public class SingletonDependencyAttribute : SelfRegisterDependencyAttribute
    { }

    public class TransientDependencyAttribute<TTypeRegisterFor> : TransientDependencyAttribute
    { }

    public class ScopedDependencyAttribute<TTypeRegisterFor> : ScopedDependencyAttribute
    { }

    public class SingletonDependencyAttribute<TTypeRegisterFor> : SingletonDependencyAttribute
    { }

    public static class ServiceCollectionExtensions
    {
        public static void RegisterSelfRegisterDependencies(this IServiceCollection services)
        {
            var appStatupInits = AssemblyUtils.AllLoadedTypes()
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(p => typeof(IAppStatupInit).IsAssignableFrom(p))
               .ToList();

            foreach (var appStatupInit in appStatupInits)
            {
                services.AddSingleton(appStatupInit);
                services.AddSingleton(typeof(IAppStatupInit), (sp) => sp.GetRequiredService(appStatupInit));
            }


            var serviceTypes = AssemblyUtils.AllLoadedTypes()
               .Where(_ => _.IsClass && !_.IsAbstract && !_.IsInterface)
               .Where(_ => _.HasAttribute<SelfRegisterDependencyAttribute>() ||
                            _.GetInterfaces().Any(i => i.HasAttribute<SelfRegisterDependencyAttribute>()))
               .Select(_ => new
               {
                   ServiceType = _,
                   Attributes = _.GetAttributes<SelfRegisterDependencyAttribute>()
                        .Union(_.GetInterfaces()
                                .SelectMany(i => i.GetAttributes<SelfRegisterDependencyAttribute>()))
               })
               .ToList();

            foreach (var serviceType in serviceTypes)
            {
                if (serviceType.Attributes.OfType<TransientDependencyAttribute>().Any())
                    services.AddTransient(serviceType.ServiceType);

                var keyedTransientAttributes = serviceType.Attributes
                    .OfType<TransientDependencyAttribute>()
                    .Where(_ => _.Key.IsNotNullOrEmpty())
                    .ToList();

                foreach (var attribute in keyedTransientAttributes)
                    services.AddKeyedTransient(attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));

                if (serviceType.Attributes.OfType<ScopedDependencyAttribute>().Any())
                    services.AddScoped(serviceType.ServiceType);

                var keyedScopedAttributes = serviceType.Attributes
                    .OfType<ScopedDependencyAttribute>()
                    .Where(_ => _.Key.IsNotNullOrEmpty())
                    .ToList();

                foreach (var attribute in keyedScopedAttributes)
                    services.AddKeyedScoped(attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));

                if (serviceType.Attributes.OfType<SingletonDependencyAttribute>().Any())
                    services.AddSingleton(serviceType.ServiceType);

                var keyedSingletonAttributes = serviceType.Attributes
                    .OfType<SingletonDependencyAttribute>()
                    .Where(_ => _.Key.IsNotNullOrEmpty())
                    .ToList();

                foreach (var attribute in keyedScopedAttributes)
                    services.AddKeyedSingleton(attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));


                if (serviceType.Attributes.Any(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(TransientDependencyAttribute<>)))
                {
                    var transientAttributes = serviceType.Attributes.Where(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(TransientDependencyAttribute<>));
                    foreach (var attribute in transientAttributes)
                    {
                        var registerForType = attribute.GetType().GetGenericArguments().First();

                        services.AddTransient(registerForType, (sp) => sp.GetRequiredService(serviceType.ServiceType));

                        if (attribute.Key.IsNotNullOrEmpty())
                            services.AddKeyedTransient(registerForType, attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));
                    }
                }

                if (serviceType.Attributes.Any(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(ScopedDependencyAttribute<>)))
                {
                    var scopedAttributes = serviceType.Attributes.Where(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(ScopedDependencyAttribute<>));
                    foreach (var attribute in scopedAttributes)
                    {
                        var registerForType = attribute.GetType().GetGenericArguments().First();

                        services.AddScoped(registerForType, (sp) => sp.GetRequiredService(serviceType.ServiceType));

                        if (attribute.Key.IsNotNullOrEmpty())
                            services.AddKeyedScoped(registerForType, attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));
                    }
                }

                if (serviceType.Attributes.Any(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(SingletonDependencyAttribute<>)))
                {
                    var singletonAttributes = serviceType.Attributes.Where(_ => _.GetType().IsGenericType && _.GetType().GetGenericTypeDefinition() == typeof(SingletonDependencyAttribute<>));
                    foreach (var attribute in singletonAttributes)
                    {
                        var registerForType = attribute.GetType().GetGenericArguments().First();

                        services.AddSingleton(registerForType, (sp) => sp.GetRequiredService(serviceType.ServiceType));

                        if (attribute.Key.IsNotNullOrEmpty())
                            services.AddKeyedSingleton(registerForType, attribute.Key, (sp, o) => sp.GetRequiredService(serviceType.ServiceType));
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
