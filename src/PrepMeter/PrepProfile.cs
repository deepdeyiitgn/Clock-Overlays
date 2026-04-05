using System;
using System.Collections.Generic;

namespace TransparentClock.PrepMeter
{
    /// <summary>
<<<<<<< HEAD
    /// Stores the user's preparation configuration.
    /// </summary>
    public class PrepProfile
    {
        public string TargetExam { get; set; } = "JEE Advanced";
        public DateTime TargetDate { get; set; } = DateTime.Today.AddYears(1);
        
        // NEW: Preparation Start Date
        public DateTime StartDate { get; set; } = DateTime.Today;
        
        public List<string> Subjects { get; set; } = new List<string> { "Physics", "Chemistry", "Mathematics" };
    }
}
=======
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
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
