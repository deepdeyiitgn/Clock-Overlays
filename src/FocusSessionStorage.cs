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

        public static void AddSession(DateTime startTime, DateTime endTime, int actualSeconds, string source)
        {
            if (endTime <= startTime)
            {
                return;
            }

            if (actualSeconds <= 0)
            {
                return;
            }

            source ??= string.Empty;

            var entries = LoadAll();
            if (entries.Any(entry => entry.StartTime == startTime && entry.EndTime == endTime))
            {
                return;
            }

            entries.Add(new FocusSessionEntry
            {
                StartTime = startTime,
                EndTime = endTime,
                ActualSeconds = actualSeconds,
                ActualMinutes = (int)Math.Round(actualSeconds / 60d),
                Source = source
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
                var data = JsonSerializer.Deserialize<List<FocusSessionEntry>>(json) ?? new List<FocusSessionEntry>();

                foreach (var entry in data)
                {
                    if (entry.StartTime == default && entry.EndTime == default && entry.CompletedAt.HasValue && entry.Minutes.HasValue)
                    {
                        var endTime = entry.CompletedAt.Value;
                        var startTime = endTime.AddMinutes(-entry.Minutes.Value);
                        entry.StartTime = startTime;
                        entry.EndTime = endTime;
                        entry.ActualMinutes = entry.Minutes.Value;
                        entry.ActualSeconds = entry.Minutes.Value * 60;
                    }

                    if (!entry.ActualSeconds.HasValue && entry.ActualMinutes.HasValue && entry.ActualMinutes.Value > 0)
                    {
                        entry.ActualSeconds = entry.ActualMinutes.Value * 60;
                    }
                }

                data = data
                    .Where(entry => entry.EndTime > entry.StartTime)
                    .GroupBy(entry => new { entry.StartTime, entry.EndTime })
                    .Select(group => group.First())
                    .ToList();

                return data;
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
