using System;
using System.Drawing;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Displays three stat cards for focus insights:
    /// 1. Average Focus per Day
    /// 2. Best Focus Slot (day + hour)
    /// 3. Worst Focus Slot (day + hour with min non-zero focus)
    /// </summary>
    public class FocusStatsDisplay : FlowLayoutPanel
    {
        private static readonly Color StatBackground = Color.White;
        private static readonly Color StatText = Color.FromArgb(50, 50, 50);
        private static readonly Color StatLabel = Color.FromArgb(110, 110, 110);
        private static readonly Font StatFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private static readonly Font StatLabelFont = new Font("Segoe UI", 9F, FontStyle.Regular);

        private Label? averageLabel;
        private Label? bestSlotLabel;
        private Label? worstSlotLabel;

        public FocusStatsDisplay()
        {
            FlowDirection = FlowDirection.LeftToRight;
            AutoSize = true;
            WrapContents = false;
            Margin = new Padding(0, 0, 0, 10);

            CreateStatCards();
        }

        private void CreateStatCards()
        {
            // Card 1: Average Focus per Day
            var card1 = CreateStatCard("Average Focus per Day", "");
            averageLabel = (Label)((TableLayoutPanel)card1.Controls[0]).Controls[1];
            Controls.Add(card1);

            // Card 2: Best Focus Slot
            var card2 = CreateStatCard("Best Focus Slot", "");
            bestSlotLabel = (Label)((TableLayoutPanel)card2.Controls[0]).Controls[1];
            Controls.Add(card2);

            // Card 3: Worst Focus Slot
            var card3 = CreateStatCard("Worst Focus Slot", "");
            worstSlotLabel = (Label)((TableLayoutPanel)card3.Controls[0]).Controls[1];
            Controls.Add(card3);
        }

        private Panel CreateStatCard(string title, string value)
        {
            var card = new Panel
            {
                BackColor = StatBackground,
                Width = 180,
                Height = 110,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(12),
                BorderStyle = BorderStyle.FixedSingle
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = false
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titleLabel = new Label
            {
                Text = title,
                Font = StatLabelFont,
                ForeColor = StatLabel,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = StatFont,
                ForeColor = StatText,
                AutoSize = true,
                Margin = new Padding(0)
            };

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(valueLabel, 0, 1);
            card.Controls.Add(layout);

            return card;
        }

        /// <summary>
        /// Update all stat cards with fresh data from insights.
        /// Safely handles null insights or missing data.
        /// </summary>
        public void UpdateStats(FocusInsights? insights)
        {
            if (insights == null || insights.DayCount == 0)
            {
                averageLabel!.Text = "No data";
                bestSlotLabel!.Text = "No data";
                worstSlotLabel!.Text = "No data";
                return;
            }

            try
            {
                // Card 1: Average Focus per Day (convert to hours and minutes)
                int avgMinutes = (int)Math.Round(insights.AverageFocusPerDay);
                int hours = avgMinutes / 60;
                int minutes = avgMinutes % 60;
                string avgText = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
                averageLabel!.Text = avgText;

                // Card 2: Best Focus Slot
                if (insights.BestFocusSlot != null)
                {
                    string dayName = insights.BestFocusSlot.Date.ToString("ddd");
                    int startHour = insights.BestFocusSlot.Hour;
                    int endHour = startHour + 1;
                    string periodText = $"{dayName} {startHour:00}–{endHour:00} ({insights.BestFocusSlot.Minutes}m)";
                    bestSlotLabel!.Text = periodText;
                }
                else
                {
                    bestSlotLabel!.Text = "No data";
                }

                // Card 3: Worst Focus Slot (only show non-zero slots)
                if (insights.WorstFocusSlot != null && insights.WorstFocusSlot.Minutes > 0)
                {
                    string dayName = insights.WorstFocusSlot.Date.ToString("ddd");
                    int startHour = insights.WorstFocusSlot.Hour;
                    int endHour = startHour + 1;
                    string periodText = $"{dayName} {startHour:00}–{endHour:00} ({insights.WorstFocusSlot.Minutes}m)";
                    worstSlotLabel!.Text = periodText;
                }
                else
                {
                    worstSlotLabel!.Text = "No sessions";
                }
            }
            catch
            {
                // Graceful degradation if any error occurs
                averageLabel!.Text = "Error";
                bestSlotLabel!.Text = "Error";
                worstSlotLabel!.Text = "Error";
            }
        }

        /// <summary>
        /// Clear all stat displays.
        /// </summary>
        public void ClearStats()
        {
            averageLabel!.Text = "—";
            bestSlotLabel!.Text = "—";
            worstSlotLabel!.Text = "—";
        }
    }
}
