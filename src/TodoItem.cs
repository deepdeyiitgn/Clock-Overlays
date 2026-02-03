using System;

namespace TransparentClock
{
    public class TodoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;
    }
}
