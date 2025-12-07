namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SecurityParentAttribute : Attribute
{
    public string PropertyName { get; set; }

    public SecurityParentAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}