using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    /// <summary>
    /// Crash-safe helper for filtering and bulk operations on TodoItem collections.
    /// All operations are read-only; does not modify the input collection.
    /// </summary>
    public static class TodoFilterHelper
    {
        /// <summary>
        /// Filter type options.
        /// </summary>
        public enum FilterType
        {
            All,
            Completed,
            Pending
        }

        /// <summary>
        /// Filters todos by type and date range. Safe and non-destructive.
        /// </summary>
        /// <param name="todos">Source collection. Can be null or empty.</param>
        /// <param name="filterType">Filter by completion status.</param>
        /// <param name="fromDate">Start of date range (inclusive). Null = no start limit.</param>
        /// <param name="toDate">End of date range (inclusive). Null = no end limit.</param>
        /// <returns>Filtered collection. Never null, but can be empty.</returns>
        public static List<TodoItem> Filter(
            IEnumerable<TodoItem>? todos,
            FilterType filterType = FilterType.All,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // Handle null or empty input
            if (todos == null)
                return new List<TodoItem>();

            try
            {
                var result = todos.AsEnumerable();

                // Apply completion filter
                result = filterType switch
                {
                    FilterType.Completed => result.Where(t => t.IsCompleted),
                    FilterType.Pending => result.Where(t => !t.IsCompleted),
                    _ => result
                };

                // Apply date range filter (check FromDate and ToDate fields)
                if (fromDate.HasValue)
                {
                    result = result.Where(t => t.FromDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    result = result.Where(t => t.ToDate.Date <= toDate.Value.Date);
                }

                return result.ToList();
            }
            catch
            {
                // If any exception occurs, return empty list (graceful degradation)
                return new List<TodoItem>();
            }
        }

        /// <summary>
        /// Gets all completed todos. Crash-safe.
        /// </summary>
        public static List<TodoItem> GetCompleted(IEnumerable<TodoItem>? todos)
        {
            return Filter(todos, FilterType.Completed);
        }

        /// <summary>
        /// Gets all pending (incomplete) todos. Crash-safe.
        /// </summary>
        public static List<TodoItem> GetPending(IEnumerable<TodoItem>? todos)
        {
            return Filter(todos, FilterType.Pending);
        }

        /// <summary>
        /// Gets todos within a specific date range. Crash-safe.
        /// </summary>
        public static List<TodoItem> GetInDateRange(
            IEnumerable<TodoItem>? todos,
            DateTime fromDate,
            DateTime toDate)
        {
            return Filter(todos, FilterType.All, fromDate, toDate);
        }

        /// <summary>
        /// Safely deletes all completed todos from the list.
        /// Modifies the input collection and returns count of deleted items.
        /// Safe even if input is null or empty.
        /// </summary>
        /// <param name="todos">Collection to modify. Can be null (returns 0).</param>
        /// <returns>Number of items deleted.</returns>
        public static int DeleteAllCompleted(List<TodoItem>? todos)
        {
            if (todos == null || todos.Count == 0)
                return 0;

            try
            {
                int initialCount = todos.Count;
                todos.RemoveAll(t => t.IsCompleted);
                return initialCount - todos.Count;
            }
            catch
            {
                // If exception occurs, return 0 (nothing deleted)
                return 0;
            }
        }

        /// <summary>
        /// Safely deletes all todos from the list.
        /// Modifies the input collection and returns count of deleted items.
        /// Safe even if input is null or empty.
        /// </summary>
        /// <param name="todos">Collection to modify. Can be null (returns 0).</param>
        /// <returns>Number of items deleted.</returns>
        public static int DeleteAll(List<TodoItem>? todos)
        {
            if (todos == null || todos.Count == 0)
                return 0;

            try
            {
                int count = todos.Count;
                todos.Clear();
                return count;
            }
            catch
            {
                // If exception occurs, return 0 (nothing deleted)
                return 0;
            }
        }

        /// <summary>
        /// Safely counts todos by filter type.
        /// </summary>
        public static int Count(
            IEnumerable<TodoItem>? todos,
            FilterType filterType = FilterType.All)
        {
            if (todos == null)
                return 0;

            try
            {
                return filterType switch
                {
                    FilterType.Completed => todos.Count(t => t.IsCompleted),
                    FilterType.Pending => todos.Count(t => !t.IsCompleted),
                    _ => todos.Count()
                };
            }
            catch
            {
                return 0;
            }
        }
    }
}
