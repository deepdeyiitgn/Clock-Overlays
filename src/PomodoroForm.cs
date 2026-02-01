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
    private bool isInitializingSettings;

    private PomodoroState State => Program.CurrentState.Pomodoro;

    private static readonly Color AppBackground = Color.FromArgb(247, 247, 247);
    private static readonly Color CardBackground = Color.White;
    private static readonly Color SubtleText = Color.FromArgb(90, 90, 90);
    private static readonly Font ModeFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
    private static readonly Font TimerFont = new Font("Segoe UI", 36F, FontStyle.Bold);

    public PomodoroForm()
    {
        Text = "Pomodoro";
        Size = new Size(420, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppBackground;
        Padding = new Padding(16);

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground,
            Padding = new Padding(18)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
            Height = 36
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
            Height = 72,
            Font = TimerFont,
            TextAlign = ContentAlignment.MiddleCenter
        };

        progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 10,
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };

        var modeButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
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
            RowCount = 5,
            Padding = new Padding(0, 10, 0, 4)
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

        var actionButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        startButton = CreateActionButton("Start", Color.FromArgb(66, 160, 100));
        pauseButton = CreateActionButton("Pause", Color.FromArgb(240, 170, 60));
        resumeButton = CreateActionButton("Resume", Color.FromArgb(60, 130, 220));
        stopButton = CreateActionButton("Stop", Color.FromArgb(220, 90, 90));

        startButton.Click += (_, __) =>
        {
            State.Start();
            AppStateStorage.Save(Program.CurrentState);
            UpdateDisplay();
            UpdateButtons();
        };

        pauseButton.Click += (_, __) =>
        {
            State.Pause();
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
            State.Stop();
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
        layout.Controls.Add(modeButtonsPanel, 0, 3);
        layout.Controls.Add(settingsLayout, 0, 4);
        layout.Controls.Add(actionButtonsPanel, 0, 5);

        card.Controls.Add(layout);
        Controls.Add(card);

        HookSettingsEvents();
        LoadSettingsIntoUi();

        uiTimer = new Timer { Interval = 1000 };
        uiTimer.Tick += (_, __) =>
        {
            State.Tick(DateTime.UtcNow);
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

            State.AutoStartNextSession = autoStartCheckBox.Checked;
            AppStateStorage.Save(Program.CurrentState);
        };
    }

    private void LoadSettingsIntoUi()
    {
        isInitializingSettings = true;

        workMinutesUpDown.Value = ClampToRange(State.WorkMinutes, workMinutesUpDown.Minimum, workMinutesUpDown.Maximum);
        shortBreakMinutesUpDown.Value = ClampToRange(State.ShortBreakMinutes, shortBreakMinutesUpDown.Minimum, shortBreakMinutesUpDown.Maximum);
        longBreakMinutesUpDown.Value = ClampToRange(State.LongBreakMinutes, longBreakMinutesUpDown.Minimum, longBreakMinutesUpDown.Maximum);
        longBreakIntervalUpDown.Value = ClampToRange(State.LongBreakInterval, longBreakIntervalUpDown.Minimum, longBreakIntervalUpDown.Maximum);
        autoStartCheckBox.Checked = State.AutoStartNextSession;

        isInitializingSettings = false;
    }

    private void ApplySettingsFromUi()
    {
        if (isInitializingSettings)
        {
            return;
        }

        State.ApplySettings(
            (int)workMinutesUpDown.Value,
            (int)shortBreakMinutesUpDown.Value,
            (int)longBreakMinutesUpDown.Value,
            (int)longBreakIntervalUpDown.Value);

        AppStateStorage.Save(Program.CurrentState);
        UpdateDisplay();
        UpdateButtons();
    }

    private void SwitchMode(PomodoroState.PomodoroMode mode)
    {
        State.SwitchMode(mode);
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

        int totalSeconds = State.GetModeTotalSeconds(State.CurrentMode);
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
            PomodoroState.PomodoroMode.Work => Color.FromArgb(70, 130, 210),
            PomodoroState.PomodoroMode.ShortBreak => Color.FromArgb(80, 170, 120),
            PomodoroState.PomodoroMode.LongBreak => Color.FromArgb(130, 100, 200),
            _ => Color.FromArgb(70, 130, 210)
        };
    }

    private static Color GetModeAccent(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => Color.FromArgb(45, 90, 170),
            PomodoroState.PomodoroMode.ShortBreak => Color.FromArgb(55, 140, 95),
            PomodoroState.PomodoroMode.LongBreak => Color.FromArgb(115, 85, 190),
            _ => Color.FromArgb(45, 90, 170)
        };
    }

    private static string GetModeText(PomodoroState.PomodoroMode mode)
    {
        return mode switch
        {
            PomodoroState.PomodoroMode.Work => "Work",
            PomodoroState.PomodoroMode.ShortBreak => "Short Break",
            PomodoroState.PomodoroMode.LongBreak => "Long Break",
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
            AutoSize = true,
            Padding = new Padding(14, 6, 14, 6),
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
            AutoSize = true,
            Padding = new Padding(10, 4, 10, 4),
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
