using Castle.Core.Internal;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.Extensions.Caching.Memory;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public class AccessService : IScopedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;

        public AccessService(IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
        }

        public bool CanRead(BaseEntity entity)
        {
            var accesscondition = ResolveAccessCondition(entity);
            var securityEntity = ResolveSecurityEntity(entity);

            return accesscondition.CanRead(securityEntity);
        }

        public IQueryable<BaseEntity> CanReadQuery(IQueryable<BaseEntity> query)
        {
            //var accesscondition = ResolveAccessCondition(entity);
            //var securityEntity = ResolveSecurityEntity(entity);

            //return accesscondition.CanRead(securityEntity);

            return query;//TODO
        }

        public bool CanCreate(BaseEntity entity)
        {
            var accesscondition = ResolveAccessCondition(entity);
            var securityEntity = ResolveSecurityEntity(entity);

            return accesscondition.CanCreate(securityEntity);
        }

        public bool CanUpdate(BaseEntity entity)
        {
            var accesscondition = ResolveAccessCondition(entity);
            var securityEntity = ResolveSecurityEntity(entity);

            return accesscondition.CanUpdate(securityEntity);
        }

        public bool CanDelete(BaseEntity entity)
        {
            var accesscondition = ResolveAccessCondition(entity);
            var securityEntity = ResolveSecurityEntity(entity);

            return accesscondition.CanDelete(securityEntity);
        }

        private IAccessCondition ResolveAccessCondition(BaseEntity entity)
        {
            var entityType = entity.GetType().UnProxy();

            var cacheKey = $"{nameof(AccessService)}_{nameof(ResolveAccessCondition)}_{entityType.FullName}";

            if (!_memoryCache.TryGetValue(cacheKey, out IAccessCondition accessCondition))
            {
                var securityEntityType = ResolveSecurityEntityType(entityType);

                var accessConditionType = typeof(IAccessCondition<>).MakeGenericType(securityEntityType);

                accessCondition = _serviceProvider.GetService(accessConditionType) as IAccessCondition;

                if (accessCondition == null)
                    throw new NotImplementedException($"IAccessCondition<{securityEntityType.Name}> missing.");

                _memoryCache.Set(cacheKey, accessCondition);
            }

            return accessCondition;
        }

        private BaseEntity ResolveSecurityEntity(BaseEntity entity)
        {
            var entityType = entity.GetType().UnProxy();
            var securityParentAttribute = entityType.GetAttribute<SecurityParentAttribute>();
            if (securityParentAttribute != null)
            {
                var securityParentProperty = entityType.GetProperty(securityParentAttribute.PropertyName);

                var securityParent = securityParentProperty.GetValue(entity, null) as BaseEntity;

                if (securityParent == null)
                    throw new InvalidOperationException($"securityparent is null '{entityType.Name}' '{entity.Id}'");

                return ResolveSecurityEntity(securityParent);
            }

            return entity;
        }

        private Type ResolveSecurityEntityType(Type entityType)
        {
            entityType = entityType.UnProxy();
            
            var securityParentAttribute = entityType.GetAttribute<SecurityParentAttribute>();
            if (securityParentAttribute != null)
            {
                var securityParentProperty = entityType.GetProperty(securityParentAttribute.PropertyName);

                return ResolveSecurityEntityType(securityParentProperty.PropertyType);
            }

            return entityType;
        }


    }
}
