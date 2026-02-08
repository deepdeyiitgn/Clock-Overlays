using System.Drawing;
using System.Windows.Forms;

namespace TransparentClock
{
    public static class AboutTabFactory
    {
        private static readonly Color AppBackground = Color.FromArgb(247, 247, 247);

        public static TabPage CreateAboutTab()
        {
            var tabPage = new TabPage("About")
            {
                Padding = new Padding(14),
                BackColor = AppBackground
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var content = AboutContentFactory.CreateAboutContent(false, null);
            card.Controls.Add(content);
            tabPage.Controls.Add(card);

            return tabPage;
        }
    }
}
