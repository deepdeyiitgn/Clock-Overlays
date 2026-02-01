using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class DashboardForm : Form
{
    private static readonly Font BaseFont = new Font("Segoe UI", 10F, FontStyle.Regular);
    private static readonly Font HeaderFont = new Font("Segoe UI", 11F, FontStyle.Bold);
    private static readonly Font LabelFont = new Font("Segoe UI", 10.5F, FontStyle.Regular);
    private static readonly Color AppBackground = Color.FromArgb(247, 247, 247);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color SeparatorColor = Color.FromArgb(220, 220, 220);

    private readonly TransparentClockForm clockForm;
    private bool allowClose;
    private readonly CheckBox clockEnabledCheckBox;
    private readonly TrackBar fontSizeTrackBar;
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

        var clockTab = new TabPage("Clock") { Padding = new Padding(12), BackColor = AppBackground };
        var pomodoroTab = new TabPage("Pomodoro") { Padding = new Padding(12), BackColor = AppBackground };
        var profileTab = new TabPage("Profile") { Padding = new Padding(12), BackColor = AppBackground };
        var settingsTab = new TabPage("Settings") { Padding = new Padding(12), BackColor = AppBackground };

        tabs.TabPages.Add(clockTab);
        tabs.TabPages.Add(pomodoroTab);
        tabs.TabPages.Add(profileTab);
        tabs.TabPages.Add(settingsTab);

        Controls.Add(tabs);

        clockForm = new TransparentClockForm();
        Program.RegisterClockForm(clockForm);
        clockForm.Show();
        Program.ApplyClockStateToOverlay();

        var clockCard = CreateCardPanel();

        var clockLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 2,
            RowCount = 9
        };
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

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
            Text = "Clock Controls"
        };

        greetingLabel = new Label
        {
            AutoSize = true,
            Font = HeaderFont,
            Text = Program.GetGreetingText()
        };

        clockEnabledCheckBox = new CheckBox
        {
            Text = "Enable Clock Overlay",
            Checked = Program.CurrentState.ClockEnabled,
            AutoSize = true
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
            AutoSize = true
        };

        fontSizeTrackBar = new TrackBar
        {
            Minimum = 12,
            Maximum = 48,
            TickFrequency = 2,
            Value = (int)Math.Round(Program.CurrentState.ClockFontSize)
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
            AutoSize = true
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
            WrapContents = false
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
            AutoSize = true
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
            "Bottom Right"
        });

        positionComboBox.SelectedIndexChanged += (_, __) =>
        {
            if (positionComboBox.SelectedItem is string position)
            {
                Program.CurrentState.ClockPosition = position;
                clockForm.ApplyClockPosition(position);
                AppStateStorage.Save(Program.CurrentState);
            }
        };

        var clockSeparator1 = CreateSeparator();
        var clockSeparator2 = CreateSeparator();

        clockLayout.Controls.Add(clockHeader, 0, 0);
        clockLayout.SetColumnSpan(clockHeader, 2);
        clockLayout.Controls.Add(greetingLabel, 0, 1);
        clockLayout.SetColumnSpan(greetingLabel, 2);
        clockLayout.Controls.Add(clockSeparator1, 0, 2);
        clockLayout.SetColumnSpan(clockSeparator1, 2);
        clockLayout.Controls.Add(clockEnabledCheckBox, 0, 3);
        clockLayout.SetColumnSpan(clockEnabledCheckBox, 2);
        clockLayout.Controls.Add(clockSeparator2, 0, 4);
        clockLayout.SetColumnSpan(clockSeparator2, 2);
        clockLayout.Controls.Add(fontSizeLabel, 0, 5);
        clockLayout.Controls.Add(fontSizeTrackBar, 1, 5);
        clockLayout.Controls.Add(colorLabel, 0, 6);
        clockLayout.Controls.Add(colorPanel, 1, 6);
        clockLayout.Controls.Add(positionLabel, 0, 7);
        clockLayout.Controls.Add(positionComboBox, 1, 7);

        clockCard.Controls.Add(clockLayout);
        clockTab.Controls.Add(clockCard);

        var pomodoroCard = CreateCardPanel();

        var pomodoroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 2
        };
        pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pomodoroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var pomodoroHeader = new Label
        {
            Text = "Pomodoro",
            AutoSize = true,
            Font = HeaderFont
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

        var profileCard = CreateCardPanel();

        var profileLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 2,
            RowCount = 4
        };
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        profileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var profileHeader = new Label
        {
            Text = "Profile",
            AutoSize = true,
            Font = HeaderFont
        };

        var nameLabel = new Label
        {
            Text = "Name",
            Font = LabelFont,
            AutoSize = true
        };

        profileNameTextBox = new TextBox
        {
            Width = 240
        };

        var genderLabel = new Label
        {
            Text = "Gender",
            Font = LabelFont,
            AutoSize = true
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
            WrapContents = false
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
        profileLayout.Controls.Add(nameLabel, 0, 1);
        profileLayout.Controls.Add(profileNameTextBox, 1, 1);
        profileLayout.Controls.Add(genderLabel, 0, 2);
        profileLayout.Controls.Add(profileGenderComboBox, 1, 2);
        profileLayout.Controls.Add(profileButtonsPanel, 0, 3);
        profileLayout.SetColumnSpan(profileButtonsPanel, 2);

        profileCard.Controls.Add(profileLayout);
        profileTab.Controls.Add(profileCard);

        var settingsCard = CreateCardPanel();

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 2,
            RowCount = 5
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var settingsHeader = new Label
        {
            Text = "Settings",
            AutoSize = true,
            Font = HeaderFont
        };

        launchOnStartupCheckBox = new CheckBox
        {
            Text = "Launch app on Windows startup",
            AutoSize = true,
            Checked = Program.CurrentState.LaunchOnStartup
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
            Checked = Program.CurrentState.ShowWelcomeOnStartup
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
            Checked = Program.CurrentState.MinimizeToTrayOnClose
        };
        minimizeToTrayCheckBox.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.MinimizeToTrayOnClose = minimizeToTrayCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };

        resetSettingsButton = new Button
        {
            Text = "Reset Settings",
            AutoSize = true
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
        settingsLayout.Controls.Add(launchOnStartupCheckBox, 0, 1);
        settingsLayout.SetColumnSpan(launchOnStartupCheckBox, 2);
        settingsLayout.Controls.Add(showWelcomeCheckBox, 0, 2);
        settingsLayout.SetColumnSpan(showWelcomeCheckBox, 2);
        settingsLayout.Controls.Add(minimizeToTrayCheckBox, 0, 3);
        settingsLayout.SetColumnSpan(minimizeToTrayCheckBox, 2);
        settingsLayout.Controls.Add(resetSettingsButton, 0, 4);
        settingsLayout.SetColumnSpan(resetSettingsButton, 2);

        settingsCard.Controls.Add(settingsLayout);
        settingsTab.Controls.Add(settingsCard);

        RestoreClockSettingsToUi();
        RefreshProfileUi();
        RefreshSettingsUi();

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
            Margin = new Padding(12)
        };
    }

    private static Panel CreateSeparator()
    {
        return new Panel
        {
            Height = 1,
            Dock = DockStyle.Fill,
            BackColor = SeparatorColor,
            Margin = new Padding(0, 8, 0, 8)
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

        if (positionComboBox.Items.Contains(Program.CurrentState.ClockPosition))
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
