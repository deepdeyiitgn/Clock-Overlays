using System;
using System.Drawing;
using System.Windows.Forms;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class PomodoroTrayForm : Form
{
    private readonly Label timeLabel;
    private readonly Label modeLabel;
    private readonly Button toggleButton;
    private readonly Timer uiTimer;

    private PomodoroState State => Program.CurrentState.Pomodoro;
    private PomodoroSettings Settings => Program.CurrentState.PomodoroSettings;

    public PomodoroTrayForm()
    {
        Text = "Pomodoro";
        Icon = Program.GetAppIcon();
        Size = new Size(260, 170);
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        modeLabel = new Label
        {
            Dock = DockStyle.Fill,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50)
        };

        timeLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 28F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 90, 160)
        };

        toggleButton = new Button
        {
            Dock = DockStyle.Fill,
            Height = 34,
            FlatStyle = FlatStyle.Flat
        };
        toggleButton.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
        toggleButton.FlatAppearance.BorderSize = 1;
        toggleButton.Click += (_, __) => ToggleSession();

        layout.Controls.Add(modeLabel, 0, 0);
        layout.Controls.Add(timeLabel, 0, 1);
        layout.Controls.Add(toggleButton, 0, 2);

        Controls.Add(layout);

        uiTimer = new Timer { Interval = 1000 };
        uiTimer.Tick += (_, __) =>
        {
            State.Tick(DateTime.UtcNow, Settings);
            UpdateDisplay();
            UpdateButtons();
            HandleCompletionNotifications();
        };
        uiTimer.Start();

        UpdateDisplay();
        UpdateButtons();
    }

    private void ToggleSession()
    {
        if (!State.IsRunning)
        {
            State.StartSession(Settings);
        }
        else if (State.IsPaused)
        {
            State.ResumeSession();
        }
        else
        {
            State.PauseSession(Settings);
        }

        AppStateStorage.Save(Program.CurrentState);
        UpdateDisplay();
        UpdateButtons();
    }

    private void UpdateDisplay()
    {
        int seconds = Math.Max(0, State.RemainingSeconds);
        var time = TimeSpan.FromSeconds(seconds);
        timeLabel.Text = $"{time.Minutes:00}:{time.Seconds:00}";

        string modeText = State.CurrentMode switch
        {
            PomodoroState.PomodoroMode.Work => "Work",
            PomodoroState.PomodoroMode.ShortBreak => "Short break",
            PomodoroState.PomodoroMode.LongBreak => "Long break",
            _ => "Work"
        };

        if (State.IsPaused)
        {
            modeText += " (Paused)";
        }

        modeLabel.Text = modeText;
    }

    private void UpdateButtons()
    {
        if (!State.IsRunning)
        {
            toggleButton.Text = "Start";
            toggleButton.BackColor = Color.FromArgb(66, 160, 100);
            toggleButton.ForeColor = Color.White;
            return;
        }

        if (State.IsPaused)
        {
            toggleButton.Text = "Resume";
            toggleButton.BackColor = Color.FromArgb(60, 130, 220);
            toggleButton.ForeColor = Color.White;
            return;
        }

        toggleButton.Text = "Pause";
        toggleButton.BackColor = Color.FromArgb(240, 170, 60);
        toggleButton.ForeColor = Color.White;
    }

    private void HandleCompletionNotifications()
    {
        if (!State.TryConsumeCompletion(out var completion))
        {
            return;
        }

        string title = completion.Mode switch
        {
            PomodoroState.PomodoroMode.Work => "Work complete",
            PomodoroState.PomodoroMode.ShortBreak => "Short break complete",
            PomodoroState.PomodoroMode.LongBreak => "Long break complete",
            _ => "Session complete"
        };

        if (completion.SessionLimitReached)
        {
            PomodoroToast.Show(title, $"Session limit reached ({completion.CompletedCycles}).");
            return;
        }

        string nextLabel = State.CurrentMode switch
        {
            PomodoroState.PomodoroMode.Work => "Work",
            PomodoroState.PomodoroMode.ShortBreak => "Short break",
            PomodoroState.PomodoroMode.LongBreak => "Long break",
            _ => "Work"
        };

        string nextState = State.IsRunning ? "started" : "ready";
        string message = completion.CycleCompleted
            ? $"Cycle {completion.CompletedCycles} complete. {nextLabel} {nextState}."
            : $"{nextLabel} {nextState}.";

        PomodoroToast.Show(title, message);
    }
}
