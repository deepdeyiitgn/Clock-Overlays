using System;
using System.Text.Json.Serialization;

namespace TransparentClock
{
    public class FocusSessionEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Source { get; set; } = string.Empty;

        public int? ActualMinutes { get; set; }

        public int? ActualSeconds { get; set; }

        public int DurationSeconds
        {
            get
            {
                if (ActualSeconds.HasValue && ActualSeconds.Value > 0)
                {
                    return ActualSeconds.Value;
                }

                if (ActualMinutes.HasValue && ActualMinutes.Value > 0)
                {
                    return ActualMinutes.Value * 60;
                }

                var seconds = (int)Math.Round((EndTime - StartTime).TotalSeconds);
                return Math.Max(0, seconds);
            }
        }

        public int DurationMinutes
        {
            get
            {
                if (ActualSeconds.HasValue && ActualSeconds.Value > 0)
                {
                    return (int)Math.Round(ActualSeconds.Value / 60d);
                }

                if (ActualMinutes.HasValue && ActualMinutes.Value > 0)
                {
                    return ActualMinutes.Value;
                }

                var minutes = (int)Math.Round((EndTime - StartTime).TotalMinutes);
                return Math.Max(0, minutes);
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? CompletedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? Minutes { get; set; }
    }
}
