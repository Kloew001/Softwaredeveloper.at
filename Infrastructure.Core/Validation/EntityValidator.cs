using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    [TransientDependency]
    public class EntityValidatorDependency<TEntity>
        where TEntity : Entity
    {
        private readonly IServiceProvider _serviceProvider;

        public IDbContext DbContext { get; private set; }
        public MultilingualService MultilingualService { get; private set; }


        public EntityValidatorDependency(IDbContext dbContext , IServiceProvider serviceProvider, MultilingualService multilingualService)
        {
            DbContext = dbContext;
            _serviceProvider = serviceProvider;
            MultilingualService = multilingualService;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }

    [ScopedDependency]
    public abstract class EntityValidator<TEntity> : AbstractValidator<TEntity>//, ITypedScopedDependency<EntityValidator<TEntity>>
        where TEntity : Entity
    {
        public EntityValidatorDependency<TEntity> Dependency { get; private set; }

        public MultilingualService MultilingualService => Dependency.MultilingualService;

        public EntityValidator(EntityValidatorDependency<TEntity> dependency)
        {
            Dependency = dependency;
        }

        //TODO ctor mit zugriff auf EntityService
    }
}
