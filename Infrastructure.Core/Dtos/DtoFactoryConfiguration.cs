using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

public interface IDtoFactoryProfile
{
    void Apply(DtoFactoryConfiguration config);
}

public interface ITypeMap
{
    Type SourceType { get; }
    Type TargetType { get; }
    bool ShouldIgnore(PropertyInfo targetProperty);
}

public sealed class DtoFactoryConfiguration
{
    private readonly List<ITypeMap> _maps = [];

    internal IEnumerable<ITypeMap> Maps => _maps;

    public DtoFactoryTypeMap<TSource, TTarget> For<TSource, TTarget>()
        where TSource : IEntity
        where TTarget : IDto
    {
        var existing = _maps
            .OfType<DtoFactoryTypeMap<TSource, TTarget>>()
            .FirstOrDefault();

        if (existing != null)
        {
            return existing;
        }

        var map = new DtoFactoryTypeMap<TSource, TTarget>();
        _maps.Add(map);
        return map;
    }
}

public class DtoFactoryTypeMap<TSource, TTarget> : ITypeMap
{
    public Type SourceType => typeof(TSource);
    public Type TargetType => typeof(TTarget);

    private readonly HashSet<string> _ignoredMembers = new(StringComparer.Ordinal);

    public DtoFactoryTypeMap<TSource, TTarget> ForMember(string memberName, Action<MemberOptions> options)
    {
        var opts = new MemberOptions();
        options(opts);

        if (opts.Ignore)
        {
            _ignoredMembers.Add(memberName);
        }

        return this;
    }

    public DtoFactoryTypeMap<TSource, TTarget> ForMember(Expression<Func<TTarget, object>> memberExpression, Action<MemberOptions> options)
    {
        if (memberExpression == null)
            throw new ArgumentNullException(nameof(memberExpression));

        string memberName = GetMemberName(memberExpression);
        return ForMember(memberName, options);
    }

    private static string GetMemberName(Expression<Func<TTarget, object>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }
        if (body is MemberExpression member && member.Member is PropertyInfo prop)
        {
            return prop.Name;
        }

        throw new ArgumentException("Expression must be a simple property access like 'x => x.Property'.", nameof(expression));
    }

    public bool ShouldIgnore(PropertyInfo targetProperty)
    {
        return !_ignoredMembers.Contains(targetProperty.Name);
    }
}

public class MemberOptions
{
    public bool Ignore { get; private set; }

    public void IgnoreMember() => Ignore = true;
}

public static class DtoFactoryConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddDtoFactoryConfiguration(
        this IServiceCollection services,
        Action<DtoFactoryConfiguration> config = null)
    {
        var dtoFactoryConfiguration = new DtoFactoryConfiguration();

        var configs = AssemblyUtils.GetDerivedConcretClasses<IDtoFactoryProfile>();
        foreach (var configType in configs)
        {
            var instance = (IDtoFactoryProfile)Activator.CreateInstance(configType);
            instance.Apply(dtoFactoryConfiguration);
        }

        if (config != null)
            config(dtoFactoryConfiguration);

        services.AddSingleton(dtoFactoryConfiguration);

        return services;
    }
}