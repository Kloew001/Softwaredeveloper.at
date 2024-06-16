using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.DemoData;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

namespace SampleApp.Application.Sections.PersonSection
{
    public class PersonService : EntityService<Person>
    {
        public PersonService(EntityServiceDependency<Person> entityServiceDependency) 
            : base(entityServiceDependency)
        {
        }

        protected override async Task OnCreateInternalAsync(Person person)
        {
            await base.OnCreateInternalAsync(person);

            if (person.FirstName.IsNullOrEmpty())
                person.FirstName = DemoDataHelper.FirstNames.GetRandom();

            _cacheService.DistributedCache.Remove(_getAllAsyncCacheKey);
        }

        override protected async Task OnUpdateInternalAsync(Person person)
        {
            await base.OnUpdateInternalAsync(person);

            _cacheService.DistributedCache.Remove(_getAllAsyncCacheKey);
        }

        override protected async Task OnDeleteInternalAsync(Person person)
        {
            await base.OnDeleteInternalAsync(person);

            _cacheService.DistributedCache.Remove(_getAllAsyncCacheKey);
        }

        public const string _getAllAsyncCacheKey = $"{nameof(PersonService)}_{nameof(GetAllAsync)}";
        public async Task<IEnumerable<PersonDto>> GetAllAsync()
        {
            return await _cacheService.DistributedCache.GetOrCreateAsync(_getAllAsyncCacheKey, (c) =>
            {
                c.SlidingExpiration = TimeSpan.FromHours(10);

                return GetCollectionAsync<PersonDto>();
            });
        }

        public class PersonOverviewFilter : PageFilter
        {
            public string SearchText { get; set; }
        }

        public async Task<PageResult<PersonDto>> GetOverviewAsync(PersonOverviewFilter filter = null)
        {
            var query = await GetQueryAsync(q =>
            {
                if (filter.SearchText.IsNotNullOrEmpty())
                    return q.Where(_=>_.FirstName.Contains(filter.SearchText));

                return q;
            });

            var dtos = await base.GetPagedCollectionAsync<PersonDto>(query, filter);

            return dtos;
        }
    }
}
