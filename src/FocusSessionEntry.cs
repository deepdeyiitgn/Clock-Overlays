using System;
using System.Text.Json.Serialization;

namespace TransparentClock
{
    public class FocusSessionEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int DurationMinutes
        {
            get
            {
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
