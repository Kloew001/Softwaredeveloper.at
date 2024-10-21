namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

[AttributeUsage(AttributeTargets.Class)]
public class DtoFactoryAttribute : Attribute
{
    public bool IgnoreId { get; set; }

    public DtoFactoryAttribute()
    {
        IgnoreId = false;
    }
}

public interface IDto
{
    Guid? Id { get; set; }
}

public class Dto : IDto
{
    [Newtonsoft.Json.JsonProperty("id",  Order = -1)]
    public Guid? Id { get; set; }
}
