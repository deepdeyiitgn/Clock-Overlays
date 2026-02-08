using System;

namespace TransparentClock
{
    /// <summary>
    /// User-configurable pomodoro settings that can be persisted locally.
    /// </summary>
    public class PomodoroSettings
    {
        /// <summary>
        /// Focus duration in minutes.
        /// </summary>
        public int FocusMinutes { get; set; }

        /// <summary>
        /// Short break duration in minutes.
        /// </summary>
        public int ShortBreakMinutes { get; set; }

        /// <summary>
        /// Long break duration in minutes.
        /// </summary>
        public int LongBreakMinutes { get; set; }

        /// <summary>
        /// Number of focus sessions before a long break.
        /// </summary>
        public int SessionsBeforeLongBreak { get; set; }

        /// <summary>
        /// Automatically start the next focus session when a break ends.
        /// </summary>
        public bool AutoStartNextSession { get; set; }

        /// <summary>
        /// Automatically start breaks when a focus session ends.
        /// </summary>
        public bool AutoStartBreaks { get; set; }

        /// <summary>
        /// Optional limit for completed cycles (0 = no limit).
        /// </summary>
        public int SessionLimit { get; set; }

        /// <summary>
        /// Creates a safe default settings profile.
        /// </summary>
        public static PomodoroSettings CreateDefault()
        {
            return new PomodoroSettings
            {
                FocusMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15,
                SessionsBeforeLongBreak = 4,
                AutoStartNextSession = false,
                AutoStartBreaks = false,
                SessionLimit = 0
            };
        }
    }
}