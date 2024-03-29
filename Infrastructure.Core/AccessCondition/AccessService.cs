﻿using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
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

        public Task<bool> CanReadAsync(Entity entity)
        {
            if (entity == null)
                return Task.FromResult(true);

            if (_sectionManager.IsActive<SecurityFreeSection>())
                return Task.FromResult(true);

            var accessConditionInfo = ResolveAccessConditionInfo(entity);

            if(accessConditionInfo == null)
                return Task.FromResult(true);

            return accessConditionInfo.AccessCondition
                .CanReadAsync(accessConditionInfo.SecurityEntity);
        }

        public IQueryable<Entity> CanReadQuery(IQueryable<Entity> query)
        {
            if (_sectionManager.IsActive<SecurityFreeSection>())
                return query;

            //var accessConditionInfo = ResolveAccessConditionInfo(entity);


            //var query = _context.Set<Meldungseintrag>()
            //    .Where(_ => EF.Property<string>(
            //                EF.Property<Organisation>(
            //                EF.Property<Meldung>(_, "Meldung"),
            //                "Organisation"), "Name") != null);


            //IQueryable<IEntity> securityParentQuery = null;

            //securityParentQuery = accessConditionInfo.AccessCondition
            //    .CanReadQuery(securityParentQuery);

            return query;//TODO
        }

        public Task<bool> CanCreateAsync(Entity entity)
        {
            if (entity == null)
                return Task.FromResult(true);

            if (_sectionManager.IsActive<SecurityFreeSection>())
                return Task.FromResult(true);

            var accessConditionInfo = ResolveAccessConditionInfo(entity);

            if (accessConditionInfo == null)
                return Task.FromResult(true);

            return accessConditionInfo.AccessCondition
                .CanCreateAsync(accessConditionInfo.SecurityEntity);
        }

        public Task<bool> CanUpdateAsync(Entity entity)
        {
            if (entity == null)
                return Task.FromResult(true);

            if (_sectionManager.IsActive<SecurityFreeSection>())
                return Task.FromResult(true);

            var accessConditionInfo = ResolveAccessConditionInfo(entity);

            if (accessConditionInfo == null)
                return Task.FromResult(true);

            return accessConditionInfo.AccessCondition
                .CanUpdateAsync(accessConditionInfo.SecurityEntity);
        }

        public Task<bool> CanDeleteAsync(Entity entity)
        {
            if (entity == null)
                return Task.FromResult(true);

            if (_sectionManager.IsActive<SecurityFreeSection>())
                return Task.FromResult(true);

            var accessConditionInfo = ResolveAccessConditionInfo(entity);

            if (accessConditionInfo == null)
                return Task.FromResult(true);

            return accessConditionInfo.AccessCondition
                .CanDeleteAsync(accessConditionInfo.SecurityEntity);
        }

        public Task<bool> CanSaveAsync(Entity entity)
        {
            if (entity == null)
                return Task.FromResult(true);

            if (_sectionManager.IsActive<SecurityFreeSection>())
                return Task.FromResult(true);

            var accessConditionInfo = ResolveAccessConditionInfo(entity);

            if (accessConditionInfo == null)
                return Task.FromResult(true);

            return accessConditionInfo.AccessCondition
                .CanSaveAsync(accessConditionInfo.SecurityEntity);
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

        private AccessConditionInfo ResolveAccessConditionInfo(Entity entity)
        {
            var accessConditionInfo = new AccessConditionInfo
            {
                Entity = entity
            };

            var securityInfo = ResolveSecurityParentInfo(entity.GetType());

            if (securityInfo.AccessConditionType != null)
            {
                var accessCondition = _serviceProvider.GetService(securityInfo.AccessConditionType) as IAccessCondition;

                accessConditionInfo.AccessCondition = accessCondition;
                accessConditionInfo.SecurityEntity = entity;

                return accessConditionInfo;
            }

            if (securityInfo.SecurityProperty != null)
            {
                var securityParent = securityInfo.SecurityProperty.GetValue(entity, null) as Entity;

                if (securityParent == null)
                    throw new InvalidOperationException($"securityparent is null '{securityInfo.EntityType.Name}' '{entity.Id}'");

                return ResolveAccessConditionInfo(securityParent);
            }

            return null;
            //throw new InvalidOperationException($"no accessDefinition found for '{securityInfo.EntityType.Name}'");
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
