using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    public class WeeklyDebriefForm : Form
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
            LoadAndAnalyzeData();
        }

        private void InitializeComponent()
        {
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to analyze war-room data: {ex.Message}", "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
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