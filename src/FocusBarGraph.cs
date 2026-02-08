using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// 24-hour bar graph showing focus minutes per hour
    /// </summary>
    public class FocusBarGraph : Control
    {
        private int[] hourlyMinutes = new int[24];
        private ToolTip tooltip;
        private int hoveredHour = -1;

        // Colors and styling
        private readonly Color barColor = Color.FromArgb(66, 139, 244);           // Blue
        private readonly Color emptyBarColor = Color.FromArgb(220, 220, 220);    // Light gray for 0 minutes
        private readonly Color barHoverColor = Color.FromArgb(255, 100, 0);       // Orange on hover
        private readonly Color gridColor = Color.FromArgb(200, 200, 200);         // Light gray
        private readonly Color axisColor = Color.FromArgb(70, 70, 70);
        private readonly Color textColor = Color.FromArgb(60, 60, 60);
        private readonly Color backgroundColor = Color.White;

        // Margins and sizes
        private const int LeftMargin = 60;
        private const int RightMargin = 24;
        private const int TopMargin = 24;
        private const int BottomMargin = 60;
        private const int BarSpacing = 2;

        public FocusBarGraph()
        {
            DoubleBuffered = true;
            BackColor = backgroundColor;
            tooltip = new ToolTip();
            MouseMove += FocusBarGraph_MouseMove;
            MouseLeave += FocusBarGraph_MouseLeave;
        }

        /// <summary>
        /// Set the hourly data (array of 24 values)
        /// </summary>
        public void SetData(int[] hourlyData)
        {
            if (hourlyData == null || hourlyData.Length != 24)
            {
                hourlyData = new int[24];
            }
            
            Array.Copy(hourlyData, hourlyMinutes, 24);
            hoveredHour = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw grid and axes
            DrawGridAndAxes(e.Graphics);

            // Draw bars
            DrawBars(e.Graphics);
        }

        private void DrawGridAndAxes(Graphics g)
        {
            int graphWidth = Width - LeftMargin - RightMargin;
            int graphHeight = Height - TopMargin - BottomMargin;

            // Draw grid lines (horizontal)
            using (Pen gridPen = new Pen(gridColor, 1f) { DashStyle = DashStyle.Dash })
            {
                int gridLines = 5;
                for (int i = 0; i <= gridLines; i++)
                {
                    int y = TopMargin + (int)(graphHeight * i / gridLines);
                    g.DrawLine(gridPen, LeftMargin, y, Width - RightMargin, y);
                }
            }

            // Draw axes
            using (Pen axisPen = new Pen(axisColor, 2f))
            {
                // Y-axis
                g.DrawLine(axisPen, LeftMargin, TopMargin, LeftMargin, Height - BottomMargin);
                // X-axis
                g.DrawLine(axisPen, LeftMargin, Height - BottomMargin, Width - RightMargin, Height - BottomMargin);
            }

            // Draw Y-axis labels (minutes)
            using (Font font = new Font("Segoe UI", 9f))
            using (Brush brush = new SolidBrush(textColor))
            {
                int maxMinutes = GetMaxMinutes();
                int gridLines = 5;
                for (int i = 0; i <= gridLines; i++)
                {
                    int y = TopMargin + (int)(graphHeight * i / gridLines);
                    int minutes = maxMinutes - (int)(maxMinutes * i / gridLines);
                    string label = minutes.ToString();
                    SizeF textSize = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, LeftMargin - textSize.Width - 8, y - textSize.Height / 2);
                }
            }

            // Draw X-axis labels (hours)
            using (Font font = new Font("Segoe UI", 9f))
            using (Brush brush = new SolidBrush(textColor))
            {
                int graphWidth2 = Width - LeftMargin - RightMargin;
                int barWidth = graphWidth2 / 24;
                
                for (int hour = 0; hour < 24; hour += 4)
                {
                    int x = LeftMargin + (int)(graphWidth2 * hour / 24) + barWidth / 2;
                    string label = hour.ToString("00");
                    SizeF textSize = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, x - textSize.Width / 2, Height - BottomMargin + 8);
                }
                
                // Always draw 23 at the end
                int lastX = LeftMargin + (int)(graphWidth2 * 23 / 24) + barWidth / 2;
                string lastLabel = "23";
                SizeF lastSize = g.MeasureString(lastLabel, font);
                g.DrawString(lastLabel, font, brush, lastX - lastSize.Width / 2, Height - BottomMargin + 8);
            }

            // Draw axis titles
            using (Font font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Brush brush = new SolidBrush(textColor))
            {
                // Y-axis title
                var state = g.Save();
                g.TranslateTransform(15, Height / 2);
                g.RotateTransform(-90);
                g.DrawString("Minutes", font, brush, 0, 0);
                g.Restore(state);

                // X-axis title
                g.DrawString("Hour of Day", font, brush, Width / 2 - 40, Height - 18);
            }
        }

        private void DrawBars(Graphics g)
        {
            int graphWidth = Width - LeftMargin - RightMargin;
            int graphHeight = Height - TopMargin - BottomMargin;
            int maxMinutes = GetMaxMinutes();

            if (maxMinutes == 0) return;

            int barWidth = (graphWidth - (BarSpacing * 23)) / 24;
            if (barWidth < 2) barWidth = 2;

            for (int hour = 0; hour < 24; hour++)
            {
                int minutes = hourlyMinutes[hour];
                float barHeight = (graphHeight * minutes) / maxMinutes;
                
                int x = LeftMargin + (hour * (barWidth + BarSpacing));
                int y = (int)(Height - BottomMargin - barHeight);

                // Determine bar color
                Color currentBarColor = minutes == 0 ? emptyBarColor : barColor;
                if (hour == hoveredHour)
                {
                    currentBarColor = barHoverColor;
                }

                using (Brush barBrush = new SolidBrush(currentBarColor))
                {
                    g.FillRectangle(barBrush, x, y, barWidth, (int)barHeight);
                }

                // Draw border
                using (Pen borderPen = new Pen(axisColor, 1f))
                {
                    g.DrawRectangle(borderPen, x, y, barWidth, (int)barHeight);
                }
            }
        }

        private int GetMaxMinutes()
        {
            int max = 0;
            for (int i = 0; i < 24; i++)
            {
                if (hourlyMinutes[i] > max)
                    max = hourlyMinutes[i];
            }

            if (max == 0) return 60; // Default max if all are 0
            
            // Round up to nearest 10
            return ((max / 10) + 1) * 10;
        }

        private void FocusBarGraph_MouseMove(object? sender, MouseEventArgs e)
        {
            int graphWidth = Width - LeftMargin - RightMargin;
            int barWidth = graphWidth / 24;

            hoveredHour = -1;

            // Check which bar the mouse is over
            if (e.X >= LeftMargin && e.X < Width - RightMargin &&
                e.Y >= TopMargin && e.Y < Height - BottomMargin)
            {
                int relativeX = e.X - LeftMargin;
                int hourIndex = relativeX / (barWidth + BarSpacing);

                if (hourIndex >= 0 && hourIndex < 24)
                {
                    hoveredHour = hourIndex;
                    string tooltipText = $"{hourIndex:00}:00 - {hourIndex + 1:00}:00{Environment.NewLine}{hourlyMinutes[hourIndex]} min";
                    tooltip.SetToolTip(this, tooltipText);
                    Invalidate();
                    return;
                }
            }

            if (hoveredHour != -1)
            {
                Invalidate();
            }
        }

        private void FocusBarGraph_MouseLeave(object? sender, EventArgs e)
        {
            hoveredHour = -1;
            tooltip.SetToolTip(this, "");
            Invalidate();
        }
    }
}
