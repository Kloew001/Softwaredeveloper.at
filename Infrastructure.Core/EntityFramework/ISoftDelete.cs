namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
