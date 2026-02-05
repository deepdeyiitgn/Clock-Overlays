using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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
        /// Downloads the QuickLink logo and caches it locally.
        /// The URL returns an SVG, which we save as-is for now.
        /// Future: Convert SVG to PNG for better compatibility.
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
                    // Save to cache
                    File.WriteAllBytes(LogoCachePath, logoContent);
                }
            }
            catch
            {
                // Silently fail - logo is optional
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
