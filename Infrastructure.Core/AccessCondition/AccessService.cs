using Microsoft.Extensions.Caching.Memory;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public class SecurityFreeSection : Section
    {
    }

    public class AccessService : IScopedDependency
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly SectionManager _sectionManager;

        public AccessService(
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            SectionManager sectionManager)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _sectionManager = sectionManager;
        }

        public class SecurityParentInfo
        {
            public Type EntityType { get; set; }
            public PropertyInfo SecurityProperty { get; set; }
            public Type AccessConditionType { get; set; }

            public SecurityParentInfo(Type entityType)
            {
                EntityType = entityType;
            }
        }

        public class AccessConditionInfo
        {
            public Entity Entity { get; set; }

            public Entity SecurityEntity { get; set; }
            public IAccessCondition AccessCondition { get; set; }
        }

        public AccessConditionInfo ResolveAccessConditionInfo(Entity entity, AccessConditionInfo accessConditionInfo = null)
        {
            if (entity == null)
                return null;

            if (accessConditionInfo == null)
            {
                accessConditionInfo = new AccessConditionInfo
                {
                    Entity = entity,
                    SecurityEntity = entity
                };
            }

            var securityInfo = ResolveSecurityParentInfo(entity.GetType());

            if (securityInfo.AccessConditionType != null)
            {
                if (_sectionManager.IsActive<SecurityFreeSection>())
                {
                    accessConditionInfo.AccessCondition = GetAllAccessCondition(entity);
                }
                else
                {
                    accessConditionInfo.AccessCondition = _serviceProvider.GetService(securityInfo.AccessConditionType) as IAccessCondition;
                }

                accessConditionInfo.SecurityEntity = entity;
            }

            else if (securityInfo.SecurityProperty != null)
            {
                var securityParent = securityInfo.SecurityProperty.GetValue(entity, null) as Entity;

                if (securityParent == null)
                    throw new InvalidOperationException($"securityparent is null '{securityInfo.EntityType.Name}' '{entity.Id}'");

                return ResolveAccessConditionInfo(securityParent, accessConditionInfo);
            }
            else
            {
                accessConditionInfo.AccessCondition = GetAllAccessCondition(entity); //wenn keine Security definiert, Zugriff erlaubt
            }

            return accessConditionInfo;
            //throw new InvalidOperationException($"no accessDefinition found for '{securityInfo.EntityType.Name}'");
        }

        private IAccessCondition GetAllAccessCondition(Entity entity)
        {
            var allAccessConditionType = typeof(AllAccessCondition<>).MakeGenericType(entity.GetType());
            return _serviceProvider.GetService(allAccessConditionType) as IAccessCondition;
        }

        private SecurityParentInfo ResolveSecurityParentInfo(Type entityType)
        {
            entityType = entityType.UnProxy();

            var cacheKey = $"{nameof(AccessService)}_{nameof(ResolveSecurityParentInfo)}_{entityType.FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out SecurityParentInfo securityInfo))
            {
                securityInfo = new SecurityParentInfo(entityType);

                var accessConditionType = typeof(IAccessCondition<>).MakeGenericType(securityInfo.EntityType);
                var accessCondition = _serviceProvider.GetService(accessConditionType) as IAccessCondition;

                if (accessCondition != null)
                {
                    securityInfo.AccessConditionType = accessConditionType;
                }
                else
                {
                    var accessConditionFromInterface = securityInfo.EntityType.GetInterfaces()
                        .Select(ResolveAccessCondition)
                        .FirstOrDefault(i => i != null);

                    if (accessConditionFromInterface != null)
                    {
                        securityInfo.AccessConditionType = accessConditionFromInterface.GetType();
                    }
                }

                var securityParentAttribute = securityInfo.EntityType.GetAttribute<SecurityParentAttribute>();
                if (securityParentAttribute != null)
                {
                    securityInfo.SecurityProperty = securityInfo.EntityType.GetProperty(securityParentAttribute.PropertyName);
                }

                _memoryCache.Set(cacheKey, securityInfo);
            }

            return securityInfo;
        }

        private IAccessCondition ResolveAccessCondition(Type entityType)
        {
            var accessConditionType = typeof(IAccessCondition<>).MakeGenericType(entityType);
            var accessCondition = _serviceProvider.GetService(accessConditionType) as IAccessCondition;

            if (accessCondition != null)
                return accessCondition;

            return null;
        }
    }
}
