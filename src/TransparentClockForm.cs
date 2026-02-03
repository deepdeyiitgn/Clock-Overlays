using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using TransparentClock;
using Timer = System.Windows.Forms.Timer;

public class TransparentClockForm : Form
{
    private Label timeLabel;
    private Timer clockTimer;
    private Timer selfHealTimer;
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
            const int WS_EX_TRANSPARENT = 0x20;
            const int WS_EX_NOACTIVATE = 0x08000000;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override bool ShowWithoutActivation => true;

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
    UseCompatibleTextRendering = false,
    Padding = new Padding(4, 0, 4, 0)
};

container.Controls.Add(timeLabel);
Controls.Add(container);


        clockTimer = new Timer { Interval = 1000 };
        clockTimer.Tick += (s, e) =>
        {
            try
            {
                timeLabel.Text = DateTime.Now.ToString("HH:mm");
                UpdateClockSize();
                EnsureTopMost();
            }
            catch { /* safe fail */ }
        };
        clockTimer.Start();

        selfHealTimer = new Timer { Interval = 15 * 60 * 1000 };
        selfHealTimer.Tick += async (s, e) =>
        {
            if (!Program.CurrentState.ClockEnabled || !Visible || IsDisposed)
            {
                return;
            }

            try
            {
                Hide();
                await Task.Delay(2500);

                if (!Program.CurrentState.ClockEnabled || IsDisposed)
                {
                    return;
                }

                ShowClockOverlay();
            }
            catch
            {
                // fail silently
            }
        };
        selfHealTimer.Start();

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
            using var form = new PositionControllerForm(this);
            form.ShowDialog();

            Program.CurrentState.ClockUseCustomPosition = true;
            Program.CurrentState.ClockPosition = "Custom";
            Program.CurrentState.ClockCustomPositionX = Location.X;
            Program.CurrentState.ClockCustomPositionY = Location.Y;
            AppStateStorage.Save(Program.CurrentState);
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
        EnsureTopMost();
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
        UpdateClockSize();
    }

    public void ApplyClockFontFamily(string family)
    {
        if (string.IsNullOrWhiteSpace(family))
        {
            return;
        }

        try
        {
            timeLabel.Font = new Font(family, timeLabel.Font.Size, FontStyle.Bold);
            UpdateClockSize();
        }
        catch
        {
            // Ignore invalid fonts.
        }
    }

    private void UpdateClockSize()
    {
        string sampleText = string.IsNullOrWhiteSpace(timeLabel.Text) ? "88:88" : timeLabel.Text;
        Size textSize = TextRenderer.MeasureText(
            sampleText,
            timeLabel.Font,
            Size.Empty,
            TextFormatFlags.LeftAndRightPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);

        int paddingX = 24;
        int paddingY = 10;
        int width = Math.Max(70, textSize.Width + paddingX);
        int height = Math.Max(34, textSize.Height + paddingY);

        Size = new Size(width, height);
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

    public void ApplyClockCustomPosition(int x, int y)
    {
        customMove = false;
        Location = new Point(x, y);
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

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        EnsureTopMost();
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_ACTIVATEAPP = 0x001C;

        base.WndProc(ref m);

        if (m.Msg == WM_ACTIVATEAPP)
        {
            EnsureTopMost();
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (!Program.CurrentState.ClockEnabled)
        {
            Hide();
        }
    }

    private void EnsureTopMost()
    {
        if (!TopMost)
        {
            TopMost = true;
        }
    }
}
