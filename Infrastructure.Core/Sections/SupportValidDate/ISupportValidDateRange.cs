using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate
{
    public interface ISupportValidDateRange
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
    }
}
