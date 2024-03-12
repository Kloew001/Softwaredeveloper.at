using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.StateHolidays;

using System.Globalization;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class DateTimeUtility
    {
        public static DateTime RandomDate(this DateTime start, DateTime end)
        {
            var gen = new Random();
            int range = (end - start).Days;
            return start.AddDays(gen.Next(range));
        }

        public static int WeekNumber(this DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var dateTimeFormat = culture.DateTimeFormat;
            return calendar.GetWeekOfYear(date, dateTimeFormat.CalendarWeekRule, dateTimeFormat.FirstDayOfWeek);
        }

        public static DateTime LastDayOfMonth(int year, int month)
        {
            DateTime date;

            if (month == 12)
                date = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddDays(-1);
            else 
                date = new DateTime(year, month + 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddDays(-1);

            if (date.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();
            else if (date.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static DateTime DayOfMonth(this DateTime dateTime, int day)
        {
            var date = new DateTime(dateTime.Year, dateTime.Month, day, 0, 0, 0, DateTimeKind.Unspecified);

            if (dateTime.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();
            else if (dateTime.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static IEnumerable<int> MonthsInQuatal(int quatal)
        {
            if (quatal == 1)
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }
            else if (quatal == 2)
            {
                yield return 4;
                yield return 5;
                yield return 6;
            }
            else if (quatal == 3)
            {
                yield return 7;
                yield return 8;
                yield return 9;
            }
            else if (quatal == 4)
            {
                yield return 10;
                yield return 11;
                yield return 12;
            }
            else
                throw new InvalidOperationException();
        }


        public static int FirstMonthInQuatral(int quatal)
        {
            return ((quatal - 1) * 3) + 1;
        }

        public static int DaysInQuatal(int quatal, int year)
        {
            return MonthsInQuatal(quatal).
                Select(q => DateTime.DaysInMonth(year, q))
                .Sum();
        }

        public static DateTime FirstDayOfYear(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            if (dateTime.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();

            else if (dateTime.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static DateTime LastDayOfYear(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

            if (dateTime.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();

            else if (dateTime.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

            if (dateTime.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();
            else if (dateTime.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, dateTime.Month,
                DateTime.DaysInMonth(dateTime.Year, dateTime.Month));

            if (dateTime.Kind == DateTimeKind.Utc)
                date = date.ToUniversalTime();
            else if (dateTime.Kind == DateTimeKind.Local)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);

            return date;
        }

        public static DateTime NextDay(this DateTime dateTime)
        {
            return dateTime.AddDays(1);
        }

        public static DateTime PreviousDay(this DateTime dateTime)
        {
            return dateTime.AddDays(-1);
        }

        public static DateTime NextWerktag(this DateTime dateTime)
        {
            var folgetag = dateTime.NextDay();

            if (folgetag.IsSaSoOrFeiertag() == false)
                return folgetag;

            return NextWerktag(folgetag);
        }

        public static DateTime LastWerktag(this DateTime dateTime)
        {
            var folgetag = dateTime.PreviousDay();

            if (folgetag.IsSaSoOrFeiertag() == false)
                return folgetag;

            return LastWerktag(folgetag);
        }

        public static bool IsSaOrSo(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday ||
                   dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsSaSoOrFeiertag(this DateTime dateTime)
        {
            return IsSaOrSo(dateTime) ||
                   IsStateHoliday(dateTime);
        }

        public static bool IsStateHoliday(this DateTime dateTime)
        {
            return StateHolidaysHelper.GetInstance(dateTime.Year)
                .IsStateHoliday(dateTime);
        }
    }
}
