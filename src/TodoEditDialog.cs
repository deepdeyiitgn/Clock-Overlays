using System;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Modal dialog for editing a TodoItem with all fields
    /// </summary>
    public class TodoEditDialog : Form
    {
        private readonly TextBox titleTextBox;
        private readonly DateTimePicker fromDatePicker;
        private readonly DateTimePicker toDatePicker;
        private readonly CheckBox isCompletedCheckBox;
        private readonly Label completedAtLabel;
        private readonly Button saveButton;
        private readonly Button cancelButton;

        private TodoItem editingItem;

        public TodoEditDialog(TodoItem item)
        {
            editingItem = item ?? new TodoItem();

            Text = "Edit Task";
            Size = new Size(400, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 6
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Title
            layout.Controls.Add(new Label { Text = "Title:", AutoSize = true }, 0, 0);
            titleTextBox = new TextBox { Dock = DockStyle.Fill, Text = editingItem.Title };
            layout.Controls.Add(titleTextBox, 1, 0);

            // From Date
            layout.Controls.Add(new Label { Text = "From Date:", AutoSize = true }, 0, 1);
            fromDatePicker = new DateTimePicker { Dock = DockStyle.Fill, Value = editingItem.FromDate };
            layout.Controls.Add(fromDatePicker, 1, 1);

            // To Date
            layout.Controls.Add(new Label { Text = "To Date:", AutoSize = true }, 0, 2);
            toDatePicker = new DateTimePicker { Dock = DockStyle.Fill, Value = editingItem.ToDate };
            layout.Controls.Add(toDatePicker, 1, 2);

            // Is Completed
            layout.Controls.Add(new Label { Text = "Completed:", AutoSize = true }, 0, 3);
            isCompletedCheckBox = new CheckBox { Checked = editingItem.IsCompleted, AutoSize = true };
            layout.Controls.Add(isCompletedCheckBox, 1, 3);

            // Completed At
            completedAtLabel = new Label
            {
                Text = editingItem.CompletedAt.HasValue
                    ? $"Completed: {editingItem.CompletedAt:g}"
                    : "Not completed",
                AutoSize = true,
                ForeColor = System.Drawing.Color.Gray
            };
            layout.Controls.Add(new Label { Text = "Status:", AutoSize = true }, 0, 4);
            layout.Controls.Add(completedAtLabel, 1, 4);

            // Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Dock = DockStyle.Bottom
            };

            cancelButton = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
            saveButton = new Button { Text = "Save", AutoSize = true, DialogResult = DialogResult.None };

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);

            layout.Controls.Add(buttonPanel, 0, 5);
            layout.SetColumnSpan(buttonPanel, 2);

            Controls.Add(layout);

            saveButton.Click += SaveButton_Click;
            CancelButton = cancelButton;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Validate title
            string title = titleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Title cannot be empty", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update the item
            editingItem.Title = title;
            editingItem.FromDate = fromDatePicker.Value.Date;
            editingItem.ToDate = toDatePicker.Value.Date;
            bool wasCompleted = editingItem.IsCompleted;
            editingItem.IsCompleted = isCompletedCheckBox.Checked;

            // Set CompletedAt timestamp if just completed
            if (!wasCompleted && isCompletedCheckBox.Checked)
            {
                editingItem.CompletedAt = DateTime.UtcNow;
            }
            else if (wasCompleted && !isCompletedCheckBox.Checked)
            {
                editingItem.CompletedAt = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Get the edited TodoItem
        /// </summary>
        public TodoItem GetItem()
        {
            return editingItem;
        }
    }
}
