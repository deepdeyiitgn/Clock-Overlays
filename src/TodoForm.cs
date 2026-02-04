using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock
{
    public class TodoForm : Form
    {
        private static readonly Font BaseFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private static readonly Color SubtleText = Color.FromArgb(110, 110, 110);

        private readonly TextBox titleTextBox;
        private readonly Button addButton;
        private readonly ListView todoListView;
        private readonly Button deleteButton;
        private bool isLoading;
        private readonly List<TodoItem> items;

        public TodoForm()
        {
            Text = "To-Do";
            Size = new Size(420, 420);
            Font = BaseFont;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
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

            todoListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true
            };
            todoListView.Columns.Add("Task", 240);
            todoListView.Columns.Add("Created", 120);

            deleteButton = new Button
            {
                Text = "Delete",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };

            layout.Controls.Add(inputPanel, 0, 0);
            layout.Controls.Add(todoListView, 0, 1);
            layout.Controls.Add(deleteButton, 0, 2);
            Controls.Add(layout);

            items = TodoStorage.Load();

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

            todoListView.ItemChecked += (_, e) =>
            {
                if (isLoading || e.Item?.Tag is not TodoItem item)
                {
                    return;
                }

                item.IsCompleted = e.Item.Checked;
                item.CompletedDate = item.IsCompleted ? DateTime.UtcNow : null;
                Save();
                RefreshList();
            };

            deleteButton.Click += (_, __) => DeleteSelected();

            RefreshList();
        }

        private void AddTodo()
        {
            string title = titleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            var item = new TodoItem
            {
                Title = title,
                CreatedDate = DateTime.UtcNow,
                IsCompleted = false,
                CompletedDate = null
            };

            items.Add(item);
            Save();
            titleTextBox.Clear();
            RefreshList();
        }

        private void DeleteSelected()
        {
            if (todoListView.SelectedItems.Count == 0)
            {
                return;
            }

            if (todoListView.SelectedItems[0].Tag is not TodoItem item)
            {
                return;
            }

            items.RemoveAll(existing => existing.Id == item.Id);
            Save();
            RefreshList();
        }

        private void RefreshList()
        {
            isLoading = true;
            todoListView.Items.Clear();

            DateTime today = DateTime.Today;
            foreach (var item in items
                .Where(todo => todo.CreatedDate.Date == today)
                .OrderByDescending(todo => todo.CreatedDate))
            {
                var listItem = new ListViewItem(item.Title)
                {
                    Tag = item,
                    Checked = item.IsCompleted
                };
                listItem.SubItems.Add(item.CreatedDate.ToString("yyyy-MM-dd"));
                if (item.IsCompleted)
                {
                    listItem.ForeColor = SubtleText;
                }
                todoListView.Items.Add(listItem);
            }

            isLoading = false;
        }

        private void Save()
        {
            TodoStorage.Save(items);
        }
    }
}
