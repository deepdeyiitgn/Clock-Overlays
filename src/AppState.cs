using System;

namespace TransparentClock
{
    /// <summary>
    /// Central, UI-independent application state for Clock-Overlays.
    /// Designed to be JSON-serializable and easy to reason about.
    /// </summary>
    public class AppState
    {
        // ------------------------------
        // User Information
        // ------------------------------

        /// <summary>
        /// Display name of the user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gender identity text (e.g., "male", "female", "other").
        /// </summary>
        public string Gender { get; set; } = "other";

        /// <summary>
        /// Optional local file path to a profile image.
        /// </summary>
        public string? ProfileImagePath { get; set; }

        // ------------------------------
        // Feature Flags
        // ------------------------------

        /// <summary>
        /// Indicates if the app is running for the first time.
        /// </summary>
        public bool IsFirstRun { get; set; }

        /// <summary>
        /// Enables the clock overlay feature.
        /// </summary>
        public bool ClockEnabled { get; set; }

        /// <summary>
        /// Selected clock color name (e.g., "White", "Red").
        /// </summary>
        public string ClockColorName { get; set; } = "White";

        /// <summary>
        /// Indicates if a custom clock color is in use.
        /// </summary>
        public bool ClockUseCustomColor { get; set; }

        /// <summary>
        /// Custom clock color stored as ARGB integer.
        /// </summary>
        public int? ClockCustomColorArgb { get; set; }

        /// <summary>
        /// Clock font size.
        /// </summary>
        public float ClockFontSize { get; set; } = 20f;

        /// <summary>
        /// Clock position preset (Top Left, Top Right, Bottom Left, Bottom Right).
        /// </summary>
        public string ClockPosition { get; set; } = "Top Right";

        /// <summary>
        /// Enables the dashboard feature (future module).
        /// </summary>
        public bool DashboardEnabled { get; set; }

        /// <summary>
        /// Enables the pomodoro feature (future module).
        /// </summary>
        public bool PomodoroEnabled { get; set; }

        /// <summary>
        /// Pomodoro session state (serializable).
        /// </summary>
        public PomodoroState Pomodoro { get; set; } = PomodoroState.CreateDefault();

        /// <summary>
        /// Enables the todo feature (future module).
        /// </summary>
        public bool TodoEnabled { get; set; }

        /// <summary>
        /// Launch the app on Windows startup.
        /// </summary>
        public bool LaunchOnStartup { get; set; }

        /// <summary>
        /// Show welcome screen on startup.
        /// </summary>
        public bool ShowWelcomeOnStartup { get; set; }

        /// <summary>
        /// Minimize to tray when the dashboard is closed.
        /// </summary>
        public bool MinimizeToTrayOnClose { get; set; }

        // ------------------------------
        // Metadata
        // ------------------------------

        /// <summary>
        /// Timestamp of the last application launch.
        /// </summary>
        public DateTime LastAppLaunch { get; set; }

        /// <summary>
        /// Creates a default app state for first-time users.
        /// </summary>
        public static AppState CreateDefault()
        {
            return new AppState
            {
                IsFirstRun = true,
                ClockEnabled = true,
                ClockColorName = "White",
                ClockUseCustomColor = false,
                ClockCustomColorArgb = null,
                ClockFontSize = 20f,
                ClockPosition = "Top Right",
                DashboardEnabled = false,
                PomodoroEnabled = false,
                TodoEnabled = false,
                Pomodoro = PomodoroState.CreateDefault(),
                LaunchOnStartup = false,
                ShowWelcomeOnStartup = true,
                MinimizeToTrayOnClose = true,
                LastAppLaunch = DateTime.UtcNow
            };
        }
    }
}
