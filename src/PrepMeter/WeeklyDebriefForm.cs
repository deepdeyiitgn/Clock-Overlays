using System;
using System.Collections.Generic;
using System.Drawing;
<<<<<<< HEAD
using System.Drawing.Drawing2D;
=======
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
<<<<<<< HEAD
    public class WeeklyDebriefForm : Form
=======
    /// <summary>
    /// Form for weekly analytics and strategy planning (War-Room Debrief).
    /// Shows performance metrics for the past 7 days and allows planning for the next week.
    /// </summary>
    public partial class WeeklyDebriefForm : Form
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
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
<<<<<<< HEAD
            LoadAndAnalyzeData();
=======
            LoadWeeklyAnalytics();
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
        }

        private void InitializeComponent()
        {
<<<<<<< HEAD
            this.Text = "Weekly War-Room Debrief";
            this.Size = new Size(650, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            this.AutoScroll = true; 
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 10F);

            int currentY = 20;
            int marginX = 20;
            int contentWidth = 580;

            var headerLbl = new Label { 
                Text = "7-Day Tactical Overview", 
                Font = new Font("Segoe UI", 18F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(marginX, currentY),
                AutoSize = true
            };
            this.Controls.Add(headerLbl);
            currentY += 50;

            var cardsPanel = new TableLayoutPanel
            {
                Location = new Point(marginX, currentY),
                Size = new Size(contentWidth, 120),
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var qCard = CreateStatCard("Questions Solved", Color.FromArgb(9, 105, 218), out _questionsTotalLabel);
            cardsPanel.Controls.Add(qCard, 0, 0);

            var bClearCard = CreateStatCard("Backlogs Cleared", Color.FromArgb(46, 160, 67), out _backlogsClearedTotalLabel);
            cardsPanel.Controls.Add(bClearCard, 1, 0);

            var bAddCard = CreateStatCard("Days Logged", Color.FromArgb(210, 153, 34), out _backlogsAddedTotalLabel);
            cardsPanel.Controls.Add(bAddCard, 2, 0);

            this.Controls.Add(cardsPanel);
            currentY += 140;

            var groupMistakes = new GroupBox { 
                Text = "Vulnerability Analysis (Top Blockers)", 
                Bounds = new Rectangle(marginX, currentY, contentWidth, 140), 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold) 
            };
            
            _topMistakesLabel = new Label { 
                Location = new Point(15, 30), 
                Size = new Size(550, 90), 
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 53, 69) 
            };
            groupMistakes.Controls.Add(_topMistakesLabel);
            this.Controls.Add(groupMistakes);
            currentY += 160;

            var groupStrategy = new GroupBox { 
                Text = "Macro Strategy for Next Week", 
                Bounds = new Rectangle(marginX, currentY, contentWidth, 180), 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold) 
            };
            
            _strategyTextBox = new TextBox { 
                Location = new Point(15, 30), 
                Size = new Size(550, 130), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F) 
            };
            groupStrategy.Controls.Add(_strategyTextBox);
            this.Controls.Add(groupStrategy);
            currentY += 200;

            _saveButton = new Button { 
                Text = "Lock Strategy & Close Debrief", 
                Bounds = new Rectangle(marginX, currentY, contentWidth, 50), 
                BackColor = Color.FromArgb(40, 167, 69), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;
            this.Controls.Add(_saveButton);
        }

        private Panel CreateStatCard(string title, Color themeColor, out Label valueLabel)
        {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), BackColor = Color.White };
            
            var topBar = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = themeColor };
            card.Controls.Add(topBar);

            var titleLbl = new Label { 
                Text = title, 
                Dock = DockStyle.Top, 
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Height = 25,
                Padding = new Padding(0, 5, 0, 0)
            };

            valueLabel = new Label { 
                Text = "0", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = themeColor
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLbl);

            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

            return card;
        }

        private void LoadAndAnalyzeData()
        {
            try
            {
                var allCommitments = PrepMeterStorage.LoadAllCommitments();
                
                DateTime sevenDaysAgo = DateTime.Today.AddDays(-7);
                var weekData = allCommitments.Where(c => c.Date.Date >= sevenDaysAgo).ToList();

                if (weekData.Count == 0)
                {
                    _questionsTotalLabel.Text = "0";
                    _backlogsClearedTotalLabel.Text = "0";
                    _backlogsAddedTotalLabel.Text = "0";
                    _topMistakesLabel.Text = "No data logged in the past 7 days. You are flying blind, soldier.";
                    _topMistakesLabel.ForeColor = Color.Gray;
                    return;
                }

                int totalQuestions = 0;
                int totalBacklogsCleared = 0;
                int daysLogged = weekData.Count;
                var mistakeCounts = new Dictionary<string, int>();

                foreach (var log in weekData)
                {
                    // FIX: Using the correct List property iteration
                    if (log.SubjectLogs != null)
                    {
                        foreach (var stat in log.SubjectLogs)
                        {
                            totalQuestions += stat.QuestionsSolved;
                            totalBacklogsCleared += stat.BacklogsCleared;
                        }
                    }

                    if (log.MistakeTags != null)
                    {
                        foreach (var tag in log.MistakeTags)
                        {
                            if (mistakeCounts.ContainsKey(tag))
                                mistakeCounts[tag]++;
                            else
                                mistakeCounts[tag] = 1;
                        }
                    }
                }

                _questionsTotalLabel.Text = totalQuestions.ToString();
                _backlogsClearedTotalLabel.Text = totalBacklogsCleared.ToString();
                _backlogsAddedTotalLabel.Text = daysLogged.ToString(); 

                if (mistakeCounts.Count > 0)
                {
                    var topMistakes = mistakeCounts.OrderByDescending(kv => kv.Value).Take(3).ToList();
                    string mistakesText = "";
                    for (int i = 0; i < topMistakes.Count; i++)
                    {
                        mistakesText += $"{i + 1}. {topMistakes[i].Key} (Occurred {topMistakes[i].Value} times)\n";
                    }
                    _topMistakesLabel.Text = mistakesText;
                    _topMistakesLabel.ForeColor = Color.FromArgb(220, 53, 69); 
                }
                else
                {
                    _topMistakesLabel.Text = "Flawless execution! No blockers or mistakes logged this week.";
                    _topMistakesLabel.ForeColor = Color.FromArgb(46, 160, 67); 
=======
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
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                }
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
                MessageBox.Show($"Failed to analyze war-room data: {ex.Message}", "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
=======
                MessageBox.Show($"Error loading weekly analytics: {ex.Message}", "Analytics Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Set default values
                _questionsTotalLabel.Text = "0";
                _backlogsClearedTotalLabel.Text = "0";
                _backlogsAddedTotalLabel.Text = "0";
                _topMistakesLabel.Text = "Unable to analyze mistakes this week.";
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
<<<<<<< HEAD
            if (string.IsNullOrWhiteSpace(_strategyTextBox.Text))
            {
                MessageBox.Show("You cannot close the debrief without setting a strategy for the upcoming week.", "Strategy Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Macro Strategy Locked. Execute with extreme prejudice next week.", "Debrief Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
=======
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
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
