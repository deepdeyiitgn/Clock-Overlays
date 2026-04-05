using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Calculates hourly focus breakdown from a list of FocusSession objects.
    /// </summary>
    public static class HourlyFocusBreakdown
    {
        /// <summary>
        /// Breaks down focus sessions into hourly buckets for a single day.
        /// </summary>
        /// <param name="sessions">List of focus sessions (must all be from the same day).</param>
        /// <param name="date">The date to filter sessions for (uses Date component only).</param>
        /// <returns>
        /// Dictionary with hours 0-23 as keys and minutes (0-60) as values.
        /// All hours are initialized; missing hours default to 0.
        /// </returns>
        public static Dictionary<int, int> CalculateHourlyBreakdown(List<FocusSession> sessions, DateTime date)
        {
            // Initialize all 24 hours to 0
            var hourlyMinutes = new Dictionary<int, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyMinutes[hour] = 0;
            }

            if (sessions == null || sessions.Count == 0)
            {
                return hourlyMinutes;
            }

            // Filter sessions for the given date and process each
            var targetDate = date.Date;
            foreach (var session in sessions)
            {
                // Skip sessions not on this date
                if (session.StartTime.Date != targetDate && session.EndTime.Date != targetDate)
                {
                    continue;
                }

                // Ensure valid session
                if (session.EndTime <= session.StartTime)
                {
                    continue;
                }

                // Clamp session to the target date boundaries
                var dayStart = targetDate.Date;
                var dayEnd = dayStart.AddDays(1);

                var sessionStart = session.StartTime < dayStart ? dayStart : session.StartTime;
                var sessionEnd = session.EndTime > dayEnd ? dayEnd : session.EndTime;

                // Skip if clamping results in invalid range
                if (sessionEnd <= sessionStart)
                {
                    continue;
                }

                // Distribute session across hours
                int startHour = sessionStart.Hour;
                int endHour = sessionEnd.Hour;

                for (int hour = startHour; hour <= endHour; hour++)
                {
                    // Hour boundaries (e.g., 10:00-11:00 for hour 10)
                    var hourStart = dayStart.AddHours(hour);
                    var hourEnd = hourStart.AddHours(1);

                    // Overlap of session with this hour
                    var overlapStart = sessionStart > hourStart ? sessionStart : hourStart;
                    var overlapEnd = sessionEnd < hourEnd ? sessionEnd : hourEnd;

                    if (overlapEnd > overlapStart)
                    {
                        int minutesInHour = (int)Math.Round((overlapEnd - overlapStart).TotalMinutes);
                        
                        // Enforce 60-minute hour cap (paranoia check)
                        if (hourlyMinutes[hour] + minutesInHour > 60)
                        {
                            hourlyMinutes[hour] = 60;
                        }
                        else
                        {
                            hourlyMinutes[hour] += minutesInHour;
                        }
                    }
                }
            }

            return hourlyMinutes;
        }

        /// <summary>
        /// Gets a human-readable summary of hourly breakdown.
        /// </summary>
        public static string GetSummary(Dictionary<int, int> hourlyMinutes)
        {
            if (hourlyMinutes == null || hourlyMinutes.Count == 0)
            {
                return "No focus data for this day.";
            }

            var activeBuckets = hourlyMinutes
                .Where(kvp => kvp.Value > 0)
                .OrderBy(kvp => kvp.Key)
                .ToList();

            if (activeBuckets.Count == 0)
            {
                return "No focus sessions recorded for this day.";
            }

            var summary = string.Join(
                ", ",
                activeBuckets.Select(kvp => $"{kvp.Key:D2}:00 ({kvp.Value} min)")
            );

            int totalMinutes = hourlyMinutes.Values.Sum();
            return $"Total: {totalMinutes} min | {summary}";
        }
    }
}
