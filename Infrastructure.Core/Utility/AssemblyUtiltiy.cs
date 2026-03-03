using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class AssemblyUtils
{
    private static readonly Lazy<Type[]> _allLoadedTypes = new(() =>
        [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.AllLoadableTypes())
            .DistinctBy(type => type.FullName)
            .OrderBy(type => type.FullName)],
        isThreadSafe: true);
      
    public static Type[] AllLoadedTypes() => _allLoadedTypes.Value;

    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(this Assembly assembly)
        where TAttribute : Attribute
    {
        return assembly.GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(TAttribute), true).Length > 0);
    }

    public static Type[] GetDerivedConcretClasses<TInterface>()
    {
        return GetDerivedConcretClasses(typeof(TInterface));
    }

    public static Type[] GetDerivedConcretClasses(Type type)
    {
        var types = GetAllConcretClasses();

        if (!type.IsGenericTypeDefinition)
        {
            return [.. types.Where(type.IsAssignableFrom)];
        }
        else
        {
            return [.. types.Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == type))];
        }
    }

    public static Type[] GetAllConcretClasses()
    {
        return [.. AllLoadedTypes().Where(t => t is { IsClass: true, IsAbstract: false })];
    }

    public static IEnumerable<Type> AllLoadableTypes(this Assembly assembly)
    {
        var types = new List<Type>();

        try
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                types.Add(type);
            }
        }
        catch (Exception)
        {
            // log
        }

        return types;
    }
}