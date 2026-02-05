using System;

namespace TransparentClock
{
    /// <summary>
    /// Production-safe data model for a to-do item.
    /// Designed for in-memory storage and JSON persistence.
    /// No UI-related fields or dependencies.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Unique identifier for this to-do item.
        /// Auto-generated as a new GUID on instantiation.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The title or description of this to-do item.
        /// Defaults to empty string. Caller responsible for validation.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether this to-do item is marked as completed.
        /// False by default.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// The start date for this to-do item (when work should begin).
        /// Defaults to today.
        /// </summary>
        public DateTime FromDate { get; set; } = DateTime.Today;

        /// <summary>
        /// The due date or target completion date for this to-do item.
        /// Defaults to today.
        /// </summary>
        public DateTime ToDate { get; set; } = DateTime.Today;

        /// <summary>
        /// When this to-do item was marked as completed, in UTC.
        /// Null if not yet completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// When this to-do item was created, in UTC.
        /// Defaults to DateTime.UtcNow at instantiation.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
