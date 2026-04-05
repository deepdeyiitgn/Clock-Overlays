using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Utility for safely rendering TodoItem data into WinForms ListView controls.
    /// Eliminates ListViewItem lifecycle crashes by never reusing control objects
    /// and using proper WinForms batching patterns.
    /// </summary>
    public static class TodoListViewRenderer
    {
        /// <summary>
        /// Safely renders a collection of TodoItem objects into a ListView control.
        /// 
        /// This method:
        /// - Clears all existing items from the ListView
        /// - Creates fresh ListViewItem objects for each TodoItem
        /// - Never reuses ListViewItem references (prevents lifetime issues)
        /// - Uses BeginUpdate/EndUpdate for performance
        /// - Stores TodoItem.Id in ListViewItem.Tag (data identity)
        /// - Sets Checked state from TodoItem.IsCompleted
        /// 
        /// Recommended ListView configuration:
        /// - View = View.Details
        /// - FullRowSelect = true
        /// - CheckBoxes = true
        /// </summary>
        /// <param name="listView">The ListView control to render into. Must not be null.</param>
        /// <param name="todos">The collection of TodoItem objects to render. Can be empty or null (treated as empty).</param>
        /// <exception cref="ArgumentNullException">Thrown if listView is null.</exception>
        public static void RenderTodos(ListView listView, IEnumerable<TodoItem> todos)
        {
            if (listView == null)
                throw new ArgumentNullException(nameof(listView));

            // Batch update for performance
            listView.BeginUpdate();

            try
            {
                // Clear all existing items - fresh start prevents stale references
                listView.Items.Clear();

                // Handle null or empty collections gracefully
                if (todos == null)
                    return;

                // Render each TodoItem as a fresh ListViewItem
                foreach (var todo in todos)
                {
                    // Create a new ListViewItem - never reuse control objects
                    var item = new ListViewItem(todo.Title)
                    {
                        // Store only the ID, not the TodoItem reference
                        // This prevents lifecycle issues and keeps data separate from UI
                        Tag = todo.Id,
                        
                        // Set checked state from data model
                        Checked = todo.IsCompleted
                    };

                    // Apply visual styling for completed tasks
                    if (todo.IsCompleted)
                    {
                        // Gray color and strikethrough for completed items
                        item.ForeColor = Color.Gray;
                        item.Font = new Font(item.Font, FontStyle.Strikeout);
                    }

                    // Add to ListView
                    listView.Items.Add(item);
                }
            }
            finally
            {
                // Always end update, even if an exception occurs
                listView.EndUpdate();
            }
        }

        /// <summary>
        /// Safely renders a collection of TodoItem objects into a ListView control
        /// with support for sub-items (columns).
        /// 
        /// This overload allows specifying which columns to populate from TodoItem data.
        /// The first column (index 0) is always the Title (primary ListViewItem text).
        /// Additional columns can be populated via the columnSelector delegate.
        /// </summary>
        /// <param name="listView">The ListView control to render into. Must not be null.</param>
        /// <param name="todos">The collection of TodoItem objects to render.</param>
        /// <param name="columnSelector">Delegate that returns sub-item values for each TodoItem.
        /// Input: the TodoItem. Output: array of strings for columns 1, 2, 3, etc.
        /// Can return null or empty array for no additional columns.</param>
        /// <exception cref="ArgumentNullException">Thrown if listView or columnSelector is null.</exception>
        public static void RenderTodos(
            ListView listView,
            IEnumerable<TodoItem> todos,
            Func<TodoItem, string[]> columnSelector)
        {
            if (listView == null)
                throw new ArgumentNullException(nameof(listView));

            if (columnSelector == null)
                throw new ArgumentNullException(nameof(columnSelector));

            listView.BeginUpdate();

            try
            {
                listView.Items.Clear();

                if (todos == null)
                    return;

                foreach (var todo in todos)
                {
                    // Create fresh ListViewItem with title as primary column
                    var item = new ListViewItem(todo.Title)
                    {
                        Tag = todo.Id,
                        Checked = todo.IsCompleted
                    };

                    // Apply visual styling for completed tasks
                    if (todo.IsCompleted)
                    {
                        // Gray color and strikethrough for completed items
                        item.ForeColor = Color.Gray;
                        item.Font = new Font(item.Font, FontStyle.Strikeout);
                    }

                    // Add sub-items (columns 1, 2, 3, etc.) from the selector
                    var subItems = columnSelector(todo);
                    if (subItems != null)
                    {
                        foreach (var subItemText in subItems)
                        {
                            item.SubItems.Add(subItemText ?? string.Empty);
                        }
                    }

                    listView.Items.Add(item);
                }
            }
            finally
            {
                listView.EndUpdate();
            }
        }
    }
}
