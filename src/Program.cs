using System;
using System.Windows.Forms;

namespace TransparentClock
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Startup info message
            MessageBox.Show(
                "Transparent Clock\n\nAn app by Deep",
                "Transparent Clock",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Application.Run(new TransparentClockForm());
        }
    }
}
