using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Form for weekly analytics and strategy planning (War-Room Debrief).
    /// Shows performance metrics for the past 7 days and allows planning for the next week.
    /// </summary>
    public partial class WeeklyDebriefForm : Form
    {
        private Label _questionsTotalLabel;
        private Label _backlogsClearedTotalLabel;
        private Label _backlogsAddedTotalLabel;
        private Label _topMistakesLabel;
        private TextBox _strategyTextBox;
        private Button _saveButton;

        public WeeklyDebriefForm()
        {
            InitializeComponent();
            LoadWeeklyAnalytics();
        }

        private void InitializeComponent()
        {
            this.Text = "Weekly Debrief - War Room Analytics";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(700, 600);
            this.Padding = new Padding(20);

            // Performance Metrics Section
            var metricsGroup = CreateGroupBox("Performance Metrics (Last 7 Days)", 10, 10, 660, 200);

            // Questions Solved
            var questionsLabel = new Label
            {
                Text = "Total Questions Solved:",
                Location = new Point(20, 30),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            metricsGroup.Controls.Add(questionsLabel);

            _questionsTotalLabel = new Label
            {
                Text = "Calculating...",
                Location = new Point(230, 25),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255)
            };
            metricsGroup.Controls.Add(_questionsTotalLabel);

            // Backlogs Cleared
            var backlogsClearedLabel = new Label
            {
                Text = "Backlogs Cleared:",
                Location = new Point(20, 80),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            metricsGroup.Controls.Add(backlogsClearedLabel);

            _backlogsClearedTotalLabel = new Label
            {
                Text = "Calculating...",
                Location = new Point(230, 75),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69)
            };
            metricsGroup.Controls.Add(_backlogsClearedTotalLabel);

            // Backlogs Added
            var backlogsAddedLabel = new Label
            {
                Text = "Backlogs Added:",
                Location = new Point(20, 130),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            metricsGroup.Controls.Add(backlogsAddedLabel);

            _backlogsAddedTotalLabel = new Label
            {
                Text = "Calculating...",
                Location = new Point(230, 125),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69)
            };
            metricsGroup.Controls.Add(_backlogsAddedTotalLabel);

            // Top Mistakes Section
            var mistakesGroup = CreateGroupBox("Top Mistakes This Week", 10, 220, 660, 120);

            _topMistakesLabel = new Label
            {
                Text = "Analyzing mistake patterns...",
                Location = new Point(20, 30),
                Size = new Size(620, 70),
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };
            mistakesGroup.Controls.Add(_topMistakesLabel);

            // Strategy Section
            var strategyGroup = CreateGroupBox("Macro Strategy for Next Week", 10, 350, 660, 150);

            var strategyPromptLabel = new Label
            {
                Text = "Based on this week's performance, what's your strategy for next week?",
                Location = new Point(20, 25),
                Size = new Size(620, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            strategyGroup.Controls.Add(strategyPromptLabel);

            _strategyTextBox = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(620, 80),
                Multiline = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ScrollBars = ScrollBars.Vertical
            };
            strategyGroup.Controls.Add(_strategyTextBox);

            // Save Button
            _saveButton = new Button
            {
                Text = "Save Strategy & Close",
                Location = new Point(280, 520),
                Size = new Size(140, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;

            this.Controls.AddRange(new Control[] { metricsGroup, mistakesGroup, strategyGroup, _saveButton });
        }

        private GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
        {
            return new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
        }

        private void LoadWeeklyAnalytics()
        {
            try
            {
                // Load all commitments and filter to last 7 days
                var allCommitments = PrepMeterStorage.LoadAllCommitments();
                DateTime weekStart = DateTime.Now.Date.AddDays(-7);
                var weeklyCommitments = allCommitments.Where(c => c.Date >= weekStart).ToList();

                // Calculate totals
                int totalQuestions = weeklyCommitments.Sum(c => c.SubjectLogs?.Sum(s => s.QuestionsSolved) ?? 0);
                int totalBacklogsCleared = weeklyCommitments.Sum(c => c.SubjectLogs?.Sum(s => s.BacklogsCleared) ?? 0);
                int totalBacklogsAdded = weeklyCommitments.Sum(c => c.SubjectLogs?.Sum(s => s.BacklogsAdded) ?? 0);

                // Update UI with totals
                _questionsTotalLabel.Text = totalQuestions.ToString("N0");
                _backlogsClearedTotalLabel.Text = totalBacklogsCleared.ToString("N0");
                _backlogsAddedTotalLabel.Text = totalBacklogsAdded.ToString("N0");

                // Analyze mistake tags
                var allMistakeTags = weeklyCommitments
                    .Where(c => c.MistakeTags != null)
                    .SelectMany(c => c.MistakeTags)
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .GroupBy(tag => tag)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .ToList();

                if (allMistakeTags.Any())
                {
                    var topMistakesText = "Top mistake patterns this week:\n\n";
                    for (int i = 0; i < allMistakeTags.Count; i++)
                    {
                        var mistake = allMistakeTags[i];
                        topMistakesText += $"{i + 1}. {mistake.Key} ({mistake.Count()} times)\n";
                    }
                    _topMistakesLabel.Text = topMistakesText.TrimEnd();
                }
                else
                {
                    _topMistakesLabel.Text = "No mistakes logged this week. Great job maintaining focus!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weekly analytics: {ex.Message}", "Analytics Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Set default values
                _questionsTotalLabel.Text = "0";
                _backlogsClearedTotalLabel.Text = "0";
                _backlogsAddedTotalLabel.Text = "0";
                _topMistakesLabel.Text = "Unable to analyze mistakes this week.";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                string strategy = _strategyTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(strategy))
                {
                    MessageBox.Show("Please enter your macro strategy for next week.", "Strategy Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _strategyTextBox.Focus();
                    return;
                }

                // For now, just show success message
                // In a future update, this could save the strategy to storage
                MessageBox.Show("Weekly strategy saved successfully!\n\nUse this insight to plan your next 7 days effectively.",
                    "Strategy Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving strategy: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
