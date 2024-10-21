namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.StateHolidays;

public class StateHolidaysHelper
{
    private static object balanceLock = new();

    private static Dictionary<int, StateHolidaysHelper> InstancesPerYear = new();

    public static StateHolidaysHelper GetInstance(int year)
    {
        lock (balanceLock)
        {
            if (InstancesPerYear.ContainsKey(year) == false)
                InstancesPerYear.Add(year, new StateHolidaysHelper(year));
        }

        return InstancesPerYear[year];
    }

    public List<StateHoliday> StateHolidays { get; set; } = new();

    public int Year { get; set; }

    public bool IsStateHoliday(DateTime value)
    {
        return StateHolidays.Any(_ => _.Datum.Date == value.Date);
    }

    private StateHolidaysHelper(int year)
    {
        Year = year;

        //https://www.wien.gv.at/amtshelfer/feiertage/
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 1, 1), "Neujahr"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 1, 6), "Heilige Drei Könige"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 5, 1), "Staatsfeiertag"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 8, 15), "Mariä Himmelfahrt"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 10, 26), "Nationalfeiertag"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 11, 1), "Allerheiligen"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 12, 8), "Mariä Empfängnis"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 12, 25), "Weihnachten"));
        StateHolidays.Add(new StateHoliday(true, new DateTime(year, 12, 26), "Stefanitag"));

        var osterSonntag = GetOsterSonntag(Year);

        StateHolidays.Add(new StateHoliday(false, osterSonntag, "Ostersonntag"));
        //StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(-3), "Gründonnerstag"));

        //https://www.wko.at/service/arbeitsrecht-sozialrecht/karfreitag-persoenlicher-feiertag.html
        //StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(-2), "Karfreitag"));

        StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(1), "Ostermontag"));

        StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(39), "Christi Himmelfahrt"));
        StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(49), "Pfingstsonntag"));
        StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(50), "Pfingstmontag"));
        StateHolidays.Add(new StateHoliday(false, osterSonntag.AddDays(60), "Fronleichnam"));

        StateHolidays = StateHolidays.OrderBy(_ => _.Datum).ToList();
    }

    public static DateTime GetOsterSonntag(int year)
    {
        int g, h, c, j, l, i;

        g = year % 19;
        c = year / 100;
        h = ((c - (c / 4)) - (((8 * c) + 13) / 25) + (19 * g) + 15) % 30;
        i = h - (h / 28) * (1 - (29 / (h + 1)) * ((21 - g) / 11));
        j = (year + (year / 4) + i + 2 - c + (c / 4)) % 7;

        l = i - j;
        int month = (int)(3 + ((l + 40) / 44));
        int day = (int)(l + 28 - 31 * (month / 4));

        return new DateTime(year, month, day);
    }

    public class StateHoliday : IComparable<StateHoliday>
    {
        public string Name { get; set; }
        public DateTime Datum { get; set; }
        public bool IsFix { get; set; }

        public StateHoliday(bool isFix, DateTime datum, string name)
        {
            IsFix = isFix;
            Datum = datum;
            Name = name;
        }

        public int CompareTo(StateHoliday other)
        {
            return this.Datum.Date.CompareTo(other.Datum.Date);
        }

        public override string ToString()
        {
            return $"{Name}: {Datum}";
        }
    }
}
