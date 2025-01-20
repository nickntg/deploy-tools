using System;

namespace DeployTools.Web.Helpers
{
    public static class TimeExtensions
    {
        public static string HumanReadableTimeDifference(this DateTimeOffset start, DateTimeOffset end)
        {
            var dif = end.Subtract(start);
            if (dif.TotalSeconds < 1)
            {
                return $"{dif.TotalMilliseconds:N2} ms";
            }

            if (dif.TotalMinutes < 1)
            {
                return $"{dif.TotalSeconds:N2} sec";
            }

            return $"{dif.TotalMinutes:N2} min";
        }
    }
}
