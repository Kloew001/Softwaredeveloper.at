using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate
{
    public interface ISupportValidDate 
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
    }
}
