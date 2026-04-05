using System;

namespace TransparentClock
{
    public class FocusHistoryEntry
    {
        public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public int TotalFocusMinutes { get; set; }
        public int[] HourlyFocus { get; set; } = new int[24];
    }
}
