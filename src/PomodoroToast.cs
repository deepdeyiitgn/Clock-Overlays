using System;
using System.Drawing;
using System.Windows.Forms;

namespace TransparentClock
{
    public static class PomodoroToast
    {
        private static Form? activeToast;

        public static void Show(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (activeToast != null && !activeToast.IsDisposed)
            {
                activeToast.Close();
                activeToast.Dispose();
            }

            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                BackColor = Color.White,
                Size = new Size(280, 88)
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White
            };

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30)
            };

            var messageLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            panel.Controls.Add(messageLabel);
            panel.Controls.Add(titleLabel);
            toast.Controls.Add(panel);

            PositionToast(toast);

            var closeTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            closeTimer.Tick += (_, __) =>
            {
                closeTimer.Stop();
                closeTimer.Dispose();
                if (!toast.IsDisposed)
                {
                    toast.Close();
                }
            };

            toast.FormClosed += (_, __) =>
            {
                closeTimer.Stop();
                closeTimer.Dispose();
                if (activeToast == toast)
                {
                    activeToast = null;
                }
            };

            activeToast = toast;
            toast.Show();
            closeTimer.Start();
        }

        private static void PositionToast(Form toast)
        {
            var area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
            int x = area.Right - toast.Width - 16;
            int y = area.Bottom - toast.Height - 16;
            toast.Location = new Point(Math.Max(area.Left, x), Math.Max(area.Top, y));
        }
    }
}
