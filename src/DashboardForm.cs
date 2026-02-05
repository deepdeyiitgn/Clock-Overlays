using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace TransparentClock
{
    public class DashboardForm : Form
    {
        private static readonly Font BaseFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private static readonly Font HeaderFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        private static readonly Font SectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Color AppBackground = Color.FromArgb(247, 247, 247);
        private static readonly Color CardBackground = Color.White;
        private static readonly Color SubtleText = Color.FromArgb(110, 110, 110);

        private readonly TransparentClockForm clockForm;
        private readonly PomodoroForm pomodoroForm;
        private readonly TodoForm todoForm;
        private readonly CheckBox clockEnabledCheckBox;
        private readonly TrackBar fontSizeTrackBar;
        private readonly ComboBox fontFamilyComboBox;
        private readonly ComboBox colorComboBox;
        private readonly Button customColorButton;
        private readonly ComboBox positionComboBox;
        private readonly Label greetingLabel;

        private readonly TextBox profileNameTextBox;
        private readonly ComboBox profileGenderComboBox;
        private readonly Button saveProfileButton;
        private readonly Button resetProfileButton;
        private readonly Button deleteDataButton;

        private readonly CheckBox launchOnStartupCheckBox;
        private readonly CheckBox showWelcomeCheckBox;
        private readonly CheckBox minimizeToTrayCheckBox;
        private readonly Button resetSettingsButton;
        private readonly Button openTodoButton;

        private readonly ListView sessionHistoryListView;
        private readonly FlowLayoutPanel focusHistoryPanel;
        private readonly ComboBox focusTimeRangeComboBox;
        private readonly FocusDatePicker focusDayPicker;
        private readonly FocusBarGraph focusBarGraph;
        private readonly FocusLineGraph focusLineGraph;
        private readonly FocusStatsDisplay focusStatsDisplay;
        private readonly Timer greetingTimer;
        private readonly Timer focusRefreshTimer;
        private FocusTimeRangeFilter.TimeRange currentTimeRange = FocusTimeRangeFilter.TimeRange.Last7Days;
        private bool allowClose;

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
            var todoTab = new TabPage("To-Do") { Padding = new Padding(14), BackColor = AppBackground };
            var profileTab = new TabPage("Profile") { Padding = new Padding(14), BackColor = AppBackground };
            var settingsTab = new TabPage("Settings") { Padding = new Padding(14), BackColor = AppBackground };
            var historyTab = new TabPage("History") { Padding = new Padding(14), BackColor = AppBackground };
            var focusHistoryTab = new TabPage("Focus Insights") { Padding = new Padding(14), BackColor = AppBackground };
            var utilitiesTab = UtilitiesTabFactory.CreateUtilitiesTab();

            tabs.TabPages.Add(clockTab);
            tabs.TabPages.Add(pomodoroTab);
            tabs.TabPages.Add(todoTab);
            tabs.TabPages.Add(profileTab);
            tabs.TabPages.Add(settingsTab);
            tabs.TabPages.Add(historyTab);
            tabs.TabPages.Add(focusHistoryTab);
            tabs.TabPages.Add(utilitiesTab);

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
                RowCount = 8
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
            clockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var clockHeader = new Label
            {
                AutoSize = true,
                Font = HeaderFont,
                Text = "Clock Controls",
                Margin = new Padding(0, 0, 0, 8)
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

            var fontFamilyLabel = new Label { Text = "Font", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            fontFamilyComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            var fonts = new InstalledFontCollection().Families.Select(f => f.Name).OrderBy(n => n).ToArray();
            fontFamilyComboBox.Items.AddRange(fonts);
            fontFamilyComboBox.SelectedItem = Program.CurrentState.ClockFontFamily;
            fontFamilyComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (fontFamilyComboBox.SelectedItem is string family)
                {
                    Program.CurrentState.ClockFontFamily = family;
                    clockForm.ApplyClockFontFamily(family);
                    AppStateStorage.Save(Program.CurrentState);
                }
            };

            var fontSizeLabel = new Label { Text = "Font Size", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
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

            var colorLabel = new Label { Text = "Color", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            colorComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            foreach (var name in Program.ClockColors.Keys.OrderBy(k => k))
            {
                colorComboBox.Items.Add(name);
            }
            colorComboBox.Items.Add("Custom");
            colorComboBox.SelectedItem = Program.CurrentState.ClockUseCustomColor ? "Custom" : Program.CurrentState.ClockColorName;

            customColorButton = new Button { Text = "Pick", Width = 60 };
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

            var colorPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            colorPanel.Controls.Add(colorComboBox);
            colorPanel.Controls.Add(customColorButton);

            var positionLabel = new Label { Text = "Position", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            positionComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            positionComboBox.Items.AddRange(new object[] { "Top Left", "Top Right", "Bottom Right", "Bottom Left", "Custom" });
            positionComboBox.SelectedItem = Program.CurrentState.ClockPosition;
            positionComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (positionComboBox.SelectedItem is not string position)
                {
                    return;
                }

                if (string.Equals(position, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    using var form = new PositionControllerForm(clockForm);
                    form.ShowDialog(this);
                    Program.CurrentState.ClockUseCustomPosition = true;
                    Program.CurrentState.ClockPosition = "Custom";
                    Program.CurrentState.ClockCustomPositionX = clockForm.Location.X;
                    Program.CurrentState.ClockCustomPositionY = clockForm.Location.Y;
                    clockForm.ApplyClockCustomPosition(clockForm.Location.X, clockForm.Location.Y);
                }
                else
                {
                    Program.CurrentState.ClockUseCustomPosition = false;
                    Program.CurrentState.ClockPosition = position;
                    clockForm.ApplyClockPosition(position);
                }

                AppStateStorage.Save(Program.CurrentState);
            };

            clockLayout.Controls.Add(clockHeader, 0, 0);
            clockLayout.SetColumnSpan(clockHeader, 2);
            clockLayout.Controls.Add(greetingLabel, 0, 1);
            clockLayout.SetColumnSpan(greetingLabel, 2);
            clockLayout.Controls.Add(clockEnabledCheckBox, 0, 2);
            clockLayout.SetColumnSpan(clockEnabledCheckBox, 2);
            clockLayout.Controls.Add(fontFamilyLabel, 0, 3);
            clockLayout.Controls.Add(fontFamilyComboBox, 1, 3);
            clockLayout.Controls.Add(fontSizeLabel, 0, 4);
            clockLayout.Controls.Add(fontSizeTrackBar, 1, 4);
            clockLayout.Controls.Add(colorLabel, 0, 5);
            clockLayout.Controls.Add(colorPanel, 1, 5);
            clockLayout.Controls.Add(positionLabel, 0, 6);
            clockLayout.Controls.Add(positionComboBox, 1, 6);

            clockCard.Controls.Add(clockLayout);
            clockTab.Controls.Add(clockCard);

            var pomodoroCard = CreateCardPanel();
            var pomodoroLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 2
            };
            pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var pomodoroHeader = new Label
            {
                Text = "Pomodoro",
                AutoSize = true,
                Font = HeaderFont,
                Margin = new Padding(0, 0, 0, 8)
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
            pomodoroLayout.Controls.Add(pomodoroHost, 0, 1);

            pomodoroCard.Controls.Add(pomodoroLayout);
            pomodoroTab.Controls.Add(pomodoroCard);

            var todoCard = CreateCardPanel();
            var todoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 2
            };
            todoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            todoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var todoHeader = new Label
            {
                Text = "To-Do",
                AutoSize = true,
                Font = HeaderFont,
                Margin = new Padding(0, 0, 0, 8)
            };

            var todoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBackground
            };

            todoForm = new TodoForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            todoHost.Controls.Add(todoForm);
            todoForm.Show();

            todoLayout.Controls.Add(todoHeader, 0, 0);
            todoLayout.Controls.Add(todoHost, 0, 1);

            todoCard.Controls.Add(todoLayout);
            todoTab.Controls.Add(todoCard);

            var profileCard = CreateCardPanel();
            var profileLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 5
            };
            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
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

            var nameLabel = new Label { Text = "Name", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            profileNameTextBox = new TextBox { Text = Program.CurrentState.UserName ?? string.Empty, Width = 220 };

            var genderLabel = new Label { Text = "Gender", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            profileGenderComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            profileGenderComboBox.Items.AddRange(new object[] { "Male", "Female", "Other" });
            profileGenderComboBox.SelectedItem = string.IsNullOrWhiteSpace(Program.CurrentState.Gender) ? "Other" : Program.CurrentState.Gender;

            var profileButtonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 6, 0, 0)
            };

            saveProfileButton = new Button { Text = "Save", AutoSize = true };
            resetProfileButton = new Button { Text = "Reset", AutoSize = true, Margin = new Padding(6, 0, 0, 0) };
            deleteDataButton = new Button { Text = "Delete Data", AutoSize = true, Margin = new Padding(6, 0, 0, 0) };

            saveProfileButton.Click += (_, __) =>
            {
                Program.CurrentState.UserName = profileNameTextBox.Text.Trim();
                Program.CurrentState.Gender = profileGenderComboBox.SelectedItem?.ToString() ?? "Other";
                AppStateStorage.Save(Program.CurrentState);
                RefreshGreeting();
            };

            resetProfileButton.Click += (_, __) =>
            {
                profileNameTextBox.Text = string.Empty;
                profileGenderComboBox.SelectedItem = "Other";
                Program.CurrentState.UserName = string.Empty;
                Program.CurrentState.Gender = "Other";
                AppStateStorage.Save(Program.CurrentState);
                RefreshGreeting();
            };

            deleteDataButton.Click += (_, __) =>
            {
                var result = MessageBox.Show(
                    "This will delete all local app data and close the app. Continue?",
                    "Delete Data",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    AppStateStorage.DeleteStateFile();
                    Program.ExitApplication();
                }
            };

            profileButtonsPanel.Controls.Add(saveProfileButton);
            profileButtonsPanel.Controls.Add(resetProfileButton);
            profileButtonsPanel.Controls.Add(deleteDataButton);

            profileLayout.Controls.Add(profileHeader, 0, 0);
            profileLayout.SetColumnSpan(profileHeader, 2);
            profileLayout.Controls.Add(nameLabel, 0, 1);
            profileLayout.Controls.Add(profileNameTextBox, 1, 1);
            profileLayout.Controls.Add(genderLabel, 0, 2);
            profileLayout.Controls.Add(profileGenderComboBox, 1, 2);
            profileLayout.Controls.Add(profileButtonsPanel, 1, 3);

            profileCard.Controls.Add(profileLayout);
            profileTab.Controls.Add(profileCard);

            var settingsCard = CreateCardPanel();
            var settingsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 6
            };
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
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
                Text = "Minimize to tray when closing",
                AutoSize = true,
                Checked = Program.CurrentState.MinimizeToTrayOnClose,
                Margin = new Padding(0, 2, 0, 6)
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
                launchOnStartupCheckBox.Checked = false;
                showWelcomeCheckBox.Checked = true;
                minimizeToTrayCheckBox.Checked = true;
                AppStateStorage.Save(Program.CurrentState);
            };

            openTodoButton = new Button
            {
                Text = "To-Do",
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0)
            };
            openTodoButton.Click += (_, __) =>
            {
                using var form = new TodoForm { StartPosition = FormStartPosition.CenterParent };
                form.ShowDialog(this);
            };

            settingsLayout.Controls.Add(settingsHeader, 0, 0);
            settingsLayout.SetColumnSpan(settingsHeader, 2);
            settingsLayout.Controls.Add(launchOnStartupCheckBox, 0, 1);
            settingsLayout.SetColumnSpan(launchOnStartupCheckBox, 2);
            settingsLayout.Controls.Add(showWelcomeCheckBox, 0, 2);
            settingsLayout.SetColumnSpan(showWelcomeCheckBox, 2);
            settingsLayout.Controls.Add(minimizeToTrayCheckBox, 0, 3);
            settingsLayout.SetColumnSpan(minimizeToTrayCheckBox, 2);
            settingsLayout.Controls.Add(resetSettingsButton, 0, 4);
            settingsLayout.SetColumnSpan(resetSettingsButton, 2);
            settingsLayout.Controls.Add(openTodoButton, 0, 5);
            settingsLayout.SetColumnSpan(openTodoButton, 2);

            settingsCard.Controls.Add(settingsLayout);
            settingsTab.Controls.Add(settingsCard);

            var historyCard = CreateCardPanel();
            var historyLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 2
            };
            historyLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            historyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var historyHeader = new Label
            {
                Text = "Session History",
                AutoSize = true,
                Font = HeaderFont,
                Margin = new Padding(0, 0, 0, 8)
            };

            sessionHistoryListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            sessionHistoryListView.Columns.Add("Date", 110);
            sessionHistoryListView.Columns.Add("Time", 80);
            sessionHistoryListView.Columns.Add("Minutes", 80);

            historyLayout.Controls.Add(historyHeader, 0, 0);
            historyLayout.Controls.Add(sessionHistoryListView, 0, 1);
            historyCard.Controls.Add(historyLayout);
            historyTab.Controls.Add(historyCard);

            var focusCard = CreateCardPanel();
            var focusLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 6
            };
            focusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            focusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            focusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            focusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            focusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            focusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            var focusHeader = new Label
            {
                Text = "Last 7 Days",
                AutoSize = true,
                Font = HeaderFont,
                Margin = new Padding(0, 0, 0, 8)
            };

            focusHistoryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };

            // Time range selector for Focus Insights
            var timeRangePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 10)
            };

            var timeRangeLabel = new Label
            {
                Text = "Time Range:",
                AutoSize = true,
                Font = SectionFont,
                ForeColor = SubtleText,
                Margin = new Padding(0, 6, 8, 0)
            };

            focusTimeRangeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140
            };
            foreach (var (range, displayName) in FocusTimeRangeFilter.GetAllOptions())
            {
                focusTimeRangeComboBox.Items.Add(new { Range = range, DisplayName = displayName });
            }
            focusTimeRangeComboBox.DisplayMember = "DisplayName";
            focusTimeRangeComboBox.SelectedIndex = 0;

            timeRangePanel.Controls.Add(timeRangeLabel);
            timeRangePanel.Controls.Add(focusTimeRangeComboBox);

            var breakdownTopPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6)
            };

            var breakdownHeader = new Label
            {
                Text = "Daily Focus Breakdown",
                AutoSize = true,
                Font = SectionFont,
                ForeColor = SubtleText,
                Margin = new Padding(0, 6, 8, 0)
            };

            focusDayPicker = new FocusDatePicker
            {
                Width = 140,
                Margin = new Padding(0, 0, 0, 0)
            };
            breakdownTopPanel.Controls.Add(breakdownHeader);
            breakdownTopPanel.Controls.Add(new Label { Text = "Day", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            breakdownTopPanel.Controls.Add(focusDayPicker);

            focusBarGraph = new FocusBarGraph
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(400, 200)
            };

            focusLineGraph = new FocusLineGraph
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(400, 200)
            };

            focusStatsDisplay = new FocusStatsDisplay();

            focusLayout.Controls.Add(focusHeader, 0, 0);
            focusLayout.Controls.Add(timeRangePanel, 0, 1);
            focusLayout.Controls.Add(focusLineGraph, 0, 2);
            focusLayout.Controls.Add(focusStatsDisplay, 0, 3);
            focusLayout.Controls.Add(breakdownTopPanel, 0, 4);
            focusLayout.Controls.Add(focusBarGraph, 0, 5);
            focusCard.Controls.Add(focusLayout);
            focusHistoryTab.Controls.Add(focusCard);

            RefreshFocusHistoryList();
            RefreshSessionHistoryList();

            focusTimeRangeComboBox.SelectedIndexChanged += (_, __) => OnTimeRangeChanged();
            focusDayPicker.ValueChanged += (_, __) => RefreshDailyBreakdown();
            tabs.SelectedIndexChanged += (_, __) =>
            {
                if (tabs.SelectedTab == focusHistoryTab)
                {
                    RefreshFocusHistoryList();
                }

                if (tabs.SelectedTab == historyTab)
                {
                    RefreshSessionHistoryList();
                }
            };

            greetingTimer = new Timer { Interval = 60_000 };
            greetingTimer.Tick += (_, __) => RefreshGreeting();
            greetingTimer.Start();

            focusRefreshTimer = new Timer { Interval = 30_000 };
            focusRefreshTimer.Tick += (_, __) =>
            {
                RefreshFocusHistoryList();
                RefreshSessionHistoryList();
            };
            focusRefreshTimer.Start();
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

        private void OnTimeRangeChanged()
        {
            if (focusTimeRangeComboBox.SelectedItem != null)
            {
                var item = (dynamic)focusTimeRangeComboBox.SelectedItem;
                currentTimeRange = item.Range;
            }
            RefreshFocusHistoryList();
        }

        private void RefreshFocusHistoryList()
        {
            focusHistoryPanel.Controls.Clear();

            var entries = FocusHistoryStorage.GetLast7Days();
            
            // Filter entries by current time range
            int dayCount = FocusTimeRangeFilter.GetDayCount(currentTimeRange);
            DateTime cutoff = DateTime.Now.AddDays(-dayCount);
            var filteredEntries = entries
                .Where(e => DateTime.Parse(e.Date) >= cutoff.Date)
                .ToList();
            
            var entryMap = filteredEntries.ToDictionary(item => item.Date, item => item.TotalFocusMinutes);

            // Populate line graph with data
            var graphData = filteredEntries
                .Select(e => (DateTime.Parse(e.Date), e.TotalFocusMinutes))
                .OrderBy(x => x.Item1)
                .ToList();
            focusLineGraph.SetData(graphData);

            // Calculate and display insights
            var insights = FocusInsightsCalculator.CalculateForRange(currentTimeRange);
            focusStatsDisplay.UpdateStats(insights);

            // Populate date picker with valid dates
            var validDates = filteredEntries
                .Select(e => DateTime.Parse(e.Date))
                .OrderBy(d => d)
                .ToList();
            focusDayPicker.SetValidDates(validDates);
            
            // Set to today if available, otherwise to the most recent date
            if (validDates.Count > 0)
            {
                DateTime targetDate = validDates.FirstOrDefault(d => d.Date == DateTime.Today);
                if (targetDate == DateTime.MinValue)
                {
                    targetDate = validDates[validDates.Count - 1]; // Most recent date
                }
                focusDayPicker.Value = targetDate;
            }

            // Display day range based on current time range
            DateTime start = DateTime.Today.AddDays(-dayCount + 1);
            for (int i = 0; i < dayCount && i < 30; i++) // Cap display at 30 days to avoid clutter
            {
                DateTime date = start.AddDays(i);
                string key = date.ToString("yyyy-MM-dd");
                int minutes = entryMap.TryGetValue(key, out var value) ? value : 0;

                var label = new Label
                {
                    AutoSize = true,
                    Text = $"{date:ddd}: {minutes} min",
                    Margin = new Padding(0, 2, 0, 2)
                };
                focusHistoryPanel.Controls.Add(label);
            }

            RefreshDailyBreakdown();
        }

        private void RefreshSessionHistoryList()
        {
            sessionHistoryListView.Items.Clear();

            var entries = FocusSessionStorage.GetAll();
            foreach (var entry in entries.OrderByDescending(item => item.EndTime))
            {
                string dateText = entry.EndTime.ToString("yyyy-MM-dd");
                string timeText = entry.EndTime.ToString("HH:mm");
                string minutesText = entry.DurationMinutes.ToString();

                var item = new ListViewItem(dateText);
                item.SubItems.Add(timeText);
                item.SubItems.Add(minutesText);
                sessionHistoryListView.Items.Add(item);
            }
        }

        private void RefreshDailyBreakdown()
        {
            var date = focusDayPicker.SelectedDate;
            var entry = FocusHistoryStorage.GetDay(date);
            var hourly = entry?.HourlyFocus ?? new int[24];

            focusBarGraph.SetData(hourly);
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
            if (!allowClose && Program.CurrentState.MinimizeToTrayOnClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            base.OnFormClosing(e);
        }
    }
}
