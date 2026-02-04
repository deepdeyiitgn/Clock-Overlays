using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TransparentClock
{
    public static class FocusSessionStorage
    {
        private static readonly string AppFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays");

        private static readonly string SessionsFilePath = Path.Combine(AppFolderPath, "focus_sessions.json");

        public static void AddSession(DateTime completedAt, int minutes)
        {
            if (minutes <= 0)
            {
                return;
            }

            var entries = LoadAll();
            entries.Add(new FocusSessionEntry
            {
                CompletedAt = completedAt,
                Minutes = minutes
            });

            SaveAll(entries);
        }

        public static IReadOnlyList<FocusSessionEntry> GetAll()
        {
            return LoadAll();
        }

        private static List<FocusSessionEntry> LoadAll()
        {
            try
            {
                if (!File.Exists(SessionsFilePath))
                {
                    return new List<FocusSessionEntry>();
                }

                string json = File.ReadAllText(SessionsFilePath);
                var data = JsonSerializer.Deserialize<List<FocusSessionEntry>>(json);
                return data ?? new List<FocusSessionEntry>();
            }
            catch
            {
                return new List<FocusSessionEntry>();
            }
        }

        private static void SaveAll(List<FocusSessionEntry> entries)
        {
            try
            {
                Directory.CreateDirectory(AppFolderPath);
                string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SessionsFilePath, json);
            }
            catch
            {
            }
        }
    }
}
