using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SkiaSharp;
using Svg.Skia;

namespace TransparentClock
{
    /// <summary>
    /// Manages QuickLink logo caching and retrieval.
    /// Downloads the logo once and caches it locally to avoid repeated web requests.
    /// </summary>
    public static class LogoManager
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays"
        );

        private static readonly string LogoCachePath = Path.Combine(AppDataPath, "quicklink-logo.png");
        private const string LogoUrl = "https://qlynk.vercel.app/quicklink-logo.svg";
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// Gets the QuickLink logo as a bitmap.
        /// Returns cached logo if available, otherwise downloads and caches it.
        /// If download fails or logo is unavailable, returns null.
        /// </summary>
        public static async Task<System.Drawing.Bitmap?> GetQuickLinkLogoAsync()
        {
            try
            {
                // If cached logo exists, load it
                if (File.Exists(LogoCachePath))
                {
                    try
                    {
                        return new System.Drawing.Bitmap(LogoCachePath);
                    }
                    catch
                    {
                        // Cached logo is corrupted, delete it and try downloading again
                        File.Delete(LogoCachePath);
                    }
                }

                // Download logo from web (one-time operation)
                await DownloadAndCacheLogoAsync();

                // Load the newly cached logo
                if (File.Exists(LogoCachePath))
                {
                    return new System.Drawing.Bitmap(LogoCachePath);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads the QuickLink logo and caches it locally as PNG.
        /// The source is an SVG that is rasterized for embedding.
        /// </summary>
        private static async Task DownloadAndCacheLogoAsync()
        {
            try
            {
                // Ensure cache directory exists
                Directory.CreateDirectory(AppDataPath);

                // Download logo
                var logoContent = await HttpClient.GetByteArrayAsync(LogoUrl);

                if (logoContent?.Length > 0)
                {
                    var pngBytes = RasterizeSvgToPng(logoContent, 256, 256);
                    if (pngBytes.Length > 0)
                    {
                        File.WriteAllBytes(LogoCachePath, pngBytes);
                    }
                }
            }
            catch
            {
                // Silently fail - logo is optional
            }
        }

        private static byte[] RasterizeSvgToPng(byte[] svgBytes, int width, int height)
        {
            try
            {
                var svg = new SKSvg();
                using var svgStream = new MemoryStream(svgBytes);
                svg.Load(svgStream);

                if (svg.Picture == null)
                {
                    return Array.Empty<byte>();
                }

                using var bitmap = new SKBitmap(width, height);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.Transparent);

                var bounds = svg.Picture.CullRect;
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    return Array.Empty<byte>();
                }

                float scaleX = width / bounds.Width;
                float scaleY = height / bounds.Height;
                float scale = Math.Min(scaleX, scaleY);

                float offsetX = (width - bounds.Width * scale) / 2f;
                float offsetY = (height - bounds.Height * scale) / 2f;

                canvas.Translate(offsetX, offsetY);
                canvas.Scale(scale);
                canvas.DrawPicture(svg.Picture);
                canvas.Flush();

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                return data?.ToArray() ?? Array.Empty<byte>();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Clears the cached logo file.
        /// </summary>
        public static void ClearCache()
        {
            try
            {
                if (File.Exists(LogoCachePath))
                    File.Delete(LogoCachePath);
            }
            catch { }
        }

        /// <summary>
        /// Checks if a valid cached logo exists.
        /// </summary>
        public static bool IsCached => File.Exists(LogoCachePath);
    }
}
