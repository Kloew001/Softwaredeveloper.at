using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    public static class ChronologyEntryServiceExtensions
    {
        public static Task<ChronologyEntry> CreateChronologyEntryInternalAsync<TEntity>(this EntityService<TEntity> service, Entity referenceEntity, string multilingualKey, params string[] textArgs)
            where TEntity : Entity
        {
            var chronologyEntryService =
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<ChronologyEntryService>();

            return chronologyEntryService.CreateInternalAsync(referenceEntity, multilingualKey, textArgs);
        }
    }

    public class ChronologyEntryService : EntityService<ChronologyEntry>
    {
        public ChronologyEntryService(EntityServiceDependency<ChronologyEntry> entityServiceDependency)
            : base(entityServiceDependency)
        {
        }

        public Task<ChronologyEntry> CreateInternalAsync<TEntity>(TEntity referenceEntity, string multilingualKey, params string[] textArgs)
            where TEntity : Entity
        {
            return CreateAsync(c =>
            {
                c.SetReference(referenceEntity);

                this.InitMultilingualProperty(c, c => c.Description, multilingualKey, textArgs);

                return ValueTask.CompletedTask;
            });
        }

        public async Task<IEnumerable<ChronologyEntryDto>> GetCollectionAsync(Guid referenceId)
        {
            return await GetCollectionAsync<ChronologyEntryDto>(query =>
                   query.Where(_ => _.ReferenceId == referenceId));
        }
    }
}
