using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    public static class ChronologyEntryServiceExtensions
    {
        public static Task<ChronologyEntry> CreateChronologyEntryAsync<TEntity>(this EntityService<TEntity> service, TEntity referenceEntity, string multilingualKey, params string[] textArgs)
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
            return CreateInternalAsync(c =>
            {
                c.SetReference(referenceEntity);

                this.InitMultilingualProperty(c, c => c.Description, multilingualKey, textArgs);

                return Task.CompletedTask;
            });
        }

        protected override IQueryable<ChronologyEntry> IncludeAutoQueryProperties(IQueryable<ChronologyEntry> query)
        {
            return query
                .Include(_ => _.Translations)
                .Include(_ => _.CreatedBy);
        }

        public async Task<IEnumerable<ChronologyEntryDto>> GetCollectionAsync(Guid referenceId)
        {
            var query = await GetCollectionQueryInternal(q =>
                   q.Where(_ => _.ReferenceId == referenceId));

            return await GetCollectionAsync<ChronologyEntryDto>(query);
        }
    }
}
