namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos
{
    public interface IDto
    {
        Guid? Id { get; set; }
    }

    public class Dto : IDto
    {
        [Newtonsoft.Json.JsonProperty("id",  Order = -1)]
        public Guid? Id { get; set; }
    }
}
