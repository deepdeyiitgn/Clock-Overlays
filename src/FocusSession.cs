using System;

namespace TransparentClock
{
    /// <summary>
    /// Data model for a single focus/study session.
    /// Tracks when a session occurred, how long it lasted, and its source.
    /// No UI-related fields or dependencies.
    /// Designed for in-memory storage and later persistence.
    /// </summary>
    public class FocusSession
    {
        /// <summary>
        /// When this focus session started, in UTC.
        /// Required field. Caller responsible for validation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When this focus session ended, in UTC.
        /// Required field. Caller responsible for validation.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Duration in minutes calculated from StartTime and EndTime.
        /// </summary>
        public int DurationMinutes
        {
            get
            {
                var minutes = (int)Math.Round((EndTime - StartTime).TotalMinutes);
                return Math.Max(0, minutes);
            }
        }

        /// <summary>
        /// The source or type of this focus session.
        /// Examples: "Pomodoro", "Manual", "App", "Timer", etc.
        /// Defaults to empty string. Caller responsible for validation.
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }
}
