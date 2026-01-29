using System;
using System.Drawing;
using System.Windows.Forms;

public class PositionControllerForm : Form
{
    private Form target;

    public PositionControllerForm(Form targetForm)
    {
        target = targetForm;

        Text = "Adjust Position";
        Size = new Size(240, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        TopMost = true;

        Button up = Btn("↑", 90, 15, () => target.Top -= 5);
        Button left = Btn("←", 30, 60, () => target.Left -= 5);
        Button right = Btn("→", 150, 60, () => target.Left += 5);
        Button down = Btn("↓", 90, 105, () => target.Top += 5);

        Button save = Btn("Save", 30, 140, Close);
        Button close = Btn("Close", 130, 140, Close);

        Controls.AddRange(new Control[] { up, left, right, down, save, close });
    }

    Button Btn(string text, int x, int y, Action action)
    {
        var b = new Button
        {
            Text = text,
            Size = new Size(70, 28),
            Location = new Point(x, y)
        };
        b.Click += (_, __) => action();
        return b;
    }
}
