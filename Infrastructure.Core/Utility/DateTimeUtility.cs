using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class DateTimeUtility
    {
        public static DateTime LastDayOfMonth(int year, int month)
        {
            if (month == 12)
            {
                return new DateTime(year + 1, 1, 1).AddDays(-1);
            }

            return new DateTime(year, month + 1, 1).AddDays(-1);
        }
    }

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

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool Includes(IDateRange<DateTime> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }
    }
}
