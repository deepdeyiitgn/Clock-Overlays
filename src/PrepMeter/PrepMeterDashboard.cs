using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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

        public PrepMeterDashboard()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.DoubleBuffered = true;
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
            };

            _streakLabel = new Label
            {
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
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
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
        public void LoadData()
        {
            try
            {
                var commitments = PrepMeterStorage.LoadAllCommitments();

                // Calculate streaks
                var (currentStreak, longestStreak) = CalculateStreaks(commitments);

                // Update streak display
                _streakLabel.Text = $"Current Streak: {currentStreak} Days | Longest: {longestStreak} Days";

                // Update calendar
                _calendar.SetData(commitments);
            }
            catch (Exception ex)
            {
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
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }

            // Calculate longest streak
            int longestStreak = 0;
            int tempStreak = 0;
            DateTime previousDate = DateTime.MinValue;

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
                else
                {
                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 1;
                }

                previousDate = commitment.Date.Date;
            }

            longestStreak = Math.Max(longestStreak, tempStreak);

            return (currentStreak, longestStreak);
        }

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
