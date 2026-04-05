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

            var start = date.Date.AddHours(hour);
            var end = start.AddMinutes(minutes);
            AddFocusSegments(new List<(DateTime Start, DateTime End)> { (start, end) });
        }

        public static void AddFocusSegments(IReadOnlyList<(DateTime Start, DateTime End)> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                return;
            }

            var history = LoadAll();
            var secondsByHour = new Dictionary<(DateTime Date, int Hour), int>();

            foreach (var segment in segments)
            {
                if (segment.End <= segment.Start)
                {
                    continue;
                }

                var current = segment.Start;
                var end = segment.End;

                while (current < end)
                {
                    var hourStart = new DateTime(current.Year, current.Month, current.Day, current.Hour, 0, 0, current.Kind);
                    var hourEnd = hourStart.AddHours(1);
                    var overlapEnd = end < hourEnd ? end : hourEnd;
                    int overlapSeconds = (int)Math.Floor((overlapEnd - current).TotalSeconds);

                    if (overlapSeconds > 0)
                    {
                        var key = (hourStart.Date, hourStart.Hour);
                        if (!secondsByHour.ContainsKey(key))
                        {
                            secondsByHour[key] = 0;
                        }
                        secondsByHour[key] += overlapSeconds;
                    }

                    current = overlapEnd;
                }
            }

            if (secondsByHour.Count == 0)
            {
                return;
            }

            int totalSeconds = secondsByHour.Values.Sum();
            int totalMinutes = totalSeconds / 60;
            if (totalMinutes <= 0)
            {
                return;
            }

            var minutesByHour = new Dictionary<(DateTime Date, int Hour), int>();
            foreach (var kvp in secondsByHour)
            {
                minutesByHour[kvp.Key] = kvp.Value / 60;
            }

            int allocatedMinutes = minutesByHour.Values.Sum();
            int remainingMinutes = totalMinutes - allocatedMinutes;
            if (remainingMinutes > 0)
            {
                var byRemainder = secondsByHour
                    .Select(kvp => new { kvp.Key, Remainder = kvp.Value % 60 })
                    .OrderByDescending(item => item.Remainder)
                    .ToList();

                for (int i = 0; i < remainingMinutes && i < byRemainder.Count; i++)
                {
                    var key = byRemainder[i].Key;
                    minutesByHour[key] += 1;
                }
            }

            foreach (var kvp in minutesByHour)
            {
                if (kvp.Value <= 0)
                {
                    continue;
                }

                string key = kvp.Key.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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

                entry.HourlyFocus[kvp.Key.Hour] += kvp.Value;
                entry.TotalFocusMinutes += kvp.Value;
            }

            history = history.OrderBy(item => item.Date).ToList();
            SaveAll(history);
        }

        public static IReadOnlyList<FocusHistory> GetLast7Days()
        {
            return GetAll();
        }

        public static IReadOnlyList<FocusHistory> GetAll()
        {
            var history = LoadAll();
            return history
                .Where(item => TryParseDate(item.Date, out _))
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