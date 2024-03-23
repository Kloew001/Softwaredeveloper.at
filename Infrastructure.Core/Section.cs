using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class SectionManager : IScopedDependency
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

        public bool IsNotActive<T>()
            where T : Section
        {
            return IsActive<T>() == false;
        }

        public bool IsActive<T>()
            where T : Section
        {
            var section = _serviceProvider.GetService<T>();

            return section.IsActive;
        }

        public IEnumerable<SectionScope> ReactivateSections(SectionManager otherScopedSectionManager)
        {
            var sectionScopes = new List<SectionScope>();

            var sectionTypes = otherScopedSectionManager.GetAllActiveSectionTypes();

            foreach (var type in sectionTypes)
            {
                var sectionScope =
                GetType()
                    .GetMethod(nameof(CreateSectionScope))
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { true })
                    .As<SectionScope>();

                sectionScopes.Add(sectionScope);
            }

            return sectionScopes;
        }

        public IEnumerable<Type> GetAllActiveSectionTypes()
        {
            return GetAllActiveSections()
                .Select(_ => _.GetType());
        }

        public IEnumerable<ISection> GetAllActiveSections()
        {
            var allSections = _serviceProvider.GetServices<ISection>();

            var s = _serviceProvider.GetService(allSections.First().GetType());

            return allSections
                .Where(_ => _.IsActive);
        }
    }

    public interface ISection
    {
        public SectionScope CurrentScope { get; }
        public bool IsActive { get; }
    }

    public abstract class Section : ISection, IScopedDependency, ITypedScopedDependency<ISection>
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
