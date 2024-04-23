using Microsoft.Extensions.DependencyInjection;

using System.Linq.Expressions;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
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
    }

    public class MultilingualService : IScopedDependency
    {
        private readonly MultilingualGlobalTextCacheService _multilingualGlobalTextCacheService;
        private readonly ICurrentLanguageService _currentLanguageService;
        private readonly IDbContext _context;

        public MultilingualService(
            MultilingualGlobalTextCacheService multilingualGlobalTextCacheService,
            ICurrentLanguageService currentLanguageService,
            IDbContext context)
        {
            _multilingualGlobalTextCacheService = multilingualGlobalTextCacheService;
            _currentLanguageService = currentLanguageService;
            _context = context;
        }

        public string GetText(string key, Guid? cultureId = null)
        {
            if (cultureId.HasValue.IsFalse())
                cultureId = _currentLanguageService.CurrentCulture.Id.Value;

            return _multilingualGlobalTextCacheService.GetText(key, cultureId.Value);
        }

        public string GetText<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, Guid? cultureId = null)
            where TTranslation : IEntityTranslation
        {
            if (cultureId.HasValue.IsFalse())
                cultureId = _currentLanguageService.CurrentCulture.Id.Value;

            if (entity.Translations == null)
                return default;

            var translation = entity.Translations.SingleOrDefault(_ => _.CultureId == cultureId);

            if (translation == null)
                return default;

            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            var text = (string)propertyInfo.GetValue(translation);

            return text;
        }

        public async Task InitMultilingualProperty<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string multilingualKey, params string[] textargs)
            where TTranslation : class, IEntityTranslation
        {
            var culturesIds = _multilingualGlobalTextCacheService.Cultures.Select(_ => _.Id.Value);

            foreach (var culturesId in culturesIds)
            {
                var text = string.Format(GetText(multilingualKey, culturesId), textargs);

                await SetMultilingualProperty(entity, property, text, culturesId);
            }
        }


        public async Task SetMultilingualProperty<TTranslation>(IMultiLingualEntity<TTranslation> entity, Expression<Func<TTranslation, string>> property, string text, Guid? cultureId = null)
        where TTranslation : class, IEntityTranslation
        {
            if (cultureId.HasValue.IsFalse())
                cultureId = _currentLanguageService.CurrentCulture.Id.Value;

            var translation = entity.Translations.SingleOrDefault(_ => _.CultureId == cultureId);

            if (translation == null)
            {
                translation = await _context.CreateEntity<TTranslation>();
                translation.CoreId = entity.Id;
                translation.Core = entity;
                translation.CultureId = cultureId.Value;
            }

            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            propertyInfo.SetValue(translation, text);
        }
    }
}
