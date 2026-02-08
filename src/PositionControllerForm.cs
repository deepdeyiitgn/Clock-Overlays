using System;
using System.Drawing;
using System.Windows.Forms;

public class PositionControllerForm : Form
{
    private readonly Form target;

    public PositionControllerForm(Form targetForm)
    {
        target = targetForm;

        Text = "Adjust Position";
        Size = new Size(260, 300);          // HEIGHT FIXED
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // === MAIN PANEL ===
        var panel = new Panel
        {
            Dock = DockStyle.Fill
        };

        // === BUTTONS ===
        Button btnUp = CreateButton("↑", 100, 20, () => Move(0, -5));
        Button btnLeft = CreateButton("←", 40, 70, () => Move(-5, 0));
        Button btnRight = CreateButton("→", 160, 70, () => Move(5, 0));
        Button btnDown = CreateButton("↓", 100, 120, () => Move(0, 5));

        Button btnSave = new Button
        {
            Text = "Save",
            Width = 90,
            Height = 32,
            Location = new Point(25, 170)
        };
        btnSave.Click += (s, e) => Close();

        Button btnClose = new Button
        {
            Text = "Close",
            Width = 90,
            Height = 32,
            Location = new Point(135, 170)
        };
        btnClose.Click += (s, e) => Close();

        // === COPYRIGHT (FADED) ===
        var copyright = new Label
        {
            Text = "© Deep Dey • Clock Overlays",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Height = 30
        };

        // === ADD CONTROLS ===
        panel.Controls.Add(btnUp);
        panel.Controls.Add(btnLeft);
        panel.Controls.Add(btnRight);
        panel.Controls.Add(btnDown);
        panel.Controls.Add(btnSave);
        panel.Controls.Add(btnClose);

        Controls.Add(panel);
        Controls.Add(copyright);
    }

    // === HELPERS ===
    private Button CreateButton(string text, int x, int y, Action action)
    {
        var btn = new Button
        {
            Text = text,
            Width = 50,
            Height = 35,
            Location = new Point(x, y)
        };
        btn.Click += (s, e) => action();
        return btn;
    }

    private new void Move(int dx, int dy)
    {
        target.Location = new Point(
            target.Location.X + dx,
            target.Location.Y + dy
        );
    }
}
