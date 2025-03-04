﻿using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

public interface IMultiLingualEntity : IEntity
{
    IEnumerable<IEntityTranslation> Translations { get; }
}

public interface IMultiLingualEntity<TTranslation> : IMultiLingualEntity
    where TTranslation : IEntityTranslation
{
    new ICollection<TTranslation> Translations { get; set; }
    
    IEnumerable<IEntityTranslation> IMultiLingualEntity.Translations => Translations.OfType<IEntityTranslation>();
}

public interface IEntityTranslation
{
    Guid CoreId { get; set; }
    IEntity Core { get; set; }

    Guid CultureId { get; set; }
    MultilingualCulture Culture { get; set; }
}

public interface IEntityTranslation<TEntity> : IEntityTranslation
    where TEntity : IEntity
{
    new TEntity Core { get; set; }
    
    IEntity IEntityTranslation.Core
    {
        get => Core;
        set => Core = (TEntity)value;
    }
}

[Index(nameof(CoreId))]
[Index(nameof(CultureId))]
public abstract class EntityTranslation<TEntity> : Entity, IEntityTranslation<TEntity>
    where TEntity : Entity
{
    public Guid CoreId { get; set; }
    public virtual TEntity Core { get; set; }

    public Guid CultureId { get; set; }
    public virtual MultilingualCulture Culture { get; set; }
}

public static class EntityTranslationExtensions
{
    //Get the translation for a specific culture or Default

    public static TTranslation GetCultureTranslationOrDefault<TTranslation>(this IMultiLingualEntity<TTranslation> entity, Guid? cultureId)
        where TTranslation : IEntityTranslation
    {
        if (cultureId != null)
        {
            var translation = entity.Translations.FirstOrDefault(t => t.CultureId == cultureId);

            if (translation != null)
                return translation;
        }

        return entity.Translations.Where(_ => _.Culture.IsDefault).SingleOrDefault();
    }
}
