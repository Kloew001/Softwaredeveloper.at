using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

[ScopedDependency]
public class SectionManager
{
    private readonly IServiceProvider _serviceProvider;

    public SectionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<SectionScope> CreateSectionScopes(IEnumerable<Type> sectionTypes, bool isActive = true)
    {
        var sectionScopes = new List<SectionScope>();

        foreach (var type in sectionTypes)
        {
            var sectionScope =
            GetType()
                .GetMethod(nameof(CreateSectionScope))
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { isActive })
                .As<SectionScope>();

            sectionScopes.Add(sectionScope);
        }

        return sectionScopes;
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

    public IEnumerable<Type> GetAllActiveSectionTypes()
    {
        return GetAllActiveSections()
            .Select(_ => _.GetType());
    }

    public IEnumerable<ISection> GetAllActiveSections()
    {
        var allSections = _serviceProvider.GetServices<ISection>();

        return allSections
            .Where(_ => _.IsActive);
    }
}

public interface ISection
{
    public SectionScope CurrentScope { get; }
    public bool IsActive { get; }
}

[ScopedDependency<ISection>]
public abstract class Section : ISection
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
