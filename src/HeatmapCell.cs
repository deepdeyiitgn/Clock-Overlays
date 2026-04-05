using System;

namespace TransparentClock
{
    /// <summary>
    /// Data model for a single cell in a weekly heatmap grid.
    /// Represents a time slot on a specific day with aggregated focus time.
    /// No UI-related fields or dependencies.
    /// Designed for heatmap visualization and analysis.
    /// </summary>
    public class HeatmapCell
    {
        /// <summary>
        /// The day of the week this cell represents.
        /// Values: Monday through Sunday.
        /// </summary>
        public DayOfWeek Day { get; set; }

        /// <summary>
        /// The start time of the time slot this cell represents.
        /// Relative to 00:00:00 (midnight).
        /// Example: 09:00:00 for 9 AM.
        /// </summary>
        public TimeSpan SlotStart { get; set; }

        /// <summary>
        /// The end time of the time slot this cell represents.
        /// Relative to 00:00:00 (midnight).
        /// Example: 10:00:00 for 10 AM.
        /// Typically SlotEnd > SlotStart.
        /// </summary>
        public TimeSpan SlotEnd { get; set; }

        /// <summary>
        /// The total focus/study minutes accumulated in this cell during this week.
        /// Can be 0 if no sessions occurred.
        /// Caller responsible for calculating and validating this value.
        /// </summary>
        public int TotalMinutes { get; set; }
    }
}
