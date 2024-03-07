using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    public class EntityValidatorDependency<TEntity> : ITransientDependency
        where TEntity : Entity
    {
        private readonly IServiceProvider _serviceProvider;

        public IDbContext DbContext { get; private set; }

        public EntityValidatorDependency(IDbContext dbContext , IServiceProvider serviceProvider)
        {
            DbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }

    public class EntityValidator<TEntity> : AbstractValidator<TEntity>, ITypedScopedDependency<EntityValidator<TEntity>>
        where TEntity : Entity
    {
        public EntityValidatorDependency<TEntity> Dependency { get; private set; }

        public EntityValidator(EntityValidatorDependency<TEntity> dependency)
        {
            Dependency = dependency;
        }

        //TODO ctor mit zugriff auf EntityService
    }
}
