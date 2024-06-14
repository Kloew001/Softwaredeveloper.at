﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface IDefaultLanguageService : IAppStatupInit, ITypedSingletonDependency<IAppStatupInit>
    {
        Guid CultureId => Culture.Id.Value;
        MultilingualCultureDto Culture { get; }
    }

    public class DefaultLanguageService : IDefaultLanguageService, ITypedSingletonDependency<IDefaultLanguageService>
    {
        public MultilingualCultureDto Culture { get; private set; }


        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DefaultLanguageService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Init()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

                var culture = await context.Set<MultilingualCulture>()
                    .Where(_ => _.IsDefault)
                    .SingleAsync();

                Culture = culture.ConvertToDto<MultilingualCultureDto>();
            }
        }
    }
}
