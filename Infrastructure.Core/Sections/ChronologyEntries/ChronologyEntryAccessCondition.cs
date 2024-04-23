using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    public class ChronologyEntryAccessCondition : IReferencedToEntityAccessCondition<ChronologyEntry>
    {
        public ChronologyEntryAccessCondition(AccessService accessService, IDbContext dbContext) : base(accessService, dbContext)
        {
        }

        public override Task<bool> CanCreateAsync(ChronologyEntry entity)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> CanUpdateAsync(ChronologyEntry entity)
        {
            return Task.FromResult(false);
        }

        public override Task<bool> CanDeleteAsync(ChronologyEntry entity)
        {
            return Task.FromResult(false);
        }
    }
}
