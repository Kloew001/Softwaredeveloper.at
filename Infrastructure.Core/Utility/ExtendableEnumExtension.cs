using ExtendableEnums;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public abstract class ExtendableEnumExtension<TEnum> : IAppStatupInit
    where TEnum : ExtendableEnum<TEnum>
{
    public virtual Task Init()
    {
        var declaringTypesProperty =
            typeof(TEnum)
            .GetProperty("DeclaringTypes", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);

        var declaringTypes =
            declaringTypesProperty.GetValue(null)
            .As<IList<Type>>();

        declaringTypes.Add(this.GetType());

        return Task.CompletedTask;
    }
}
