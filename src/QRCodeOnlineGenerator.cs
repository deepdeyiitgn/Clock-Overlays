using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;

namespace TransparentClock
{
    [SupportedOSPlatform("windows")]
    public static class QRCodeOnlineGenerator
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static Bitmap? GenerateBitmap(string payload, Color foreground, Color background, int size, int margin)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            size = Math.Max(220, size);
            margin = Math.Max(0, margin);

            string fg = $"{foreground.R}-{foreground.G}-{foreground.B}";
            string bg = $"{background.R}-{background.G}-{background.B}";
            string url =
                "https://api.qrserver.com/v1/create-qr-code/" +
                $"?size={size}x{size}" +
                $"&margin={margin}" +
                $"&color={fg}" +
                $"&bgcolor={bg}" +
                $"&data={Uri.EscapeDataString(payload)}";

            try
            {
                var bytes = HttpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                using var stream = new MemoryStream(bytes);
                using var bitmap = new Bitmap(stream);
                return new Bitmap(bitmap);
            }
            catch
            {
                return null;
            }
        }
    }
}
