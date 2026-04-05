using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    public class DailyLogForm : Form
    {
        private Label _countdownLabel = new Label(); 
        private Label _quoteLabel = new Label();
        private Label _yesterdayGoalLabel = new Label();
        private Label _focusTimeLabel = new Label();
        private ComboBox _executionStatusCombo = new ComboBox();
        private TableLayoutPanel _subjectsPanel = new TableLayoutPanel();
        private TextBox _tomorrowGoalTextBox = new TextBox();
        private TrackBar _focusTrackBar = new TrackBar();
        private TrackBar _moodTrackBar = new TrackBar();
        private Label _focusValueLabel = new Label();
        private Label _moodValueLabel = new Label();
        private CheckedListBox _mistakeTagsList = new CheckedListBox();
        private CheckBox _oathCheckBox = new CheckBox(); // NEW: Commander's Oath
        private Button _saveButton = new Button();
        
        private PrepProfile _profile = new PrepProfile();
        private List<DailyCommitment> _allCommitments = new List<DailyCommitment>();
        private DateTime _logDate;

        private static readonly string[] LogQuotes = new string[]
        {
            "Excuses don't build empires. What did you conquer today?",
            "Did you beat your yesterday's self? Be honest.",
            "Pain of discipline > Pain of regret. Log your progress.",
            "Your competition is not resting. Did you outwork them?",
            "What gets measured, gets managed. Enter your intel.",
            "Don't lie to this log. It reflects your future."
        };

        public DailyLogForm(DateTime? targetDate = null)
        {
            _logDate = targetDate ?? DateTime.Today;
            InitializeComponent();
            LoadData();
            LoadFocusTime();
        }

        private void InitializeComponent()
        {
            this.Text = $"Daily War-Log: {_logDate.ToShortDateString()}";
            this.Size = new Size(680, 950); 
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AutoScroll = true; 
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 10F);

            int currentY = 15;
            int marginX = 25;
            int contentWidth = 600;

            // --- Countdown Timer ---
            _countdownLabel = new Label {
                Text = "⏳ Calculating time to target...",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69), 
                Bounds = new Rectangle(marginX, currentY, contentWidth, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_countdownLabel);
            currentY += 40;

            // --- Dynamic Quote ---
            _quoteLabel = new Label {
                Text = LogQuotes[new Random().Next(LogQuotes.Length)],
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100),
                Bounds = new Rectangle(marginX, currentY, contentWidth, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_quoteLabel);
            currentY += 40;

            // --- Part A: Previous Goal Verification ---
            var groupA = new GroupBox { Text = "Part A: Mission Status (From Previous Log)", Bounds = new Rectangle(marginX, currentY, contentWidth, 130), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _yesterdayGoalLabel = new Label { Location = new Point(15, 30), Size = new Size(570, 45), Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(0, 102, 204) };
            
            var statusLabel = new Label { Text = "Execution Level:", Location = new Point(15, 85), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            _executionStatusCombo = new ComboBox { Location = new Point(140, 82), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            _executionStatusCombo.Items.AddRange(new object[] { "Overachieved (110%)", "Completed (100%)", "Partial (50%)", "Failed (0%)" });
            _executionStatusCombo.SelectedIndex = 1;
            
            groupA.Controls.AddRange(new Control[] { _yesterdayGoalLabel, statusLabel, _executionStatusCombo });
            this.Controls.Add(groupA);
            currentY += 150;

            // --- Focus Time ---
            _focusTimeLabel = new Label { 
                Bounds = new Rectangle(marginX, currentY, contentWidth, 30), 
                ForeColor = Color.FromArgb(46, 160, 67), 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_focusTimeLabel);
            currentY += 40;

            // --- Part B: Performance Stats ---
            var groupB = new GroupBox { Text = "Part B: Tactical Performance", Bounds = new Rectangle(marginX, currentY, contentWidth, 220), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _subjectsPanel = new TableLayoutPanel { Location = new Point(10, 30), Size = new Size(580, 175), AutoScroll = true, ColumnCount = 4, RowCount = 1 };
            _subjectsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            _subjectsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            _subjectsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            _subjectsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            _subjectsPanel.Controls.Add(new Label { Text = "Subject", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 0, 0);
            _subjectsPanel.Controls.Add(new Label { Text = "Lectures", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 1, 0);
            _subjectsPanel.Controls.Add(new Label { Text = "Questions", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 2, 0);
            _subjectsPanel.Controls.Add(new Label { Text = "Backlogs", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 3, 0);
            groupB.Controls.Add(_subjectsPanel);
            this.Controls.Add(groupB);
            currentY += 240;

            // --- Part C: Future Planning ---
            var groupC = new GroupBox { Text = "Part C: Next Target / Strategy", Bounds = new Rectangle(marginX, currentY, contentWidth, 130), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _tomorrowGoalTextBox = new TextBox { Location = new Point(15, 30), Size = new Size(570, 80), Multiline = true, Font = new Font("Segoe UI", 9.5F), ScrollBars = ScrollBars.Vertical };
            groupC.Controls.Add(_tomorrowGoalTextBox);
            this.Controls.Add(groupC);
            currentY += 150;

            // --- Part D: Mindset & Mistakes ---
            var groupD = new GroupBox { Text = "Part D: Mindset & Blockers", Bounds = new Rectangle(marginX, currentY, contentWidth, 190), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            
            var focusLbl = new Label { Text = "Focus Intensity:", Location = new Point(15, 30), AutoSize = true };
            _focusTrackBar = new TrackBar { Location = new Point(130, 30), Size = new Size(130, 45), Minimum = 1, Maximum = 10, Value = 5 };
            _focusValueLabel = new Label { Text = "5", Location = new Point(265, 30), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _focusTrackBar.ValueChanged += (s, e) => _focusValueLabel.Text = _focusTrackBar.Value.ToString();
            
            var moodLbl = new Label { Text = "Energy/Mood:", Location = new Point(15, 80), AutoSize = true };
            _moodTrackBar = new TrackBar { Location = new Point(130, 80), Size = new Size(130, 45), Minimum = 1, Maximum = 10, Value = 5 };
            _moodValueLabel = new Label { Text = "5", Location = new Point(265, 80), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _moodTrackBar.ValueChanged += (s, e) => _moodValueLabel.Text = _moodTrackBar.Value.ToString();

            var mistakeLbl = new Label { Text = "Vulnerabilities:", Location = new Point(320, 20), AutoSize = true };
            _mistakeTagsList = new CheckedListBox { Location = new Point(320, 45), Size = new Size(265, 130), CheckOnClick = true, Font = new Font("Segoe UI", 9F) };
            
            // Hardcore Tags
            _mistakeTagsList.Items.AddRange(new string[] { "#CalculationError", "#ConceptNotClear", "#SocialMediaTrap", "#SillyMistake", "#DistractedByPhone", "#FormulaForgotten", "#TimeManagementIssue", "#Procrastination", "#Overthinking", "#NoRevision", "#Burnout", "#DayDreaming" });
            
            groupD.Controls.AddRange(new Control[] { focusLbl, _focusTrackBar, _focusValueLabel, moodLbl, _moodTrackBar, _moodValueLabel, mistakeLbl, _mistakeTagsList });
            this.Controls.Add(groupD);
            currentY += 210;

            // --- Commander's Oath ---
            _oathCheckBox = new CheckBox {
                Text = "I swear as the Commander of my future that the intel provided above is 100% accurate and truthful.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 53, 69),
                Bounds = new Rectangle(marginX, currentY, contentWidth, 30),
                Cursor = Cursors.Hand
            };
            this.Controls.Add(_oathCheckBox);
            currentY += 40;

            // --- Save Button ---
            _saveButton = new Button { Text = "Secure & Lock Log Entry", Bounds = new Rectangle(marginX, currentY, contentWidth, 55), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, Font = new Font("Segoe UI", 13F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;
            this.Controls.Add(_saveButton);
        }

        private void LoadData()
        {
            _profile = PrepMeterStorage.LoadProfile() ?? new PrepProfile();
            _allCommitments = PrepMeterStorage.LoadAllCommitments();

            // Setup Countdown Timer
            TimeSpan timeToExam = _profile.TargetDate - DateTime.Today;
            int daysLeft = (int)timeToExam.TotalDays;
            
            if (daysLeft > 0)
                _countdownLabel.Text = $"⏳ Days to {_profile.TargetExam}: {daysLeft} Days Remaining";
            else if (daysLeft == 0)
                _countdownLabel.Text = $"🚨 TODAY IS THE {_profile.TargetExam.ToUpper()} EXAM! WAR-ROOM ACTIVE! 🚨";
            else
                _countdownLabel.Text = $"🎯 {_profile.TargetExam} has passed. Awaiting new orders.";

            // Setup Subjects
            var subjects = _profile.Subjects ?? new List<string> { "Physics", "Chemistry", "Mathematics" };
            _subjectsPanel.RowCount = subjects.Count + 1;
            
            var existingLog = _allCommitments.FirstOrDefault(c => c.Date.Date == _logDate.Date);

            for (int i = 0; i < subjects.Count; i++)
            {
                string subj = subjects[i];
                int row = i + 1;
                
                var lecInput = new NumericUpDown { Width = 70, Minimum = 0, Maximum = 50, Font = new Font("Segoe UI", 10F) };
                var quesInput = new NumericUpDown { Width = 80, Minimum = 0, Maximum = 1000, Font = new Font("Segoe UI", 10F) };
                var backlogInput = new NumericUpDown { Width = 70, Minimum = 0, Maximum = 200, Font = new Font("Segoe UI", 10F) };

                if (existingLog != null && existingLog.SubjectLogs != null && i < existingLog.SubjectLogs.Count)
                {
                    var stat = existingLog.SubjectLogs[i];
                    lecInput.Value = stat.LecturesCompleted;
                    quesInput.Value = stat.QuestionsSolved;
                    backlogInput.Value = stat.BacklogsCleared;
                }

                _subjectsPanel.Controls.Add(new Label { Text = subj, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10F, FontStyle.Bold) }, 0, row);
                _subjectsPanel.Controls.Add(lecInput, 1, row);
                _subjectsPanel.Controls.Add(quesInput, 2, row);
                _subjectsPanel.Controls.Add(backlogInput, 3, row);
            }

            // Get "Yesterday's Promise"
            var previousDayLog = _allCommitments.FirstOrDefault(c => c.Date.Date == _logDate.AddDays(-1).Date);
            if (previousDayLog != null && !string.IsNullOrWhiteSpace(previousDayLog.TomorrowGoal))
            {
                _yesterdayGoalLabel.Text = previousDayLog.TomorrowGoal;
            }
            else
            {
                _yesterdayGoalLabel.Text = "No tactical goal found from yesterday. Starting fresh!";
                _yesterdayGoalLabel.ForeColor = Color.Gray;
                _executionStatusCombo.Enabled = false;
            }

            if (existingLog != null)
            {
                _tomorrowGoalTextBox.Text = existingLog.TomorrowGoal;
                _focusTrackBar.Value = existingLog.FocusRating > 0 ? existingLog.FocusRating : 5;
                _moodTrackBar.Value = existingLog.MoodRating > 0 ? existingLog.MoodRating : 5;
                
                if (_executionStatusCombo.Items.Contains(existingLog.YesterdayVerification))
                    _executionStatusCombo.SelectedItem = existingLog.YesterdayVerification;

                if (existingLog.MistakeTags != null)
                {
                    for (int i = 0; i < _mistakeTagsList.Items.Count; i++)
                    {
                        if (existingLog.MistakeTags.Contains(_mistakeTagsList.Items[i].ToString()))
                            _mistakeTagsList.SetItemChecked(i, true);
                    }
                }
            }
        }

        private void LoadFocusTime()
        {
            try
            {
                var dayFocus = FocusHistoryStorage.GetDay(_logDate);
                int totalMins = dayFocus != null ? dayFocus.TotalFocusMinutes : 0;
                int hours = totalMins / 60;
                int mins = totalMins % 60;
                _focusTimeLabel.Text = $"⚡ Auto-Synced Focus Time: {hours} hrs {mins} mins";
            }
            catch { _focusTimeLabel.Text = "⚡ Focus data unavailable"; }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!_oathCheckBox.Checked)
            {
                MessageBox.Show("You must swear the Commander's Oath to lock this intel.", "Oath Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_tomorrowGoalTextBox.Text))
            {
                MessageBox.Show("Please define a tactical strategy for tomorrow.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var commitment = new DailyCommitment
            {
                Date = _logDate.Date,
                TomorrowGoal = _tomorrowGoalTextBox.Text.Trim(),
                YesterdayVerification = _executionStatusCombo.Enabled && _executionStatusCombo.SelectedItem != null ? _executionStatusCombo.SelectedItem.ToString() : "N/A",
                FocusRating = _focusTrackBar.Value,
                MoodRating = _moodTrackBar.Value,
                MistakeTags = new List<string>(),
                SubjectLogs = new List<SubjectStat>()
            };

            foreach (var item in _mistakeTagsList.CheckedItems)
            {
                commitment.MistakeTags.Add(item.ToString());
            }

            for (int row = 1; row <= _subjectsPanel.RowCount - 1; row++)
            {
                var lecNode = (NumericUpDown)_subjectsPanel.GetControlFromPosition(1, row);
                var quesNode = (NumericUpDown)_subjectsPanel.GetControlFromPosition(2, row);
                var backlogNode = (NumericUpDown)_subjectsPanel.GetControlFromPosition(3, row);
                
                if (lecNode != null && quesNode != null && backlogNode != null)
                {
                    commitment.SubjectLogs.Add(new SubjectStat {
                        LecturesCompleted = (int)lecNode.Value,
                        QuestionsSolved = (int)quesNode.Value,
                        BacklogsCleared = (int)backlogNode.Value
                    });
                }
            }

            try
            {
                PrepMeterStorage.SaveDailyCommitment(commitment);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to lock log: {ex.Message}", "Storage Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}