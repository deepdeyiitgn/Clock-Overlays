namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Represents the daily progress statistics for a specific subject.
    /// </summary>
    public class SubjectStat
    {
        /// <summary>
        /// The name of the subject (e.g., "Physics", "Mathematics", "Biology").
        /// </summary>
        public string SubjectName { get; set; } = string.Empty;

        /// <summary>
        /// Number of lectures completed for this subject on the tracked day.
        /// </summary>
        public int LecturesCompleted { get; set; }

        /// <summary>
        /// Number of questions solved for this subject on the tracked day.
        /// </summary>
        public int QuestionsSolved { get; set; }

        /// <summary>
        /// Number of previous backlogs (incomplete tasks) cleared for this subject.
        /// </summary>
        public int BacklogsCleared { get; set; }

        /// <summary>
        /// Number of new backlogs added for this subject.
        /// </summary>
        public int BacklogsAdded { get; set; }
    }
}
