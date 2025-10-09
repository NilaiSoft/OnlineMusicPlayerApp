using System;
using System.Globalization;

public static class DateTimeExtensions
{
    public static string ToPersianDateTime(this DateTime datetime)
    {
        PersianCalendar pc = new PersianCalendar();

        int year = pc.GetYear(datetime);
        int month = pc.GetMonth(datetime);
        int day = pc.GetDayOfMonth(datetime);

        int hour = pc.GetHour(datetime);
        int minute = pc.GetMinute(datetime);

        return $"{year:0000}/{month:00}/{day:00} - {hour:00}:{minute:00}";
    }
}
