namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.DateTimeUtilities;

public interface IDateTimeRange : IRange<DateTime>
{
}

public static class DateRangeExensions
{
    public static bool Includes(this IRange<DateTime> range, DateTime other)
    {
        return range.Start <= other && other <= range.End;
    }

    public static bool Includes(this IRange<DateTime> range, IRange<DateTime> other)
    {
        return range.Start <= other.Start && other.End <= range.End;
    }
}

public class DateRange : IRange<DateTime>
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