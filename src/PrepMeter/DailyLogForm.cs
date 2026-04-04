using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Form for logging daily preparation commitments and tracking progress.
    /// </summary>
    public partial class DailyLogForm : Form
    {
        private Label _yesterdayGoalLabel;
        private Label _focusTimeLabel;
        private ComboBox _executionStatusCombo;
        private FlowLayoutPanel _subjectsPanel;
        private TextBox _tomorrowGoalTextBox;
        private TrackBar _focusTrackBar;
        private TrackBar _moodTrackBar;
        private Label _focusValueLabel;
        private Label _moodValueLabel;
        private CheckedListBox _mistakeTagsList;
        private Button _saveButton;

        private PrepProfile _profile;
        private DailyCommitment _latestCommitment;
        private readonly List<SubjectInputRow> _subjectRows = new List<SubjectInputRow>();

        // Default subjects if no profile exists
        private readonly string[] _defaultSubjects = { "Physics", "Chemistry", "Mathematics" };

        public DailyLogForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Log Today's Commitment";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(600, 700);
            this.Padding = new Padding(20);

            // Part A: Yesterday's Promise
            var yesterdayGroup = CreateGroupBox("Part A: Yesterday's Promise", 10, 10, 560, 80);

            _yesterdayGoalLabel = new Label
            {
                Text = "Loading previous goal...",
                Location = new Point(10, 20),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            yesterdayGroup.Controls.Add(_yesterdayGoalLabel);

            _focusTimeLabel = new Label
            {
                Text = "Loading focus time...",
                Location = new Point(10, 50),
                Size = new Size(540, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255)
            };
            yesterdayGroup.Controls.Add(_focusTimeLabel);

            var statusLabel = new Label
            {
                Text = "Did you achieve yesterday's goal?",
                Location = new Point(10, 75),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            yesterdayGroup.Controls.Add(statusLabel);

            _executionStatusCombo = new ComboBox
            {
                Location = new Point(220, 47),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            _executionStatusCombo.Items.AddRange(new[] { "Yes", "Partial", "No" });
            _executionStatusCombo.SelectedIndex = 0;
            yesterdayGroup.Controls.Add(_executionStatusCombo);

            // Part B: Today's Reality
            var todayGroup = CreateGroupBox("Part B: Today's Reality", 10, 100, 560, 200);

            var subjectsLabel = new Label
            {
                Text = "Subject Progress:",
                Location = new Point(10, 20),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            todayGroup.Controls.Add(subjectsLabel);

            _subjectsPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 45),
                Size = new Size(540, 140),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            todayGroup.Controls.Add(_subjectsPanel);

            // Part C: Tomorrow's Blueprint
            var tomorrowGroup = CreateGroupBox("Part C: Tomorrow's Blueprint", 10, 310, 560, 100);

            var goalLabel = new Label
            {
                Text = "What is your goal for tomorrow?",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            tomorrowGroup.Controls.Add(goalLabel);

            _tomorrowGoalTextBox = new TextBox
            {
                Location = new Point(10, 45),
                Size = new Size(540, 45),
                Multiline = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ScrollBars = ScrollBars.Vertical
            };
            tomorrowGroup.Controls.Add(_tomorrowGoalTextBox);

            // Part D: Mindset & Mistakes
            var mindsetGroup = CreateGroupBox("Part D: Mindset & Mistakes", 10, 420, 560, 180);

            // Focus Rating
            var focusLabel = new Label
            {
                Text = "Focus Rating (1-10):",
                Location = new Point(10, 20),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            mindsetGroup.Controls.Add(focusLabel);

            _focusTrackBar = new TrackBar
            {
                Location = new Point(140, 15),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                TickFrequency = 1,
                LargeChange = 1,
                SmallChange = 1
            };
            _focusTrackBar.ValueChanged += FocusTrackBar_ValueChanged;
            mindsetGroup.Controls.Add(_focusTrackBar);

            _focusValueLabel = new Label
            {
                Text = "5",
                Location = new Point(350, 20),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mindsetGroup.Controls.Add(_focusValueLabel);

            // Mood Rating
            var moodLabel = new Label
            {
                Text = "Mood Rating (1-10):",
                Location = new Point(10, 65),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            mindsetGroup.Controls.Add(moodLabel);

            _moodTrackBar = new TrackBar
            {
                Location = new Point(140, 60),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                TickFrequency = 1,
                LargeChange = 1,
                SmallChange = 1
            };
            _moodTrackBar.ValueChanged += MoodTrackBar_ValueChanged;
            mindsetGroup.Controls.Add(_moodTrackBar);

            _moodValueLabel = new Label
            {
                Text = "5",
                Location = new Point(350, 65),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mindsetGroup.Controls.Add(_moodValueLabel);

            // Mistake Tags
            var mistakesLabel = new Label
            {
                Text = "Mistakes made today:",
                Location = new Point(10, 110),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            mindsetGroup.Controls.Add(mistakesLabel);

            _mistakeTagsList = new CheckedListBox
            {
                Location = new Point(10, 135),
                Size = new Size(540, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                CheckOnClick = true
            };
            _mistakeTagsList.Items.AddRange(new[]
            {
                "#CalculationError", "#Procrastination", "#ToughConcept",
                "#Distracted", "#SillyMistake", "#TimeManagement", "#LackOfFocus"
            });
            mindsetGroup.Controls.Add(_mistakeTagsList);

            // Save Button
            _saveButton = new Button
            {
                Text = "Save Commitment",
                Location = new Point(240, 620),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;

            this.Controls.AddRange(new Control[] { yesterdayGroup, todayGroup, tomorrowGroup, mindsetGroup, _saveButton });
        }

        private GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
        {
            return new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
        }

        private void LoadData()
        {
            try
            {
                // Load profile
                _profile = PrepMeterStorage.LoadProfile();
                var subjects = _profile?.Subjects?.Count > 0 ? _profile.Subjects : _defaultSubjects.ToList();

                // Load latest commitment for yesterday's goal
                _latestCommitment = PrepMeterStorage.LoadLatestCommitment();

                // Update yesterday's goal display
                string yesterdayGoal = _latestCommitment?.TomorrowGoal ?? "No previous goal set.";
                _yesterdayGoalLabel.Text = $"Previous Goal: {yesterdayGoal}";

                // Load and display today's focus time
                LoadTodaysFocusTime();

                // Create subject input rows
                _subjectsPanel.Controls.Clear();
                _subjectRows.Clear();

                foreach (var subject in subjects)
                {
                    var row = new SubjectInputRow(subject);
                    _subjectRows.Add(row);
                    _subjectsPanel.Controls.Add(row.Panel);
                }

                // Set default execution status based on previous goal achievement
                if (_latestCommitment != null)
                {
                    // Default to "Yes" if there was a previous goal, otherwise "No"
                    _executionStatusCombo.SelectedItem = "Yes";
                }
                else
                {
                    _executionStatusCombo.SelectedItem = "No";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTodaysFocusTime()
        {
            try
            {
                var todaysFocus = FocusHistoryService.GetDay(DateTime.Today);
                if (todaysFocus != null && todaysFocus.TotalFocusMinutes > 0)
                {
                    int hours = todaysFocus.TotalFocusMinutes / 60;
                    int minutes = todaysFocus.TotalFocusMinutes % 60;

                    string timeText = hours > 0
                        ? $"Auto-Synced Focus Time: {hours} hrs {minutes} mins"
                        : $"Auto-Synced Focus Time: {minutes} mins";

                    _focusTimeLabel.Text = timeText;
                }
                else
                {
                    _focusTimeLabel.Text = "Auto-Synced Focus Time: 0 mins (No focus sessions recorded today)";
                }
            }
            catch (Exception ex)
            {
                _focusTimeLabel.Text = $"Auto-Synced Focus Time: Unable to load ({ex.Message})";
            }
        }

        private void FocusTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _focusValueLabel.Text = _focusTrackBar.Value.ToString();
        }

        private void MoodTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _moodValueLabel.Text = _moodTrackBar.Value.ToString();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(_tomorrowGoalTextBox.Text))
                {
                    MessageBox.Show("Please enter a goal for tomorrow.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _tomorrowGoalTextBox.Focus();
                    return;
                }

                // Build subject logs
                var subjectLogs = new List<SubjectStat>();
                foreach (var row in _subjectRows)
                {
                    subjectLogs.Add(new SubjectStat
                    {
                        SubjectName = row.SubjectName,
                        LecturesCompleted = (int)row.LecturesUpDown.Value,
                        QuestionsSolved = (int)row.QuestionsUpDown.Value,
                        BacklogsCleared = (int)row.BacklogsClearedUpDown.Value,
                        BacklogsAdded = 0 // We'll handle this in a future update
                    });
                }

                // Build mistake tags
                var mistakeTags = new List<string>();
                foreach (var item in _mistakeTagsList.CheckedItems)
                {
                    mistakeTags.Add(item.ToString());
                }

                // Create the commitment
                var commitment = new DailyCommitment
                {
                    Date = DateTime.Now.Date,
                    YesterdayVerification = _executionStatusCombo.SelectedItem?.ToString() ?? "No",
                    SubjectLogs = subjectLogs,
                    TomorrowGoal = _tomorrowGoalTextBox.Text.Trim(),
                    FocusRating = _focusTrackBar.Value,
                    MoodRating = _moodTrackBar.Value,
                    MistakeTags = mistakeTags
                };

                // Save to storage
                PrepMeterStorage.SaveDailyCommitment(commitment);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving commitment: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper class to manage input controls for a single subject.
        /// </summary>
        private class SubjectInputRow
        {
            public string SubjectName { get; }
            public Panel Panel { get; }
            public NumericUpDown LecturesUpDown { get; }
            public NumericUpDown QuestionsUpDown { get; }
            public NumericUpDown BacklogsClearedUpDown { get; }

            public SubjectInputRow(string subjectName)
            {
                SubjectName = subjectName;

                Panel = new Panel
                {
                    Size = new Size(540, 30),
                    Margin = new Padding(0, 2, 0, 2)
                };

                var subjectLabel = new Label
                {
                    Text = subjectName,
                    Location = new Point(0, 5),
                    Size = new Size(80, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                Panel.Controls.Add(subjectLabel);

                // Lectures
                var lecturesLabel = new Label
                {
                    Text = "Lectures:",
                    Location = new Point(90, 5),
                    Size = new Size(60, 20),
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(lecturesLabel);

                LecturesUpDown = new NumericUpDown
                {
                    Location = new Point(150, 2),
                    Size = new Size(50, 23),
                    Minimum = 0,
                    Maximum = 50,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(LecturesUpDown);

                // Questions
                var questionsLabel = new Label
                {
                    Text = "Questions:",
                    Location = new Point(220, 5),
                    Size = new Size(70, 20),
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(questionsLabel);

                QuestionsUpDown = new NumericUpDown
                {
                    Location = new Point(290, 2),
                    Size = new Size(50, 23),
                    Minimum = 0,
                    Maximum = 200,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(QuestionsUpDown);

                // Backlogs Cleared
                var backlogsLabel = new Label
                {
                    Text = "Backlogs:",
                    Location = new Point(360, 5),
                    Size = new Size(60, 20),
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(backlogsLabel);

                BacklogsClearedUpDown = new NumericUpDown
                {
                    Location = new Point(420, 2),
                    Size = new Size(50, 23),
                    Minimum = 0,
                    Maximum = 100,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                Panel.Controls.Add(BacklogsClearedUpDown);
            }
        }
    }
}
