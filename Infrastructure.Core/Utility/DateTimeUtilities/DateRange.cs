namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public interface IDateRange : IDateRange<DateTime>
    {
    }
    public interface INullableDateRange : IDateRange<DateTime>
    {
    }

    public interface IDateRange<T>
    {
        T Start { get; }
        T End { get; }
    }

    public static class DateRangeExensions
    {
        public static bool Includes(this IDateRange<DateTime> range, DateTime other)
        {
            return range.Start <= other && other <= range.End;
        }

        public static bool Includes(this IDateRange<DateTime> range, IDateRange<DateTime> other)
        {
            return range.Start <= other.Start && other.End <= range.End;
        }
    }

    public class DateRange : IDateRange<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public override string ToString()
        {
            return $"{Start} - {End}";
        }

    }
}
