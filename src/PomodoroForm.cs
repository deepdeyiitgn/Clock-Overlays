using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
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
    private readonly RainbowProgressBar progressBar;
    private readonly Label progressPercentLabel;
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
    private static readonly Font ModeFont = new Font("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Font TimerFont = new Font("Segoe UI", 36F, FontStyle.Bold);
    private static readonly Color DisabledButtonBack = Color.FromArgb(220, 220, 220);
    private static readonly Color DisabledButtonText = Color.FromArgb(120, 120, 120);

    public PomodoroForm()
    {
        Text = "Pomodoro";
        Size = new Size(500, 460);
        MinimumSize = new Size(420, 420);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppBackground;
        Padding = new Padding(10);
        RightToLeft = RightToLeft.No;

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBackground,
            Padding = new Padding(12)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(2)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RightToLeft = RightToLeft.No;

        modeLabel = new Label
        {
            AutoSize = false,
            Font = ModeFont,
            ForeColor = Color.White,
            Padding = new Padding(6, 1, 6, 1),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Height = 22,
            MinimumSize = new Size(0, 22),
            Margin = new Padding(0, 0, 0, 2)
        };

        timeLabel = new Label
        {
            Dock = DockStyle.Fill,
            Height = 90,
            Font = TimerFont,
            TextAlign = ContentAlignment.MiddleCenter,
            RightToLeft = RightToLeft.No,
            Margin = new Padding(0, 2, 0, 2),
            AutoSize = false
        };

        progressBar = new RainbowProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 8,
            Maximum = 100,
            MinimumSize = new Size(0, 8),
            MaximumSize = new Size(0, 8),
            Margin = new Padding(0, 2, 0, 0)
        };

        progressPercentLabel = new Label
        {
            AutoSize = true,
            ForeColor = SubtleText,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 2)
        };

        var modeButtonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        modeButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        modeButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        modeButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        workModeButton = CreateModeButton("Work");
        shortBreakButton = CreateModeButton("Short Break");
        longBreakButton = CreateModeButton("Long Break");

        workModeButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.Work);
        shortBreakButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.ShortBreak);
        longBreakButton.Click += (_, __) => SwitchMode(PomodoroState.PomodoroMode.LongBreak);

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(2, 2, 2, 2),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 4, 0, 2)
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        for (int i = 0; i < 6; i++)
        {
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

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
            ForeColor = SubtleText,
            Margin = new Padding(0, 4, 0, 0)
        };

        autoStartBreaksCheckBox = new CheckBox
        {
            Text = "Auto-start breaks",
            AutoSize = true,
            ForeColor = SubtleText,
            Margin = new Padding(0, 2, 0, 0)
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

        var actionButtonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(2, 2, 2, 2),
            AutoSize = false,
            Margin = new Padding(0, 4, 0, 0),
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            MinimumSize = new Size(0, 72)
        };
        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actionButtonsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        actionButtonsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        startButton = CreateActionButton("Start", Color.FromArgb(66, 160, 100));
        pauseButton = CreateActionButton("Pause", Color.FromArgb(240, 170, 60));
        resumeButton = CreateActionButton("Resume", Color.FromArgb(60, 130, 220));
        stopButton = CreateActionButton("Stop", Color.FromArgb(220, 90, 90));

        WireActionButtonState(startButton);
        WireActionButtonState(pauseButton);
        WireActionButtonState(resumeButton);
        WireActionButtonState(stopButton);

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

        startButton.Dock = DockStyle.Fill;
        pauseButton.Dock = DockStyle.Fill;
        resumeButton.Dock = DockStyle.Fill;
        stopButton.Dock = DockStyle.Fill;
        actionButtonsPanel.Controls.Add(startButton, 0, 0);
        actionButtonsPanel.Controls.Add(pauseButton, 1, 0);
        actionButtonsPanel.Controls.Add(resumeButton, 0, 1);
        actionButtonsPanel.Controls.Add(stopButton, 1, 1);

        workModeButton.Dock = DockStyle.Fill;
        shortBreakButton.Dock = DockStyle.Fill;
        longBreakButton.Dock = DockStyle.Fill;
        modeButtonsPanel.Controls.Add(workModeButton, 0, 0);
        modeButtonsPanel.Controls.Add(shortBreakButton, 1, 0);
        modeButtonsPanel.Controls.Add(longBreakButton, 2, 0);

        var timerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0)
        };
        timerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        timerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        timerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        timerLayout.Controls.Add(timeLabel, 0, 0);
        timerLayout.Controls.Add(progressPercentLabel, 0, 1);
        timerLayout.Controls.Add(progressBar, 0, 2);

        var modeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };
        modeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        modeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        modeLayout.Controls.Add(modeLabel, 0, 0);
        modeLayout.Controls.Add(modeButtonsPanel, 0, 1);

        layout.Controls.Add(modeLayout, 0, 0);
        layout.Controls.Add(timerLayout, 0, 1);
        layout.Controls.Add(settingsLayout, 0, 2);
        layout.Controls.Add(actionButtonsPanel, 0, 3);

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
            progressBar.AdvancePhase();
        };
        uiTimer.Start();

        UpdateDisplay();
        UpdateButtons();

        Resize += (_, __) => AdjustTimerFont();
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
        UpdateModeButtonsAppearance();

        int totalSeconds = State.GetModeTotalSeconds(State.CurrentMode, Settings);
        if (totalSeconds <= 0)
        {
            progressBar.Value = 0;
            progressPercentLabel.Text = "0%";
        }
        else
        {
            int percent = (int)Math.Round(100.0 * seconds / totalSeconds);
            percent = Math.Max(0, Math.Min(100, percent));
            progressBar.Value = percent;
            progressPercentLabel.Text = $"{percent}%";
        }

        AdjustTimerFont();
    }

    private void AdjustTimerFont()
    {
        if (timeLabel.Width <= 0 || timeLabel.Height <= 0)
        {
            return;
        }

        float maxSize = TimerFont.Size;
        float minSize = 24F;
        float targetSize = maxSize;

        using var testFont = new Font(timeLabel.Font.FontFamily, targetSize, FontStyle.Bold);
        Size textSize = TextRenderer.MeasureText(timeLabel.Text, testFont);
        int availableWidth = Math.Max(0, timeLabel.Width - 10);
        int availableHeight = Math.Max(0, timeLabel.Height - 6);

        if (textSize.Width > availableWidth || textSize.Height > availableHeight)
        {
            float widthScale = availableWidth > 0 ? (float)availableWidth / textSize.Width : 1F;
            float heightScale = availableHeight > 0 ? (float)availableHeight / textSize.Height : 1F;
            float scale = Math.Min(widthScale, heightScale);
            targetSize = Math.Max(minSize, maxSize * scale);
        }

        if (Math.Abs(timeLabel.Font.Size - targetSize) > 0.5F)
        {
            timeLabel.Font = new Font(timeLabel.Font.FontFamily, targetSize, FontStyle.Bold);
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

    private void UpdateModeButtonsAppearance()
    {
        ApplyModeButtonState(workModeButton, State.CurrentMode == PomodoroState.PomodoroMode.Work);
        ApplyModeButtonState(shortBreakButton, State.CurrentMode == PomodoroState.PomodoroMode.ShortBreak);
        ApplyModeButtonState(longBreakButton, State.CurrentMode == PomodoroState.PomodoroMode.LongBreak);
    }

    private static void ApplyModeButtonState(Button button, bool isActive)
    {
        if (isActive)
        {
            button.ForeColor = GetModeAccentFromText(button.Text);
            button.FlatAppearance.BorderColor = GetModeColorFromText(button.Text);
            button.FlatAppearance.BorderSize = 2;
        }
        else
        {
            button.ForeColor = Color.FromArgb(50, 50, 50);
            button.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            button.FlatAppearance.BorderSize = 1;
        }
    }

    private static Color GetModeColorFromText(string text)
    {
        return text.StartsWith("Short", StringComparison.OrdinalIgnoreCase)
            ? GetModeColor(PomodoroState.PomodoroMode.ShortBreak)
            : text.StartsWith("Long", StringComparison.OrdinalIgnoreCase)
                ? GetModeColor(PomodoroState.PomodoroMode.LongBreak)
                : GetModeColor(PomodoroState.PomodoroMode.Work);
    }

    private static Color GetModeAccentFromText(string text)
    {
        return text.StartsWith("Short", StringComparison.OrdinalIgnoreCase)
            ? GetModeAccent(PomodoroState.PomodoroMode.ShortBreak)
            : text.StartsWith("Long", StringComparison.OrdinalIgnoreCase)
                ? GetModeAccent(PomodoroState.PomodoroMode.LongBreak)
                : GetModeAccent(PomodoroState.PomodoroMode.Work);
    }

    private static void WireActionButtonState(Button button)
    {
        button.Tag = button.BackColor;
        button.UseVisualStyleBackColor = false;
        button.EnabledChanged += (_, __) => ApplyActionButtonState(button);
        ApplyActionButtonState(button);
    }

    private static void ApplyActionButtonState(Button button)
    {
        if (button.Enabled)
        {
            button.BackColor = button.Tag is Color color ? color : button.BackColor;
            button.ForeColor = Color.White;
        }
        else
        {
            button.BackColor = DisabledButtonBack;
            button.ForeColor = DisabledButtonText;
        }
    }

    private static Button CreateActionButton(string text, Color background)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 100,
            Height = 34,
            MinimumSize = new Size(96, 32),
            Margin = new Padding(4, 3, 4, 3),
            Padding = new Padding(6, 4, 6, 4),
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
            Width = 96,
            Height = 26,
            Margin = new Padding(3, 0, 3, 0),
            Padding = new Padding(4, 2, 4, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(50, 50, 50),
            UseVisualStyleBackColor = false
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
            ForeColor = SubtleText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static NumericUpDown CreateMinutesUpDown()
    {
        return new NumericUpDown
        {
            Minimum = 1,
            Maximum = 180,
            Width = 70,
            Anchor = AnchorStyles.Right
        };
    }

    private static NumericUpDown CreateIntervalUpDown()
    {
        return new NumericUpDown
        {
            Minimum = 1,
            Maximum = 12,
            Width = 70,
            Anchor = AnchorStyles.Right
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

public class RainbowProgressBar : Control
{
    private int value;
    private float phase;

    public int Maximum { get; set; } = 100;

    public int Value
    {
        get => value;
        set
        {
            int clamped = Math.Max(0, Math.Min(Maximum, value));
            if (clamped == this.value)
            {
                return;
            }

            this.value = clamped;
            Invalidate();
        }
    }

    public RainbowProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Height = 8;
        MinimumSize = new Size(0, 8);
    }

    public void AdvancePhase()
    {
        phase += 6F;
        if (phase >= 360F)
        {
            phase = 0F;
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var background = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var backBrush = new SolidBrush(Color.FromArgb(235, 235, 235)))
        {
            e.Graphics.FillRectangle(backBrush, background);
        }

        int fillWidth = (int)Math.Round(Width * (Maximum == 0 ? 0 : (double)Value / Maximum));
        if (fillWidth <= 0)
        {
            return;
        }

        var fillRect = new Rectangle(0, 0, Math.Min(fillWidth, Width), Height - 1);
        using (var brush = CreateRainbowBrush(fillRect, phase))
        {
            e.Graphics.FillRectangle(brush, fillRect);
        }
    }

    private static Brush CreateRainbowBrush(Rectangle bounds, float phase)
    {
        var brush = new LinearGradientBrush(bounds, Color.Red, Color.Blue, 0F);
        var blend = new ColorBlend
        {
            Colors = new[]
            {
                Color.Red,
                Color.Orange,
                Color.Yellow,
                Color.Green,
                Color.DeepSkyBlue,
                Color.MediumBlue,
                Color.MediumVioletRed
            },
            Positions = new[] { 0F, 0.16F, 0.33F, 0.5F, 0.66F, 0.83F, 1F }
        };
        brush.InterpolationColors = blend;
        brush.WrapMode = WrapMode.Tile;
        var matrix = new Matrix();
        matrix.Translate(phase, 0F);
        brush.Transform = matrix;
        return brush;
    }
}
