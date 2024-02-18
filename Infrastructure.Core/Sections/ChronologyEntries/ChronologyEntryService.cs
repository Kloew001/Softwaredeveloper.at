﻿using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    public class ChronologyEntryService : EntityService<ChronologyEntry>
    {
        public ChronologyEntryService(
            IDbContext context,
            AccessService accessService,
            SectionManager sectionManager)
            : base(context, accessService, sectionManager)
        {
        }

        public Task<IEnumerable<ChronologyEntryDto>> GetCollectionAsync(Guid referenceId)
        {
            //TODO Security
            return GetCollectionAsync<ChronologyEntryDto>(q =>
                   q.Where(_ => _.ReferenceId == referenceId));
        }
    }


    public class ChronologyEntryAccessCondition : AllAccessCondition<ChronologyEntry>
    {
        //TODO Security
    }
}
