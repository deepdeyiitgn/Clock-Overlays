using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TransparentClock;

public class MainForm : Form
{
    private readonly TransparentClockForm clockForm;
    private readonly CheckBox clockToggle;
    private readonly ComboBox colorCombo;
    private readonly Label greetingLabel;
    private readonly PomodoroForm pomodoroForm;

    public MainForm(TransparentClockForm clockForm)
    {
        this.clockForm = clockForm;

        Text = $"{AppInfo.AppName} — {AppInfo.DisplayVersion}";
        Icon = Program.GetAppIcon();
        Size = new Size(420, 320);
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var clockTab = new TabPage("Clock");
        var pomodoroTab = new TabPage("Pomodoro");

        // ---- Clock Tab ----
        var clockLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 4
        };
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        clockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        greetingLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Text = Program.GetGreetingText()
        };

        clockToggle = new CheckBox
        {
            Text = "Clock Overlay",
            Checked = Program.CurrentState.ClockEnabled,
            AutoSize = true
        };
        clockToggle.CheckedChanged += (_, __) =>
        {
            Program.CurrentState.ClockEnabled = clockToggle.Checked;
            clockForm.ApplyClockEnabled(clockToggle.Checked);
            clockForm.RefreshClockToggleText();
            AppStateStorage.Save(Program.CurrentState);
        };

        var colorLabel = new Label
        {
            Text = "Color",
            AutoSize = true
        };

        colorCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200
        };

        foreach (var name in Program.ClockColors.Keys.OrderBy(k => k))
        {
            colorCombo.Items.Add(name);
        }

        string currentColorName = Program.CurrentState.ClockColorName;
        if (!colorCombo.Items.Contains(currentColorName))
        {
            currentColorName = "White";
        }
        colorCombo.SelectedItem = currentColorName;

        colorCombo.SelectedIndexChanged += (_, __) =>
        {
            if (colorCombo.SelectedItem is string name)
            {
                Program.CurrentState.ClockColorName = name;
                clockForm.ApplyClockColor(Program.ResolveClockColor(name));
                AppStateStorage.Save(Program.CurrentState);
            }
        };

        clockLayout.Controls.Add(clockToggle, 0, 0);
        clockLayout.SetColumnSpan(clockToggle, 2);
        clockLayout.Controls.Add(greetingLabel, 0, 1);
        clockLayout.SetColumnSpan(greetingLabel, 2);
        clockLayout.Controls.Add(colorLabel, 0, 2);
        clockLayout.Controls.Add(colorCombo, 1, 2);

        clockTab.Controls.Add(clockLayout);

        // ---- Pomodoro Tab ----
        pomodoroForm = new PomodoroForm
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };

        pomodoroTab.Controls.Add(pomodoroForm);
        pomodoroForm.Show();

        tabs.TabPages.Add(clockTab);
        tabs.TabPages.Add(pomodoroTab);
        Controls.Add(tabs);

    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Program.ApplyClockStateToOverlay();
        greetingLabel.Text = Program.GetGreetingText();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnFormClosing(e);
    }

}
