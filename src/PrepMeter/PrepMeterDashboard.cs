using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
<<<<<<< HEAD
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
=======

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// The main dashboard user control for the Prep. Meter feature.
    /// Displays commitment tracking statistics and the contribution calendar.
    /// </summary>
    public partial class PrepMeterDashboard : UserControl
    {
        private Label _titleLabel;
        private Panel _streakPanel;
        private Label _streakLabel;
        private ContributionCalendar _calendar;
        private Button _logButton;
        private Button _debriefButton;
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a

        public PrepMeterDashboard()
        {
            InitializeComponent();
            LoadData();
<<<<<<< HEAD
            SetupWeekendGlow();
=======
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
        }

        private void InitializeComponent()
        {
            this.DoubleBuffered = true;
<<<<<<< HEAD
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
=======
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Title Label
            _titleLabel = new Label
            {
                Text = "Prep. Meter - Commitment Tracker",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Streak Panel
            _streakPanel = new Panel
            {
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(400, 60),
                Location = new Point(0, 60)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            };

            _streakLabel = new Label
            {
<<<<<<< HEAD
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
=======
                Text = "Loading streaks...",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(20, 18)
            };
            _streakPanel.Controls.Add(_streakLabel);

            // Contribution Calendar
            _calendar = new ContributionCalendar
            {
                Location = new Point(0, 140),
                Size = new Size(800, 120) // Will be adjusted based on calendar's preferred size
            };

            // Log Button
            _logButton = new Button
            {
                Text = "Log Today's Commitment",
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
<<<<<<< HEAD
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

=======
                Size = new Size(200, 45),
                Location = new Point(0, 280),
                Cursor = Cursors.Hand
            };

            // Style the button
            _logButton.FlatAppearance.BorderSize = 0;
            _logButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            _logButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 98, 204);

            // Debrief Button
            _debriefButton = new Button
            {
                Text = "Weekly Debrief (Sunday)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 167, 69),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 45),
                Location = new Point(210, 280),
                Cursor = Cursors.Hand
            };

            // Style the debrief button
            _debriefButton.FlatAppearance.BorderSize = 0;
            _debriefButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 139, 57);
            _debriefButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 126, 52);

            // Wire up the button click events
            _logButton.Click += LogButton_Click;
            _debriefButton.Click += DebriefButton_Click;

            // Add controls to the user control
            this.Controls.AddRange(new Control[] { _titleLabel, _streakPanel, _calendar, _logButton, _debriefButton });

            // Set the overall size
            this.Size = new Size(850, 350);
        }

        /// <summary>
        /// Loads data from storage and updates the UI components.
        /// </summary>
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
        public void LoadData()
        {
            try
            {
                var commitments = PrepMeterStorage.LoadAllCommitments();
<<<<<<< HEAD
                var (currentStreak, longestStreak) = CalculateStreaks(commitments);

                _streakLabel.Text = $"🔥 Current Streak: {currentStreak} Days  |  🏆 Longest: {longestStreak} Days";
=======

                // Calculate streaks
                var (currentStreak, longestStreak) = CalculateStreaks(commitments);

                // Update streak display
                _streakLabel.Text = $"Current Streak: {currentStreak} Days | Longest: {longestStreak} Days";

                // Update calendar
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                _calendar.SetData(commitments);
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
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
=======
                MessageBox.Show($"Error loading Prep Meter data: {ex.Message}",
                    "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _streakLabel.Text = "Error loading streak data";
            }
        }

        /// <summary>
        /// Calculates the current and longest streaks from the commitment data.
        /// A streak is defined as consecutive days with commitment entries.
        /// </summary>
        /// <param name="commitments">List of daily commitments.</param>
        /// <returns>A tuple containing (currentStreak, longestStreak).</returns>
        private (int currentStreak, int longestStreak) CalculateStreaks(List<DailyCommitment> commitments)
        {
            if (commitments == null || commitments.Count == 0)
                return (0, 0);

            // Sort commitments by date (newest first)
            var sortedCommitments = commitments.OrderByDescending(c => c.Date.Date).ToList();

            // Get unique dates with commitments
            var commitmentDates = new HashSet<DateTime>(sortedCommitments.Select(c => c.Date.Date));

            // Calculate current streak (from today backwards)
            int currentStreak = 0;
            DateTime checkDate = DateTime.Today;
            while (commitmentDates.Contains(checkDate))
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }

<<<<<<< HEAD
=======
            // Calculate longest streak
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            int longestStreak = 0;
            int tempStreak = 0;
            DateTime previousDate = DateTime.MinValue;

<<<<<<< HEAD
            foreach (var date in sortedDates.OrderBy(d => d))
            {
                if (previousDate == DateTime.MinValue) tempStreak = 1;
                else if (date == previousDate.AddDays(1)) tempStreak++;
=======
            foreach (var commitment in sortedCommitments.OrderBy(c => c.Date.Date))
            {
                if (previousDate == DateTime.MinValue)
                {
                    tempStreak = 1;
                }
                else if (commitment.Date.Date == previousDate.AddDays(1))
                {
                    tempStreak++;
                }
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                else
                {
                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 1;
                }
<<<<<<< HEAD
                previousDate = date;
            }
=======

                previousDate = commitment.Date.Date;
            }

>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            longestStreak = Math.Max(longestStreak, tempStreak);

            return (currentStreak, longestStreak);
        }

<<<<<<< HEAD
        private void DebriefButton_Click(object? sender, EventArgs e)
=======
        /// <summary>
        /// Gets the log button for attaching click event handlers.
        /// </summary>
        public Button LogButton => _logButton;

        /// <summary>
        /// Refreshes the dashboard data.
        /// </summary>
        public void RefreshData()
        {
            LoadData();
        }

/// <summary>
        /// Handles the click event for the "Weekly Debrief (Sunday)" button.
        /// Opens the weekly analytics form.
        /// </summary>
        private void DebriefButton_Click(object sender, EventArgs e)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
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
<<<<<<< HEAD
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
=======
                MessageBox.Show($"Error opening weekly debrief: {ex.Message}",
                    "Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // <-- Ye wala bracket miss ho gaya tha!

        /// <summary>
        /// Handles the click event for the "Log Today's Commitment" button.
        /// Opens the daily log form and refreshes data after it closes.
        /// </summary>
        private void LogButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var logForm = new DailyLogForm())
                {
                    logForm.ShowDialog();
                }
                
                // Form close hone ke baad calendar aur streak ko refresh karega
                LoadData(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening daily log: {ex.Message}",
                    "Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
