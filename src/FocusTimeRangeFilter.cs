using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Filters FocusSession data by time range.
    /// </summary>
    public static class FocusTimeRangeFilter
    {
        public enum TimeRange
        {
            Last7Days,
            Last30Days,
            Last6Months,
            Last1Year
        }

        /// <summary>
        /// Gets the number of days for the given time range.
        /// </summary>
        public static int GetDayCount(TimeRange range) => range switch
        {
            TimeRange.Last7Days => 7,
            TimeRange.Last30Days => 30,
            TimeRange.Last6Months => 180,
            TimeRange.Last1Year => 365,
            _ => 7
        };

        /// <summary>
        /// Gets a display name for the time range.
        /// </summary>
        public static string GetDisplayName(TimeRange range) => range switch
        {
            TimeRange.Last7Days => "Last 7 Days",
            TimeRange.Last30Days => "Last 30 Days",
            TimeRange.Last6Months => "Last 6 Months",
            TimeRange.Last1Year => "Last 1 Year",
            _ => "Last 7 Days"
        };

        /// <summary>
        /// Filters focus sessions by time range.
        /// </summary>
        public static List<FocusSession> FilterByTimeRange(List<FocusSession> sessions, TimeRange range)
        {
            if (sessions == null || sessions.Count == 0)
                return new List<FocusSession>();

            int days = GetDayCount(range);
            DateTime cutoff = DateTime.Now.AddDays(-days);

            return sessions
                .Where(s => s.EndTime >= cutoff)
                .ToList();
        }

        /// <summary>
        /// Gets all time range options as a list of tuples (enum value, display name).
        /// </summary>
        public static List<(TimeRange Range, string DisplayName)> GetAllOptions()
        {
            return new List<(TimeRange, string)>
            {
                (TimeRange.Last7Days, GetDisplayName(TimeRange.Last7Days)),
                (TimeRange.Last30Days, GetDisplayName(TimeRange.Last30Days)),
                (TimeRange.Last6Months, GetDisplayName(TimeRange.Last6Months)),
                (TimeRange.Last1Year, GetDisplayName(TimeRange.Last1Year))
            };
        }
    }
}
