using FluentValidation;
using FluentValidation.Resources;

using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    public class EntityValidatorDependency<TEntity> : ITransientDependency
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

    public abstract class EntityValidator<TEntity> : AbstractValidator<TEntity>, IScopedDependency, ITypedScopedDependency<EntityValidator<TEntity>>
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
