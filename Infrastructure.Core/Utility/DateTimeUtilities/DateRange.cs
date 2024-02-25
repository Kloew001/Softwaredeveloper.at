namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public interface IDateRange<T>
    {
        T Start { get; }
        T End { get; }
        bool Includes(T value);
        bool Includes(IDateRange<T> range);
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

        public bool Includes(DateTime value)
        {
            return Start <= value && value <= End;
        }

        public bool Includes(IDateRange<DateTime> range)
        {
            return Start <= range.Start && range.End <= End;
        }
    }
}
