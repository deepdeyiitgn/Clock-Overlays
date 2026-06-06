using System.Drawing;
using System.Windows.Forms;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class PrivacyPopupForm : Form
{
    public PrivacyPopupForm()
    {
        Text = "Privacy Details";
        Size = new Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = ThemeColors.Surface;
        
        var txt = new TextBox {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            BackColor = ThemeColors.Background, ForeColor = ThemeColors.TextPrimary,
            Font = UIHelper.RegularFont(12f),
            Text = "Collected Information:\r\n- Windows version\r\n- RAM & Disk space\r\n- Error logs\r\n\r\nOptional:\r\n- Network IP\r\n\r\nAll data is kept local unless explicitly shared by you via email."
        };
        Controls.Add(txt);
        UIHelper.ApplyDarkTitleBar(this.Handle);
    }
}
