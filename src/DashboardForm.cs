using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Drawing.Text;
using System.Windows.Forms;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class DashboardForm : Form
{
    private static readonly Font BaseFont = new Font("Segoe UI", 10F, FontStyle.Regular);
    private static readonly Font HeaderFont = new Font("Segoe UI", 11F, FontStyle.Bold);
    private static readonly Font SectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
    private static readonly Font LabelFont = new Font("Segoe UI", 10.5F, FontStyle.Regular);
    private static readonly Color AppBackground = Color.FromArgb(247, 247, 247);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color SubtleText = Color.FromArgb(110, 110, 110);
    private static readonly Color SeparatorColor = Color.FromArgb(220, 220, 220);

    private readonly TransparentClockForm clockForm;
    private bool allowClose;
    private readonly CheckBox clockEnabledCheckBox;
    private readonly TrackBar fontSizeTrackBar;
    private readonly ComboBox fontFamilyComboBox;
    private readonly ComboBox colorComboBox;
    private readonly Button customColorButton;
    private readonly ComboBox positionComboBox;
    private readonly Label greetingLabel;
    private readonly PomodoroForm pomodoroForm;
    private readonly TextBox profileNameTextBox;
    private readonly ComboBox profileGenderComboBox;
    private readonly Button saveProfileButton;
    private readonly Button resetProfileButton;
    private readonly Button deleteDataButton;
    private readonly CheckBox launchOnStartupCheckBox;
    private readonly CheckBox showWelcomeCheckBox;
    private readonly CheckBox minimizeToTrayCheckBox;
    private readonly Button resetSettingsButton;
    private readonly Timer greetingTimer;
    private readonly TextBox todoInputTextBox;
    private readonly DateTimePicker todoStartDatePicker;
    private readonly DateTimePicker todoEndDatePicker;
    private readonly Button todoAddButton;
    private readonly Button todoDeleteButton;
    private readonly Button todoDeleteDoneButton;
    private readonly Button todoDeleteAllButton;
    private readonly ListView todoListView;
    private readonly CheckBox todoFilterCheckBox;
    private readonly DateTimePicker todoFilterDatePicker;
    private readonly ContextMenuStrip todoContextMenu;
    private bool isLoadingTodos;
    private List<TodoItem> todoItems = new List<TodoItem>();
    private readonly ListView focusHistoryListView;
    private readonly Panel insightsPanel;
    private readonly Label insightsEmptyLabel;
    private readonly Panel insightsDailyPanel;
    private readonly Label insightsDailyEmptyLabel;
    private readonly ComboBox insightsRangeComboBox;
    private readonly Label insightsComingSoonLabel;
    private DateTime? selectedInsightDate;

    public DashboardForm()
    {
        Text = "Transparent Clock";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(640, 460);
        Font = BaseFont;
        BackColor = AppBackground;

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };

        var clockTab = new TabPage("Clock") { Padding = new Padding(14), BackColor = AppBackground };
        var pomodoroTab = new TabPage("Pomodoro") { Padding = new Padding(14), BackColor = AppBackground };
        var profileTab = new TabPage("Profile") { Padding = new Padding(14), BackColor = AppBackground };
        var settingsTab = new TabPage("Settings") { Padding = new Padding(14), BackColor = AppBackground };
        var todoTab = new TabPage("To-Do") { Padding = new Padding(14), BackColor = AppBackground };
        var focusHistoryTab = new TabPage("Focus History") { Padding = new Padding(14), BackColor = AppBackground };
        var insightsTab = new TabPage("Insights") { Padding = new Padding(14), BackColor = AppBackground };

        tabs.TabPages.Add(clockTab);
        tabs.TabPages.Add(pomodoroTab);
        tabs.TabPages.Add(profileTab);
        tabs.TabPages.Add(settingsTab);
        tabs.TabPages.Add(todoTab);
        tabs.TabPages.Add(focusHistoryTab);
        tabs.TabPages.Add(insightsTab);

        Controls.Add(tabs);

        clockForm = new TransparentClockForm();
        Program.RegisterClockForm(clockForm);
        clockForm.Show();
        Program.ApplyClockStateToOverlay();

        var clockCard = CreateCardPanel();

        var clockLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 2,
            RowCount = 12
        };
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        clockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var clockHeader = new Label
        {
            AutoSize = true,
            Font = HeaderFont,
            Text = "Clock Controls",
            Margin = new Padding(0, 0, 0, 8)
        };

        var clockOverviewHeader = new Label
        {
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Text = "Overview",
            Margin = new Padding(0, 2, 0, 4)
        };

        greetingLabel = new Label
        {
            AutoSize = true,
            Font = HeaderFont,
            Text = Program.GetGreetingText(),
            Margin = new Padding(0, 0, 0, 8)
        };

        clockEnabledCheckBox = new CheckBox
        {
            Text = "Enable Clock Overlay",
            Checked = Program.CurrentState.ClockEnabled,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        clockEnabledCheckBox.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.ClockEnabled = clockEnabledCheckBox.Checked;
            clockForm.ApplyClockEnabled(clockEnabledCheckBox.Checked);
            clockForm.RefreshClockToggleText();
            AppStateStorage.Save(Program.CurrentState);
        };

        var fontSizeLabel = new Label
        {
            Text = "Font Size",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 2)
        };

        fontSizeTrackBar = new TrackBar
        {
            Minimum = 12,
            Maximum = 48,
            TickFrequency = 2,
            Value = (int)Math.Round(Program.CurrentState.ClockFontSize),
            Margin = new Padding(0, 0, 0, 8)
        };
        fontSizeTrackBar.ValueChanged += (_, __) =>
        {
            Program.CurrentState.ClockFontSize = fontSizeTrackBar.Value;
            clockForm.ApplyClockFontSize(Program.CurrentState.ClockFontSize);
            AppStateStorage.Save(Program.CurrentState);
        };

        var colorLabel = new Label
        {
            Text = "Color",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 2)
        };

        colorComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220
        };

        foreach (var name in Program.ClockColors.Keys.OrderBy(k => k))
        {
            colorComboBox.Items.Add(name);
        }
        colorComboBox.Items.Add("Custom");

        var colorPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        customColorButton = new Button
        {
            Text = "Pick Color",
            Width = 100
        };
        customColorButton.Click += (_, __) =>
        {
            using var dialog = new ColorDialog();
            if (Program.CurrentState.ClockCustomColorArgb.HasValue)
            {
                dialog.Color = Color.FromArgb(Program.CurrentState.ClockCustomColorArgb.Value);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Program.CurrentState.ClockUseCustomColor = true;
                Program.CurrentState.ClockCustomColorArgb = dialog.Color.ToArgb();
                colorComboBox.SelectedItem = "Custom";
                clockForm.ApplyClockColor(dialog.Color);
                AppStateStorage.Save(Program.CurrentState);
            }
        };

        colorComboBox.SelectedIndexChanged += (_, __) =>
        {
            if (colorComboBox.SelectedItem is string name)
            {
                if (string.Equals(name, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    Program.CurrentState.ClockUseCustomColor = true;
                    if (Program.CurrentState.ClockCustomColorArgb.HasValue)
                    {
                        var color = Color.FromArgb(Program.CurrentState.ClockCustomColorArgb.Value);
                        clockForm.ApplyClockColor(color);
                    }
                }
                else
                {
                    Program.CurrentState.ClockUseCustomColor = false;
                    Program.CurrentState.ClockColorName = name;
                    clockForm.ApplyClockColor(Program.ResolveClockColor(name));
                }

                AppStateStorage.Save(Program.CurrentState);
            }
        };

        colorPanel.Controls.Add(colorComboBox);
        colorPanel.Controls.Add(customColorButton);

        var positionLabel = new Label
        {
            Text = "Position",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 2)
        };

        var clockAppearanceHeader = new Label
        {
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Text = "Appearance",
            Margin = new Padding(0, 2, 0, 4)
        };

        var fontFamilyLabel = new Label
        {
            Text = "Font",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 2)
        };

        fontFamilyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220
        };

        foreach (var family in new InstalledFontCollection().Families.OrderBy(f => f.Name))
        {
            fontFamilyComboBox.Items.Add(family.Name);
        }

        fontFamilyComboBox.SelectedIndexChanged += (_, __) =>
        {
            if (fontFamilyComboBox.SelectedItem is string family)
            {
                Program.CurrentState.ClockFontFamily = family;
                clockForm.ApplyClockFontFamily(family);
                AppStateStorage.Save(Program.CurrentState);
            }
        };

        positionComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220
        };
        positionComboBox.Items.AddRange(new object[]
        {
            "Top Left",
            "Top Right",
            "Bottom Left",
            "Bottom Right",
            "Custom"
        });

        positionComboBox.SelectedIndexChanged += (_, __) =>
        {
            if (positionComboBox.SelectedItem is string position)
            {
                Program.CurrentState.ClockPosition = position;

                if (string.Equals(position, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    Program.CurrentState.ClockUseCustomPosition = true;

                    if (Program.CurrentState.ClockCustomPositionX.HasValue &&
                        Program.CurrentState.ClockCustomPositionY.HasValue)
                    {
                        clockForm.ApplyClockCustomPosition(
                            Program.CurrentState.ClockCustomPositionX.Value,
                            Program.CurrentState.ClockCustomPositionY.Value);
                    }
                }
                else
                {
                    Program.CurrentState.ClockUseCustomPosition = false;
                    clockForm.ApplyClockPosition(position);
                }

                AppStateStorage.Save(Program.CurrentState);
            }
        };

        var positionPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        var customPositionButton = new Button
        {
            Text = "Custom",
            Width = 80
        };
        customPositionButton.Click += (_, __) =>
        {
            using var form = new PositionControllerForm(clockForm);
            form.ShowDialog(this);

            Program.CurrentState.ClockUseCustomPosition = true;
            Program.CurrentState.ClockPosition = "Custom";
            Program.CurrentState.ClockCustomPositionX = clockForm.Location.X;
            Program.CurrentState.ClockCustomPositionY = clockForm.Location.Y;
            positionComboBox.SelectedItem = "Custom";
            AppStateStorage.Save(Program.CurrentState);
        };

        positionPanel.Controls.Add(positionComboBox);
        positionPanel.Controls.Add(customPositionButton);

        var masterResetButton = new Button
        {
            Text = "Master Reset",
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0)
        };
        masterResetButton.Click += (_, __) =>
        {
            Program.CurrentState.ClockFontSize = 20f;
            Program.CurrentState.ClockFontFamily = "Segoe UI";
            Program.CurrentState.ClockUseCustomColor = false;
            Program.CurrentState.ClockCustomColorArgb = null;
            Program.CurrentState.ClockColorName = "White";
            Program.CurrentState.ClockPosition = "Top Right";
            Program.CurrentState.ClockUseCustomPosition = false;
            Program.CurrentState.ClockCustomPositionX = null;
            Program.CurrentState.ClockCustomPositionY = null;

            fontSizeTrackBar.Value = (int)Math.Round(Program.CurrentState.ClockFontSize);
            fontFamilyComboBox.SelectedItem = Program.CurrentState.ClockFontFamily;
            colorComboBox.SelectedItem = "White";
            positionComboBox.SelectedItem = "Top Right";

            clockForm.ApplyClockFontSize(Program.CurrentState.ClockFontSize);
            clockForm.ApplyClockFontFamily(Program.CurrentState.ClockFontFamily);
            clockForm.ApplyClockColor(Program.ResolveClockColor("White"));
            clockForm.ApplyClockPosition(Program.CurrentState.ClockPosition);

            AppStateStorage.Save(Program.CurrentState);
        };

        var clockSeparator1 = CreateSeparator();

        clockLayout.Controls.Add(clockHeader, 0, 0);
        clockLayout.SetColumnSpan(clockHeader, 2);
        clockLayout.Controls.Add(clockOverviewHeader, 0, 1);
        clockLayout.SetColumnSpan(clockOverviewHeader, 2);
        clockLayout.Controls.Add(greetingLabel, 0, 2);
        clockLayout.SetColumnSpan(greetingLabel, 2);
        clockLayout.Controls.Add(clockEnabledCheckBox, 0, 3);
        clockLayout.SetColumnSpan(clockEnabledCheckBox, 2);
        clockLayout.Controls.Add(clockSeparator1, 0, 4);
        clockLayout.SetColumnSpan(clockSeparator1, 2);
        clockLayout.Controls.Add(clockAppearanceHeader, 0, 5);
        clockLayout.SetColumnSpan(clockAppearanceHeader, 2);
        clockLayout.Controls.Add(fontFamilyLabel, 0, 6);
        clockLayout.Controls.Add(fontFamilyComboBox, 1, 6);
        clockLayout.Controls.Add(fontSizeLabel, 0, 7);
        clockLayout.Controls.Add(fontSizeTrackBar, 1, 7);
        clockLayout.Controls.Add(colorLabel, 0, 8);
        clockLayout.Controls.Add(colorPanel, 1, 8);
        clockLayout.Controls.Add(positionLabel, 0, 9);
        clockLayout.Controls.Add(positionPanel, 1, 9);
        clockLayout.Controls.Add(masterResetButton, 1, 10);

        clockCard.Controls.Add(clockLayout);
        clockTab.Controls.Add(clockCard);

        var pomodoroCard = CreateCardPanel();

        var pomodoroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 3
        };
        pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var pomodoroHeader = new Label
        {
            Text = "Pomodoro",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        var pomodoroSectionHeader = new Label
        {
            Text = "Session",
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Margin = new Padding(0, 2, 0, 6)
        };

        var pomodoroHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground
        };

        pomodoroForm = new PomodoroForm
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };

        pomodoroHost.Controls.Add(pomodoroForm);
        pomodoroForm.Show();

        pomodoroLayout.Controls.Add(pomodoroHeader, 0, 0);
        pomodoroLayout.Controls.Add(pomodoroSectionHeader, 0, 1);
        pomodoroLayout.Controls.Add(pomodoroHost, 0, 2);

        pomodoroCard.Controls.Add(pomodoroLayout);
        pomodoroTab.Controls.Add(pomodoroCard);

        var todoCard = CreateCardPanel();
        var todoLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 5
        };
        todoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        todoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        todoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        todoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        todoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var todoHeader = new Label
        {
            Text = "To-Do",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        var todoInputPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        todoInputTextBox = new TextBox
        {
            Width = 220
        };

        var todoStartLabel = new Label
        {
            Text = "Start",
            AutoSize = true,
            Margin = new Padding(6, 6, 0, 0)
        };

        todoStartDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 110
        };

        var todoEndLabel = new Label
        {
            Text = "End",
            AutoSize = true,
            Margin = new Padding(6, 6, 0, 0)
        };

        todoEndDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 110
        };

        todoAddButton = new Button
        {
            Text = "Add",
            AutoSize = true
        };

        todoInputPanel.Controls.Add(todoInputTextBox);
        todoInputPanel.Controls.Add(todoStartLabel);
        todoInputPanel.Controls.Add(todoStartDatePicker);
        todoInputPanel.Controls.Add(todoEndLabel);
        todoInputPanel.Controls.Add(todoEndDatePicker);
        todoInputPanel.Controls.Add(todoAddButton);

        var todoFilterPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        todoFilterCheckBox = new CheckBox
        {
            Text = "Filter by date",
            AutoSize = true
        };

        todoFilterDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 110,
            Enabled = false
        };

        todoFilterPanel.Controls.Add(todoFilterCheckBox);
        todoFilterPanel.Controls.Add(todoFilterDatePicker);

        todoListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            CheckBoxes = true
        };
        todoListView.Columns.Add("Task", 220);
        todoListView.Columns.Add("Start", 90);
        todoListView.Columns.Add("End", 90);
        todoListView.Columns.Add("Created", 110);

        todoContextMenu = new ContextMenuStrip();
        todoContextMenu.Items.Add("Edit", null, (_, __) => EditSelectedTodo());
        todoContextMenu.Items.Add("Mark Complete", null, (_, __) => MarkSelectedTodoComplete());
        todoContextMenu.Items.Add("Delete", null, (_, __) => DeleteSelectedTodo());
        todoListView.ContextMenuStrip = todoContextMenu;

        todoListView.ItemChecked += (_, e) =>
        {
            if (isLoadingTodos || e.Item?.Tag is not TodoItem item)
            {
                return;
            }

            item.IsDone = e.Item.Checked;
            SaveTodos();
        };

        todoListView.MouseUp += (_, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = todoListView.HitTest(e.Location);
                if (hit.Item != null)
                {
                    todoListView.FocusedItem = hit.Item;
                    todoContextMenu.Show(todoListView, e.Location);
                }
            }
        };

        todoDeleteButton = new Button
        {
            Text = "Delete Selected",
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0)
        };

        todoDeleteDoneButton = new Button
        {
            Text = "Delete Done",
            AutoSize = true,
            Margin = new Padding(6, 6, 0, 0)
        };

        todoDeleteAllButton = new Button
        {
            Text = "Delete All",
            AutoSize = true,
            Margin = new Padding(6, 6, 0, 0)
        };

        var todoActionsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 0)
        };
        todoActionsPanel.Controls.Add(todoDeleteButton);
        todoActionsPanel.Controls.Add(todoDeleteDoneButton);
        todoActionsPanel.Controls.Add(todoDeleteAllButton);

        todoLayout.Controls.Add(todoHeader, 0, 0);
        todoLayout.Controls.Add(todoInputPanel, 0, 1);
        todoLayout.Controls.Add(todoFilterPanel, 0, 2);
        todoLayout.Controls.Add(todoListView, 0, 3);
        todoLayout.Controls.Add(todoActionsPanel, 0, 4);
        todoCard.Controls.Add(todoLayout);
        todoTab.Controls.Add(todoCard);

        var focusCard = CreateCardPanel();
        var focusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 2
        };
        focusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        focusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var focusHeader = new Label
        {
            Text = "Focus History",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        focusHistoryListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true
        };
        focusHistoryListView.Columns.Add("Date", 140);
        focusHistoryListView.Columns.Add("Total Focus Time (minutes)", 220);

        focusLayout.Controls.Add(focusHeader, 0, 0);
        focusLayout.Controls.Add(focusHistoryListView, 0, 1);
        focusCard.Controls.Add(focusLayout);
        focusHistoryTab.Controls.Add(focusCard);

        var insightsCard = CreateCardPanel();
        var insightsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 5
        };
        insightsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        insightsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        insightsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        insightsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        insightsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        var insightsHeader = new Label
        {
            Text = "Last 7 Days",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        var insightsRangePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        insightsRangeComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 140,
            DrawMode = DrawMode.OwnerDrawFixed
        };
        insightsRangeComboBox.Items.AddRange(new object[] { "7 days", "1 month", "6 months", "1 year" });
        insightsRangeComboBox.SelectedIndex = 0;
        insightsRangeComboBox.DrawItem += InsightsRangeComboBox_DrawItem;
        insightsRangeComboBox.SelectedIndexChanged += (_, __) =>
        {
            if (insightsRangeComboBox.SelectedIndex > 0)
            {
                insightsRangeComboBox.SelectedIndex = 0;
            }
        };

        insightsComingSoonLabel = new Label
        {
            Text = "Coming soon",
            AutoSize = true,
            ForeColor = SubtleText,
            Margin = new Padding(6, 6, 0, 0)
        };

        insightsRangePanel.Controls.Add(insightsRangeComboBox);
        insightsRangePanel.Controls.Add(insightsComingSoonLabel);

        insightsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        insightsPanel.Paint += InsightsPanel_Paint;
        insightsPanel.MouseDown += InsightsPanel_MouseDown;

        insightsEmptyLabel = new Label
        {
            Text = "No focus data yet",
            AutoSize = true,
            ForeColor = SubtleText,
            Visible = false
        };
        insightsPanel.Controls.Add(insightsEmptyLabel);

        var insightsDailyHeader = new Label
        {
            Text = "Daily Focus Map",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 8, 0, 6)
        };

        insightsDailyPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        insightsDailyPanel.Paint += InsightsDailyPanel_Paint;

        insightsDailyEmptyLabel = new Label
        {
            Text = "No focus data yet",
            AutoSize = true,
            ForeColor = SubtleText,
            Visible = false
        };
        insightsDailyPanel.Controls.Add(insightsDailyEmptyLabel);

        insightsLayout.Controls.Add(insightsHeader, 0, 0);
        insightsLayout.Controls.Add(insightsRangePanel, 0, 1);
        insightsLayout.Controls.Add(insightsPanel, 0, 2);
        insightsLayout.Controls.Add(insightsDailyHeader, 0, 3);
        insightsLayout.Controls.Add(insightsDailyPanel, 0, 4);
        insightsCard.Controls.Add(insightsLayout);
        insightsTab.Controls.Add(insightsCard);

        var profileCard = CreateCardPanel();

        var profileLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 2,
            RowCount = 6
        };
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var profileHeader = new Label
        {
            Text = "Profile",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        var profileDetailsHeader = new Label
        {
            Text = "Profile Details",
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Margin = new Padding(0, 2, 0, 6)
        };

        var nameLabel = new Label
        {
            Text = "Name",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 2)
        };

        profileNameTextBox = new TextBox
        {
            Width = 240
        };

        var genderLabel = new Label
        {
            Text = "Gender",
            Font = LabelFont,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 2)
        };

        profileGenderComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 240
        };
        profileGenderComboBox.Items.AddRange(new object[] { "Male", "Female", "Other" });

        var profileButtonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0)
        };

        var profileActionsHeader = new Label
        {
            Text = "Actions",
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Margin = new Padding(0, 8, 0, 4)
        };

        saveProfileButton = new Button
        {
            Text = "Save Profile",
            AutoSize = true
        };
        saveProfileButton.Click += (_, __) =>
        {
            Program.CurrentState.UserName = profileNameTextBox.Text.Trim();
            Program.CurrentState.Gender = NormalizeGender(profileGenderComboBox.SelectedItem as string);
            if (!string.IsNullOrWhiteSpace(Program.CurrentState.UserName))
            {
                Program.CurrentState.IsProfileCompleted = true;
                Program.CurrentState.IsFirstRun = false;
                Program.CurrentState.ShowWelcomeOnStartup = false;
            }
            AppStateStorage.Save(Program.CurrentState);
            RefreshGreeting();
        };

        resetProfileButton = new Button
        {
            Text = "Reset Profile",
            AutoSize = true
        };
        resetProfileButton.Click += (_, __) =>
        {
            Program.CurrentState.UserName = string.Empty;
            Program.CurrentState.Gender = "other";
            AppStateStorage.Save(Program.CurrentState);
            RefreshProfileUi();
            RefreshGreeting();
        };

        deleteDataButton = new Button
        {
            Text = "Delete All App Data",
            AutoSize = true
        };
        deleteDataButton.Click += (_, __) =>
        {
            var confirm = MessageBox.Show(
                "This will delete all saved app data and restart the app. Continue?",
                "Delete All App Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            if (!AppStateStorage.DeleteStateFile())
            {
                MessageBox.Show("Unable to delete app data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process.Start(new ProcessStartInfo(Application.ExecutablePath)
            {
                UseShellExecute = true
            });
            Environment.Exit(0);
        };

        profileButtonsPanel.Controls.Add(saveProfileButton);
        profileButtonsPanel.Controls.Add(resetProfileButton);
        profileButtonsPanel.Controls.Add(deleteDataButton);

        profileLayout.Controls.Add(profileHeader, 0, 0);
        profileLayout.SetColumnSpan(profileHeader, 2);
        profileLayout.Controls.Add(profileDetailsHeader, 0, 1);
        profileLayout.SetColumnSpan(profileDetailsHeader, 2);
        profileLayout.Controls.Add(nameLabel, 0, 2);
        profileLayout.Controls.Add(profileNameTextBox, 1, 2);
        profileLayout.Controls.Add(genderLabel, 0, 3);
        profileLayout.Controls.Add(profileGenderComboBox, 1, 3);
        profileLayout.Controls.Add(profileActionsHeader, 0, 4);
        profileLayout.SetColumnSpan(profileActionsHeader, 2);
        profileLayout.Controls.Add(profileButtonsPanel, 0, 5);
        profileLayout.SetColumnSpan(profileButtonsPanel, 2);

        profileCard.Controls.Add(profileLayout);
        profileTab.Controls.Add(profileCard);

        var settingsCard = CreateCardPanel();

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 2,
            RowCount = 7
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var settingsHeader = new Label
        {
            Text = "Settings",
            AutoSize = true,
            Font = HeaderFont,
            Margin = new Padding(0, 0, 0, 8)
        };

        var settingsBehaviorHeader = new Label
        {
            Text = "Startup & Behavior",
            AutoSize = true,
            Font = SectionFont,
            ForeColor = SubtleText,
            Margin = new Padding(0, 2, 0, 6)
        };

        launchOnStartupCheckBox = new CheckBox
        {
            Text = "Launch app on Windows startup",
            AutoSize = true,
            Checked = Program.CurrentState.LaunchOnStartup,
            Margin = new Padding(0, 2, 0, 6)
        };
        launchOnStartupCheckBox.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.LaunchOnStartup = launchOnStartupCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };

        showWelcomeCheckBox = new CheckBox
        {
            Text = "Show welcome screen on startup",
            AutoSize = true,
            Checked = Program.CurrentState.ShowWelcomeOnStartup,
            Margin = new Padding(0, 2, 0, 6)
        };
        showWelcomeCheckBox.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.ShowWelcomeOnStartup = showWelcomeCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };

        minimizeToTrayCheckBox = new CheckBox
        {
            Text = "Minimize to tray on close",
            AutoSize = true,
            Checked = Program.CurrentState.MinimizeToTrayOnClose,
            Margin = new Padding(0, 2, 0, 8)
        };
        minimizeToTrayCheckBox.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.MinimizeToTrayOnClose = minimizeToTrayCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };

        resetSettingsButton = new Button
        {
            Text = "Reset Settings",
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 0)
        };
        resetSettingsButton.Click += (_, __) =>
        {
            Program.CurrentState.LaunchOnStartup = false;
            Program.CurrentState.ShowWelcomeOnStartup = true;
            Program.CurrentState.MinimizeToTrayOnClose = true;
            AppStateStorage.Save(Program.CurrentState);
            RefreshSettingsUi();
        };

        settingsLayout.Controls.Add(settingsHeader, 0, 0);
        settingsLayout.SetColumnSpan(settingsHeader, 2);
        settingsLayout.Controls.Add(settingsBehaviorHeader, 0, 1);
        settingsLayout.SetColumnSpan(settingsBehaviorHeader, 2);
        settingsLayout.Controls.Add(launchOnStartupCheckBox, 0, 2);
        settingsLayout.SetColumnSpan(launchOnStartupCheckBox, 2);
        settingsLayout.Controls.Add(showWelcomeCheckBox, 0, 3);
        settingsLayout.SetColumnSpan(showWelcomeCheckBox, 2);
        settingsLayout.Controls.Add(minimizeToTrayCheckBox, 0, 4);
        settingsLayout.SetColumnSpan(minimizeToTrayCheckBox, 2);
        settingsLayout.Controls.Add(resetSettingsButton, 0, 5);
        settingsLayout.SetColumnSpan(resetSettingsButton, 2);

        settingsCard.Controls.Add(settingsLayout);
        settingsTab.Controls.Add(settingsCard);

        RestoreClockSettingsToUi();
        RefreshProfileUi();
        RefreshSettingsUi();

        todoItems = TodoStorage.Load();
        todoAddButton.Click += (_, __) => AddTodoFromInput();
        todoInputTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddTodoFromInput();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };
        todoDeleteButton.Click += (_, __) => DeleteSelectedTodo();
        todoDeleteDoneButton.Click += (_, __) => DeleteAllDoneTodos();
        todoDeleteAllButton.Click += (_, __) => DeleteAllTodos();
        todoFilterCheckBox.CheckedChanged += (_, __) =>
        {
            todoFilterDatePicker.Enabled = todoFilterCheckBox.Checked;
            RefreshTodoList();
        };
        todoFilterDatePicker.ValueChanged += (_, __) => RefreshTodoList();

        RefreshTodoList();
        RefreshFocusHistoryList();
        RefreshInsights();

        tabs.SelectedIndexChanged += (_, __) =>
        {
            if (tabs.SelectedTab == focusHistoryTab)
            {
                RefreshFocusHistoryList();
            }
            else if (tabs.SelectedTab == insightsTab)
            {
                RefreshInsights();
            }
        };

        greetingTimer = new Timer { Interval = 60_000 };
        greetingTimer.Tick += (_, __) => RefreshGreeting();
        greetingTimer.Start();
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground,
            Padding = new Padding(16),
            Margin = new Padding(8)
        };
    }

    private static Panel CreateSeparator()
    {
        return new Panel
        {
            Height = 1,
            Dock = DockStyle.Fill,
            BackColor = SeparatorColor,
            Margin = new Padding(0, 6, 0, 6)
        };
    }

    private static string NormalizeGender(string? value)
    {
        if (string.Equals(value, "Male", StringComparison.OrdinalIgnoreCase))
        {
            return "male";
        }

        if (string.Equals(value, "Female", StringComparison.OrdinalIgnoreCase))
        {
            return "female";
        }

        return "other";
    }

    private void AddTodoFromInput()
    {
        string text = todoInputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        DateTime startDate = todoStartDatePicker.Value.Date;
        DateTime endDate = todoEndDatePicker.Value.Date;
        if (endDate < startDate)
        {
            endDate = startDate;
        }

        var item = new TodoItem
        {
            Text = text,
            CreatedDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            IsDone = false
        };

        todoItems.Add(item);
        SaveTodos();
        todoInputTextBox.Clear();
        RefreshTodoList();
    }

    private void DeleteSelectedTodo()
    {
        if (todoListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (todoListView.SelectedItems[0].Tag is not TodoItem item)
        {
            return;
        }

        todoItems.RemoveAll(todo => todo.Id == item.Id);
        SaveTodos();
        RefreshTodoList();
    }

    private void DeleteAllDoneTodos()
    {
        todoItems.RemoveAll(todo => todo.IsDone);
        SaveTodos();
        RefreshTodoList();
    }

    private void DeleteAllTodos()
    {
        todoItems.Clear();
        SaveTodos();
        RefreshTodoList();
    }

    private void MarkSelectedTodoComplete()
    {
        if (todoListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (todoListView.SelectedItems[0].Tag is not TodoItem item)
        {
            return;
        }

        item.IsDone = true;
        SaveTodos();
        RefreshTodoList();
    }

    private void EditSelectedTodo()
    {
        if (todoListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (todoListView.SelectedItems[0].Tag is not TodoItem item)
        {
            return;
        }

        if (ShowEditTodoDialog(item))
        {
            SaveTodos();
            RefreshTodoList();
        }
    }

    private void RefreshTodoList()
    {
        isLoadingTodos = true;
        todoListView.Items.Clear();

        DateTime today = DateTime.Today;
        IEnumerable<TodoItem> items = todoItems;

        if (todoFilterCheckBox.Checked)
        {
            DateTime filterDate = todoFilterDatePicker.Value.Date;
            items = items.Where(item => filterDate >= item.StartDate.Date && filterDate <= item.EndDate.Date);
        }

        var ordered = items
            .OrderByDescending(item => today >= item.StartDate.Date && today <= item.EndDate.Date)
            .ThenBy(item => item.StartDate.Date)
            .ThenBy(item => item.CreatedDate);

        foreach (var item in ordered)
        {
            var listItem = new ListViewItem(item.Text)
            {
                Tag = item,
                Checked = item.IsDone
            };
            listItem.SubItems.Add(item.StartDate.ToString("yyyy-MM-dd"));
            listItem.SubItems.Add(item.EndDate.ToString("yyyy-MM-dd"));
            listItem.SubItems.Add(item.CreatedDate.ToString("yyyy-MM-dd"));
            todoListView.Items.Add(listItem);
        }

        isLoadingTodos = false;
    }

    private void SaveTodos()
    {
        TodoStorage.Save(todoItems);
    }

    private bool ShowEditTodoDialog(TodoItem item)
    {
        using var dialog = new Form
        {
            Text = "Edit Task",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            Width = 360,
            Height = 220
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 2,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var textLabel = new Label { Text = "Task", AutoSize = true, Margin = new Padding(0, 6, 6, 6) };
        var textBox = new TextBox { Text = item.Text, Dock = DockStyle.Fill };

        var startLabel = new Label { Text = "Start", AutoSize = true, Margin = new Padding(0, 6, 6, 6) };
        var startPicker = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = item.StartDate };

        var endLabel = new Label { Text = "End", AutoSize = true, Margin = new Padding(0, 6, 6, 6) };
        var endPicker = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = item.EndDate };

        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        var okButton = new Button { Text = "OK", AutoSize = true, DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Controls.Add(cancelButton);

        layout.Controls.Add(textLabel, 0, 0);
        layout.Controls.Add(textBox, 1, 0);
        layout.Controls.Add(startLabel, 0, 1);
        layout.Controls.Add(startPicker, 1, 1);
        layout.Controls.Add(endLabel, 0, 2);
        layout.Controls.Add(endPicker, 1, 2);
        layout.Controls.Add(buttonsPanel, 1, 3);

        dialog.Controls.Add(layout);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        string text = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        DateTime start = startPicker.Value.Date;
        DateTime end = endPicker.Value.Date;
        if (end < start)
        {
            end = start;
        }

        item.Text = text;
        item.StartDate = start;
        item.EndDate = end;
        return true;
    }

    private void RefreshFocusHistoryList()
    {
        focusHistoryListView.Items.Clear();

        foreach (var day in Program.CurrentState.FocusHistory.Values.OrderByDescending(item => item.Date))
        {
            var listItem = new ListViewItem(day.Date.ToString("yyyy-MM-dd"));
            listItem.SubItems.Add(day.TotalFocusMinutes.ToString());
            focusHistoryListView.Items.Add(listItem);
        }
    }

    private void RefreshInsights()
    {
        var entries = FocusHistoryService.GetLast7Days();
        bool hasData = entries.Count > 0;
        insightsEmptyLabel.Visible = !hasData;
        insightsDailyEmptyLabel.Visible = !hasData;

        if (hasData)
        {
            if (!selectedInsightDate.HasValue || entries.All(e => e.Date != selectedInsightDate.Value.ToString("yyyy-MM-dd")))
            {
                string todayKey = DateTime.Today.ToString("yyyy-MM-dd");
                var today = entries.FirstOrDefault(e => e.Date == todayKey);
                selectedInsightDate = today != null
                    ? DateTime.Today
                    : DateTime.Parse(entries.OrderByDescending(e => e.Date).First().Date);
            }
        }

        CenterInsightsEmptyLabel();
        CenterInsightsDailyEmptyLabel();
        insightsPanel.Invalidate();
        insightsDailyPanel.Invalidate();
    }

    private void CenterInsightsEmptyLabel()
    {
        if (!insightsEmptyLabel.Visible)
        {
            return;
        }

        int x = Math.Max(0, (insightsPanel.Width - insightsEmptyLabel.Width) / 2);
        int y = Math.Max(0, (insightsPanel.Height - insightsEmptyLabel.Height) / 2);
        insightsEmptyLabel.Location = new Point(x, y);
    }

    private void CenterInsightsDailyEmptyLabel()
    {
        if (!insightsDailyEmptyLabel.Visible)
        {
            return;
        }

        int x = Math.Max(0, (insightsDailyPanel.Width - insightsDailyEmptyLabel.Width) / 2);
        int y = Math.Max(0, (insightsDailyPanel.Height - insightsDailyEmptyLabel.Height) / 2);
        insightsDailyEmptyLabel.Location = new Point(x, y);
    }

    private void InsightsRangeComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            return;
        }

        e.DrawBackground();
        bool enabled = e.Index == 0;
        using var brush = new SolidBrush(enabled ? e.ForeColor : SubtleText);
        e.Graphics.DrawString(insightsRangeComboBox.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
        e.DrawFocusRectangle();
    }

    private void InsightsPanel_Paint(object? sender, PaintEventArgs e)
    {
        var entries = FocusHistoryService.GetLast7Days();
        if (entries.Count == 0)
        {
            CenterInsightsEmptyLabel();
            return;
        }

        insightsEmptyLabel.Visible = false;
        e.Graphics.Clear(Color.White);
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        int padding = 16;
        int barHeight = 18;
        int rowSpacing = 10;
        int labelWidth = 90;
        int maxMinutes = Math.Max(1, entries.Max(item => item.TotalFocusMinutes));

        int startY = padding;
        int availableWidth = Math.Max(50, insightsPanel.Width - padding * 2 - labelWidth);

        foreach (var entry in entries.OrderBy(item => item.Date))
        {
            if (!DateTime.TryParse(entry.Date, out var date))
            {
                continue;
            }

            int barWidth = (int)Math.Round(availableWidth * entry.TotalFocusMinutes / (double)maxMinutes);
            var barRect = new Rectangle(padding + labelWidth, startY, barWidth, barHeight);

            bool isToday = date.Date == DateTime.Today;
            bool isSelected = selectedInsightDate.HasValue && selectedInsightDate.Value.Date == date.Date;

            using var barBrush = new SolidBrush(isSelected ? Color.FromArgb(80, 135, 215)
                : isToday ? Color.FromArgb(90, 175, 125)
                : Color.FromArgb(210, 210, 210));

            using var textBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
            e.Graphics.FillRectangle(barBrush, barRect);

            string dateText = date.ToString("yyyy-MM-dd");
            e.Graphics.DrawString(dateText, Font, textBrush, padding, startY - 1);

            string valueText = entry.TotalFocusMinutes.ToString();
            e.Graphics.DrawString(valueText, Font, textBrush, padding + labelWidth + barWidth + 6, startY - 1);

            startY += barHeight + rowSpacing;
        }
    }

    private void InsightsDailyPanel_Paint(object? sender, PaintEventArgs e)
    {
        var entries = FocusHistoryService.GetLast7Days();
        if (entries.Count == 0 || !selectedInsightDate.HasValue)
        {
            CenterInsightsDailyEmptyLabel();
            return;
        }

        var entry = FocusHistoryService.GetDay(selectedInsightDate.Value);
        if (entry == null || entry.HourlyFocus == null || entry.HourlyFocus.Length != 24)
        {
            insightsDailyEmptyLabel.Visible = true;
            CenterInsightsDailyEmptyLabel();
            return;
        }

        insightsDailyEmptyLabel.Visible = false;
        e.Graphics.Clear(Color.White);
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        int padding = 16;
        int barWidth = 10;
        int barGap = 6;
        int maxMinutes = Math.Max(1, entry.HourlyFocus.Max());
        int chartHeight = Math.Max(40, insightsDailyPanel.Height - padding * 2 - 14);
        int startX = padding;
        int baseY = padding + chartHeight;

        for (int hour = 0; hour < 24; hour++)
        {
            int minutes = entry.HourlyFocus[hour];
            int height = (int)Math.Round(chartHeight * (minutes / (double)maxMinutes));
            var barRect = new Rectangle(startX, baseY - height, barWidth, height);
            using var brush = new SolidBrush(Color.FromArgb(180, 180, 180));
            e.Graphics.FillRectangle(brush, barRect);

            if (hour % 3 == 0)
            {
                string label = hour.ToString();
                e.Graphics.DrawString(label, Font, Brushes.Gray, startX - 2, baseY + 2);
            }

            startX += barWidth + barGap;
        }
    }

    private void InsightsPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        var entries = FocusHistoryService.GetLast7Days().OrderBy(item => item.Date).ToList();
        if (entries.Count == 0)
        {
            return;
        }

        int padding = 16;
        int barHeight = 18;
        int rowSpacing = 10;
        int startY = padding;

        foreach (var entry in entries)
        {
            if (!DateTime.TryParse(entry.Date, out var date))
            {
                startY += barHeight + rowSpacing;
                continue;
            }

            var rowRect = new Rectangle(0, startY, insightsPanel.Width, barHeight);
            if (rowRect.Contains(e.Location))
            {
                selectedInsightDate = date.Date;
                insightsPanel.Invalidate();
                insightsDailyPanel.Invalidate();
                return;
            }

            startY += barHeight + rowSpacing;
        }
    }

    private void RefreshProfileUi()
    {
        profileNameTextBox.Text = Program.CurrentState.UserName ?? string.Empty;

        string gender = Program.CurrentState.Gender ?? "other";
        if (string.Equals(gender, "male", StringComparison.OrdinalIgnoreCase))
        {
            profileGenderComboBox.SelectedItem = "Male";
        }
        else if (string.Equals(gender, "female", StringComparison.OrdinalIgnoreCase))
        {
            profileGenderComboBox.SelectedItem = "Female";
        }
        else
        {
            profileGenderComboBox.SelectedItem = "Other";
        }
    }

    private void RefreshSettingsUi()
    {
        launchOnStartupCheckBox.Checked = Program.CurrentState.LaunchOnStartup;
        showWelcomeCheckBox.Checked = Program.CurrentState.ShowWelcomeOnStartup;
        minimizeToTrayCheckBox.Checked = Program.CurrentState.MinimizeToTrayOnClose;
    }

    private void RestoreClockSettingsToUi()
    {
        clockEnabledCheckBox.Checked = Program.CurrentState.ClockEnabled;

        if (fontFamilyComboBox.Items.Contains(Program.CurrentState.ClockFontFamily))
        {
            fontFamilyComboBox.SelectedItem = Program.CurrentState.ClockFontFamily;
        }
        else
        {
            fontFamilyComboBox.SelectedItem = "Segoe UI";
        }

        int fontValue = (int)Math.Round(Program.CurrentState.ClockFontSize);
        fontSizeTrackBar.Value = Math.Max(fontSizeTrackBar.Minimum, Math.Min(fontValue, fontSizeTrackBar.Maximum));

        if (Program.CurrentState.ClockUseCustomColor)
        {
            colorComboBox.SelectedItem = "Custom";
        }
        else if (colorComboBox.Items.Contains(Program.CurrentState.ClockColorName))
        {
            colorComboBox.SelectedItem = Program.CurrentState.ClockColorName;
        }
        else
        {
            colorComboBox.SelectedItem = "White";
        }

        if (Program.CurrentState.ClockUseCustomPosition && positionComboBox.Items.Contains("Custom"))
        {
            positionComboBox.SelectedItem = "Custom";
        }
        else if (positionComboBox.Items.Contains(Program.CurrentState.ClockPosition))
        {
            positionComboBox.SelectedItem = Program.CurrentState.ClockPosition;
        }
        else
        {
            positionComboBox.SelectedItem = "Top Right";
        }

        Program.ApplyClockStateToOverlay();
    }

    public void RefreshGreeting()
    {
        greetingLabel.Text = Program.GetGreetingText();
    }

    public void RequestClose()
    {
        allowClose = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            if (Program.CurrentState.MinimizeToTrayOnClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }
        }

        base.OnFormClosing(e);
    }
}
