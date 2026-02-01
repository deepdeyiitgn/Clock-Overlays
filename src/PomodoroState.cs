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
        /// Work session length (minutes).
        /// </summary>
        public int WorkMinutes { get; set; } = 25;

        /// <summary>
        /// Short break length (minutes).
        /// </summary>
        public int ShortBreakMinutes { get; set; } = 5;

        /// <summary>
        /// Long break length (minutes).
        /// </summary>
        public int LongBreakMinutes { get; set; } = 15;

        /// <summary>
        /// Number of work sessions before a long break.
        /// </summary>
        public int LongBreakInterval { get; set; } = 4;

        /// <summary>
        /// Automatically start the next session when one completes.
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
        public void Start()
        {
            IsRunning = true;
            IsPaused = false;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            }
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Pause a running session.
        /// </summary>
        public void Pause()
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            Tick(DateTime.UtcNow);
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
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            CurrentMode = PomodoroMode.Work;
            CompletedWorkSessions = 0;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Apply settings updates and normalize remaining time if idle.
        /// </summary>
        public void ApplySettings(int workMinutes, int shortBreakMinutes, int longBreakMinutes, int longBreakInterval)
        {
            WorkMinutes = Math.Max(1, workMinutes);
            ShortBreakMinutes = Math.Max(1, shortBreakMinutes);
            LongBreakMinutes = Math.Max(1, longBreakMinutes);
            LongBreakInterval = Math.Max(1, longBreakInterval);

            if (!IsRunning)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            }
        }

        /// <summary>
        /// Manually switch modes and reset the timer for that mode.
        /// </summary>
        public void SwitchMode(PomodoroMode mode)
        {
            CurrentMode = mode;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            IsRunning = false;
            IsPaused = false;
            LastTickUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Advance time based on UTC now.
        /// </summary>
        public void Tick(DateTime utcNow)
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

            ApplyElapsedSeconds(elapsedSeconds);
            LastTickUtc = utcNow;
        }

        /// <summary>
        /// Normalize after loading from disk.
        /// Ensures paused state and accurate remaining time.
        /// </summary>
        public void RestoreAfterLoad(DateTime utcNow)
        {
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            }

            if (!IsRunning)
            {
                IsPaused = false;
                LastTickUtc = utcNow;
                return;
            }

            if (!IsPaused)
            {
                int elapsedSeconds = (int)Math.Floor((utcNow - LastTickUtc).TotalSeconds);
                if (elapsedSeconds > 0)
                {
                    ApplyElapsedSeconds(elapsedSeconds);
                }
            }
            else if (RemainingSeconds <= 0)
            {
                CompleteCurrentMode();
            }

            LastTickUtc = utcNow;
        }

        private void ApplyElapsedSeconds(int elapsedSeconds)
        {
            if (elapsedSeconds <= 0)
            {
                return;
            }

            int remaining = RemainingSeconds - elapsedSeconds;
            while (remaining <= 0)
            {
                int overflow = -remaining;
                CompleteCurrentMode();
                if (!IsRunning)
                {
                    RemainingSeconds = GetModeLengthSeconds(CurrentMode);
                    return;
                }
                remaining = RemainingSeconds - overflow;
            }

            RemainingSeconds = remaining;
        }

        private void CompleteCurrentMode()
        {
            if (CurrentMode == PomodoroMode.Work)
            {
                CompletedWorkSessions++;
                CurrentMode = (CompletedWorkSessions % LongBreakInterval == 0)
                    ? PomodoroMode.LongBreak
                    : PomodoroMode.ShortBreak;
            }
            else
            {
                CurrentMode = PomodoroMode.Work;
            }

            RemainingSeconds = GetModeLengthSeconds(CurrentMode);
            if (!AutoStartNextSession)
            {
                IsRunning = false;
                IsPaused = false;
            }
        }

        public int GetModeTotalSeconds(PomodoroMode mode)
        {
            int minutes = mode switch
            {
                PomodoroMode.Work => WorkMinutes,
                PomodoroMode.ShortBreak => ShortBreakMinutes,
                PomodoroMode.LongBreak => LongBreakMinutes,
                _ => WorkMinutes
            };

            return Math.Max(1, minutes) * 60;
        }

        private int GetModeLengthSeconds(PomodoroMode mode)
        {
            return GetModeTotalSeconds(mode);
        }
    }
}
