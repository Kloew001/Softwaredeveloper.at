using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface IMultiLingualEntity<TTranslation>
        where TTranslation : class, IEntityTranslation
    {
        ICollection<TTranslation> Translations { get; set; }
    }

    public interface IEntityTranslation
    {
        Guid CoreId { get; set; }
        LanguageCulture Culture { get; set; }
    }

    public interface IEntityTranslation<TEntity> : IEntityTranslation
        where TEntity : Entity
    {
        TEntity Core { get; set; }
    }

    public abstract class EntityTranslation<TEntity> : Entity, IEntityTranslation<TEntity>
        where TEntity : Entity
    {
        public Guid CoreId { get; set; }
        public virtual TEntity Core { get; set; }

        public Guid CultureId { get; set; }
        public virtual LanguageCulture Culture { get; set; }
    }

    public class ICurrentLanguageService : IScopedDependency
    {
        LanguageCulture Current { get; set; }
    }

    public class CurrentLanguageService : ICurrentLanguageService
    {
        public LanguageCulture Current { get; set; }
    }

    public class MultilingualService : IScopedDependency
    {

    }
}
