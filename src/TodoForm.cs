using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// To-Do list form with stable data model and safe UI updates.
    /// Data is stored in List&lt;TodoItem&gt;; ListView is rebuilt on every change to avoid lifecycle issues.
    /// All UI operations occur on the UI thread.
    /// </summary>
    public class TodoForm : Form
    {
        private static readonly Font BaseFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private static readonly Color SubtleText = Color.FromArgb(110, 110, 110);

        private readonly TextBox titleTextBox;
        private readonly Button addButton;
        private readonly ListView todoListView;
        private readonly Button deleteButton;
        private readonly ComboBox filterComboBox;
        private readonly DateTimePicker fromDatePicker;
        private readonly DateTimePicker toDatePicker;
        private readonly Button deleteCompletedButton;
        private readonly Button deleteAllButton;
        
        // Data model - single source of truth
        private List<TodoItem> todos;
        
        // Current filter state
        private TodoFilterHelper.FilterType currentFilter = TodoFilterHelper.FilterType.All;
        private DateTime? currentFromDate;
        private DateTime? currentToDate;
        
        // Flag to prevent event re-entry during UI updates
        private bool isUpdatingUI;

        public TodoForm()
        {
            Text = "To-Do";
            Size = new Size(600, 500);
            Font = BaseFont;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 5
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var inputPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };

            titleTextBox = new TextBox { Width = 240 };
            addButton = new Button { Text = "Add", AutoSize = true };

            inputPanel.Controls.Add(titleTextBox);
            inputPanel.Controls.Add(addButton);

            // Filter panel with date range and completion status filters
            var filterPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };

            Label BuildFilterLabel(string text, int leftMargin) => new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = SubtleText,
                Margin = new Padding(leftMargin, 6, 6, 0)
            };

            filterComboBox = new ComboBox
            {
                Items = { "All", "Completed", "Pending" },
                SelectedIndex = 0,
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 2, 0, 0)
            };

            fromDatePicker = new DateTimePicker
            {
                Value = DateTime.Today.AddDays(-7),
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(0, 2, 0, 0)
            };

            toDatePicker = new DateTimePicker
            {
                Value = DateTime.Today.AddDays(7),
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(0, 2, 0, 0)
            };

            filterPanel.Controls.Add(BuildFilterLabel("Status", 0));
            filterPanel.Controls.Add(filterComboBox);
            filterPanel.Controls.Add(BuildFilterLabel("From", 12));
            filterPanel.Controls.Add(fromDatePicker);
            filterPanel.Controls.Add(BuildFilterLabel("To", 10));
            filterPanel.Controls.Add(toDatePicker);

            todoListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                HideSelection = false,
                MultiSelect = false,
                GridLines = true
            };
            todoListView.Columns.Add("Task", 260, HorizontalAlignment.Left);
            todoListView.Columns.Add("From", 90, HorizontalAlignment.Left);
            todoListView.Columns.Add("To", 90, HorizontalAlignment.Left);
            todoListView.Columns.Add("Created", 90, HorizontalAlignment.Left);

            var rowImageList = new ImageList
            {
                ImageSize = new Size(1, 26)
            };
            todoListView.SmallImageList = rowImageList;

            // Add context menu for right-click
            var contextMenu = new ContextMenuStrip();
            var editMenuItem = new ToolStripMenuItem("Edit", null, (_, __) => EditSelected());
            var toggleMenuItem = new ToolStripMenuItem("Mark as Done", null, (_, __) => ToggleSelectedCompletion());
            var deleteMenuItem = new ToolStripMenuItem("Delete", null, (_, __) => DeleteSelected());
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(toggleMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            todoListView.ContextMenuStrip = contextMenu;

            // Action buttons panel
            var actionPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 0)
            };

            deleteButton = new Button { Text = "Delete Selected", AutoSize = true };
            deleteCompletedButton = new Button { Text = "Delete Completed", AutoSize = true, Margin = new Padding(4, 0, 0, 0) };
            deleteAllButton = new Button { Text = "Delete All", AutoSize = true, Margin = new Padding(4, 0, 0, 0) };

            actionPanel.Controls.Add(deleteButton);
            actionPanel.Controls.Add(deleteCompletedButton);
            actionPanel.Controls.Add(deleteAllButton);

            layout.Controls.Add(inputPanel, 0, 0);
            layout.Controls.Add(filterPanel, 0, 1);
            layout.Controls.Add(todoListView, 0, 2);
            layout.Controls.Add(actionPanel, 0, 3);
            Controls.Add(layout);

            // Load data from storage
            todos = TodoStorage.Load();

            // Wire up event handlers
            addButton.Click += (_, __) => AddTodo();
            titleTextBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AddTodo();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            todoListView.ItemChecked += (_, e) => HandleItemChecked(e);
            todoListView.MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var item = todoListView.GetItemAt(e.X, e.Y);
                    if (item != null)
                    {
                        item.Selected = true;
                        todoListView.FocusedItem = item;

                        toggleMenuItem.Text = item.Checked ? "Undo" : "Mark as Done";
                    }
                }
            };
            deleteButton.Click += (_, __) => DeleteSelected();
            filterComboBox.SelectedIndexChanged += (_, __) => ApplyFilters();
            fromDatePicker.ValueChanged += (_, __) => ApplyFilters();
            toDatePicker.ValueChanged += (_, __) => ApplyFilters();
            deleteCompletedButton.Click += (_, __) => DeleteAllCompleted();
            deleteAllButton.Click += (_, __) => DeleteAllTodos();

            // Initial UI population
            RefreshUI();
        }

        /// <summary>
        /// Adds a new todo from the text input. Safe and idempotent.
        /// Must be called from the UI thread.
        /// </summary>
        private void AddTodo()
        {
            string title = titleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            // Create new todo item
            var newTodo = new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            // Update data model
            todos.Add(newTodo);

            // Persist to storage
            SaveToStorage();

            // Clear input and refresh UI
            titleTextBox.Clear();
            RefreshUI();
        }

        /// <summary>
        /// Toggles completion status of a todo item. Safe and re-entrant.
        /// Must be called from the UI thread.
        /// </summary>
        private void HandleItemChecked(ItemCheckedEventArgs e)
        {
            // Ignore events while rebuilding the list
            if (isUpdatingUI || e.Item?.Tag is not Guid itemId)
            {
                return;
            }

            // Find the todo in the data model
            var todo = todos.FirstOrDefault(t => t.Id == itemId);
            if (todo == null)
            {
                return;
            }

            // Update the data model
            todo.IsCompleted = e.Item.Checked;

            // Persist to storage
            SaveToStorage();

            // Refresh UI to reflect the change
            RefreshUI();
        }

        /// <summary>
        /// Deletes the selected todo item. Safe even if nothing is selected.
        /// Must be called from the UI thread.
        /// </summary>
        private void DeleteSelected()
        {
            if (todoListView.SelectedItems.Count == 0)
            {
                return;
            }

            if (todoListView.SelectedItems[0].Tag is not Guid itemId)
            {
                return;
            }

            // Remove from data model
            todos.RemoveAll(t => t.Id == itemId);

            // Persist to storage
            SaveToStorage();

            // Refresh UI
            RefreshUI();
        }

        /// <summary>
        /// Opens edit dialog for the selected todo item.
        /// Must be called from the UI thread.
        /// </summary>
        private void EditSelected()
        {
            if (todoListView.SelectedItems.Count == 0)
            {
                return;
            }

            if (todoListView.SelectedItems[0].Tag is not Guid itemId)
            {
                return;
            }

            // Find the todo in the data model
            var todo = todos.FirstOrDefault(t => t.Id == itemId);
            if (todo == null)
            {
                return;
            }

            // Open edit dialog
            using (var dialog = new TodoEditDialog(todo))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Get the edited item
                    var editedItem = dialog.GetItem();

                    // Update data model
                    var index = todos.FindIndex(t => t.Id == itemId);
                    if (index >= 0)
                    {
                        todos[index] = editedItem;

                        // Persist to storage
                        SaveToStorage();

                        // Refresh UI
                        RefreshUI();
                    }
                }
            }
        }

        /// <summary>
        /// Toggles completion status for the selected task.
        /// </summary>
        private void ToggleSelectedCompletion()
        {
            if (todoListView.SelectedItems.Count == 0)
            {
                return;
            }

            if (todoListView.SelectedItems[0].Tag is not Guid itemId)
            {
                return;
            }

            var todo = todos.FirstOrDefault(t => t.Id == itemId);
            if (todo == null)
            {
                return;
            }

            todo.IsCompleted = !todo.IsCompleted;
            todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;

            SaveToStorage();
            RefreshUI();
        }

        /// <summary>
        /// Applies current filter settings and refreshes the UI.
        /// Safe and crash-proof; handles null/empty collections gracefully.
        /// </summary>
        private void ApplyFilters()
        {
            try
            {
                // Parse filter type from combobox
                currentFilter = filterComboBox.SelectedIndex switch
                {
                    1 => TodoFilterHelper.FilterType.Completed,
                    2 => TodoFilterHelper.FilterType.Pending,
                    _ => TodoFilterHelper.FilterType.All
                };

                // Get date range from pickers
                currentFromDate = fromDatePicker.Value.Date;
                currentToDate = toDatePicker.Value.Date;

                // Refresh UI with filters applied
                RefreshUI();
            }
            catch
            {
                // If any error occurs, silently revert to showing all
                currentFilter = TodoFilterHelper.FilterType.All;
                currentFromDate = null;
                currentToDate = null;
                RefreshUI();
            }
        }

        /// <summary>
        /// Safely deletes all completed todos with confirmation.
        /// Crash-safe; handles storage errors gracefully.
        /// </summary>
        private void DeleteAllCompleted()
        {
            try
            {
                int completedCount = TodoFilterHelper.Count(todos, TodoFilterHelper.FilterType.Completed);
                if (completedCount == 0)
                {
                    MessageBox.Show("No completed tasks to delete.", "Delete Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Delete {completedCount} completed task(s)? This cannot be undone.",
                    "Delete Completed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    int deleted = TodoFilterHelper.DeleteAllCompleted(todos);
                    SaveToStorage();
                    RefreshUI();
                    MessageBox.Show($"Deleted {deleted} completed task(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting completed tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Safely deletes all todos with confirmation.
        /// Crash-safe; handles storage errors gracefully.
        /// </summary>
        private void DeleteAllTodos()
        {
            try
            {
                if (todos.Count == 0)
                {
                    MessageBox.Show("No tasks to delete.", "Delete All", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Delete all {todos.Count} task(s)? This cannot be undone.",
                    "Delete All",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    int deleted = TodoFilterHelper.DeleteAll(todos);
                    SaveToStorage();
                    RefreshUI();
                    MessageBox.Show($"Deleted {deleted} task(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting all tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Rebuilds the ListView from the todo data model.
        /// Called after every data change to ensure ListView state matches data.
        /// Must be called from the UI thread.
        /// </summary>
        private void RefreshUI()
        {
            // Set flag to prevent ItemChecked events during rebuild
            isUpdatingUI = true;

            try
            {
                // Clear all ListView items - never reuse ListViewItem objects
                todoListView.Items.Clear();

                // Apply current filters to the todo list
                var filteredTodos = TodoFilterHelper.Filter(
                    todos,
                    currentFilter,
                    currentFromDate,
                    currentToDate);

                // Sort by creation time (newest first)
                var sortedTodos = filteredTodos
                    .OrderByDescending(todo => todo.CreatedAt)
                    .ToList();

                // Rebuild ListView from filtered data model
                foreach (var todo in sortedTodos)
                {
                    var listItem = new ListViewItem(todo.Title)
                    {
                        Tag = todo.Id,  // Store only the ID, not the TodoItem
                        Checked = todo.IsCompleted
                    };

                    listItem.SubItems.Add(todo.FromDate.ToString("yyyy-MM-dd"));
                    listItem.SubItems.Add(todo.ToDate.ToString("yyyy-MM-dd"));
                    listItem.SubItems.Add(todo.CreatedAt.ToString("yyyy-MM-dd"));

                    // Apply visual style for completed items
                    if (todo.IsCompleted)
                    {
                        listItem.ForeColor = Color.FromArgb(130, 130, 130);
                        listItem.Font = new Font(listItem.Font, FontStyle.Strikeout);
                    }

                    todoListView.Items.Add(listItem);
                }

                todoListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                todoListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            catch
            {
                // If any exception occurs, clear list to prevent UI corruption
                todoListView.Items.Clear();
            }
            finally
            {
                // Always reset the flag, even if an exception occurs
                isUpdatingUI = false;
            }
        }

        /// <summary>
        /// Saves the todo list to persistent storage.
        /// Safe to call multiple times; handles storage errors gracefully.
        /// </summary>
        private void SaveToStorage()
        {
            TodoStorage.Save(todos);
        }
    }
}
