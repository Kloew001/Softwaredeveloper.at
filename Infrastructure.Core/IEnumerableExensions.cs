namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ISupportValidDate
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
    }

    public static class IEnumerableExensions
    {
        public static IEnumerable<T> IsValid<T>(this IEnumerable<T> query, DateTime? date = null)
            where T : ISupportValidDate
        {
            if (date == null)
                date = DateTime.Now;

            return query
                .Where(_ => (_.ValidFrom.HasValue == false || _.ValidFrom <= date) &&
                            (_.ValidTo.HasValue == false || _.ValidTo >= date));
        }
    }
}
