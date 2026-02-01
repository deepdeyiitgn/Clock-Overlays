using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class TransparentClockForm : Form
{
    private Label timeLabel;
    private Timer clockTimer;
    private NotifyIcon trayIcon;
    private PomodoroForm? pomodoroForm;
    private ToolStripMenuItem? clockToggleItem;

    private Color currentColor = Color.White;
    private Point defaultPos;
    private bool customMove = false;

    public TransparentClockForm()
    {
        InitializeWindow();
        InitializeClock();
        InitializeTray();
    }

    private void InitializeWindow()
    {
        Size = new Size(105, 42);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Magenta;          
        TransparencyKey = Color.Magenta;

        StartPosition = FormStartPosition.Manual;

        var area = Screen.PrimaryScreen.WorkingArea;
        defaultPos = new Point(
            area.Right - Width - 5,
            area.Top + 15
        );
        Location = defaultPos;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TOOLWINDOW = 0x80;
            const int WS_EX_TOPMOST = 0x08;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
            return cp;
        }
    }

    private void InitializeClock()
    {
var container = new Panel
{
    Dock = DockStyle.Fill,
    BackColor = Color.Magenta   // SAME as TransparencyKey
};

timeLabel = new Label
{
    Font = new Font("Segoe UI", 20, FontStyle.Bold),
    ForeColor = currentColor,
    Dock = DockStyle.Fill,
    TextAlign = ContentAlignment.MiddleCenter,
    BackColor = Color.Magenta,  // IMPORTANT
    UseCompatibleTextRendering = false
};

container.Controls.Add(timeLabel);
Controls.Add(container);


        clockTimer = new Timer { Interval = 1000 };
        clockTimer.Tick += (s, e) =>
        {
            try
            {
                timeLabel.Text = DateTime.Now.ToString("HH:mm");
            }
            catch { /* safe fail */ }
        };
        clockTimer.Start();

        MouseDown += HandleDrag;
    }

    private void HandleDrag(object s, MouseEventArgs e)
    {
        if (!customMove || e.Button != MouseButtons.Left) return;

        ReleaseCapture();
        SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

    private void InitializeTray()
    {
trayIcon = new NotifyIcon
{
    Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
    Visible = true,
    Text = "Transparent Clock"
};

        trayIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Program.ToggleDashboard();
            }
        };

        trayIcon.ContextMenuStrip = BuildMenu();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        clockToggleItem = new ToolStripMenuItem();
        clockToggleItem.Click += (s, e) => ToggleClockOverlay();
        UpdateClockToggleText();
        menu.Items.Add(clockToggleItem);

        menu.Items.Add("Open Dashboard", null, (s, e) => Program.ShowMainForm());
        menu.Items.Add("Pomodoro", null, (s, e) => Program.ShowPomodoro());

        // Theme
        var theme = new ToolStripMenuItem("Theme");
        AddTheme(theme, "White", Color.White);
        AddTheme(theme, "Soft Black", Color.FromArgb(30, 30, 30));
        AddTheme(theme, "Red", Color.Red);
        AddTheme(theme, "Green", Color.LimeGreen);
        AddTheme(theme, "Blue", Color.DeepSkyBlue);
        AddTheme(theme, "Pink", Color.HotPink);
        AddTheme(theme, "Yellow", Color.Gold);
        menu.Items.Add(theme);

        // Position
        var pos = new ToolStripMenuItem("Position");
        pos.DropDownItems.Add("Top Left", null, (s,e)=>SetCorner(0));
        pos.DropDownItems.Add("Top Right", null, (s,e)=>SetCorner(1));
        pos.DropDownItems.Add("Bottom Right", null, (s,e)=>SetCorner(2));
        pos.DropDownItems.Add("Bottom Left", null, (s,e)=>SetCorner(3));
        pos.DropDownItems.Add("Custom Adjust", null, (s, e) =>
        {
            new PositionControllerForm(this).Show();
        });

        menu.Items.Add(pos);

        menu.Items.Add("Reset", null, (s,e)=>ResetClock());
        menu.Items.Add("About", null, (s,e)=> new AboutForm().ShowDialog());
        menu.Items.Add("Exit", null, (s,e)=> Program.ExitApplication());

        return menu;
    }

    private void AddTheme(ToolStripMenuItem menu, string name, Color color)
    {
        menu.DropDownItems.Add(name, null, (s,e)=>{
            currentColor = color;
            timeLabel.ForeColor = color;
            Program.CurrentState.ClockColorName = name;
            Program.CurrentState.ClockUseCustomColor = false;
            AppStateStorage.Save(Program.CurrentState);
        });
    }

    private void SetCorner(int pos)
    {
        var a = Screen.PrimaryScreen.WorkingArea;
        customMove = false;

        Location = pos switch
        {
            0 => new Point(20,20),
            1 => new Point(a.Width-Width-20,20),
            2 => new Point(a.Width-Width-20,a.Height-Height-20),
            _ => new Point(20,a.Height-Height-20)
        };
    }

    private void ResetClock()
    {
        currentColor = Color.White;
        timeLabel.ForeColor = Color.White;
        Location = defaultPos;
        customMove = false;
    }

    private void ToggleClockOverlay()
    {
        Program.CurrentState.ClockEnabled = !Program.CurrentState.ClockEnabled;
        UpdateClockToggleText();

        if (Program.CurrentState.ClockEnabled)
        {
            ShowClockOverlay();
        }
        else
        {
            Hide();
        }

        AppStateStorage.Save(Program.CurrentState);
    }

    private void UpdateClockToggleText()
    {
        if (clockToggleItem == null)
        {
            return;
        }

        clockToggleItem.Text = Program.CurrentState.ClockEnabled
            ? "Clock Overlay: ON"
            : "Clock Overlay: OFF";
    }

    private void ShowClockOverlay()
    {
        Show();
        BringToFront();
    }

    public void ApplyClockColor(Color color)
    {
        currentColor = color;
        timeLabel.ForeColor = color;
    }

    public void ApplyClockFontSize(float fontSize)
    {
        if (fontSize <= 0)
        {
            return;
        }

        timeLabel.Font = new Font(timeLabel.Font.FontFamily, fontSize, FontStyle.Bold);
    }

    public void ApplyClockPosition(string? position)
    {
        var a = Screen.PrimaryScreen.WorkingArea;
        customMove = false;

        var pos = position?.Trim() ?? "Top Right";
        Location = pos switch
        {
            "Top Left" => new Point(20, 20),
            "Top Right" => new Point(a.Width - Width - 20, 20),
            "Bottom Right" => new Point(a.Width - Width - 20, a.Height - Height - 20),
            "Bottom Left" => new Point(20, a.Height - Height - 20),
            _ => new Point(a.Width - Width - 20, 20)
        };
    }

    public void ApplyClockEnabled(bool enabled)
    {
        if (enabled)
        {
            ShowClockOverlay();
        }
        else
        {
            Hide();
        }

        UpdateClockToggleText();
    }

    public void RefreshClockToggleText()
    {
        UpdateClockToggleText();
    }

    private void ShowPomodoro()
    {
        if (pomodoroForm == null || pomodoroForm.IsDisposed)
        {
            pomodoroForm = new PomodoroForm();
        }

        pomodoroForm.Show();
        pomodoroForm.BringToFront();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon.Visible = false;
        base.OnFormClosing(e);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (!Program.CurrentState.ClockEnabled)
        {
            Hide();
        }
    }
}
