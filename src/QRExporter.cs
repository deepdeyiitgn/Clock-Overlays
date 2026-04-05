using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TransparentClock
{
    /// <summary>
    /// Exports QR codes in multiple formats (PNG, JPG, JPEG, SVG, PDF, HEIC).
    /// Embeds metadata where supported and handles format conversions safely.
    /// </summary>
    public static class QRExporter
    {
        public enum ExportFormat
        {
            PNG,
            JPG,
            JPEG,
            SVG,
            PDF,
            HEIC
        }

        private const string AppName = "Transparent Clock / Clock Overlays";
        private const string Creator = "Deep Dey";
        private const string GeneratedVia = "https://qlynk.vercel.app";

        /// <summary>
        /// Exports a QR code image in the specified format.
        /// Adds metadata where supported.
        /// </summary>
        public static void ExportQRCode(
            Bitmap qrImage,
            string outputPath,
            ExportFormat format)
        {
            try
            {
                if (qrImage == null)
                    throw new ArgumentNullException(nameof(qrImage));

                if (string.IsNullOrWhiteSpace(outputPath))
                    throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

                // Ensure directory exists
                string? directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                switch (format)
                {
                    case ExportFormat.PNG:
                        ExportAsPNG(qrImage, outputPath);
                        break;

                    case ExportFormat.JPG:
                    case ExportFormat.JPEG:
                        ExportAsJPEG(qrImage, outputPath);
                        break;

                    case ExportFormat.SVG:
                        ExportAsSVG(qrImage, outputPath);
                        break;

                    case ExportFormat.PDF:
                        ExportAsPDF(qrImage, outputPath);
                        break;

                    case ExportFormat.HEIC:
                        ExportAsHEIC(qrImage, outputPath);
                        break;

                    default:
                        throw new NotSupportedException($"Format {format} is not supported");
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to export QR code as {format}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports QR code as PNG with metadata.
        /// </summary>
        private static void ExportAsPNG(Bitmap qrImage, string outputPath)
        {
            using (var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
                GetPixelData(qrImage),
                qrImage.Width,
                qrImage.Height))
            {
                image.Metadata.ExifProfile ??= new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile();
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Software, AppName);
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Artist, Creator);
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.ImageDescription, GeneratedVia);
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Copyright, $"© {DateTime.Now:yyyy} {Creator}");

                var pngEncoder = new PngEncoder();
                image.Save(outputPath, pngEncoder);
            }

            // Add metadata using Graphics if possible
            try
            {
                using (var img = new Bitmap(outputPath))
                {
                    var pi = img.PropertyItems;
                    // Note: GDI+ has limited metadata support
                }
            }
            catch { }
        }

        /// <summary>
        /// Exports QR code as JPEG with metadata.
        /// </summary>
        private static void ExportAsJPEG(Bitmap qrImage, string outputPath)
        {
            using (var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
                GetPixelData(qrImage),
                qrImage.Width,
                qrImage.Height))
            {
                // Convert to RGB (JPEG doesn't support alpha) by compositing on white background
                image.Mutate(x => x
                    .BackgroundColor(SixLabors.ImageSharp.Color.White));

                image.Metadata.ExifProfile ??= new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile();
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Software, AppName);
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Artist, Creator);
                image.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.ImageDescription, GeneratedVia);

                var jpegEncoder = new JpegEncoder { Quality = 95 };
                image.Save(outputPath, jpegEncoder);
            }
        }

        /// <summary>
        /// Exports QR code as SVG (vector format).
        /// Uses a simple SVG representation.
        /// </summary>
        private static void ExportAsSVG(Bitmap qrImage, string outputPath)
        {
            // Simple SVG generation from bitmap
            // Each pixel becomes a small rectangle
            int size = qrImage.Width;
            var svg = new StringBuilder();

            svg.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\">");
            svg.AppendLine($"<!-- Generated by {AppName} | {Creator} | {GeneratedVia} -->");

            // Simple pixel-to-rectangle conversion
            // For large QR codes, we could optimize this
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    System.Drawing.Color pixel = qrImage.GetPixel(x, y);
                    if (pixel.GetBrightness() < 0.5f) // Dark pixel
                    {
                        svg.AppendLine($"<rect x=\"{x}\" y=\"{y}\" width=\"1\" height=\"1\" fill=\"black\"/>");
                    }
                }
            }

            svg.AppendLine("</svg>");

            File.WriteAllText(outputPath, svg.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Exports QR code as PDF.
        /// </summary>
        private static void ExportAsPDF(Bitmap qrImage, string outputPath)
        {
            using (var document = new PdfDocument())
            {
                document.Info.Title = AppName;
                document.Info.Author = Creator;
                document.Info.Subject = GeneratedVia;
                var page = document.AddPage();

                // A4 size with margins
                double margin = 20;
                double pageWidth = page.Width.Point - 2 * margin;
                double pageHeight = page.Height.Point - 2 * margin;

                using (var xgraphics = XGraphics.FromPdfPage(page))
                {
                    // Draw white background
                    xgraphics.DrawRectangle(
                        XBrushes.White,
                        margin, margin,
                        pageWidth, pageHeight);

                    // Calculate size to fit on page
                    double qrSize = Math.Min(pageWidth * 0.8, pageHeight * 0.8);
                    double x = margin + (pageWidth - qrSize) / 2;
                    double y = margin + (pageHeight - qrSize) / 2;

                    // Save QR to temp PNG and load it
                    string tempPng = Path.GetTempFileName() + ".png";
                    try
                    {
                        qrImage.Save(tempPng, ImageFormat.Png);

                        using (var xImage = XImage.FromFile(tempPng))
                        {
                            xgraphics.DrawImage(xImage, x, y, qrSize, qrSize);
                        }
                    }
                    finally
                    {
                        if (File.Exists(tempPng))
                            File.Delete(tempPng);
                    }

                    // Add metadata text
                    var font = new XFont("Arial", 10);
                    xgraphics.DrawString(
                        $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                        font,
                        XBrushes.Gray,
                        new XRect(margin, page.Height.Point - margin - 20, pageWidth, 15),
                        XStringFormats.BottomCenter);
                }

                document.Save(outputPath);
            }
        }

        /// <summary>
        /// Exports QR code as HEIC (High Efficiency Image Container).
        /// Note: Full HEIC support requires additional codecs.
        /// Falls back to JPEG for now.
        /// </summary>
        private static void ExportAsHEIC(Bitmap qrImage, string outputPath)
        {
            // HEIC encoding requires system codec support or additional libraries
            // For now, fall back to high-quality JPEG as HEIC equivalent
            string jpegPath = Path.ChangeExtension(outputPath, ".jpg");
            ExportAsJPEG(qrImage, jpegPath);

            // If the user specifically requested .heic, note the limitation
            if (outputPath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase))
            {
                // Create a note file explaining the fallback
                string noteFile = Path.ChangeExtension(outputPath, ".txt");
                File.WriteAllText(noteFile,
                    "HEIC export is not supported on this system.\n" +
                    "A high-quality JPEG has been saved instead as: " +
                    Path.GetFileName(jpegPath));
            }
        }

        /// <summary>
        /// Extracts RGBA pixel data from a Bitmap.
        /// </summary>
        private static byte[] GetPixelData(Bitmap bitmap)
        {
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);
            bitmap.UnlockBits(bitmapData);

            return rgbValues;
        }

        /// <summary>
        /// Gets the file extension for a given export format.
        /// </summary>
        public static string GetFileExtension(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.PNG => ".png",
                ExportFormat.JPG => ".jpg",
                ExportFormat.JPEG => ".jpeg",
                ExportFormat.SVG => ".svg",
                ExportFormat.PDF => ".pdf",
                ExportFormat.HEIC => ".heic",
                _ => ".png"
            };
        }
    }
}
