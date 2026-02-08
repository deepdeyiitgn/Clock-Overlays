using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Indicates the profile setup has been completed.
        /// </summary>
        public bool IsProfileCompleted { get; set; }

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
        /// Clock font family name.
        /// </summary>
        public string ClockFontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Enables the clock border.
        /// </summary>
        public bool ClockBorderEnabled { get; set; }

        /// <summary>
        /// Selected clock border color name (e.g., "White", "Red").
        /// </summary>
        public string ClockBorderColorName { get; set; } = "White";

        /// <summary>
        /// Indicates if a custom clock border color is in use.
        /// </summary>
        public bool ClockBorderUseCustomColor { get; set; }

        /// <summary>
        /// Custom clock border color stored as ARGB integer.
        /// </summary>
        public int? ClockBorderCustomColorArgb { get; set; }

        /// <summary>
        /// Clock border width in pixels (base size).
        /// </summary>
        public int ClockBorderWidth { get; set; } = 2;

        /// <summary>
        /// Clock position preset (Top Left, Top Right, Bottom Left, Bottom Right).
        /// </summary>
        public string ClockPosition { get; set; } = "Top Right";

        /// <summary>
        /// Indicates if a custom position is in use.
        /// </summary>
        public bool ClockUseCustomPosition { get; set; }

        /// <summary>
        /// Custom clock X position.
        /// </summary>
        public int? ClockCustomPositionX { get; set; }

        /// <summary>
        /// Custom clock Y position.
        /// </summary>
        public int? ClockCustomPositionY { get; set; }

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
        /// Pomodoro settings (serializable).
        /// </summary>
        private PomodoroSettings? _pomodoroSettings = PomodoroSettings.CreateDefault();

        /// <summary>
        /// Pomodoro settings (serializable).
        /// </summary>
        public PomodoroSettings PomodoroSettings
        {
            get => _pomodoroSettings ??= PomodoroSettings.CreateDefault();
            set => _pomodoroSettings = value ?? PomodoroSettings.CreateDefault();
        }

        /// <summary>
        /// Enables the todo feature (future module).
        /// </summary>
        public bool TodoEnabled { get; set; }

        private List<TodoItem>? _todos;

        /// <summary>
        /// Todo items.
        /// </summary>
        public List<TodoItem> Todos
        {
            get => _todos ??= new List<TodoItem>();
            set => _todos = value ?? new List<TodoItem>();
        }

        private Dictionary<DateTime, FocusDay>? _focusHistory;

        /// <summary>
        /// Focus history (last 7 days).
        /// </summary>
        public Dictionary<DateTime, FocusDay> FocusHistory
        {
            get => _focusHistory ??= new Dictionary<DateTime, FocusDay>();
            set => _focusHistory = value ?? new Dictionary<DateTime, FocusDay>();
        }

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
                IsProfileCompleted = false,
                ClockEnabled = true,
                ClockColorName = "White",
                ClockUseCustomColor = false,
                ClockCustomColorArgb = null,
                ClockFontSize = 20f,
                ClockFontFamily = "Segoe UI",
                ClockBorderEnabled = false,
                ClockBorderColorName = "White",
                ClockBorderUseCustomColor = false,
                ClockBorderCustomColorArgb = null,
                ClockBorderWidth = 2,
                ClockPosition = "Top Right",
                ClockUseCustomPosition = false,
                ClockCustomPositionX = null,
                ClockCustomPositionY = null,
                DashboardEnabled = false,
                PomodoroEnabled = false,
                TodoEnabled = false,
                Todos = new List<TodoItem>(),
                FocusHistory = new Dictionary<DateTime, FocusDay>(),
                Pomodoro = PomodoroState.CreateDefault(),
                PomodoroSettings = PomodoroSettings.CreateDefault(),
                LaunchOnStartup = false,
                ShowWelcomeOnStartup = true,
                MinimizeToTrayOnClose = true,
                LastAppLaunch = DateTime.UtcNow
            };
        }
    }
}
