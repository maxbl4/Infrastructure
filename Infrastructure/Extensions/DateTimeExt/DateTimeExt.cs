using System;
using System.Globalization;

namespace maxbl4.Infrastructure.Extensions.DateTimeExt
{
    public static class DateTimeExt
    {
        public static DateTime ParseAsUtc(string value, string format)
        {
            var t = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            return new DateTime(t.Ticks, DateTimeKind.Utc);
        }

        public static bool TryParseAsUtc(string value, string format, DateTimeStyles styles, out DateTime time)
        {
            var r = DateTime.TryParseExact(value,
                format, CultureInfo.InvariantCulture,
                styles, out var t);
            time = new DateTime(t.Ticks, DateTimeKind.Utc);
            return r;
        }
        
        public static DateTime TakeSmaller(this DateTime current, DateTime other)
        {
            if (current == default)
                return other;
            if (other == default)
                return current;
            return current < other ? current : other;
        }
        
        public static DateTime TakeLarger(this DateTime current, DateTime other)
        {
            if (current == default)
                return other;
            if (other == default)
                return current;
            return current > other ? current : other;
        }
    }
}