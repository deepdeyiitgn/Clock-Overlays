using System;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Safe event wiring for WinForms ListView controls managing to-do items.
    /// Handles checkbox toggles and delete operations with proper UI thread synchronization
    /// and comprehensive error handling.
    /// </summary>
    public static class TodoListViewEventWirer
    {
        /// <summary>
        /// Wires up a ListView ItemChecked event to toggle todos via TodoManager.
        /// 
        /// This handler:
        /// - Executes on the UI thread (guaranteed by WinForms event model)
        /// - Reads TodoItem.Id from ListViewItem.Tag
        /// - Toggles the todo via TodoManager
        /// - Re-renders the list via the provided renderCallback
        /// - Silently ignores invalid items (missing Tag, not a Guid)
        /// - Does not throw exceptions (fails gracefully)
        /// 
        /// Usage:
        /// <code>
        /// listView.ItemChecked += (sender, e) =>
        /// {
        ///     TodoListViewEventWirer.HandleCheckboxToggle(
        ///         e,
        ///         todoManager,
        ///         () => TodoListViewRenderer.RenderTodos(listView, todoManager.GetAllTodos())
        ///     );
        /// };
        /// </code>
        /// </summary>
        /// <param name="e">The ItemCheckedEventArgs from ListView.ItemChecked event.</param>
        /// <param name="manager">The TodoManager instance managing todos. Must not be null.</param>
        /// <param name="renderCallback">Delegate to re-render the list after toggle.
        /// Typically calls TodoListViewRenderer.RenderTodos(). Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if manager or renderCallback is null.</exception>
        public static void HandleCheckboxToggle(
            ItemCheckedEventArgs e,
            TodoManager manager,
            Action renderCallback)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            if (renderCallback == null)
                throw new ArgumentNullException(nameof(renderCallback));

            // Validate that the ItemCheckedEventArgs and Item are valid
            if (e == null || e.Item == null)
                return;

            // Extract the TodoItem.Id from the Tag
            // Safe to proceed only if Tag is a valid Guid
            if (!(e.Item.Tag is Guid todoId))
                return;

            // Toggle the todo in the manager
            // Safe operation: returns bool indicating success
            var toggled = manager.ToggleTodo(todoId);

            // Re-render the list to reflect the change
            // Always render, even if toggle failed (prevents UI stale state)
            try
            {
                renderCallback();
            }
            catch
            {
                // Silently fail on render errors - list may be disposed or invalid
                // Do not throw; keep the application stable
            }
        }

        /// <summary>
        /// Wires up a delete button Click event to remove the selected todo.
        /// 
        /// This handler:
        /// - Executes on the UI thread (guaranteed by WinForms event model)
        /// - Validates that an item is selected in the ListView
        /// - Reads TodoItem.Id from SelectedItem.Tag
        /// - Removes the todo via TodoManager
        /// - Re-renders the list via the provided renderCallback
        /// - Silently ignores invalid selections
        /// - Does not throw exceptions (fails gracefully)
        /// 
        /// Usage:
        /// <code>
        /// deleteButton.Click += (sender, e) =>
        /// {
        ///     TodoListViewEventWirer.HandleDeleteClick(
        ///         listView,
        ///         todoManager,
        ///         () => TodoListViewRenderer.RenderTodos(listView, todoManager.GetAllTodos())
        ///     );
        /// };
        /// </code>
        /// </summary>
        /// <param name="listView">The ListView control with selected items. Must not be null.</param>
        /// <param name="manager">The TodoManager instance managing todos. Must not be null.</param>
        /// <param name="renderCallback">Delegate to re-render the list after deletion.
        /// Typically calls TodoListViewRenderer.RenderTodos(). Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if listView, manager, or renderCallback is null.</exception>
        public static void HandleDeleteClick(
            ListView listView,
            TodoManager manager,
            Action renderCallback)
        {
            if (listView == null)
                throw new ArgumentNullException(nameof(listView));

            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            if (renderCallback == null)
                throw new ArgumentNullException(nameof(renderCallback));

            // Validate that an item is selected
            if (listView.SelectedItems.Count == 0)
                return;

            // Get the first (and typically only) selected item
            var selectedItem = listView.SelectedItems[0];

            // Extract the TodoItem.Id from the Tag
            // Safe to proceed only if Tag is a valid Guid
            if (!(selectedItem.Tag is Guid todoId))
                return;

            // Remove the todo from the manager
            // Safe operation: returns bool indicating success
            var removed = manager.RemoveTodo(todoId);

            // Re-render the list to reflect the deletion
            // Always render, even if removal failed (prevents UI stale state)
            try
            {
                renderCallback();
            }
            catch
            {
                // Silently fail on render errors - list may be disposed or invalid
                // Do not throw; keep the application stable
            }
        }
    }
}
