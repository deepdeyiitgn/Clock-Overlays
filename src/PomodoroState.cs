using System;

namespace TransparentClock
{
    /// <summary>
    /// Serializable state for a single pomodoro session.
    /// </summary>
    public class PomodoroState
    {
        public enum PomodoroMode
        {
            Work,
            ShortBreak,
            LongBreak
        }

        /// <summary>
        /// Legacy work session length (minutes). Kept for backward compatibility.
        /// </summary>
        public int WorkMinutes { get; set; } = 25;

        /// <summary>
        /// Legacy short break length (minutes). Kept for backward compatibility.
        /// </summary>
        public int ShortBreakMinutes { get; set; } = 5;

        /// <summary>
        /// Legacy long break length (minutes). Kept for backward compatibility.
        /// </summary>
        public int LongBreakMinutes { get; set; } = 15;

        /// <summary>
        /// Legacy number of work sessions before a long break. Kept for backward compatibility.
        /// </summary>
        public int LongBreakInterval { get; set; } = 4;

        /// <summary>
        /// Legacy auto-start flag. Kept for backward compatibility.
        /// </summary>
        public bool AutoStartNextSession { get; set; } = true;

        /// <summary>
        /// Indicates a session has been started.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Indicates the session is paused.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Current pomodoro mode (Work / ShortBreak / LongBreak).
        /// </summary>
        public PomodoroMode CurrentMode { get; set; } = PomodoroMode.Work;

        /// <summary>
        /// Number of completed work sessions in the current cycle.
        /// </summary>
        public int CompletedWorkSessions { get; set; }

        /// <summary>
        /// Remaining time in seconds.
        /// </summary>
        public int RemainingSeconds { get; set; } = 25 * 60;

        /// <summary>
        /// UTC timestamp of the last tick update.
        /// </summary>
        public DateTime LastTickUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a default pomodoro state.
        /// </summary>
        public static PomodoroState CreateDefault()
        {
            return new PomodoroState
            {
                IsRunning = false,
                IsPaused = false,
                CurrentMode = PomodoroMode.Work,
                CompletedWorkSessions = 0,
                RemainingSeconds = 25 * 60,
                LastTickUtc = DateTime.UtcNow,
                WorkMinutes = 25,
                ShortBreakMinutes = 5,
                LongBreakMinutes = 15,
                LongBreakInterval = 4,
                AutoStartNextSession = true
            };
        }

        /// <summary>
        /// Start a new session from full length.
        /// </summary>
        public void Start(PomodoroSettings settings)
        {
            NormalizeSettings(settings);
            IsRunning = true;
            IsPaused = false;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            }
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Pause a running session.
        /// </summary>
        public void Pause(PomodoroSettings settings)
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            Tick(DateTime.UtcNow, settings);
            IsPaused = true;
        }

        /// <summary>
        /// Resume a paused session.
        /// </summary>
        public void Resume()
        {
            if (!IsRunning || !IsPaused)
            {
                return;
            }

            IsPaused = false;
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Stop the session and reset the remaining time.
        /// </summary>
        public void Stop(PomodoroSettings settings)
        {
            NormalizeSettings(settings);
            IsRunning = false;
            IsPaused = false;
            CurrentMode = PomodoroMode.Work;
            CompletedWorkSessions = 0;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Apply settings updates and normalize remaining time if idle.
        /// </summary>
        public void ApplySettings(PomodoroSettings settings)
        {
            NormalizeSettings(settings);

            if (!IsRunning)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            }
        }

        /// <summary>
        /// Manually switch modes and reset the timer for that mode.
        /// </summary>
        public void SwitchMode(PomodoroMode mode, PomodoroSettings settings)
        {
            NormalizeSettings(settings);
            CurrentMode = mode;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            IsRunning = false;
            IsPaused = false;
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Advance time based on UTC now.
        /// </summary>
        public void Tick(DateTime utcNow, PomodoroSettings settings)
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            int elapsedSeconds = (int)Math.Floor((utcNow - LastTickUtc).TotalSeconds);
            if (elapsedSeconds <= 0)
            {
                return;
            }

            ApplyElapsedSeconds(elapsedSeconds, settings);
            LastTickUtc = utcNow;
        }

        /// <summary>
        /// Normalize after loading from disk.
        /// Ensures paused state and accurate remaining time.
        /// </summary>
        public void RestoreAfterLoad(DateTime utcNow, PomodoroSettings settings)
        {
            MigrateLegacySettingsIfMissing(settings);
            NormalizeSettings(settings);

            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            }

            if (!IsRunning)
            {
                IsPaused = false;
                LastTickUtc = utcNow;
                return;
            }

            bool allowAutoStart = CurrentMode == PomodoroMode.Work
                ? settings.AutoStartNextSession
                : settings.AutoStartBreaks;

            if (!allowAutoStart)
            {
                IsPaused = true;
                LastTickUtc = utcNow;
                return;
            }

            if (!IsPaused)
            {
                int elapsedSeconds = (int)Math.Floor((utcNow - LastTickUtc).TotalSeconds);
                if (elapsedSeconds > 0)
                {
                    ApplyElapsedSeconds(elapsedSeconds, settings);
                }
            }

            LastTickUtc = utcNow;
        }

