namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex
{
    public interface ISupportIndex
    {
        int Index { get; set; }
    }
    public static class ISupportIndexExtensions
    {
        public static IEnumerable<T> OrderByIndex<T>(this IEnumerable<T> query)
            where T : ISupportIndex
        {
            return query.OrderBy(_ => _.Index);
        }

        public static IQueryable<T> OrderByIndex<T>(this IQueryable<T> query)
            where T : ISupportIndex
        {
            return query.OrderBy(_ => _.Index);
        }

    }
}
