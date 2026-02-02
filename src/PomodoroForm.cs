using System;
using System.Drawing;
using System.Windows.Forms;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class PomodoroForm : Form
{
    private readonly Label timeLabel;
    private readonly Label modeLabel;
    private readonly Button startButton;
    private readonly Button pauseButton;
    private readonly Button resumeButton;
    private readonly Button stopButton;
    private readonly Button workModeButton;
    private readonly Button shortBreakButton;
    private readonly Button longBreakButton;
    private readonly ProgressBar progressBar;
    private readonly Timer uiTimer;
    private readonly NumericUpDown workMinutesUpDown;
    private readonly NumericUpDown shortBreakMinutesUpDown;
    private readonly NumericUpDown longBreakMinutesUpDown;
    private readonly NumericUpDown longBreakIntervalUpDown;
    private readonly CheckBox autoStartCheckBox;
    private readonly CheckBox autoStartBreaksCheckBox;
    private bool isInitializingSettings;

    private PomodoroState State => Program.CurrentState.Pomodoro;
    private PomodoroSettings Settings => Program.CurrentState.PomodoroSettings;

    private static readonly Color AppBackground = Color.FromArgb(244, 246, 249);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color SubtleText = Color.FromArgb(95, 95, 95);
    private static readonly Font ModeFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);
    private static readonly Font TimerFont = new Font("Segoe UI", 44F, FontStyle.Bold);

    public PomodoroForm()
    {
        Text = "Pomodoro";
        Size = new Size(430, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppBackground;
        Padding = new Padding(16);
        RightToLeft = RightToLeft.No;

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground,
            Padding = new Padding(20)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RightToLeft = RightToLeft.No;

        modeLabel = new Label
        {
            AutoSize = true,
            Font = ModeFont,
            ForeColor = Color.White,
            Padding = new Padding(10, 4, 10, 4),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var modePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill
        };
        modePanel.Controls.Add(modeLabel);

        var modeContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 28
        };
        modeContainer.Controls.Add(modePanel);
        modePanel.Anchor = AnchorStyles.None;
        modePanel.Location = new Point((modeContainer.Width - modePanel.Width) / 2, 0);
        modeContainer.Resize += (_, __) =>
        {
            modePanel.Left = (modeContainer.Width - modePanel.Width) / 2;
        };

        timeLabel = new Label
        {
            Dock = DockStyle.Fill,
            Height = 100,
            Font = TimerFont,
            TextAlign = ContentAlignment.MiddleCenter,
            RightToLeft = RightToLeft.No,
            Margin = new Padding(0)
        };

        progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 8,
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };

        var modeButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 6, 0, 6)
        };

        var modeButtonsContainer = new Panel
        {
            Dock = DockStyle.Fill
        };
        modeButtonsContainer.Controls.Add(modeButtonsPanel);
        modeButtonsPanel.Anchor = AnchorStyles.None;
        modeButtonsContainer.Resize += (_, __) =>
        {
            modeButtonsPanel.Left = (modeButtonsContainer.Width - modeButtonsPanel.Width) / 2;
            modeButtonsPanel.Top = (modeButtonsContainer.Height - modeButtonsPanel.Height) / 2;
        };

        workModeButton = CreateModeButton("Work");
        shortBreakButton = CreateModeButton("Short Break");
        longBreakButton = CreateModeButton("Long Break");

        workModeButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.Work);
        shortBreakButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.ShortBreak);
        longBreakButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.LongBreak);

        modeButtonsPanel.Controls.Add(workModeButton);
        modeButtonsPanel.Controls.Add(shortBreakButton);
        modeButtonsPanel.Controls.Add(longBreakButton);

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(0, 6, 0, 4)
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var workLabel = CreateSettingsLabel("Work (minutes)");
        workMinutesUpDown = CreateMinutesUpDown();

        var shortLabel = CreateSettingsLabel("Short break (minutes)");
        shortBreakMinutesUpDown = CreateMinutesUpDown();

        var longLabel = CreateSettingsLabel("Long break (minutes)");
        longBreakMinutesUpDown = CreateMinutesUpDown();

        var intervalLabel = CreateSettingsLabel("Sessions before long break");
        longBreakIntervalUpDown = CreateIntervalUpDown();

        autoStartCheckBox = new CheckBox
        {
            Text = "Auto-start next session",
            AutoSize = true,
            ForeColor = SubtleText
        };

        autoStartBreaksCheckBox = new CheckBox
        {
            Text = "Auto-start breaks",
            AutoSize = true,
            ForeColor = SubtleText
        };

        settingsLayout.Controls.Add(workLabel, 0, 0);
        settingsLayout.Controls.Add(workMinutesUpDown, 1, 0);
        settingsLayout.Controls.Add(shortLabel, 0, 1);
        settingsLayout.Controls.Add(shortBreakMinutesUpDown, 1, 1);
        settingsLayout.Controls.Add(longLabel, 0, 2);
        settingsLayout.Controls.Add(longBreakMinutesUpDown, 1, 2);
        settingsLayout.Controls.Add(intervalLabel, 0, 3);
        settingsLayout.Controls.Add(longBreakIntervalUpDown, 1, 3);
        settingsLayout.Controls.Add(autoStartCheckBox, 0, 4);
        settingsLayout.SetColumnSpan(autoStartCheckBox, 2);
        settingsLayout.Controls.Add(autoStartBreaksCheckBox, 0, 5);
        settingsLayout.SetColumnSpan(autoStartBreaksCheckBox, 2);

        var actionButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        var actionButtonsContainer = new Panel
        {
            Dock = DockStyle.Fill
        };
        actionButtonsContainer.Controls.Add(actionButtonsPanel);
        actionButtonsPanel.Anchor = AnchorStyles.None;
        actionButtonsContainer.Resize += (_, __) =>
        {
            actionButtonsPanel.Left = (actionButtonsContainer.Width - actionButtonsPanel.Width) / 2;
            actionButtonsPanel.Top = (actionButtonsContainer.Height - actionButtonsPanel.Height) / 2;
        };

        startButton = CreateActionButton("Start", Color.FromArgb(66, 160, 100));
        pauseButton = CreateActionButton("Pause", Color.FromArgb(240, 170, 60));
        resumeButton = CreateActionButton("Resume", Color.FromArgb(60, 130, 220));
        stopButton = CreateActionButton("Stop", Color.FromArgb(220, 90, 90));

        startButton.Click += (_, __) =>
        {
            State.Start(Settings);
            AppStateStorage.Save(Program.CurrentState);
            UpdateDisplay();
            UpdateButtons();
        };

        pauseButton.Click += (_, __) =>
        {
            State.Pause(Settings);
            AppStateStorage.Save(Program.CurrentState);
            UpdateDisplay();
            UpdateButtons();
        };

        resumeButton.Click += (_, __) =>
        {
            State.Resume();
            AppStateStorage.Save(Program.CurrentState);
            UpdateDisplay();
            UpdateButtons();
        };

        stopButton.Click += (_, __) =>
        {
            State.Stop(Settings);
            AppStateStorage.Save(Program.CurrentState);
            UpdateDisplay();
            UpdateButtons();
        };

        actionButtonsPanel.Controls.Add(startButton);
        actionButtonsPanel.Controls.Add(pauseButton);
        actionButtonsPanel.Controls.Add(resumeButton);
        actionButtonsPanel.Controls.Add(stopButton);

        layout.Controls.Add(modeContainer, 0, 0);
        layout.Controls.Add(timeLabel, 0, 1);
        layout.Controls.Add(progressBar, 0, 2);
        layout.Controls.Add(modeButtonsContainer, 0, 3);
        layout.Controls.Add(settingsLayout, 0, 4);
        layout.Controls.Add(actionButtonsContainer, 0, 5);

        card.Controls.Add(layout);
        Controls.Add(card);

        HookSettingsEvents();
        LoadSettingsIntoUi();

        uiTimer = new Timer { Interval = 1000 };
        uiTimer.Tick += (_, __) =>
        {
            State.Tick(DateTime.UtcNow, Settings);
            UpdateDisplay();
            UpdateButtons();
        };
        uiTimer.Start();

        UpdateDisplay();
        UpdateButtons();
    }

    private void HookSettingsEvents()
    {
        workMinutesUpDown.ValueChanged += (_, __) => ApplySettingsFromUi();
        shortBreakMinutesUpDown.ValueChanged += (_, __) => ApplySettingsFromUi();
        longBreakMinutesUpDown.ValueChanged += (_, __) => ApplySettingsFromUi();
        longBreakIntervalUpDown.ValueChanged += (_, __) => ApplySettingsFromUi();
        autoStartCheckBox.CheckedChanged += (_, __) =>
        {
            if (isInitializingSettings)
            {
                return;
            }

            Settings.AutoStartNextSession = autoStartCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };

        autoStartBreaksCheckBox.CheckedChanged += (_, __) =>
        {
            if (isInitializingSettings)
            {
                return;
            }

            Settings.AutoStartBreaks = autoStartBreaksCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };
    }

    private void LoadSettingsIntoUi()
    {
        isInitializingSettings = true;

        workMinutesUpDown.Value = ClampToRange(Settings.FocusMinutes, workMinutesUpDown.Minimum, workMinutesUpDown.Maximum);
        shortBreakMinutesUpDown.Value = ClampToRange(Settings.ShortBreakMinutes, shortBreakMinutesUpDown.Minimum, shortBreakMinutesUpDown.Maximum);
        longBreakMinutesUpDown.Value = ClampToRange(Settings.LongBreakMinutes, longBreakMinutesUpDown.Minimum, longBreakMinutesUpDown.Maximum);
        longBreakIntervalUpDown.Value = ClampToRange(Settings.SessionsBeforeLongBreak, longBreakIntervalUpDown.Minimum, longBreakIntervalUpDown.Maximum);
        autoStartCheckBox.Checked = Settings.AutoStartNextSession;
        autoStartBreaksCheckBox.Checked = Settings.AutoStartBreaks;

        isInitializingSettings = false;
    }

    private void ApplySettingsFromUi()
    {
        if (isInitializingSettings)
        {
            return;
        }

        Settings.FocusMinutes = (int)workMinutesUpDown.Value;
        Settings.ShortBreakMinutes = (int)shortBreakMinutesUpDown.Value;
        Settings.LongBreakMinutes = (int)longBreakMinutesUpDown.Value;
        Settings.SessionsBeforeLongBreak = (int)longBreakIntervalUpDown.Value;

        State.ApplySettings(Settings);

        AppStateStorage.Save(Program.CurrentState);
        UpdateDisplay();
        UpdateButtons();
    }

    private void SwitchMode(PomodoroState.PomodoroMode mode)
    {
        State.SwitchMode(mode, Settings);
        AppStateStorage.Save(Program.CurrentState);
        UpdateDisplay();
        UpdateButtons();
    }

    private void UpdateDisplay()
    {
        int seconds = Math.Max(0, State.RemainingSeconds);
        var time = TimeSpan.FromSeconds(seconds);
        timeLabel.Text = $"{time.Minutes:00}:{time.Seconds:00}";
        modeLabel.Text = GetModeText(State.CurrentMode);
        modeLabel.BackColor = GetModeColor(State.CurrentMode);
        timeLabel.ForeColor = GetModeAccent(State.CurrentMode);
        BackColor = GetModeBackground(State.CurrentMode);

        int totalSeconds = State.GetModeTotalSeconds(State.CurrentMode, Settings);
        if (totalSeconds <= 0)
        {
            progressBar.Value = 0;
        }
        else
        {
            int percent = (int)Math.Round(100.0 * seconds / totalSeconds);
            progressBar.Value = Math.Max(0, Math.Min(100, percent));
        }
    }

    private static Color GetModeColor(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => Color.FromArgb(80, 135, 215),
            PomodoroState.PomodoroMode.ShortBreak => Color.FromArgb(90, 175, 125),
            PomodoroState.PomodoroMode.LongBreak => Color.FromArgb(150, 115, 210),
            _ => Color.FromArgb(80, 135, 215)
        };
    }

    private static Color GetModeBackground(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => Color.FromArgb(244, 246, 249),
            PomodoroState.PomodoroMode.ShortBreak => Color.FromArgb(241, 248, 244),
            PomodoroState.PomodoroMode.LongBreak => Color.FromArgb(246, 242, 251),
            _ => Color.FromArgb(244, 246, 249)
        };
    }

    private static Color GetModeAccent(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => Color.FromArgb(50, 100, 175),
            PomodoroState.PomodoroMode.ShortBreak => Color.FromArgb(60, 145, 100),
            PomodoroState.PomodoroMode.LongBreak => Color.FromArgb(125, 95, 195),
            _ => Color.FromArgb(50, 100, 175)
        };
    }

    private static string GetModeText(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => "Work",
            PomodoroState.PomodoroMode.ShortBreak => "Short",
            PomodoroState.PomodoroMode.LongBreak => "Long",
            _ => "Work"
        };
    }

    private void UpdateButtons()
    {
        bool running = State.IsRunning;
        bool paused = State.IsPaused;

        startButton.Enabled = !running || State.RemainingSeconds == 0;
        pauseButton.Enabled = running && !paused;
        resumeButton.Enabled = running && paused;
        stopButton.Enabled = running || State.RemainingSeconds > 0;
    }

    private static Button CreateActionButton(string text, Color background)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 86,
            Height = 34,
            Margin = new Padding(6, 0, 6, 0),
            Padding = new Padding(8, 6, 8, 6),
            FlatStyle = FlatStyle.Flat,
            BackColor = background,
            ForeColor = Color.White
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static Button CreateModeButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 110,
            Height = 30,
            Margin = new Padding(6, 0, 6, 0),
            Padding = new Padding(8, 4, 8, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(50, 50, 50)
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static Label CreateSettingsLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = SubtleText
        };
    }

    private static NumericUpDown CreateMinutesUpDown()
    {
        return new NumericUpDown
        {
            Minimum = 1,
            Maximum = 180,
            Width = 80
        };
    }

    private static NumericUpDown CreateIntervalUpDown()
    {
        return new NumericUpDown
        {
            Minimum = 1,
            Maximum = 12,
            Width = 80
        };
    }

    private static decimal ClampToRange(int value, decimal min, decimal max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
