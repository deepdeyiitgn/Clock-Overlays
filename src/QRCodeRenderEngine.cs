using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using QRCoder;
using System.Runtime.Versioning;

namespace TransparentClock
{
    [SupportedOSPlatform("windows")]
    public static class QRCodeRenderEngine
    {
        public static Bitmap Render(string encodedText, QRCustomization customization, int pixelsPerModule, string eccLevel)
        {
            if (string.IsNullOrWhiteSpace(encodedText))
            {
                throw new InvalidOperationException("QR payload is empty.");
            }

            customization ??= new QRCustomization();
            int paddingModules = Math.Max(0, customization.Padding);
            int moduleSize = Math.Max(1, pixelsPerModule);

            using var generator = new QRCoder.QRCodeGenerator();
            var ecc = eccLevel.ToUpperInvariant() switch
            {
                "L" => QRCoder.QRCodeGenerator.ECCLevel.L,
                "M" => QRCoder.QRCodeGenerator.ECCLevel.M,
                "Q" => QRCoder.QRCodeGenerator.ECCLevel.Q,
                _ => QRCoder.QRCodeGenerator.ECCLevel.H
            };

            QRCodeData data = generator.CreateQrCode(encodedText, ecc);
            int modules = data.ModuleMatrix.Count;
            int imageSize = (modules + paddingModules * 2) * moduleSize;

            var bitmap = new Bitmap(imageSize, imageSize);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(customization.BackgroundColor);

            using var moduleBrush = CreateForegroundBrush(customization, imageSize);
            using var eyeBrush = new SolidBrush(customization.EyeColor);

            DrawFinderPattern(graphics, eyeBrush, customization.BackgroundColor, paddingModules, moduleSize, modules, 0, 0, customization.CornerEyeStyle);
            DrawFinderPattern(graphics, eyeBrush, customization.BackgroundColor, paddingModules, moduleSize, modules, modules - 7, 0, customization.CornerEyeStyle);
            DrawFinderPattern(graphics, eyeBrush, customization.BackgroundColor, paddingModules, moduleSize, modules, 0, modules - 7, customization.CornerEyeStyle);

            for (int y = 0; y < modules; y++)
            {
                var row = data.ModuleMatrix[y];
                for (int x = 0; x < modules; x++)
                {
                    if (!GetModule(row, x))
                    {
                        continue;
                    }

                    if (IsInFinderPattern(modules, x, y))
                    {
                        continue;
                    }

                    int px = (x + paddingModules) * moduleSize;
                    int py = (y + paddingModules) * moduleSize;
                    DrawModule(graphics, moduleBrush, customization.Shape, px, py, moduleSize);
                }
            }

            return bitmap;
        }

        private static Brush CreateForegroundBrush(QRCustomization customization, int size)
        {
            if (customization.UseGradient)
            {
                return new LinearGradientBrush(
                    new Rectangle(0, 0, size, size),
                    customization.ForegroundColor,
                    customization.GradientColor,
                    45f);
            }

            return new SolidBrush(customization.ForegroundColor);
        }

        private static void DrawModule(Graphics graphics, Brush brush, QRCustomization.QRShape shape, int x, int y, int size)
        {
            switch (shape)
            {
                case QRCustomization.QRShape.Dots:
                    graphics.FillEllipse(brush, x, y, size, size);
                    break;

                case QRCustomization.QRShape.Rounded:
                    DrawRoundedRect(graphics, brush, x, y, size, size, Math.Max(2, size / 4));
                    break;

                case QRCustomization.QRShape.ExtraRounded:
                    DrawRoundedRect(graphics, brush, x, y, size, size, Math.Max(2, size / 2));
                    break;

                default:
                    graphics.FillRectangle(brush, x, y, size, size);
                    break;
            }
        }

        private static void DrawFinderPattern(
            Graphics graphics,
            Brush eyeBrush,
            Color background,
            int paddingModules,
            int moduleSize,
            int modules,
            int startX,
            int startY,
            QRCustomization.EyeStyle eyeStyle)
        {
            int px = (startX + paddingModules) * moduleSize;
            int py = (startY + paddingModules) * moduleSize;
            int outerSize = moduleSize * 7;
            int middleSize = moduleSize * 5;
            int innerSize = moduleSize * 3;

            DrawEyeShape(graphics, eyeBrush, eyeStyle, px, py, outerSize);
            using (var bgBrush = new SolidBrush(background))
            {
                DrawEyeShape(graphics, bgBrush, eyeStyle, px + moduleSize, py + moduleSize, middleSize);
            }
            DrawEyeShape(graphics, eyeBrush, eyeStyle, px + moduleSize * 2, py + moduleSize * 2, innerSize);
        }

        private static void DrawEyeShape(Graphics graphics, Brush brush, QRCustomization.EyeStyle style, int x, int y, int size)
        {
            switch (style)
            {
                case QRCustomization.EyeStyle.Circle:
                    graphics.FillEllipse(brush, x, y, size, size);
                    break;

                case QRCustomization.EyeStyle.Diamond:
                    var center = new PointF(x + size / 2f, y + size / 2f);
                    var points = new[]
                    {
                        new PointF(center.X, y),
                        new PointF(x + size, center.Y),
                        new PointF(center.X, y + size),
                        new PointF(x, center.Y)
                    };
                    graphics.FillPolygon(brush, points);
                    break;

                default:
                    graphics.FillRectangle(brush, x, y, size, size);
                    break;
            }
        }

        private static void DrawRoundedRect(Graphics graphics, Brush brush, int x, int y, int width, int height, int radius)
        {
            using var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(x, y, diameter, diameter, 180, 90);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            graphics.FillPath(brush, path);
        }

        private static bool IsInFinderPattern(int modules, int x, int y)
        {
            bool inTopLeft = x < 7 && y < 7;
            bool inTopRight = x >= modules - 7 && y < 7;
            bool inBottomLeft = x < 7 && y >= modules - 7;
            return inTopLeft || inTopRight || inBottomLeft;
        }

        private static bool GetModule(object row, int x)
        {
            if (row is BitArray bitArray)
            {
                return bitArray[x];
            }

            if (row is System.Collections.Generic.List<bool> list)
            {
                return list[x];
            }

            return false;
        }
    }
}
