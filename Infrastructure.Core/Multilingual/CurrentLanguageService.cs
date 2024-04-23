using Microsoft.Extensions.DependencyInjection;

using System;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface ICurrentLanguageService
    {
        MultilingualCultureDto CurrentCulture { get; }

        void Init();
    }

    public class CurrentLanguageService : ICurrentLanguageService, ITypedScopedDependency<ICurrentLanguageService>
    {
        public MultilingualCultureDto CurrentCulture
        {
            get
            {
                if (_currentCulture.IsNull())
                {
                    Init();
                }

                return _currentCulture;
            }
            private set
            {
                _currentCulture = value;
            }
        }
        private MultilingualCultureDto _currentCulture;

        private readonly IDefaultLanguageService _defaultLanguageService;
        private readonly IServiceProvider _serviceProvider;

        public CurrentLanguageService(
            IDefaultLanguageService defaultLanguageService,
            IServiceProvider serviceProvider)
        {
            _defaultLanguageService = defaultLanguageService;
            _serviceProvider = serviceProvider;
        }

        public void Init()
        {
            CurrentCulture = _defaultLanguageService.Culture;

            var currentUserService = _serviceProvider.GetService<ICurrentUserService>();
            //TODO GetLanguage from _currentUserService
        }
    }
}
