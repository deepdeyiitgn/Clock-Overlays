using System;
using System.IO;
using System.Text.Json;

namespace TransparentClock
{
    /// <summary>
    /// Handles local, JSON-based persistence for AppState.
    /// </summary>
    public static class AppStateStorage
    {
        // Base folder: %LOCALAPPDATA%/Clock-Overlays/
        private static readonly string AppFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays");

        // Fixed state file: appstate.json
        private static readonly string StateFilePath = Path.Combine(AppFolderPath, "appstate.json");

        /// <summary>
        /// Load AppState from disk; falls back to default on any failure.
        /// </summary>
        public static AppState Load()
        {
            try
            {
                if (!File.Exists(StateFilePath))
                {
                    return AppState.CreateDefault();
                }

                string json = File.ReadAllText(StateFilePath);
                AppState? state = JsonSerializer.Deserialize<AppState>(json);

                return state ?? AppState.CreateDefault();
            }
            catch
            {
                // Fail safely and return defaults.
                return AppState.CreateDefault();
            }
        }

        /// <summary>
        /// Save AppState to disk; silently ignores any failures.
        /// </summary>
        public static void Save(AppState state)
        {
            try
            {
                Directory.CreateDirectory(AppFolderPath);

                string json = JsonSerializer.Serialize(state, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(StateFilePath, json);
            }
            catch
            {
                // Fail silently to avoid UI crashes.
            }
        }

        /// <summary>
        /// Delete the stored AppState file from disk.
        /// </summary>
        public static bool DeleteStateFile()
        {
            try
            {
                if (File.Exists(StateFilePath))
                {
                    File.Delete(StateFilePath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
