using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer; 

namespace TransparentClock.PrepMeter
{
    public partial class PrepMeterDashboard : UserControl
    {
        private Label _titleLabel = new Label();
        private Panel _streakPanel = new Panel();
        private Label _streakLabel = new Label();
        private MonthlyCalendarView _calendar = new MonthlyCalendarView();
        private Button _logButton = new Button();
        private Button _debriefButton = new Button();
        private Timer _glowTimer = new Timer();
        private bool _isGlowing = false;

        public PrepMeterDashboard()
        {
            InitializeComponent();
            LoadData();
            SetupWeekendGlow();
        }

        private void InitializeComponent()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(20);
            this.AutoScroll = true; 
            this.Size = new Size(850, 750);

            int currentY = 20;

            _titleLabel = new Label
            {
                Text = "Prep. Meter - War Room Dashboard",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(20, currentY)
            };
            this.Controls.Add(_titleLabel);
            currentY += 50;

            _streakPanel = new Panel
            {
                BackColor = Color.White,
                Size = new Size(800, 60),
                Location = new Point(20, currentY)
            };
            _streakPanel.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, _streakPanel.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
            };

            _streakLabel = new Label
            {
                Text = "🔥 Current Streak: 0 Days  |  🏆 Longest Streak: 0 Days",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            _streakPanel.Controls.Add(_streakLabel);
            this.Controls.Add(_streakPanel);
            currentY += 80;

            _calendar = new MonthlyCalendarView
            {
                Location = new Point(20, currentY),
                Size = new Size(800, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            // FIXED WIRING: Linking the new SmartDateClicked event
            _calendar.SmartDateClicked += Calendar_SmartDateClicked;
            this.Controls.Add(_calendar);
            currentY += 500;

            _logButton = new Button
            {
                Text = "📝 Log Today's Commitment",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(260, 55),
                Location = new Point(20, currentY),
                Cursor = Cursors.Hand
            };
            _logButton.FlatAppearance.BorderSize = 0;
            _logButton.Click += (s, e) => OpenLogForm(DateTime.Today);

            _debriefButton = new Button
            {
                Text = "📊 Weekly Debrief (Sunday)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 167, 69),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(260, 55),
                Location = new Point(300, currentY),
                Cursor = Cursors.Hand
            };
            _debriefButton.FlatAppearance.BorderSize = 0;
            _debriefButton.Click += DebriefButton_Click;

            this.Controls.Add(_logButton);
            this.Controls.Add(_debriefButton);
        }

        private void SetupWeekendGlow()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            if (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday)
            {
                _debriefButton.Text = "🚨 WAR-ROOM DEBRIEF DUE!";
                _glowTimer = new Timer { Interval = 800 };
                _glowTimer.Tick += (s, e) =>
                {
                    _isGlowing = !_isGlowing;
                    _debriefButton.BackColor = _isGlowing ? Color.FromArgb(30, 215, 96) : Color.FromArgb(34, 139, 57);
                };
                _glowTimer.Start();
            }
        }

        private void Calendar_SmartDateClicked(object? sender, MonthlyCalendarView.DateClickEventArgs e)
        {
            if (e.ActionType == "Daily")
            {
                OpenLogForm(e.Date);
            }
            else if (e.ActionType == "Weekly")
            {
                DebriefButton_Click(this, EventArgs.Empty);
            }
            else if (e.ActionType == "Monthly")
            {
                try
                {
                    using (var monthlyForm = new MonthlyDebriefForm(e.Date))
                    {
                        if (monthlyForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening monthly review: {ex.Message}", "Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenLogForm(DateTime date)
        {
            try
            {
                using (var logForm = new DailyLogForm(date))
                {
                    if (logForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log for {date.ToShortDateString()}: {ex.Message}", "Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadData()
        {
            try
            {
                var commitments = PrepMeterStorage.LoadAllCommitments();
                var (currentStreak, longestStreak) = CalculateStreaks(commitments);

                _streakLabel.Text = $"🔥 Current Streak: {currentStreak} Days  |  🏆 Longest: {longestStreak} Days";
                _calendar.SetData(commitments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Prep Meter data: {ex.Message}", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (int currentStreak, int longestStreak) CalculateStreaks(List<DailyCommitment> commitments)
        {
            if (commitments == null || commitments.Count == 0) return (0, 0);

            var sortedDates = commitments.Select(c => c.Date.Date).Distinct().OrderByDescending(d => d).ToList();
            var dateSet = new HashSet<DateTime>(sortedDates);

            int currentStreak = 0;
            DateTime checkDate = DateTime.Today;
            
            if (!dateSet.Contains(checkDate)) checkDate = checkDate.AddDays(-1);

            while (dateSet.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }

            int longestStreak = 0;
            int tempStreak = 0;
            DateTime previousDate = DateTime.MinValue;

            foreach (var date in sortedDates.OrderBy(d => d))
            {
                if (previousDate == DateTime.MinValue) tempStreak = 1;
                else if (date == previousDate.AddDays(1)) tempStreak++;
                else
                {
                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 1;
                }
                previousDate = date;
            }
            longestStreak = Math.Max(longestStreak, tempStreak);

            return (currentStreak, longestStreak);
        }

        private void DebriefButton_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var debriefForm = new WeeklyDebriefForm())
                {
                    debriefForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening weekly debrief: {ex.Message}", "Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && _glowTimer != null)
            {
                _glowTimer.Stop();
                _glowTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}