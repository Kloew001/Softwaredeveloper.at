namespace SoftwaredeveloperDotAt.Infrastructure.Core.Dtos
{
    public interface IDto
    {
        Guid? Id { get; set; }
    }

    public class DtoBase : IDto
    {
        public Guid? Id { get; set; }
    }
}
