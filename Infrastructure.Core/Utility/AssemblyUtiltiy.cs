using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class AssemblyUtils
{
    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(this Assembly assembly)
        where TAttribute : Attribute
    {
        return assembly.GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(TAttribute), true).Length > 0);
    }

    public static Type[] GetDerivedTypes<TInterface>()
    {
        return GetDerivedTypes(typeof(TInterface));
    }

    public static Type[] GetDerivedTypes(Type type)
    {
        var types = AllLoadedTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false });

        if (!type.IsGenericTypeDefinition)
        {
            types = types.Where(t => type.IsAssignableFrom(t));
        }
        else
        {
            types = types.Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == type));
        }

        return types.ToArray();
    }

    public static Type[] AllLoadedTypes()
    {
        lock (__lockObj)
        {
            if (_allLoadedTypes == null)
            {
                _allLoadedTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.AllLoadableTypes())
                    .DistinctBy(type => type.FullName)
                    .OrderBy(type => type.FullName)
                    .ToArray();
            }

            return _allLoadedTypes;
        }
    }

    private static Type[] _allLoadedTypes = null;
    private static object __lockObj = new object();

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