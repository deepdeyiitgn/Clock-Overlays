using System;
using System.Collections.Generic;

namespace TransparentClock.PrepMeter
{
    /// <summary>
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