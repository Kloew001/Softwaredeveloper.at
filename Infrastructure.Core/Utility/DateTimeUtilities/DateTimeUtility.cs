using System;

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
}
