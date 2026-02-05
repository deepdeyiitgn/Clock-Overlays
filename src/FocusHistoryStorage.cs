using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TransparentClock
{
    public static class FocusHistoryStorage
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

            var entry = history.FirstOrDefault(item => item.Date == key);
            if (entry == null)
            {
                entry = new FocusHistory { Date = key, TotalFocusMinutes = 0 };
                history.Add(entry);
            }

            if (entry.HourlyFocus == null || entry.HourlyFocus.Length != 24)
            {
                entry.HourlyFocus = NormalizeHourly(entry.HourlyFocus);
            }

            entry.TotalFocusMinutes += minutes;
            entry.HourlyFocus[hour] += minutes;

            DateTime cutoff = DateTime.Today.AddDays(-6);
            history = history
                .Where(item => TryParseDate(item.Date, out var parsed) && parsed >= cutoff)
                .OrderBy(item => item.Date)
                .ToList();

            SaveAll(history);
        }

        public static IReadOnlyList<FocusHistory> GetLast7Days()
        {
            var history = LoadAll();
            DateTime cutoff = DateTime.Today.AddDays(-6);

            return history
                .Where(item => TryParseDate(item.Date, out var parsed) && parsed >= cutoff)
                .OrderBy(item => item.Date)
                .ToList();
        }

        public static FocusHistory? GetDay(DateTime date)
        {
            var history = LoadAll();
            string key = date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return history.FirstOrDefault(item => item.Date == key);
        }

        private static List<FocusHistory> LoadAll()
        {
            try
            {
                if (!File.Exists(HistoryFilePath))
                {
                    return new List<FocusHistory>();
                }

                string json = File.ReadAllText(HistoryFilePath);
                var data = JsonSerializer.Deserialize<List<FocusHistory>>(json);
                var list = data ?? new List<FocusHistory>();
                foreach (var entry in list)
                {
                    if (entry.HourlyFocus == null || entry.HourlyFocus.Length != 24)
                    {
                        entry.HourlyFocus = NormalizeHourly(entry.HourlyFocus);
                    }
                }
                return list;
            }
            catch
            {
                return new List<FocusHistory>();
            }
        }

        private static void SaveAll(List<FocusHistory> history)
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