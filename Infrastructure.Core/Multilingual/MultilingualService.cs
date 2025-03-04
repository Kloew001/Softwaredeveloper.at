﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Linq.Expressions;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

public static class MultilingualServiceExtensions
{
    public static string GetText<TEntity>(this EntityService<TEntity> service, string key, Guid? cultureId = null)
        where TEntity : Entity
    {
        var multilingualService =
        service.EntityServiceDependency.ServiceProvider
            .GetRequiredService<MultilingualService>();

        return multilingualService.GetText(key, cultureId);
    }

    public static Task InitMultilingualProperty<TEntity, TTranslation>(this EntityService<TEntity> service, IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string multilingualKey, params string[] textArgs)
        where TEntity : Entity
        where TTranslation : class, IEntityTranslation
    {
        var multilingualService =
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<MultilingualService>();

        return multilingualService.InitMultilingualProperty(entity, property, multilingualKey, textArgs);
    }
    public static Task InitMultilingualProperty<TEntity, TTranslation>(this EntityService<TEntity> service, IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string text)
        where TEntity : Entity
        where TTranslation : class, IEntityTranslation
    {
        var multilingualService =
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<MultilingualService>();

        return multilingualService.InitMultilingualProperty(entity, property, text);
    }
    public static Task SetMultilingualProperty<TEntity, TTranslation>(this EntityService<TEntity> service, IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string text, Guid? cultureId = null)
        where TEntity : Entity
        where TTranslation : class, IEntityTranslation
    {
        var multilingualService =
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<MultilingualService>();

        return multilingualService.SetMultilingualProperty(entity, property, text, cultureId);
    }
}

[ScopedDependency]
public class MultilingualService
{
    private readonly MultilingualGlobalTextCacheService _multilingualGlobalTextCacheService;
    private readonly ICurrentLanguageService _currentLanguageService;
    private readonly IDefaultLanguageService _defaultLanguageService;
    private readonly ILogger<MultilingualService> _logger;

    private readonly IDbContext _context;

    public MultilingualService(
        MultilingualGlobalTextCacheService multilingualGlobalTextCacheService,
        ICurrentLanguageService currentLanguageService,
        IDefaultLanguageService defaultLanguageService,
        ILogger<MultilingualService> logger,
        IDbContext context)
    {
        _multilingualGlobalTextCacheService = multilingualGlobalTextCacheService;
        _currentLanguageService = currentLanguageService;
        _defaultLanguageService = defaultLanguageService;
        _logger = logger;
        _context = context;
    }

    public string GetText(string key, Guid? cultureId = null)
    {
        if (cultureId.HasValue.IsFalse())
            cultureId = _currentLanguageService.CurrentCultureId;

        var text = _multilingualGlobalTextCacheService.GetText(key, cultureId.Value);

        if (text == null &&
           cultureId.Value != _defaultLanguageService.CultureId)
        {
            text = _multilingualGlobalTextCacheService.GetText(key, _defaultLanguageService.CultureId);
        }

        if (text == null)
            _logger.LogWarning("Translation not found for key {0} and culture {1}", key, cultureId);

        return text;
    }

    public string GetText<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, Guid? cultureId = null)
        where TTranslation : IEntityTranslation
    {
        if (cultureId.HasValue.IsFalse())
            cultureId = _currentLanguageService.CurrentCultureId;

        if (entity.Translations == null)
            return null;

        var translation = entity.Translations.SingleOrDefault(_ => _.CultureId == cultureId);

        if (translation == null &&
            cultureId.Value != _defaultLanguageService.CultureId)
        {
            translation = entity.Translations.SingleOrDefault(_ => _.CultureId == _defaultLanguageService.CultureId);
        }

        if (translation == null)
        {
            _logger.LogWarning("Translation not found for entity {0} and culture {1}", entity.Id, cultureId);
            return null;
        }

        var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

        if (propertyInfo == null)
            throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

        var text = (string)propertyInfo.GetValue(translation);

        return text;
    }

    public async Task InitMultilingualProperty<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string multilingualKey, params string[] textargs)
        where TTranslation : class, IEntityTranslation
    {
        var culturesIds = GetAllCultureIds();

        foreach (var culturesId in culturesIds)
        {
            var text = string.Format(GetText(multilingualKey, culturesId), textargs);

            await SetMultilingualProperty(entity, property, text, culturesId);
        }
    }

    public async Task InitMultilingualProperty<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string text)
        where TTranslation : class, IEntityTranslation
    {
        var culturesIds = GetAllCultureIds();

        foreach (var culturesId in culturesIds)
        {
            await SetMultilingualProperty(entity, property, text, culturesId);
        }
    }

    private IEnumerable<Guid> GetAllCultureIds()
    {
        return _multilingualGlobalTextCacheService.Cultures.Select(_ => _.Id.Value);
    }

    public async Task SetMultilingualProperty<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string text, Guid? cultureId = null)
    where TTranslation : class, IEntityTranslation
    {
        if (cultureId.HasValue.IsFalse())
            cultureId = _currentLanguageService.CurrentCultureId;

        var translation = entity.Translations.SingleOrDefault(_ => _.CultureId == cultureId);

        if (translation == null)
        {
            translation = await _context.CreateEntityAync<TTranslation>();
            translation.CoreId = entity.Id;
            translation.Core = entity;
            entity.Translations.Add(translation);
            translation.CultureId = cultureId.Value;
        }

        var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
        if (propertyInfo == null)
            throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

        propertyInfo.SetValue(translation, text);
    }
}
