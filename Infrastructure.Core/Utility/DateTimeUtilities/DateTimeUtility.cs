namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class DateTimeUtility
    {
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

        public static bool IsSaOrSoLocalTime(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday ||
                   dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsFeiertag(this DateTime dateTime)
        {
            return FeiertagService.GetInstance(dateTime.Year)
                .IsFeiertag(dateTime);
        }

        public static bool IsSaSoOrFeiertag(this DateTime dateTime)
        {
            return IsSaOrSoLocalTime(dateTime) ||
                   IsFeiertag(dateTime);
        }
    }

    /// <summary>
    /// https://www.feiertage-oesterreich.at
    /// </summary>
    public class FeiertagService
    {
        private static object balanceLock = new();

        private static Dictionary<int, FeiertagService> InstancesPerYear = new();

        public static FeiertagService GetInstance(int year)
        {
            lock (balanceLock)
            {
                if (InstancesPerYear.ContainsKey(year) == false)
                    InstancesPerYear.Add(year, new FeiertagService(year));
            }

            return InstancesPerYear[year];
        }

        public List<Feiertag> Feiertage { get; set; } = new();

        public int Year { get; set; }

        public bool IsFeiertag(DateTime value)
        {
            return Feiertage.Any(_ => _.Datum.Date == value.Date);
        }

        private FeiertagService(int year)
        {
            Year = year;

            Feiertage.Add(new Feiertag(true, new DateTime(year, 1, 1), "Neujahr"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 1, 6), "Heilige Drei Könige"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 5, 1), "Staatsfeiertag"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 8, 15), "Mariä Himmelfahrt"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 10, 26), "Nationalfeiertag"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 11, 1), "Allerheiligen"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 12, 8), "Mariä Empfängnis"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 12, 25), "Weihnachten"));
            Feiertage.Add(new Feiertag(true, new DateTime(year, 12, 26), "Stefanitag"));

            var osterSonntag = GetOsterSonntag();

            Feiertage.Add(new Feiertag(false, osterSonntag, "Ostersonntag"));
            //Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(-3), "Gründonnerstag"));

            //https://www.wko.at/service/arbeitsrecht-sozialrecht/karfreitag-persoenlicher-feiertag.html
            //Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(-2), "Karfreitag"));

            Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(1), "Ostermontag"));

            Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(39), "Christi Himmelfahrt"));
            Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(49), "Pfingstsonntag"));
            Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(50), "Pfingstmontag"));
            Feiertage.Add(new Feiertag(false, osterSonntag.AddDays(60), "Fronleichnam"));
        }

        private DateTime GetOsterSonntag()
        {
            int g, h, c, j, l, i;

            g = Year % 19;
            c = Year / 100;
            h = ((c - (c / 4)) - (((8 * c) + 13) / 25) + (19 * g) + 15) % 30;
            i = h - (h / 28) * (1 - (29 / (h + 1)) * ((21 - g) / 11));
            j = (Year + (Year / 4) + i + 2 - c + (c / 4)) % 7;

            l = i - j;
            int month = (int)(3 + ((l + 40) / 44));
            int day = (int)(l + 28 - 31 * (month / 4));

            return new DateTime(Year, month, day);
        }

        public class Feiertag : IComparable<Feiertag>
        {
            public string Name { get; set; }
            public DateTime Datum { get; set; }
            public bool IsFix { get; set; }


            public Feiertag(bool isFix, DateTime datum, string name)
            {
                IsFix = isFix;
                Datum = datum;
                Name = name;
            }

            public int CompareTo(Feiertag other)
            {
                return this.Datum.Date.CompareTo(other.Datum.Date);
            }
        }
    }

}
