using System;
using System.Collections.Generic;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Represents a user's exam preparation profile with target exam details and timeline.
    /// </summary>
    public class PrepProfile
    {
        /// <summary>
        /// The target exam the user is preparing for (e.g., "JEE Main", "NEET", "SAT").
        /// </summary>
        public string TargetExam { get; set; } = string.Empty;

        /// <summary>
        /// List of subjects being prepared for the exam.
        /// </summary>
        public List<string> Subjects { get; set; } = new List<string>();

        /// <summary>
        /// The date when the user started their preparation journey.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The target date for the exam.
        /// </summary>
        public DateTime TargetDate { get; set; }
    }
}
