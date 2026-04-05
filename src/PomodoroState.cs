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
        /// Number of completed cycles (counted after a long break finishes).
        /// </summary>
        public int CompletedCycles { get; set; }

        /// <summary>
        /// Remaining time in seconds.
        /// </summary>
        public int RemainingSeconds { get; set; } = 25 * 60;

        /// <summary>
        /// UTC timestamp of the last tick update.
        /// </summary>
        public DateTime LastTickUtc { get; set; } = DateTime.UtcNow;

        private DateTime? focusSegmentStartUtc;
        private readonly System.Collections.Generic.List<FocusSegment> focusSegments = new();
        private int completionSequence;
        private int lastNotifiedCompletionSequence;
        private PomodoroCompletionInfo lastCompletionInfo;

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
                CompletedCycles = 0,
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
            StartSession(settings);
        }

        /// <summary>
        /// Pause a running session.
        /// </summary>
        public void Pause(PomodoroSettings settings)
        {
            PauseSession(settings);
        }

        /// <summary>
        /// Resume a paused session.
        /// </summary>
        public void Resume()
        {
            ResumeSession();
        }

        /// <summary>
        /// Stop the session and reset the remaining time.
        /// </summary>
        public void Stop(PomodoroSettings settings)
        {
            StopSession(settings);
        }

        public void StartSession(PomodoroSettings settings)
        {
            NormalizeSettings(settings);
            IsRunning = true;
            IsPaused = false;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            }
            LastTickUtc = DateTime.UtcNow;

            if (CurrentMode == PomodoroMode.Work)
            {
                BeginFocusSegment(LastTickUtc);
            }
        }

        public void PauseSession(PomodoroSettings settings)
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            Tick(DateTime.UtcNow, settings);
            EndFocusSegment(DateTime.UtcNow);
            IsPaused = true;
        }

        public void ResumeSession()
        {
            if (!IsRunning || !IsPaused)
            {
                return;
            }

            IsPaused = false;
            LastTickUtc = DateTime.UtcNow;
            if (CurrentMode == PomodoroMode.Work)
            {
                BeginFocusSegment(LastTickUtc);
            }
        }

        public void StopSession(PomodoroSettings settings)
        {
            NormalizeSettings(settings);

            if (IsRunning && !IsPaused)
            {
                Tick(DateTime.UtcNow, settings);
            }

            CommitFocusData(DateTime.UtcNow);
            ResetSession(settings);
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
            if (IsRunning && !IsPaused)
            {
                Tick(DateTime.UtcNow, settings);
            }

            if (CurrentMode == PomodoroMode.Work)
            {
                CommitFocusData(DateTime.UtcNow);
            }

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

            // Discard unfinished sessions after restart for data integrity.
            ResetSession(settings);
            return;
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

            var cursorUtc = LastTickUtc;

            while (elapsedSeconds > 0)
            {
                int step = Math.Min(elapsedSeconds, RemainingSeconds);

                if (CurrentMode == PomodoroMode.Work)
                {
                    BeginFocusSegment(cursorUtc);
                }

                RemainingSeconds -= step;
                cursorUtc = cursorUtc.AddSeconds(step);
                elapsedSeconds -= step;

                if (RemainingSeconds <= 0)
                {
                    CompleteCurrentMode(settings, cursorUtc);
                    if (!IsRunning)
                    {
                        return;
                    }
                }
            }
        }

        private void CompleteCurrentMode(PomodoroSettings settings, DateTime completedAtUtc)
        {
            var completedMode = CurrentMode;
            if (CurrentMode == PomodoroMode.Work)
            {
                CompletedWorkSessions++;
            EndFocusSegment(completedAtUtc);
            CommitFocusData(completedAtUtc);

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

                RegisterCompletion(completedMode, false, false);

                return;
            }

            bool cycleCompleted = CurrentMode == PomodoroMode.LongBreak;
            bool sessionLimitReached = false;

            if (cycleCompleted)
            {
                CompletedCycles++;
                sessionLimitReached = settings.SessionLimit > 0 && CompletedCycles >= settings.SessionLimit;
            }

            CurrentMode = PomodoroMode.Work;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);

            if (sessionLimitReached)
            {
                IsRunning = false;
                IsPaused = false;
                RegisterCompletion(completedMode, cycleCompleted, true);
                return;
            }

            if (settings.AutoStartNextSession)
            {
                IsRunning = true;
                IsPaused = false;
                ResetFocusTracking();
            }
            else
            {
                IsRunning = false;
                IsPaused = false;
            }

            RegisterCompletion(completedMode, cycleCompleted, false);
        }

        private void CommitFocusData(DateTime commitUtc)
        {
            EndFocusSegment(commitUtc);

            if (focusSegments.Count == 0)
            {
                ResetFocusTracking();
                return;
            }

            int totalSeconds = 0;
            foreach (var segment in focusSegments)
            {
                totalSeconds += Math.Max(0, (int)Math.Floor((segment.EndUtc - segment.StartUtc).TotalSeconds));
            }

            if (totalSeconds < 1)
            {
                ResetFocusTracking();
                return;
            }

            var segmentsLocal = new System.Collections.Generic.List<(DateTime Start, DateTime End)>();
            foreach (var segment in focusSegments)
            {
                segmentsLocal.Add((segment.StartUtc.ToLocalTime(), segment.EndUtc.ToLocalTime()));
            }

            FocusHistoryStorage.AddFocusSegments(segmentsLocal);

            var startLocal = segmentsLocal[0].Start;
            var endLocal = segmentsLocal[0].End;
            foreach (var segment in segmentsLocal)
            {
                if (segment.Start < startLocal)
                {
                    startLocal = segment.Start;
                }

                if (segment.End > endLocal)
                {
                    endLocal = segment.End;
                }
            }

            FocusSessionStorage.AddSession(startLocal, endLocal, totalSeconds, "Pomodoro");
            ResetFocusTracking();
        }

        private void BeginFocusSegment(DateTime utcNow)
        {
            if (CurrentMode != PomodoroMode.Work)
            {
                return;
            }

            if (focusSegmentStartUtc.HasValue)
            {
                return;
            }

            focusSegmentStartUtc = utcNow;
        }

        private void EndFocusSegment(DateTime utcNow)
        {
            if (!focusSegmentStartUtc.HasValue)
            {
                return;
            }

            var startUtc = focusSegmentStartUtc.Value;
            if (utcNow > startUtc)
            {
                focusSegments.Add(new FocusSegment(startUtc, utcNow));
            }

            focusSegmentStartUtc = null;
        }

        private void ResetSession(PomodoroSettings settings)
        {
            IsRunning = false;
            IsPaused = false;
            CurrentMode = PomodoroMode.Work;
            CompletedWorkSessions = 0;
            CompletedCycles = 0;
            RemainingSeconds = GetModeLengthSeconds(CurrentMode, settings);
            LastTickUtc = DateTime.UtcNow;
            ResetFocusTracking();
            completionSequence = 0;
            lastNotifiedCompletionSequence = 0;
        }

        private void ResetFocusTracking()
        {
            focusSegmentStartUtc = null;
            focusSegments.Clear();
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
            settings.SessionLimit = Math.Max(0, settings.SessionLimit);
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
                settings.AutoStartBreaks == false &&
                settings.SessionLimit == 0;

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

        public bool TryConsumeCompletion(out PomodoroCompletionInfo info)
        {
            if (completionSequence == lastNotifiedCompletionSequence)
            {
                info = default;
                return false;
            }

            lastNotifiedCompletionSequence = completionSequence;
            info = lastCompletionInfo;
            return true;
        }

        private void RegisterCompletion(PomodoroMode mode, bool cycleCompleted, bool sessionLimitReached)
        {
            completionSequence++;
            lastCompletionInfo = new PomodoroCompletionInfo(
                mode,
                cycleCompleted,
                sessionLimitReached,
                CompletedCycles);
        }

        private readonly struct FocusSegment
        {
            public FocusSegment(DateTime startUtc, DateTime endUtc)
            {
                StartUtc = startUtc;
                EndUtc = endUtc;
            }

            public DateTime StartUtc { get; }
            public DateTime EndUtc { get; }
        }
    }

    public readonly struct PomodoroCompletionInfo
    {
        public PomodoroCompletionInfo(PomodoroState.PomodoroMode mode, bool cycleCompleted, bool sessionLimitReached, int completedCycles)
        {
            Mode = mode;
            CycleCompleted = cycleCompleted;
            SessionLimitReached = sessionLimitReached;
            CompletedCycles = completedCycles;
        }

        public PomodoroState.PomodoroMode Mode { get; }
        public bool CycleCompleted { get; }
        public bool SessionLimitReached { get; }
        public int CompletedCycles { get; }
    }
}
