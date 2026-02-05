using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Data structure holding calculated focus insights
    /// </summary>
    public class FocusInsights
    {
        /// <summary>
        /// Average focus minutes per day across the range
        /// </summary>
        public double AverageFocusPerDay { get; set; }

        /// <summary>
        /// Total focus time in minutes across the entire range
        /// </summary>
        public int TotalFocusTime { get; set; }

        /// <summary>
        /// Best focus slot (date + hour) with highest minutes
        /// </summary>
        public FocusSlot? BestFocusSlot { get; set; }

        /// <summary>
        /// Worst focus slot (date + hour) with lowest minutes
        /// </summary>
        public FocusSlot? WorstFocusSlot { get; set; }

        /// <summary>
        /// Number of days in the analyzed range
        /// </summary>
        public int DayCount { get; set; }
    }

    /// <summary>
    /// Represents a single focus slot (hour on a specific day)
    /// </summary>
    public class FocusSlot
    {
        /// <summary>
        /// Date of the focus slot
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Hour of the day (0-23)
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// Minutes focused during this slot
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Formatted string representation
        /// </summary>
        public override string ToString()
        {
            return $"{Date:MMM d} at {Hour:00}:00 ({Minutes} min)";
        }
    }

    /// <summary>
    /// Calculates various focus metrics from focus history data
    /// </summary>
    public class FocusInsightsCalculator
    {
        /// <summary>
        /// Calculate focus insights from focus history entries
        /// </summary>
        public static FocusInsights Calculate(IEnumerable<FocusHistory> entries)
        {
            var insights = new FocusInsights
            {
                AverageFocusPerDay = 0,
                TotalFocusTime = 0,
                BestFocusSlot = null,
                WorstFocusSlot = null,
                DayCount = 0
            };

            if (entries == null || !entries.Any())
            {
                return insights;
            }

            // Calculate total focus time and day count
            int totalMinutes = 0;
            var dateSet = new HashSet<DateTime>();

            foreach (var entry in entries)
            {
                totalMinutes += entry.TotalFocusMinutes;
                dateSet.Add(DateTime.Parse(entry.Date));
            }

            insights.DayCount = dateSet.Count;
            insights.TotalFocusTime = totalMinutes;

            // Calculate average per day
            if (insights.DayCount > 0)
            {
                insights.AverageFocusPerDay = (double)totalMinutes / insights.DayCount;
            }

            // Find best and worst focus slots
            int bestMinutes = -1;
            int worstMinutes = 61; // Start with 61 since max is 60
            var bestSlot = new FocusSlot();
            var worstSlot = new FocusSlot();

            foreach (var entry in entries)
            {
                try
                {
                    DateTime date = DateTime.Parse(entry.Date);
                    var hourly = entry.HourlyFocus ?? new int[24];

                    for (int hour = 0; hour < 24; hour++)
                    {
                        int minutes = hourly.Length > hour ? hourly[hour] : 0;

                        // Check for best slot
                        if (minutes > bestMinutes)
                        {
                            bestMinutes = minutes;
                            bestSlot = new FocusSlot
                            {
                                Date = date,
                                Hour = hour,
                                Minutes = minutes
                            };
                        }

                        // Check for worst slot (only non-zero focus hours)
                        if (minutes > 0 && minutes < worstMinutes)
                        {
                            worstMinutes = minutes;
                            worstSlot = new FocusSlot
                            {
                                Date = date,
                                Hour = hour,
                                Minutes = minutes
                            };
                        }
                    }
                }
                catch
                {
                    // Skip malformed entries
                    continue;
                }
            }

            insights.BestFocusSlot = bestSlot;
            // Only set worst slot if we found a non-zero value
            insights.WorstFocusSlot = worstMinutes < 61 ? worstSlot : null;

            return insights;
        }

        /// <summary>
        /// Calculate insights for a specific date
        /// </summary>
        public static FocusInsights CalculateForDate(DateTime date)
        {
            var entry = FocusHistoryStorage.GetDay(date);
            
            if (entry == null)
            {
                return new FocusInsights { DayCount = 1 };
            }

            var insights = new FocusInsights
            {
                DayCount = 1,
                TotalFocusTime = entry.TotalFocusMinutes,
                AverageFocusPerDay = entry.TotalFocusMinutes
            };

            // Find best and worst hours for the day
            var hourly = entry.HourlyFocus ?? new int[24];
            int bestMinutes = -1;
            int worstMinutes = 61;
            var bestSlot = new FocusSlot();
            var worstSlot = new FocusSlot();

            for (int hour = 0; hour < 24; hour++)
            {
                int minutes = hourly.Length > hour ? hourly[hour] : 0;

                if (minutes > bestMinutes)
                {
                    bestMinutes = minutes;
                    bestSlot = new FocusSlot
                    {
                        Date = date,
                        Hour = hour,
                        Minutes = minutes
                    };
                }

                if (minutes < worstMinutes)
                {
                    worstMinutes = minutes;
                    worstSlot = new FocusSlot
                    {
                        Date = date,
                        Hour = hour,
                        Minutes = minutes
                    };
                }
            }

            insights.BestFocusSlot = bestSlot;
            insights.WorstFocusSlot = worstSlot;

            return insights;
        }

        /// <summary>
        /// Calculate insights for a specific time range
        /// Works with available data from GetLast7Days()
        /// </summary>
        public static FocusInsights CalculateForRange(FocusTimeRangeFilter.TimeRange range)
        {
            int dayCount = FocusTimeRangeFilter.GetDayCount(range);
            DateTime cutoff = DateTime.Now.AddDays(-dayCount);

            // Get all available history
            var entries = FocusHistoryStorage.GetLast7Days();
            var filteredEntries = entries
                .Where(e => 
                {
                    try
                    {
                        var entryDate = DateTime.Parse(e.Date);
                        return entryDate >= cutoff.Date;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();

            return Calculate(filteredEntries);
        }

        /// <summary>
        /// Get formatted summary of insights
        /// </summary>
        public static string GetSummary(FocusInsights insights)
        {
            if (insights == null || insights.DayCount == 0)
            {
                return "No focus data available";
            }

            var lines = new List<string>
            {
                $"Total Focus Time: {insights.TotalFocusTime} minutes",
                $"Average per Day: {insights.AverageFocusPerDay:F1} minutes",
                $"Days Tracked: {insights.DayCount}",
            };

            if (insights.BestFocusSlot != null)
            {
                lines.Add($"Best Hour: {insights.BestFocusSlot}");
            }

            if (insights.WorstFocusSlot != null)
            {
                lines.Add($"Worst Hour: {insights.WorstFocusSlot}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
