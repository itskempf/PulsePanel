
using System;

namespace PulsePanel.Core.Services
{
    internal static class CronHelper
    {
        // Supports: * , */n , single number, comma lists. 5 fields: m h dom mon dow
        public static bool IsMatch(DateTime dt, string cron)
        {
            var parts = cron.Split(new[] {' ', '	'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return false;
            int[] values = { dt.Minute, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek };
            for (int i = 0; i < 5; i++)
                if (!FieldMatches(values[i], parts[i])) return false;
            return true;
        }
        private static bool FieldMatches(int value, string field)
        {
            if (field == "*") return true;
            if (field.StartsWith("*/") && int.TryParse(field[2..], out var n) && n>0)
                return value % n == 0;
            foreach (var part in field.Split(','))
                if (int.TryParse(part, out var v) && v == value) return true;
            return false;
        }
    }
}
