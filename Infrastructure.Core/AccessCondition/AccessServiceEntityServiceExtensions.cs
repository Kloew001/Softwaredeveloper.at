namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public static class AccessServiceEntityServiceExtensions
    {
        public static ValueTask<bool> CanCreateAsync<TEntity>(this EntityService<TEntity> service)
            where TEntity : Entity
        {
            return CanCreateAsync(service, (Dto)null);
        }

        public static async ValueTask<bool> CanCreateAsync<TEntity, TDto>(this EntityService<TEntity> service, TDto dto)
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

        public static async ValueTask<bool> CanCreateAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            if (await service.CanReadAsync(entity) == false)
                return false;

            var canCreate = await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanCreateAsync(securityEntity));

            return canCreate;
        }

        public static async ValueTask<bool> CanReadAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            try
            {
                var entity = await service.GetSingleByIdAsync(id);
                return entity != null;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static async ValueTask<bool> CanReadAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            var canRead = await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                         accessCondition.CanReadAsync(securityEntity));

            return canRead;
        }

        public static async ValueTask<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            var entity = await service.GetSingleByIdAsync(id);

            return await CanUpdateAsync(service, entity);
        }

        public static async ValueTask<bool> CanUpdateAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            if(await service.CanReadAsync(entity) == false) 
                return false; 

            var canUpdate = await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanUpdateAsync(securityEntity));

            return canUpdate;
        }

        public static async ValueTask<bool> CanSaveAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            var canUpdate = await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanSaveAsync(securityEntity));

            return canUpdate;
        }

        public static async ValueTask<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, Guid id)
            where TEntity : Entity
        {
            var entity = await service.GetSingleByIdAsync(id);

            return await CanDeleteAsync(service, entity);
        }

        public static async ValueTask<bool> CanDeleteAsync<TEntity>(this EntityService<TEntity> service, TEntity entity)
            where TEntity : Entity
        {
            if (await service.CanReadAsync(entity) == false)
                return false;

            var canDelete = await service.EntityServiceDependency.AccessService
                .EvaluateAsync(entity, (accessCondition, securityEntity) =>
                    accessCondition.CanDeleteAsync(securityEntity));

            return canDelete;
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
