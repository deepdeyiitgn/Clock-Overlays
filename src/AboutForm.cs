using System.Drawing;
using System.Windows.Forms;
using TransparentClock;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About Transparent Clock";
        Icon = Program.GetAppIcon();
        Size = new Size(720, 640);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        var content = AboutContentFactory.CreateCompactAboutContent(true, Close);
        Controls.Add(content);
    }
}
