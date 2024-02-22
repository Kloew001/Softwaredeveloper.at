//using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
//using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
//using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage;

//namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EmailMessaga
//{
//    public class EmailMessageService : IScopedService
//    {
//        public EmailMessageService(IDbContext context)
//        {
//        }

//        public Task<IEnumerable<EmailMessage>> GetCollectionAsync(Guid referenceId)
//        {
//            //TODO Security
//            return GetCollectionAsync<ChronologyEntryDto>(q =>
//                   q.Where(_ => _.ReferenceId == referenceId));
//        }
//    }
//}
