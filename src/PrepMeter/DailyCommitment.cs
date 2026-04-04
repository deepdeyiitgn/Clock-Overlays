using System;
using System.Collections.Generic;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Represents a single day's preparation log including subject progress, goals, and ratings.
    /// </summary>
    public class DailyCommitment
    {
        /// <summary>
        /// The date for which this commitment log is recorded.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// User's verification of whether yesterday's goal was achieved.
        /// Valid values: "Yes", "Partial", "No".
        /// </summary>
        public string YesterdayVerification { get; set; } = string.Empty;

        /// <summary>
        /// Daily progress statistics for each subject.
        /// </summary>
        public List<SubjectStat> SubjectLogs { get; set; } = new List<SubjectStat>();

        /// <summary>
        /// The goal the user intends to accomplish tomorrow.
        /// </summary>
        public string TomorrowGoal { get; set; } = string.Empty;

        /// <summary>
        /// User's focus rating for the day on a scale of 1-10.
        /// </summary>
        public int FocusRating { get; set; }

        /// <summary>
        /// User's mood rating for the day on a scale of 1-10.
        /// </summary>
        public int MoodRating { get; set; }

        /// <summary>
        /// Tags representing common mistakes made during the day for review and improvement.
        /// </summary>
        public List<string> MistakeTags { get; set; } = new List<string>();
    }
}
