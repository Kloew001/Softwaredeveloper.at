﻿using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public static class AccessServiceEntityServiceExtensions
    {
        public static Task<bool> CanCreateAsync<TEntity>(this EntityService<TEntity> service)
            where TEntity : Entity
        {
            return CanCreateAsync(service, (Dto)null);
        }

        public static async Task<bool> CanCreateAsync<TEntity, TDto>(this EntityService<TEntity> service, TDto dto)
            where TEntity : Entity
            where TDto : Dto
        {
            try
            {
                using (var tran = await service.EntityServiceDependency.DbContext.Database.BeginTransactionAsync())
                {
                    using (service.EntityServiceDependency.SectionManager
                            .CreateSectionScope<SuppressSaveChangesSection>())
                    {
                        await service.QuickCreateAsync(dto);
                    }
                    await tran.RollbackAsync();
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static async Task<bool> CanReadAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            try
            {
                var entity = await service.GetSingleByIdInternalAsync(id);
                return entity != null;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }   
        }

        public static async ValueTask<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await CanUpdateAsync(service, entity);
        }

        public static async ValueTask<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            return await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanUpdateAsync(securityEntity));
        }

        public static async ValueTask<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await CanDeleteAsync(service, entity);
        }

        public static ValueTask<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            return service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanDeleteAsync(securityEntity));
        }

        //public IQueryable<Entity> CanReadQuery(IQueryable<Entity> query)
        //{
        //    if (_sectionManager.IsActive<SecurityFreeSection>())
        //        return query;

        //    //var accessConditionInfo = ResolveAccessConditionInfo(entity);


        //    //var query = _context.Set<Meldungseintrag>()
        //    //    .Where(_ => EF.Property<string>(
        //    //                EF.Property<Organisation>(
        //    //                EF.Property<Meldung>(_, "Meldung"),
        //    //                "Organisation"), "Name") != null);


        //    //IQueryable<IEntity> securityParentQuery = null;

        //    //securityParentQuery = accessConditionInfo.AccessCondition
        //    //    .CanReadQuery(securityParentQuery);

        //    return query;//TODO
        //}

    }
}
