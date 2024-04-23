using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete
{
    public static class AccessServiceEntityServiceExtensions
    {
        public static async Task<bool> CanCreateAsync<TEntity>(this EntityService<TEntity> service)
            where TEntity : Entity, ISoftDelete
        {
            try
            {
                using (var tran = await service.EntityServiceDependency.DbContext.Database.BeginTransactionAsync())
                {
                    await service.QuickCreateInternalAsync();
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
            where TEntity : Entity, ISoftDelete
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

        public static async Task<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await CanUpdateAsync(service, entity);
        }

        public static Task<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity, ISoftDelete
        {
            return service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanUpdateAsync(securityEntity));
        }

        public static async Task<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity, ISoftDelete
        {
            var entity = await service.GetSingleByIdInternalAsync(id);

            return await CanDeleteAsync(service, entity);
        }

        public static Task<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity, ISoftDelete
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
