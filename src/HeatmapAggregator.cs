using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Aggregates FocusSession data into HeatmapCell objects.
    /// Creates a weekly heatmap with 2-hour time slots (00-02, 02-04, ..., 22-24).
    /// Correctly splits sessions that span multiple time slots or days.
    /// No UI-related code or dependencies.
    /// </summary>
    public class HeatmapAggregator
    {
        /// <summary>
        /// The size of each time slot in hours.
        /// Fixed at 2 hours per cell.
        /// </summary>
        private const int SlotSizeHours = 2;

        /// <summary>
        /// Aggregates focus sessions into a weekly heatmap.
        /// 
        /// This method:
        /// - Creates 12 time slots per day (00-02, 02-04, ..., 22-24 UTC)
        /// - Iterates through all days of the week (Monday-Sunday)
        /// - For each session, calculates overlap with each cell
        /// - Splits sessions correctly across time slots and days
        /// - Returns all cells, including zeros (no activity)
        /// 
        /// Session splitting logic:
        /// - A session from Monday 23:00 to Tuesday 02:00 (3 hours) contributes:
        ///   - Monday 22-24 slot: 1 hour (60 minutes)
        ///   - Tuesday 00-02 slot: 2 hours (120 minutes)
        /// </summary>
        /// <param name="sessions">The list of focus sessions to aggregate. Can be null or empty.</param>
        /// <param name="weekStartDate">The Monday of the week to aggregate.
        /// Should be a Monday; if not, behavior is undefined (no validation performed).</param>
        /// <returns>A list of 84 HeatmapCell objects (12 slots × 7 days).
        /// Each cell contains aggregated minutes for its (Day, TimeSlot) combination.
        /// Includes cells with TotalMinutes = 0 for slots with no activity.</returns>
        public List<HeatmapCell> AggregateByWeek(List<FocusSession> sessions, DateTime weekStartDate)
        {
            // Dictionary to accumulate minutes by (DayOfWeek, SlotStart)
            var cellMinutes = new Dictionary<(DayOfWeek, TimeSpan), int>();

            // Initialize all cells for the week (0 activity)
            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                DayOfWeek dayOfWeek = GetDayOfWeek(dayOffset);

                for (int slotIndex = 0; slotIndex < 12; slotIndex++)
                {
                    var slotStart = new TimeSpan(slotIndex * SlotSizeHours, 0, 0);
                    cellMinutes[(dayOfWeek, slotStart)] = 0;
                }
            }

            // If no sessions, return empty cells
            if (sessions == null || sessions.Count == 0)
            {
                return CreateCellsFromDictionary(cellMinutes);
            }

            // Define week bounds (UTC)
            DateTime weekEnd = weekStartDate.AddDays(7);

            // Aggregate each session into cells
            foreach (var session in sessions)
            {
                AggregateSessionIntoCells(session, weekStartDate, weekEnd, cellMinutes);
            }

            // Convert dictionary to list of HeatmapCell objects
            return CreateCellsFromDictionary(cellMinutes);
        }

        /// <summary>
        /// Distributes a single session's minutes across the cells it overlaps with.
        /// Correctly splits sessions that span multiple time slots or days.
        /// </summary>
        private void AggregateSessionIntoCells(
            FocusSession session,
            DateTime weekStart,
            DateTime weekEnd,
            Dictionary<(DayOfWeek, TimeSpan), int> cellMinutes)
        {
            // Calculate session end time
            DateTime sessionStart = session.StartTime;
            DateTime sessionEnd = session.EndTime;

            if (sessionEnd <= sessionStart)
                return;

            // Ignore sessions completely outside the week
            if (sessionEnd <= weekStart || sessionStart >= weekEnd)
                return;

            // Clamp session to week boundaries
            DateTime effectiveStart = sessionStart < weekStart ? weekStart : sessionStart;
            DateTime effectiveEnd = sessionEnd > weekEnd ? weekEnd : sessionEnd;

            // Iterate through each hour in the session (in UTC)
            // We'll calculate which slot each hour belongs to
            DateTime currentTime = effectiveStart;

            while (currentTime < effectiveEnd)
            {
                // Determine which 2-hour slot this time falls into
                int hourOfDay = currentTime.Hour;
                int slotIndex = hourOfDay / SlotSizeHours;
                TimeSpan slotStart = new TimeSpan(slotIndex * SlotSizeHours, 0, 0);
                TimeSpan slotEnd = new TimeSpan((slotIndex + 1) * SlotSizeHours, 0, 0);

                // Calculate the end of the current slot
                DateTime slotEndTime = currentTime.Date.AddHours((slotIndex + 1) * SlotSizeHours);

                // Calculate overlap between session and this slot
                DateTime overlapEnd = effectiveEnd < slotEndTime ? effectiveEnd : slotEndTime;
                int overlapMinutes = (int)(overlapEnd - currentTime).TotalMinutes;

                // Add minutes to the appropriate cell
                DayOfWeek dayOfWeek = currentTime.DayOfWeek;
                var cellKey = (dayOfWeek, slotStart);

                if (cellMinutes.ContainsKey(cellKey))
                {
                    cellMinutes[cellKey] += overlapMinutes;
                }

                // Move to the next slot
                currentTime = overlapEnd;
            }
        }

        /// <summary>
        /// Converts the accumulated minutes dictionary into a list of HeatmapCell objects.
        /// </summary>
        private List<HeatmapCell> CreateCellsFromDictionary(
            Dictionary<(DayOfWeek, TimeSpan), int> cellMinutes)
        {
            var cells = new List<HeatmapCell>();

            foreach (var kvp in cellMinutes.OrderBy(x => (int)x.Key.Item1).ThenBy(x => x.Key.Item2))
            {
                var (dayOfWeek, slotStart) = kvp.Key;
                int totalMinutes = kvp.Value;

                var cell = new HeatmapCell
                {
                    Day = dayOfWeek,
                    SlotStart = slotStart,
                    SlotEnd = slotStart.Add(new TimeSpan(SlotSizeHours, 0, 0)),
                    TotalMinutes = totalMinutes
                };

                cells.Add(cell);
            }

            return cells;
        }

        /// <summary>
        /// Converts a day offset (0-6) to a DayOfWeek, assuming 0 = Monday.
        /// </summary>
        private DayOfWeek GetDayOfWeek(int dayOffset)
        {
            return (DayOfWeek)((1 + dayOffset) % 7);
        }
    }
}
