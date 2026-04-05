using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// A custom control that displays a GitHub-style contribution calendar grid.
    /// Shows the last 365 days of preparation activity with color-coded squares.
    /// </summary>
    public class ContributionCalendar : Control
    {
        private const int SquareSize = 15;
        private const int GapSize = 3;
        private const int WeeksToShow = 53; // Approximately 365 days
        private const int DaysPerWeek = 7;

        private readonly ToolTip _toolTip;
        private List<DailyCommitment> _commitments = new List<DailyCommitment>();
        private readonly Dictionary<DateTime, DailyCommitment> _commitmentLookup = new Dictionary<DateTime, DailyCommitment>();

        // Color scheme matching GitHub's contribution calendar
        private static readonly Color ColorEmpty = ColorTranslator.FromHtml("#EBEBEB");
        private static readonly Color ColorYes = ColorTranslator.FromHtml("#39D353");
        private static readonly Color ColorPartial = ColorTranslator.FromHtml("#F6C343");
        private static readonly Color ColorNo = ColorTranslator.FromHtml("#F85149");

        public ContributionCalendar()
        {
            DoubleBuffered = true;
            _toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true
            };

            // Calculate preferred size based on grid layout
            int gridWidth = WeeksToShow * (SquareSize + GapSize) - GapSize;
            int gridHeight = DaysPerWeek * (SquareSize + GapSize) - GapSize;
            Size = new Size(gridWidth, gridHeight + 40); // Extra space for potential labels

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Sets the data to display in the contribution calendar.
        /// </summary>
        /// <param name="commitments">List of daily commitments to visualize.</param>
        public void SetData(List<DailyCommitment> commitments)
        {
            _commitments = commitments ?? new List<DailyCommitment>();
            _commitmentLookup.Clear();

            foreach (var commitment in _commitments)
            {
                _commitmentLookup[commitment.Date.Date] = commitment;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(-(WeeksToShow * DaysPerWeek - 1));

            // Draw the grid starting from the top-left
            for (int week = 0; week < WeeksToShow; week++)
            {
                for (int dayOfWeek = 0; dayOfWeek < DaysPerWeek; dayOfWeek++)
                {
                    DateTime currentDate = startDate.AddDays(week * DaysPerWeek + dayOfWeek);

                    // Skip future dates
                    if (currentDate > today)
                        continue;

                    int x = week * (SquareSize + GapSize);
                    int y = dayOfWeek * (SquareSize + GapSize);

                    Color squareColor = GetSquareColor(currentDate);
                    DrawSquare(e.Graphics, x, y, squareColor);
                }
            }
        }

        private void DrawSquare(Graphics g, int x, int y, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                // Draw rounded rectangle for modern look
                int cornerRadius = 2;
                using (var path = CreateRoundedRectangle(x, y, SquareSize, SquareSize, cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();

            // Top-left corner
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            // Top-right corner
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            // Bottom-right corner
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            // Bottom-left corner
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);

            path.CloseFigure();
            return path;
        }

        private Color GetSquareColor(DateTime date)
        {
            if (_commitmentLookup.TryGetValue(date.Date, out var commitment))
            {
                return commitment.YesterdayVerification?.ToLower() switch
                {
                    "yes" => ColorYes,
                    "partial" => ColorPartial,
                    "no" => ColorNo,
                    _ => ColorEmpty
                };
            }

            return ColorEmpty;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Calculate which square is being hovered
            int week = e.X / (SquareSize + GapSize);
            int dayOfWeek = e.Y / (SquareSize + GapSize);

            if (week >= 0 && week < WeeksToShow && dayOfWeek >= 0 && dayOfWeek < DaysPerWeek)
            {
                DateTime today = DateTime.Today;
                DateTime startDate = today.AddDays(-(WeeksToShow * DaysPerWeek - 1));
                DateTime hoveredDate = startDate.AddDays(week * DaysPerWeek + dayOfWeek);

                if (hoveredDate <= today && _commitmentLookup.TryGetValue(hoveredDate.Date, out var commitment))
                {
                    string toolTipText = $"{hoveredDate:MMM dd, yyyy}\n{commitment.TomorrowGoal}";
                    _toolTip.SetToolTip(this, toolTipText);
                    return;
                }
            }

            _toolTip.SetToolTip(this, string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
