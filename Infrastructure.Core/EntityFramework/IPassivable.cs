namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IActivateable
    {
        bool IsEnabled { get; set; }
    }

    public interface ISupportDefault
    {
        bool IsDefault { get; set; }
    }
}
