using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Activateable;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

public class MultilingualCultureDto : Dto
{
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string Name { get; set; }
}

public class MultilingualGlobalTextDto : Dto
{
    public int Index { get; set; }

    public MultilingualProtectionLevel ViewLevel { get; set; }
    public MultilingualProtectionLevel EditLevel { get; set; }

    public string Key { get; set; }

    public string Text { get; set; }
}

[SingletonDependency]
public class MultilingualGlobalTextCacheService : IAppStatupInit
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MultilingualGlobalTextCacheService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ResetCache()
    {
        await Init();
    }

    private IEnumerable<MultilingualCultureDto> _cacheCulture;

    private IDictionary<Guid, IEnumerable<MultilingualGlobalTextDto>> _cache;

    public IEnumerable<MultilingualCultureDto> Cultures => _cacheCulture;

    public async Task Init()
    {
        var cache = new Dictionary<Guid, IEnumerable<MultilingualGlobalTextDto>>();

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

            var cultures = await context.Set<MultilingualCulture>()
                .IsActive()
                .ToListAsync();

            var cultureDtos = cultures.ConvertToDtos<MultilingualCultureDto>();

            _cacheCulture = cultureDtos;

            foreach (var cultureDto in cultureDtos)
            {
                var texts = await context.Set<MultilingualGlobalText>()
                    .Where(_ => _.CultureId == cultureDto.Id)
                    .OrderBy(_ => _.ViewLevel)
                    .ThenBy(_ => _.Index)
                    .ToListAsync();

                var textDtos = texts.ConvertToDtos<MultilingualGlobalTextDto>();

                cache.Add(cultureDto.Id.Value, textDtos);
            }
        }

        _cache = cache;
    }

    public IEnumerable<MultilingualGlobalTextDto> GetTexts(string cultureName, MultilingualProtectionLevel protectionLevel = MultilingualProtectionLevel.Public)
    {
        var cultureId = _cacheCulture.Single(_ => _.Name == cultureName).Id.Value;

        return GetTexts(cultureId, protectionLevel);
    }

    public IEnumerable<MultilingualGlobalTextDto> GetTexts(Guid cultureId, MultilingualProtectionLevel protectionLevel = MultilingualProtectionLevel.Public)
    {
        return _cache[cultureId]
            .Where(_ => _.ViewLevel == protectionLevel);
    }

    public string GetText(string key, Guid cultureId)
    {
        var text = _cache[cultureId]
                .SingleOrDefault(_ => _.Key == key)?
                .Text;

        return text;
    }
}