namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries;

public class ChronologyEntryAccessCondition : IReferencedToEntityAccessCondition<ChronologyEntry>
{
    public ChronologyEntryAccessCondition(AccessService accessService, IDbContext dbContext) : base(accessService, dbContext)
    {
    }

    public override ValueTask<bool> CanCreateAsync(ChronologyEntry entity)
    {
        return ValueTask.FromResult(true);
    }

    public override ValueTask<bool> CanUpdateAsync(ChronologyEntry entity)
    {
        return ValueTask.FromResult(false);
    }

    public override ValueTask<bool> CanDeleteAsync(ChronologyEntry entity)
    {
        return ValueTask.FromResult(false);
    }
}
