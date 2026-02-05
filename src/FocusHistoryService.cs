using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TransparentClock
{
    public static class FocusHistoryService
    {
        private static readonly string AppFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays");

        private static readonly string HistoryFilePath = Path.Combine(AppFolderPath, "focus_history.json");

        public static void AddFocusMinutes(DateTime date, int minutes, int hour)
        {
            if (minutes <= 0)
            {
                return;
            }

            hour = Math.Max(0, Math.Min(23, hour));
            var history = LoadAll();

            string key = date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!history.TryGetValue(key, out var entry))
            {
                entry = new FocusHistoryEntry { Date = key };
                history[key] = entry;
            }

            if (entry.HourlyFocus.Length != 24)
            {
                entry.HourlyFocus = NormalizeHourly(entry.HourlyFocus);
            }

            entry.TotalFocusMinutes += minutes;
            entry.HourlyFocus[hour] += minutes;

            SaveAll(history);
        }

        public static IReadOnlyList<FocusHistoryEntry> GetLast7Days()
        {
            var history = LoadAll();
            DateTime cutoff = DateTime.Today.AddDays(-6);

            return history.Values
                .Where(entry => TryParseDate(entry.Date, out var date) && date >= cutoff)
                .OrderByDescending(entry => entry.Date)
                .ToList();
        }

        public static FocusHistoryEntry? GetDay(DateTime date)
        {
            var history = LoadAll();
            string key = date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return history.TryGetValue(key, out var entry) ? entry : null;
        }

        private static Dictionary<string, FocusHistoryEntry> LoadAll()
        {
            try
            {
                if (!File.Exists(HistoryFilePath))
                {
                    return new Dictionary<string, FocusHistoryEntry>();
                }

                string json = File.ReadAllText(HistoryFilePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, FocusHistoryEntry>>(json);
                return data ?? new Dictionary<string, FocusHistoryEntry>();
            }
            catch
            {
                return new Dictionary<string, FocusHistoryEntry>();
            }
        }

        private static void SaveAll(Dictionary<string, FocusHistoryEntry> history)
        {
            try
            {
                Directory.CreateDirectory(AppFolderPath);
                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(HistoryFilePath, json);
            }
            catch
            {
            }
        }

        private static bool TryParseDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out date);
        }

        private static int[] NormalizeHourly(int[]? hourly)
        {
            var normalized = new int[24];
            if (hourly == null)
            {
                return normalized;
            }

            int length = Math.Min(24, hourly.Length);
            Array.Copy(hourly, normalized, length);
            return normalized;
        }
    }
}
