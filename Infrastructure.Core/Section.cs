using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class SectionManager : IScopedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SectionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public SectionScope CreateSectionScope<T>(bool isActive = true) where T : Section
        {
            var section = _serviceProvider.GetService<T>();

            return section.CreateScope(isActive);
        }

        public bool IsActive<T>() 
            where T : Section
        {
            var section = _serviceProvider.GetService<T>();

            return section.IsActive;
        }
    }

    public abstract class Section : IScopedService
    {
        public List<SectionScope> Scopes { get; set; }
        public SectionScope CurrentScope => Scopes?.LastOrDefault(_ => _.IsActive != null);

        public bool IsActive
        {
            get
            {
                if (CurrentScope == null)
                    return false;

                if (CurrentScope.IsActive == true)
                    return true;

                return false;
            }
        }

        public Section()
        {
            Scopes = new List<SectionScope>();
        }

        public virtual SectionScope CreateScope(bool isActive = true)
        {
            OnCreateScope();

            Scopes.Add(new SectionScope(isActive));

            return CurrentScope;
        }

        protected virtual void OnCreateScope()
        {
        }
    }

    public class SectionScope : IDisposable
    {
        public bool? IsActive { get; set; }

        public SectionScope(bool isActive = true)
        {
            IsActive = isActive;
        }

        public void Dispose()
        {
            IsActive = null;
        }
    }
}
