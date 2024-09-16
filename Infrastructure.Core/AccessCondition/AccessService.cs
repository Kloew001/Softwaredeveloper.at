using Microsoft.Extensions.Caching.Memory;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public class SecurityFreeSection : Section
    {
    }

    [ScopedDependency]
    public class AccessService
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

        private class SecurityTypeInfo
        {
            public Type EntityType { get; set; }
            public PropertyInfo SecurityProperty { get; set; }

            public Type AccessConditionType { get; set; }

            public SecurityTypeInfo(Type entityType)
            {
                EntityType = entityType;
            }
        }

        public class AccessConditionInfo
        {
            public Type EntityType { get; set; }

            public AccessConditionInfo ParentAccessConditionInfo { get; set; }
            public PropertyInfo ParentSecurityProperty { get; set; }
            public AccessConditionInfo RootAccessConditionInfo => ParentAccessConditionInfo?.RootAccessConditionInfo ?? this;

            //public IEntity Entity { get; set; }

            //public IEntity SecurityEntity { get; set; }

            public IAccessCondition AccessCondition
            {
                get
                {
                    if (_accessCondition != null)
                        return _accessCondition;

                    return RootAccessConditionInfo._accessCondition;
                }
                set { _accessCondition = value; }
            }
            private IAccessCondition _accessCondition;
        }

        public ValueTask<bool> EvaluateAsync<TAccessCondition>(IEntity entity, Func<TAccessCondition, IEntity, ValueTask<bool>> canAsync)
                where TAccessCondition : IAccessCondition
        {
            return EvaluateAsync(entity, (ac, se) =>
            {
                return EvaluateInternalAsync(canAsync, ac, se);
            });
        }

        private async ValueTask<bool> EvaluateInternalAsync<TAccessCondition>(Func<TAccessCondition, IEntity, ValueTask<bool>> canAsync, IAccessCondition accessCondition, IEntity securityEntity) 
            where TAccessCondition : IAccessCondition
        {
            if (_sectionManager.IsActive<SecurityFreeSection>())
                return true;

            var result = await canAsync((TAccessCondition)accessCondition, securityEntity);
            return result;
        }

        public ValueTask<bool> EvaluateAsync(IEntity entity, Func<IAccessCondition, IEntity, ValueTask<bool>> canAsync)
        {
            var accessConditionInfo = ResolveAccessConditionInfo(entity);
            var securityEntity = GetSecurityEntityInternal(entity, accessConditionInfo);
            var accessCondition = accessConditionInfo.AccessCondition;

            return canAsync(accessCondition, securityEntity);
        }

        private IEntity GetSecurityEntity(IEntity entity)
        {
            var accessConditionInfo = ResolveAccessConditionInfo(entity.GetType());

            return GetSecurityEntityInternal(entity, accessConditionInfo);
        }

        private IEntity GetSecurityEntityInternal(IEntity entity, AccessConditionInfo accessConditionInfo = null)
        {
            if (accessConditionInfo?.ParentSecurityProperty == null)
                return entity;

            var parentSecurityEntity = accessConditionInfo.ParentSecurityProperty.GetValue(entity).As<IEntity>();

            var parentAccessConditionInfo = ResolveAccessConditionInfo(parentSecurityEntity);
            return GetSecurityEntityInternal(parentSecurityEntity, parentAccessConditionInfo);
        }

        public AccessConditionInfo ResolveAccessConditionInfo<TEntity>()
        {
            return ResolveAccessConditionInfo(typeof(TEntity));
        }

        public AccessConditionInfo ResolveAccessConditionInfo(IEntity entity)
        {
            return ResolveAccessConditionInfo(entity.GetType());
        }

        private AccessConditionInfo ResolveAccessConditionInfo(Type entityType)
        {
            if (entityType == null)
                return null;

            var accessConditionInfo = new AccessConditionInfo
            {
                EntityType = entityType,
            };

            var securityInfo = ResolveSecurityTypeInfo(entityType);

            if (securityInfo.AccessConditionType != null)
            {
                if (_sectionManager.IsActive<SecurityFreeSection>())
                {
                    accessConditionInfo.AccessCondition = GetAllAccessCondition(entityType);
                }
                else
                {
                    accessConditionInfo.AccessCondition = _serviceProvider.GetService(securityInfo.AccessConditionType) as IAccessCondition;
                }
            }
            else if (securityInfo.SecurityProperty != null)
            {
                //var securityParent = securityInfo.SecurityProperty.GetValue(entity, null) as IEntity;

                //if (securityParent == null)
                //    throw new InvalidOperationException($"securityparent is null '{securityInfo.EntityType.Name}' '{entity.Id}'");

                accessConditionInfo.ParentSecurityProperty = securityInfo.SecurityProperty;

                accessConditionInfo.ParentAccessConditionInfo =
                    ResolveAccessConditionInfo(accessConditionInfo.ParentSecurityProperty.PropertyType);
            }
            else
            {
                accessConditionInfo.AccessCondition = GetAllAccessCondition(entityType); //wenn keine Security definiert, Zugriff erlaubt
            }

            return accessConditionInfo;
            //throw new InvalidOperationException($"no accessDefinition found for '{securityInfo.EntityType.Name}'");
        }

        private SecurityTypeInfo ResolveSecurityTypeInfo(Type entityType)
        {
            entityType = entityType.UnProxy();

            var cacheKey = $"{nameof(AccessService)}_{nameof(ResolveSecurityTypeInfo)}_{entityType.FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out SecurityTypeInfo securityInfo))
            {
                securityInfo = new SecurityTypeInfo(entityType);

                var securityParentAttribute = securityInfo.EntityType.GetAttribute<SecurityParentAttribute>();
                if (securityParentAttribute != null)
                {
                    securityInfo.SecurityProperty = securityInfo.EntityType.GetProperty(securityParentAttribute.PropertyName);
                }
                else
                {
                    var accessConditionType = typeof(IAccessCondition<>).MakeGenericType(securityInfo.EntityType);
                    var accessCondition = _serviceProvider.GetService(accessConditionType) as IAccessCondition;

                    if (accessCondition != null)
                    {
                        securityInfo.AccessConditionType = accessConditionType;
                    }
                    //else
                    //{
                    //    var accessConditionFromInterface = securityInfo.EntityType.GetInterfaces()
                    //Where interfacde is IEntity
                    //        .Select(ResolveAccessCondition)
                    //        .FirstOrDefault(i => i != null);

                    //    if (accessConditionFromInterface != null)
                    //    {
                    //        securityInfo.AccessConditionType = accessConditionFromInterface.GetType();
                    //    }
                    //}
                }

                _memoryCache.Set(cacheKey, securityInfo);
            }

            return securityInfo;
        }

        private IAccessCondition GetAllAccessCondition(Type entityType)
        {
            var allAccessConditionType = typeof(AllAccessCondition<>).MakeGenericType(entityType);
            return _serviceProvider.GetService(allAccessConditionType) as IAccessCondition;
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
