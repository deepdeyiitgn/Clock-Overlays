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
        private readonly CheckBox borderEnabledCheckBox;
        private readonly ComboBox borderColorComboBox;
        private readonly NumericUpDown borderWidthInput;
        private readonly ComboBox positionComboBox;
        private readonly Button clockSaveButton;
        private readonly Button clockResetButton;
        private readonly Button clockHardResetButton;
        private readonly Label greetingLabel;

        private bool isClockSettingsLoading;
        private ClockSettings savedClockSettings = new ClockSettings();
        private ClockSettings pendingClockSettings = new ClockSettings();

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
            Text = $"{AppInfo.AppName} — {AppInfo.DisplayVersion}";
            Icon = Program.GetAppIcon();
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
            var aboutTab = AboutTabFactory.CreateAboutTab();

            tabs.TabPages.Add(clockTab);
            tabs.TabPages.Add(pomodoroTab);
            tabs.TabPages.Add(todoTab);
            tabs.TabPages.Add(profileTab);
            tabs.TabPages.Add(settingsTab);
            tabs.TabPages.Add(historyTab);
            tabs.TabPages.Add(focusHistoryTab);
            tabs.TabPages.Add(utilitiesTab);
            tabs.TabPages.Add(aboutTab);

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
                if (isClockSettingsLoading)
                {
                    return;
                }

                pendingClockSettings.ClockEnabled = clockEnabledCheckBox.Checked;
                ApplyClockSettingsToOverlay(pendingClockSettings);
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
                    if (isClockSettingsLoading)
                    {
                        return;
                    }

                    pendingClockSettings.ClockFontFamily = family;
                    ApplyClockSettingsToOverlay(pendingClockSettings);
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
                if (isClockSettingsLoading)
                {
                    return;
                }

                pendingClockSettings.ClockFontSize = fontSizeTrackBar.Value;
                ApplyClockSettingsToOverlay(pendingClockSettings);
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
                if (pendingClockSettings.ClockCustomColorArgb.HasValue)
                {
                    dialog.Color = Color.FromArgb(pendingClockSettings.ClockCustomColorArgb.Value);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pendingClockSettings.ClockUseCustomColor = true;
                    pendingClockSettings.ClockCustomColorArgb = dialog.Color.ToArgb();
                    colorComboBox.SelectedItem = "Custom";
                    ApplyClockSettingsToOverlay(pendingClockSettings);
                }
            };

            colorComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (colorComboBox.SelectedItem is string name)
                {
                    if (isClockSettingsLoading)
                    {
                        return;
                    }

                    if (string.Equals(name, "Custom", StringComparison.OrdinalIgnoreCase))
                    {
                        pendingClockSettings.ClockUseCustomColor = true;
                        if (pendingClockSettings.ClockCustomColorArgb.HasValue)
                        {
                            var color = Color.FromArgb(pendingClockSettings.ClockCustomColorArgb.Value);
                            ApplyClockSettingsToOverlay(pendingClockSettings);
                        }
                    }
                    else
                    {
                        pendingClockSettings.ClockUseCustomColor = false;
                        pendingClockSettings.ClockColorName = name;
                        ApplyClockSettingsToOverlay(pendingClockSettings);
                    }
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

            borderEnabledCheckBox = new CheckBox
            {
                Text = "Enable Border",
                Checked = Program.CurrentState.ClockBorderEnabled,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            borderEnabledCheckBox.CheckedChanged += (_, __) =>
            {
                if (isClockSettingsLoading)
                {
                    return;
                }

                pendingClockSettings.ClockBorderEnabled = borderEnabledCheckBox.Checked;
                ApplyClockSettingsToOverlay(pendingClockSettings);
            };

            var borderColorLabel = new Label { Text = "Border Color", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            borderColorComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            foreach (var name in Program.ClockColors.Keys.OrderBy(k => k))
            {
                borderColorComboBox.Items.Add(name);
            }
            borderColorComboBox.Items.Add("Custom");
            borderColorComboBox.SelectedItem = Program.CurrentState.ClockBorderUseCustomColor
                ? "Custom"
                : Program.CurrentState.ClockBorderColorName;

            var borderCustomColorButton = new Button { Text = "Pick", Width = 60 };
            borderCustomColorButton.Click += (_, __) =>
            {
                using var dialog = new ColorDialog();
                if (pendingClockSettings.ClockBorderCustomColorArgb.HasValue)
                {
                    dialog.Color = Color.FromArgb(pendingClockSettings.ClockBorderCustomColorArgb.Value);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pendingClockSettings.ClockBorderUseCustomColor = true;
                    pendingClockSettings.ClockBorderCustomColorArgb = dialog.Color.ToArgb();
                    borderColorComboBox.SelectedItem = "Custom";
                    ApplyClockSettingsToOverlay(pendingClockSettings);
                }
            };

            borderColorComboBox.SelectedIndexChanged += (_, __) =>
            {
                if (borderColorComboBox.SelectedItem is string name)
                {
                    if (isClockSettingsLoading)
                    {
                        return;
                    }

                    if (string.Equals(name, "Custom", StringComparison.OrdinalIgnoreCase))
                    {
                        pendingClockSettings.ClockBorderUseCustomColor = true;
                        if (pendingClockSettings.ClockBorderCustomColorArgb.HasValue)
                        {
                            ApplyClockSettingsToOverlay(pendingClockSettings);
                        }
                    }
                    else
                    {
                        pendingClockSettings.ClockBorderUseCustomColor = false;
                        pendingClockSettings.ClockBorderColorName = name;
                        ApplyClockSettingsToOverlay(pendingClockSettings);
                    }
                }
            };

            var borderColorPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            borderColorPanel.Controls.Add(borderColorComboBox);
            borderColorPanel.Controls.Add(borderCustomColorButton);

            var borderWidthLabel = new Label { Text = "Border Width", AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
            borderWidthInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 10,
                Value = Math.Max(1, Math.Min(10, Program.CurrentState.ClockBorderWidth)),
                Width = 70
            };
            borderWidthInput.ValueChanged += (_, __) =>
            {
                if (isClockSettingsLoading)
                {
                    return;
                }

                pendingClockSettings.ClockBorderWidth = (int)borderWidthInput.Value;
                ApplyClockSettingsToOverlay(pendingClockSettings);
            };

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

                if (isClockSettingsLoading)
                {
                    return;
                }

                if (string.Equals(position, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    using var form = new PositionControllerForm(clockForm);
                    form.ShowDialog(this);
                    pendingClockSettings.ClockUseCustomPosition = true;
                    pendingClockSettings.ClockPosition = "Custom";
                    pendingClockSettings.ClockCustomPositionX = clockForm.Location.X;
                    pendingClockSettings.ClockCustomPositionY = clockForm.Location.Y;
                    ApplyClockSettingsToOverlay(pendingClockSettings);
                }
                else
                {
                    pendingClockSettings.ClockUseCustomPosition = false;
                    pendingClockSettings.ClockPosition = position;
                    ApplyClockSettingsToOverlay(pendingClockSettings);
                }
            };

            var clockButtonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 6, 0, 0)
            };

            clockSaveButton = new Button
            {
                Text = "Save",
                AutoSize = true,
                BackColor = Color.FromArgb(66, 160, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clockSaveButton.FlatAppearance.BorderSize = 0;

            clockResetButton = new Button
            {
                Text = "Reset",
                AutoSize = true,
                Margin = new Padding(6, 0, 0, 0)
            };

            clockHardResetButton = new Button
            {
                Text = "Hard Reset",
                AutoSize = true,
                Margin = new Padding(6, 0, 0, 0),
                BackColor = Color.FromArgb(220, 90, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clockHardResetButton.FlatAppearance.BorderSize = 0;

            clockSaveButton.Click += (_, __) =>
            {
                savedClockSettings = pendingClockSettings;
                ApplyClockSettingsToState(savedClockSettings);
                AppStateStorage.Save(Program.CurrentState);
            };

            clockResetButton.Click += (_, __) =>
            {
                pendingClockSettings = savedClockSettings;
                LoadClockSettingsIntoUi(savedClockSettings, true);
            };

            clockHardResetButton.Click += (_, __) =>
            {
                var result = MessageBox.Show(
                    "This will reset all app data to factory defaults and close the app. Continue?",
                    "Hard Reset",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    AppStateStorage.DeleteStateFile();
                    Program.ExitApplication();
                }
            };

            clockButtonsPanel.Controls.Add(clockSaveButton);
            clockButtonsPanel.Controls.Add(clockResetButton);
            clockButtonsPanel.Controls.Add(clockHardResetButton);

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
            clockLayout.Controls.Add(borderEnabledCheckBox, 0, 6);
            clockLayout.SetColumnSpan(borderEnabledCheckBox, 2);
            clockLayout.Controls.Add(borderColorLabel, 0, 7);
            clockLayout.Controls.Add(borderColorPanel, 1, 7);
            clockLayout.Controls.Add(borderWidthLabel, 0, 8);
            clockLayout.Controls.Add(borderWidthInput, 1, 8);
            clockLayout.Controls.Add(positionLabel, 0, 9);
            clockLayout.Controls.Add(positionComboBox, 1, 9);
            clockLayout.Controls.Add(clockButtonsPanel, 1, 10);

            clockCard.Controls.Add(clockLayout);
            clockTab.Controls.Add(clockCard);

            savedClockSettings = CaptureClockSettingsFromState(Program.CurrentState);
            pendingClockSettings = savedClockSettings;
            LoadClockSettingsIntoUi(savedClockSettings, true);

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
            sessionHistoryListView.Columns.Add("Duration", 90);

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
                Margin = new Padding(0, 0, 0, 6)
            };

            var timeRangeLabel = new Label
            {
                Text = "Time Range:",
                AutoSize = true,
                Font = SectionFont,
                ForeColor = SubtleText,
                Margin = new Padding(0, 4, 6, 0)
            };

            focusTimeRangeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                Margin = new Padding(0, 2, 0, 0)
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
                Margin = new Padding(0, 4, 6, 0)
            };

            focusDayPicker = new FocusDatePicker
            {
                Width = 140,
                Margin = new Padding(0, 0, 0, 0)
            };
            breakdownTopPanel.Controls.Add(breakdownHeader);
            breakdownTopPanel.Controls.Add(new Label { Text = "Day", AutoSize = true, Margin = new Padding(0, 4, 6, 0) });
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

            AddTabFooter(clockTab);
            AddTabFooter(pomodoroTab);
            AddTabFooter(todoTab);
            AddTabFooter(profileTab);
            AddTabFooter(settingsTab);
            AddTabFooter(historyTab);
            AddTabFooter(focusHistoryTab);
            AddTabFooter(utilitiesTab);
            AddTabFooter(aboutTab);
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

        private void AddTabFooter(TabPage tab)
        {
            var footer = new Label
            {
                Text = $"© {DateTime.Now.Year} Deep Dey | All Right Reserved | Quicklink x Transperent Clock",
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SubtleText,
                BackColor = tab.BackColor,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular)
            };

            tab.Controls.Add(footer);
            footer.BringToFront();
        }

        private ClockSettings CaptureClockSettingsFromState(AppState state)
        {
            return new ClockSettings
            {
                ClockEnabled = state.ClockEnabled,
                ClockColorName = state.ClockColorName,
                ClockUseCustomColor = state.ClockUseCustomColor,
                ClockCustomColorArgb = state.ClockCustomColorArgb,
                ClockFontSize = state.ClockFontSize,
                ClockFontFamily = state.ClockFontFamily,
                ClockBorderEnabled = state.ClockBorderEnabled,
                ClockBorderColorName = state.ClockBorderColorName,
                ClockBorderUseCustomColor = state.ClockBorderUseCustomColor,
                ClockBorderCustomColorArgb = state.ClockBorderCustomColorArgb,
                ClockBorderWidth = state.ClockBorderWidth,
                ClockPosition = state.ClockPosition,
                ClockUseCustomPosition = state.ClockUseCustomPosition,
                ClockCustomPositionX = state.ClockCustomPositionX,
                ClockCustomPositionY = state.ClockCustomPositionY
            };
        }

        private void ApplyClockSettingsToState(ClockSettings settings)
        {
            Program.CurrentState.ClockEnabled = settings.ClockEnabled;
            Program.CurrentState.ClockColorName = settings.ClockColorName;
            Program.CurrentState.ClockUseCustomColor = settings.ClockUseCustomColor;
            Program.CurrentState.ClockCustomColorArgb = settings.ClockCustomColorArgb;
            Program.CurrentState.ClockFontSize = settings.ClockFontSize;
            Program.CurrentState.ClockFontFamily = settings.ClockFontFamily;
            Program.CurrentState.ClockBorderEnabled = settings.ClockBorderEnabled;
            Program.CurrentState.ClockBorderColorName = settings.ClockBorderColorName;
            Program.CurrentState.ClockBorderUseCustomColor = settings.ClockBorderUseCustomColor;
            Program.CurrentState.ClockBorderCustomColorArgb = settings.ClockBorderCustomColorArgb;
            Program.CurrentState.ClockBorderWidth = settings.ClockBorderWidth;
            Program.CurrentState.ClockPosition = settings.ClockPosition;
            Program.CurrentState.ClockUseCustomPosition = settings.ClockUseCustomPosition;
            Program.CurrentState.ClockCustomPositionX = settings.ClockCustomPositionX;
            Program.CurrentState.ClockCustomPositionY = settings.ClockCustomPositionY;

            ApplyClockSettingsToOverlay(settings);
        }

        private void LoadClockSettingsIntoUi(ClockSettings settings, bool applyOverlay)
        {
            isClockSettingsLoading = true;

            clockEnabledCheckBox.Checked = settings.ClockEnabled;

            if (fontFamilyComboBox.Items.Contains(settings.ClockFontFamily))
            {
                fontFamilyComboBox.SelectedItem = settings.ClockFontFamily;
            }
            else if (fontFamilyComboBox.Items.Count > 0)
            {
                fontFamilyComboBox.SelectedIndex = 0;
            }

            fontSizeTrackBar.Value = Math.Max(fontSizeTrackBar.Minimum, Math.Min(fontSizeTrackBar.Maximum, (int)Math.Round(settings.ClockFontSize)));

            if (settings.ClockUseCustomColor)
            {
                colorComboBox.SelectedItem = "Custom";
            }
            else if (colorComboBox.Items.Contains(settings.ClockColorName))
            {
                colorComboBox.SelectedItem = settings.ClockColorName;
            }
            else
            {
                colorComboBox.SelectedItem = "White";
            }

            borderEnabledCheckBox.Checked = settings.ClockBorderEnabled;
            if (settings.ClockBorderUseCustomColor)
            {
                borderColorComboBox.SelectedItem = "Custom";
            }
            else if (borderColorComboBox.Items.Contains(settings.ClockBorderColorName))
            {
                borderColorComboBox.SelectedItem = settings.ClockBorderColorName;
            }
            else
            {
                borderColorComboBox.SelectedItem = "White";
            }
            borderWidthInput.Value = Math.Max(borderWidthInput.Minimum, Math.Min(borderWidthInput.Maximum, settings.ClockBorderWidth));

            if (settings.ClockUseCustomPosition)
            {
                positionComboBox.SelectedItem = "Custom";
            }
            else if (positionComboBox.Items.Contains(settings.ClockPosition))
            {
                positionComboBox.SelectedItem = settings.ClockPosition;
            }
            else
            {
                positionComboBox.SelectedItem = "Top Right";
            }

            isClockSettingsLoading = false;

            if (applyOverlay)
            {
                ApplyClockSettingsToOverlay(settings);
            }
        }

        private void ApplyClockSettingsToOverlay(ClockSettings settings)
        {
            if (clockForm == null || clockForm.IsDisposed)
            {
                return;
            }

            clockForm.ApplyClockColor(ResolveClockColor(settings));
            clockForm.ApplyClockFontFamily(settings.ClockFontFamily);
            clockForm.ApplyClockFontSize(settings.ClockFontSize);
            clockForm.ApplyClockBorder(
                settings.ClockBorderEnabled,
                ResolveClockBorderColor(settings),
                settings.ClockBorderWidth);

            if (settings.ClockUseCustomPosition &&
                settings.ClockCustomPositionX.HasValue &&
                settings.ClockCustomPositionY.HasValue)
            {
                clockForm.ApplyClockCustomPosition(
                    settings.ClockCustomPositionX.Value,
                    settings.ClockCustomPositionY.Value);
            }
            else
            {
                clockForm.ApplyClockPosition(settings.ClockPosition);
            }

            clockForm.ApplyClockEnabled(settings.ClockEnabled);
        }

        private static Color ResolveClockColor(ClockSettings settings)
        {
            if (settings.ClockUseCustomColor && settings.ClockCustomColorArgb.HasValue)
            {
                return Color.FromArgb(settings.ClockCustomColorArgb.Value);
            }

            return Program.ResolveClockColor(settings.ClockColorName);
        }

        private static Color ResolveClockBorderColor(ClockSettings settings)
        {
            if (settings.ClockBorderUseCustomColor && settings.ClockBorderCustomColorArgb.HasValue)
            {
                return Color.FromArgb(settings.ClockBorderCustomColorArgb.Value);
            }

            return Program.ResolveClockColor(settings.ClockBorderColorName);
        }

        private sealed class ClockSettings
        {
            public bool ClockEnabled { get; set; }
            public string ClockColorName { get; set; } = "White";
            public bool ClockUseCustomColor { get; set; }
            public int? ClockCustomColorArgb { get; set; }
            public float ClockFontSize { get; set; } = 20f;
            public string ClockFontFamily { get; set; } = "Segoe UI";
            public bool ClockBorderEnabled { get; set; }
            public string ClockBorderColorName { get; set; } = "White";
            public bool ClockBorderUseCustomColor { get; set; }
            public int? ClockBorderCustomColorArgb { get; set; }
            public int ClockBorderWidth { get; set; } = 2;
            public string ClockPosition { get; set; } = "Top Right";
            public bool ClockUseCustomPosition { get; set; }
            public int? ClockCustomPositionX { get; set; }
            public int? ClockCustomPositionY { get; set; }
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

            var entries = FocusHistoryStorage.GetAll();
            
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
                int seconds = Math.Max(0, entry.DurationSeconds);
                int minutes = seconds / 60;
                int leftoverSeconds = seconds % 60;
                string durationText = $"{minutes:D2}:{leftoverSeconds:D2}";

                var item = new ListViewItem(dateText);
                item.SubItems.Add(timeText);
                item.SubItems.Add(durationText);
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
