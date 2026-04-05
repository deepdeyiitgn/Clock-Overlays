using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace TransparentClock
{
    public class SplashForm : Form
    {
        private Panel contentPanel = null!;
        private PictureBox logoPictureBox = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Label factHeaderLabel = null!;
        private Label factTextLabel = null!;
        private Panel progressBg = null!;
        private Panel progressFill = null!;
        
        private Timer progressTimer = null!;
        private Timer closeTimer = null!;

        // 40+ Premium War-Room Insights
        private static readonly string[] productivityFacts = new string[]
        {
            "Short breaks (e.g., Pomodoro technique) can significantly improve mental focus and prevent decision fatigue.",
            "A cluttered workspace can lead to a cluttered mind. Clear your desk, clear your mind.",
            "Dual-encoding information—using both visuals and text—helps create more robust memories.",
            "Getting 7-8 hours of sleep is critical for memory consolidation. Pulling an all-nighter destroys recall.",
            "The Ebbinghaus Forgetting Curve shows that spaced repetition is the most effective way to retain formulas and concepts.",
            "Deep Work is the ability to focus without distraction on a cognitively demanding task. It's a superpower today.",
            "Explaining a concept to a wall or a friend helps you identify gaps in your own understanding (Feynman Technique).",
            "Motivation gets you started. Discipline is what keeps you going when the motivation fades.",
            "The pain of regret is far worse than the pain of discipline. Choose your pain wisely.",
            "Don't practice until you get it right. Practice until you can't get it wrong.",
            "Success in competitive exams is 20% intellect and 80% emotional control and consistency.",
            "Amateurs sit and wait for inspiration, the rest of us just get up and go to work.",
            "You do not rise to the level of your goals. You fall to the level of your systems.",
            "Focus on the process, not the outcome. A solid daily routine guarantees long-term success.",
            "Hydration directly impacts cognitive performance. Keep a water bottle in your war-room.",
            "Every minute you spend scrolling is a minute your competition spends revising.",
            "Active recall (testing yourself) is 300% more effective than passive reading.",
            "If it is important to you, you will find a way. If not, you will find an excuse.",
            "Your phone is the enemy of your focus. Keep it in another room while doing Deep Work.",
            "Doubt kills more dreams than failure ever will. Trust your preparation.",
            "Hard work beats talent when talent doesn't work hard.",
            "A year from now, you may wish you had started today. Start now.",
            "Sacrifice today for the life you want tomorrow. The grind is temporary, the glory is permanent.",
            "The difference between ordinary and extraordinary is that little extra.",
            "It never gets easier, you just get stronger.",
            "Stop waiting for the perfect time. The time is never perfect. Execute now.",
            "Small, daily improvements over time lead to stunning results.",
            "To achieve what 1% of people have, you must be willing to do what 99% won't.",
            "Your future is created by what you do today, not tomorrow.",
            "Anxiety is solved by action. If you're stressed about the syllabus, start studying.",
            "Consistency > Intensity. Studying 6 hours daily is better than 14 hours once a week.",
            "Review your mistakes aggressively. A mistake ignored is a mistake repeated.",
            "Rule of 21: It takes 21 days to build a habit. Don't break the streak.",
            "The hardest part of any task is starting. Just commit to 5 minutes.",
            "Track your progress. What gets measured, gets managed.",
            "You are the average of the 5 habits you repeat daily.",
            "Don't stop when you're tired. Stop when you're done.",
            "There are no shortcuts to any place worth going.",
            "Focus is a muscle. The more you resist distraction, the stronger your focus gets.",
            "Embrace the struggle. It's building the mental calluses you need for the final exam."
        };

        public SplashForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(480, 320);
            this.BackColor = Color.FromArgb(247, 247, 247);
            
            // Set Taskbar Icon
            this.Icon = TryLoadAppIcon();

            InitializeComponents();
            StartLoadingSequence();
        }

        private void InitializeComponents()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(contentPanel);

            // --- Custom Smooth Progress Bar ---
            progressBg = new Panel
            {
                Size = new Size(this.Width, 5),
                Location = new Point(0, this.Height - 5),
                BackColor = Color.FromArgb(230, 230, 230)
            };
            
            progressFill = new Panel
            {
                Size = new Size(0, 5),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(0, 123, 255) // Blue loading line
            };
            progressBg.Controls.Add(progressFill);
            this.Controls.Add(progressBg);

            // --- Quotes/Facts Panel ---
            Panel factPanel = new Panel
            {
                Size = new Size(this.Width, 90),
                Location = new Point(0, progressBg.Top - 90),
                BackColor = Color.White
            };
            this.Controls.Add(factPanel);

            factHeaderLabel = new Label
            {
                Text = "War-Room Intel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(this.Width, 20),
                Location = new Point(0, 10)
            };
            factPanel.Controls.Add(factHeaderLabel);

            factTextLabel = new Label
            {
                Text = GetRandomProductivityFact(),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(this.Width - 40, 55),
                Location = new Point(20, factHeaderLabel.Bottom + 5)
            };
            factPanel.Controls.Add(factTextLabel);

            // --- Upper Content: Logo & Brand ---
            logoPictureBox = new PictureBox
            {
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((this.Width - 100) / 2, 20) 
            };
            logoPictureBox.Image = TryLoadAppLogo();
            contentPanel.Controls.Add(logoPictureBox);

            titleLabel = new Label
            {
                Text = "Transparent Clock",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(contentPanel.Width, 35),
                Location = new Point(0, logoPictureBox.Bottom + 10)
            };
            contentPanel.Controls.Add(titleLabel);

            subtitleLabel = new Label
            {
                Text = "Initializing War-Room Environment...",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(contentPanel.Width, 20),
                Location = new Point(0, titleLabel.Bottom + 5)
            };
            contentPanel.Controls.Add(subtitleLabel);
        }

        private void StartLoadingSequence()
        {
            // Random time between 5000ms (5s) and 10000ms (10s)
            int totalSplashTime = new Random().Next(5000, 10001);
            int elapsed = 0;

            // Timer to animate the progress bar smoothly (60 FPS)
            progressTimer = new Timer { Interval = 16 };
            progressTimer.Tick += (s, e) =>
            {
                elapsed += 16;
                float percent = Math.Min(1f, elapsed / (float)totalSplashTime);
                progressFill.Width = (int)(progressBg.Width * percent);
            };
            progressTimer.Start();

            // Timer to close the splash screen and launch the main app
            closeTimer = new Timer { Interval = totalSplashTime };
            closeTimer.Tick += (s, e) =>
            {
                progressTimer.Stop();
                closeTimer.Stop();
                this.Close(); // THIS triggers the main app to open
            };
            closeTimer.Start();
        }

        private string GetRandomProductivityFact()
        {
            Random random = new Random();
            int index = random.Next(productivityFacts.Length);
            return productivityFacts[index];
        }

        private Icon? TryLoadAppIcon()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { return null; }
        }

        private Image? TryLoadAppLogo()
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo.png");
            if (File.Exists(logoPath))
            {
                try { return Image.FromFile(logoPath); } catch { }
            }
            
            var icon = TryLoadAppIcon();
            if (icon != null) return icon.ToBitmap();

            return null;
        }
    }
}