        /// <summary>
        /// Total seconds for the given mode.
        /// </summary>
        public int GetModeTotalSeconds(PomodoroMode mode, PomodoroSettings settings)
        {
            NormalizeSettings(settings);
            return mode switch
            {
                PomodoroMode.Work => Math.Max(1, settings.FocusMinutes) * 60,
                PomodoroMode.ShortBreak => Math.Max(1, settings.ShortBreakMinutes) * 60,
                PomodoroMode.LongBreak => Math.Max(1, settings.LongBreakMinutes) * 60,
                _ => Math.Max(1, settings.FocusMinutes) * 60
            };
        }

        private void ApplyElapsedSeconds(int elapsedSeconds, PomodoroSettings settings)
        {
            if (elapsedSeconds <= 0)
            {
                return;
            }

            int remaining = RemainingSeconds - elapsedSeconds;
            while (remaining <= 0)
            {
                int overflow = -remaining;
                CompleteCurrentMode(settings);
                if (!IsRunning)
                {
                    RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
                    return;
                }
                remaining = RemainingSeconds - overflow;
            }

            RemainingSeconds = Math.Max(0, remaining);
        }

        private void CompleteCurrentMode(PomodoroSettings settings)
        {
            if (CurrentMode == PomodoroMode.Work)
            {
                CompletedWorkSessions++;

                int interval = Math.Max(1, settings.SessionsBeforeLongBreak);
                if (CompletedWorkSessions >= interval)
                {
                    CurrentMode = PomodoroMode.LongBreak;
                    CompletedWorkSessions = 0;
                }
                else
                {
                    CurrentMode = PomodoroMode.ShortBreak;
                }

                RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);

                if (settings.AutoStartBreaks)
                {
                    IsRunning = true;
                    IsPaused = false;
                }
                else
                {
                    IsRunning = false;
                    IsPaused = false;
                }

                return;
            }

            CurrentMode = PomodoroMode.Work;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);

            if (settings.AutoStartNextSession)
            {
                IsRunning = true;
                IsPaused = false;
            }
            else
            {
                IsRunning = false;
                IsPaused = false;
            }
        }

        private int GetModeLengthSeconds(PomodoroMode mode, PomodoroSettings settings)
        {
            return GetModeTotalSeconds(mode, settings);
        }

        private static void NormalizeSettings(PomodoroSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.FocusMinutes = Math.Max(1, settings.FocusMinutes);
            settings.ShortBreakMinutes = Math.Max(1, settings.ShortBreakMinutes);
            settings.LongBreakMinutes = Math.Max(1, settings.LongBreakMinutes);
            settings.SessionsBeforeLongBreak = Math.Max(1, settings.SessionsBeforeLongBreak);
        }

        private void MigrateLegacySettingsIfMissing(PomodoroSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            bool settingsAreDefault =
                settings.FocusMinutes == 25 &&
                settings.ShortBreakMinutes == 5 &&
                settings.LongBreakMinutes == 15 &&
                settings.SessionsBeforeLongBreak == 4 &&
                settings.AutoStartNextSession == false &&
                settings.AutoStartBreaks == false;

            if (!settingsAreDefault)
            {
                return;
            }

            bool legacyHasCustom =
                WorkMinutes != 25 ||
                ShortBreakMinutes != 5 ||
                LongBreakMinutes != 15 ||
                LongBreakInterval != 4 ||
                AutoStartNextSession != true;

            if (!legacyHasCustom)
            {
                return;
            }

            settings.FocusMinutes = WorkMinutes;
            settings.ShortBreakMinutes = ShortBreakMinutes;
            settings.LongBreakMinutes = LongBreakMinutes;
            settings.SessionsBeforeLongBreak = LongBreakInterval;
            settings.AutoStartNextSession = AutoStartNextSession;
        }
    }
}
