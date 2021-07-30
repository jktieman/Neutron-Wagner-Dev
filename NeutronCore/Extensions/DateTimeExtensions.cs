using System;

namespace NeutronCore.Extensions
{
    public static partial class DateTimeExtensions
    {
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
                diff += 7;
            var date = dt.AddDays(-diff).Date;
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            var date = dt.FirstDayOfWeek().AddDays(6);
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
        }

        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            var date = dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public static DateTime FirstDayOfNextMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1);
        }
    }
}
