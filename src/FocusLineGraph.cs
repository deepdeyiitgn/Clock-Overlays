using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock
{
    public class FocusLineGraph : Control
    {
        private List<(DateTime Date, int Minutes)> dataPoints = new List<(DateTime, int)>();
        private ToolTip tooltip;
        private int hoveredPointIndex = -1;

        // Colors and styling
        private readonly Color lineColor = Color.FromArgb(66, 139, 244);        // Blue
        private readonly Color pointColor = Color.FromArgb(66, 139, 244);       // Blue
        private readonly Color gridColor = Color.FromArgb(200, 200, 200);       // Light gray
        private readonly Color axisColor = Color.Black;
        private readonly Color textColor = Color.Black;
        private readonly Color backgroundColor = Color.White;

        // Margins and sizes
        private const int LeftMargin = 50;
        private const int RightMargin = 20;
        private const int TopMargin = 30;
        private const int BottomMargin = 50;
        private const int PointRadius = 5;

        public FocusLineGraph()
        {
            DoubleBuffered = true;
            BackColor = backgroundColor;
            tooltip = new ToolTip();
            MouseMove += FocusLineGraph_MouseMove;
            MouseLeave += FocusLineGraph_MouseLeave;
        }

        public void SetData(List<(DateTime Date, int Minutes)> data)
        {
            dataPoints = new List<(DateTime, int)>(data.OrderBy(x => x.Date));
            hoveredPointIndex = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw grid and axes
            DrawGridAndAxes(e.Graphics);

            // Draw line and points
            if (dataPoints.Count > 0)
            {
                DrawLineAndPoints(e.Graphics);
            }
            else
            {
                DrawEmptyMessage(e.Graphics);
            }
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
            using (Font font = new Font("Arial", 9f))
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

            // Draw X-axis labels (dates)
            if (dataPoints.Count > 0)
            {
                using (Font font = new Font("Arial", 9f))
                using (Brush brush = new SolidBrush(textColor))
                {
                    int labelInterval = Math.Max(1, dataPoints.Count / 6);
                    for (int i = 0; i < dataPoints.Count; i += labelInterval)
                    {
                        int x = LeftMargin + (int)(graphWidth * i / (dataPoints.Count - 1));
                        string label = dataPoints[i].Date.ToString("MM/dd");
                        SizeF textSize = g.MeasureString(label, font);
                        g.DrawString(label, font, brush, x - textSize.Width / 2, Height - BottomMargin + 8);
                    }

                    // Draw final date if not already drawn
                    if ((dataPoints.Count - 1) % labelInterval != 0)
                    {
                        int x = Width - RightMargin;
                        string label = dataPoints[^1].Date.ToString("MM/dd");
                        SizeF textSize = g.MeasureString(label, font);
                        g.DrawString(label, font, brush, x - textSize.Width, Height - BottomMargin + 8);
                    }
                }
            }

            // Draw axis titles
            using (Font font = new Font("Arial", 10f, FontStyle.Bold))
            using (Brush brush = new SolidBrush(textColor))
            {
                // Y-axis title
                var state = g.Save();
                g.TranslateTransform(15, Height / 2);
                g.RotateTransform(-90);
                g.DrawString("Focus Minutes", font, brush, 0, 0);
                g.Restore(state);

                // X-axis title
                g.DrawString("Date", font, brush, Width / 2 - 20, Height - 15);
            }
        }

        private void DrawLineAndPoints(Graphics g)
        {
            int graphWidth = Width - LeftMargin - RightMargin;
            int graphHeight = Height - TopMargin - BottomMargin;
            int maxMinutes = GetMaxMinutes();

            if (maxMinutes == 0) return;

            // Calculate points
            List<PointF> screenPoints = new List<PointF>();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                float x = LeftMargin + (graphWidth * i / (dataPoints.Count - 1));
                float y = Height - BottomMargin - (graphHeight * dataPoints[i].Minutes / maxMinutes);
                screenPoints.Add(new PointF(x, y));
            }

            // Draw smooth line using spline
            if (screenPoints.Count > 1)
            {
                using (Pen linePen = new Pen(lineColor, 2.5f) { EndCap = LineCap.Round, StartCap = LineCap.Round })
                {
                    g.DrawCurve(linePen, screenPoints.ToArray(), 0.5f);
                }
            }

            // Draw points
            using (Brush pointBrush = new SolidBrush(pointColor))
            using (Pen pointPen = new Pen(Color.White, 2f))
            {
                for (int i = 0; i < screenPoints.Count; i++)
                {
                    PointF point = screenPoints[i];
                    
                    // Highlight hovered point
                    if (i == hoveredPointIndex)
                    {
                        using (Brush hoverBrush = new SolidBrush(Color.FromArgb(255, 100, 0)))
                        {
                            g.FillEllipse(hoverBrush, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
                        }
                    }
                    else
                    {
                        g.FillEllipse(pointBrush, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
                        g.DrawEllipse(pointPen, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
                    }
                }
            }
        }

        private void DrawEmptyMessage(Graphics g)
        {
            using (Font font = new Font("Arial", 12f))
            using (Brush brush = new SolidBrush(Color.Gray))
            {
                string message = "No focus data available";
                SizeF textSize = g.MeasureString(message, font);
                g.DrawString(message, font, brush, Width / 2 - textSize.Width / 2, Height / 2 - textSize.Height / 2);
            }
        }

        private int GetMaxMinutes()
        {
            if (dataPoints.Count == 0) return 100;
            int max = Math.Max(dataPoints.Max(x => x.Minutes), 60);
            // Round up to nearest 10
            return ((max / 10) + 1) * 10;
        }

        private void FocusLineGraph_MouseMove(object sender, MouseEventArgs e)
        {
            int graphWidth = Width - LeftMargin - RightMargin;
            int graphHeight = Height - TopMargin - BottomMargin;
            int maxMinutes = GetMaxMinutes();

            if (maxMinutes == 0 || dataPoints.Count == 0) return;

            hoveredPointIndex = -1;

            // Check if cursor is near any point
            for (int i = 0; i < dataPoints.Count; i++)
            {
                float x = LeftMargin + (graphWidth * i / (dataPoints.Count - 1));
                float y = Height - BottomMargin - (graphHeight * dataPoints[i].Minutes / maxMinutes);

                float distance = (float)Math.Sqrt(Math.Pow(e.X - x, 2) + Math.Pow(e.Y - y, 2));
                if (distance <= PointRadius + 3)
                {
                    hoveredPointIndex = i;
                    string tooltipText = $"{dataPoints[i].Date:ddd, MMM d}{Environment.NewLine}{dataPoints[i].Minutes} min";
                    tooltip.SetToolTip(this, tooltipText);
                    Invalidate();
                    return;
                }
            }

            if (hoveredPointIndex != -1)
            {
                Invalidate();
            }
        }

        private void FocusLineGraph_MouseLeave(object sender, EventArgs e)
        {
            hoveredPointIndex = -1;
            tooltip.SetToolTip(this, "");
            Invalidate();
        }
    }
}
