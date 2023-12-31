using System;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class EnumExtensionAttribute : Attribute
    {
        public string Id { get; set; }
        public int SortOrder { get; set; } = -1;
        public string ShortName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
