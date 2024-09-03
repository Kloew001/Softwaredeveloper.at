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

        protected override async Task OnCreateAsync(Person person)
        {
            await base.OnCreateAsync(person);

            if (person.FirstName.IsNullOrEmpty())
                person.FirstName = DemoDataHelper.FirstNames.GetRandom();

            _cacheService.DistributedCache.Remove(_getAllAsyncCacheKey);
        }

        override protected async Task OnUpdateAsync(Person person)
        {
            await base.OnUpdateAsync(person);

            _cacheService.DistributedCache.Remove(_getAllAsyncCacheKey);
        }

        override protected async Task OnDeleteAsync(Person person)
        {
            await base.OnDeleteAsync(person);

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
            var dtos = await base.GetPagedCollectionAsync<PersonDto>(filter, query =>
            {
                if (filter.SearchText.IsNotNullOrEmpty())
                    return query.Where(_ => _.FirstName.Contains(filter.SearchText));

                return query;
            });

            return dtos;
        }
    }
}
