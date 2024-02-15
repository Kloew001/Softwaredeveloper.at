namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IEnableable
    {
        bool IsEnabled { get; set; }
    }
    public interface ISupportDefault
    {
        bool IsDefault { get; set; }
    }
}
