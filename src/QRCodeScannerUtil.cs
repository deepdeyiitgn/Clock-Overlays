using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using ZXing;
using ZXing.Common;

namespace TransparentClock
{
    /// <summary>
    /// Production-ready QR Code Scanner for WinForms
    /// Uses ZXing.Net with proper BarcodeReaderGeneric configuration
    /// </summary>
    public static class QRCodeScanner
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        public static string? LastErrorMessage { get; private set; }

        /// <summary>
        /// Scan QR code from image file
        /// Supports PNG, JPG, JPEG formats
        /// </summary>
        /// <param name="filePath">Path to image file</param>
        /// <returns>Decoded QR text or null if not found</returns>
        public static string? ScanQrFromImage(string filePath)
        {
            LastErrorMessage = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                LastErrorMessage = "No QR code detected in this image";
                return null;
            }

            if (!File.Exists(filePath))
            {
                LastErrorMessage = "No QR code detected in this image";
                return null;
            }

            try
            {
                var local = ScanQrFromImageLocal(filePath);
                if (!string.IsNullOrWhiteSpace(local))
                {
                    return local;
                }

                var online = ScanQrFromImageOnline(filePath);
                if (!string.IsNullOrWhiteSpace(online))
                {
                    return online;
                }

                LastErrorMessage = "No QR code detected in this image";
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Scan Error: {ex.Message}");
                LastErrorMessage = "No QR code detected in this image";
                return null;
            }
        }

        private static string? ScanQrFromImageLocal(string filePath)
        {
            try
            {
                using var bitmap = new Bitmap(filePath);

                var decoded = TryDecodeBitmap(bitmap, tryHarder: false, tryInverted: true);
                if (!string.IsNullOrWhiteSpace(decoded))
                {
                    return decoded;
                }

                decoded = TryDecodeBitmap(bitmap, tryHarder: true, tryInverted: true);
                if (!string.IsNullOrWhiteSpace(decoded))
                {
                    return decoded;
                }

                using var inverted = InvertBitmap(bitmap);
                decoded = TryDecodeBitmap(inverted, tryHarder: true, tryInverted: true);
                if (!string.IsNullOrWhiteSpace(decoded))
                {
                    return decoded;
                }

                if (bitmap.Width < 400 || bitmap.Height < 400)
                {
                    using var scaled = new Bitmap(bitmap, new Size(bitmap.Width * 2, bitmap.Height * 2));
                    decoded = TryDecodeBitmap(scaled, tryHarder: true, tryInverted: true);
                    if (!string.IsNullOrWhiteSpace(decoded))
                    {
                        return decoded;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Local QR decode failed: {ex.Message}");
            }

            return null;
        }

        private static string? TryDecodeBitmap(Bitmap bitmap, bool tryHarder, bool tryInverted)
        {
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = tryHarder,
                    TryInverted = tryInverted,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            using var sourceBitmap = EnsureArgb32(bitmap);
            var luminance = CreateLuminanceSource(sourceBitmap);
            var result = reader.Decode(luminance);
            return result?.Text;
        }

        private static Bitmap EnsureArgb32(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return (Bitmap)bitmap.Clone();
            }

            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);
        }

        private static LuminanceSource CreateLuminanceSource(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int byteCount = Math.Abs(data.Stride) * bitmap.Height;
                var buffer = new byte[byteCount];
                Marshal.Copy(data.Scan0, buffer, 0, byteCount);
                return new RGBLuminanceSource(buffer, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.BGRA32);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static Bitmap InvertBitmap(Bitmap source)
        {
            var inverted = new Bitmap(source.Width, source.Height);
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var pixel = source.GetPixel(x, y);
                    inverted.SetPixel(x, y, Color.FromArgb(pixel.A, 255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }

            return inverted;
        }

        private static string? ScanQrFromImageOnline(string filePath)
        {
            using var form = new MultipartFormDataContent();
            var fileBytes = File.ReadAllBytes(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            using var response = HttpClient.PostAsync("https://api.qrserver.com/v1/read-qr-code/", form)
                .GetAwaiter()
                .GetResult();

            var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("["))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                {
                    return null;
                }

                var symbolArray = root[0].GetProperty("symbol");
                if (symbolArray.ValueKind != JsonValueKind.Array || symbolArray.GetArrayLength() == 0)
                {
                    return null;
                }

                var data = symbolArray[0].GetProperty("data").GetString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    return null;
                }

                return data;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// QR type detection from decoded text
    /// </summary>
    public static class QRTypeDetector
    {
        public static QRType DetectQRType(string decodedText)
        {
            if (string.IsNullOrWhiteSpace(decodedText))
                return QRType.Text;

            if (decodedText.StartsWith("http://") || decodedText.StartsWith("https://"))
            {
                if (TryDetectSocial(decodedText, out _))
                    return QRType.SocialProfile;
                if (decodedText.Contains("wa.me"))
                    return QRType.WhatsApp;
                if (decodedText.Contains("maps.google.com") || decodedText.StartsWith("geo:"))
                    return QRType.GeoLocation;
                return QRType.URL;
            }

            if (decodedText.StartsWith("WIFI:"))
                return QRType.WiFi;

            if (decodedText.StartsWith("mailto:"))
                return QRType.Email;

            if (decodedText.StartsWith("tel:"))
                return QRType.Phone;

            if (decodedText.StartsWith("smsto:"))
                return QRType.SMS;

            if (decodedText.StartsWith("BEGIN:VEVENT"))
                return QRType.Calendar;

            if (decodedText.StartsWith("BEGIN:VCARD"))
                return QRType.VCard;

            if (decodedText.StartsWith("upi://"))
                return QRType.UPI;

            return QRType.Text;
        }

        public static QRPayload ParseQRData(string decodedText, QRType type)
        {
            var payload = new QRPayload { Type = type };

            try
            {
                switch (type)
                {
                    case QRType.URL:
                    case QRType.AppLink:
                    case QRType.SocialProfile:
                        ParseSocialProfile(decodedText, payload);
                        break;

                    case QRType.WiFi:
                        ParseWiFi(decodedText, payload);
                        break;

                    case QRType.Email:
                        ParseEmail(decodedText, payload);
                        break;

                    case QRType.Phone:
                        payload.Data["phone"] = decodedText.Replace("tel:", "").Trim();
                        break;

                    case QRType.SMS:
                        ParseSMS(decodedText, payload);
                        break;

                    case QRType.WhatsApp:
                        ParseWhatsApp(decodedText, payload);
                        break;

                    case QRType.Calendar:
                        ParseCalendar(decodedText, payload);
                        break;

                    case QRType.VCard:
                        ParseVCard(decodedText, payload);
                        break;

                    case QRType.GeoLocation:
                        ParseGeoLocation(decodedText, payload);
                        break;

                    case QRType.UPI:
                        ParseUPI(decodedText, payload);
                        break;

                    default:
                        payload.Data["text"] = decodedText;
                        break;
                }
            }
            catch
            {
                payload.Data["raw"] = decodedText;
            }

            return payload;
        }

        private static void ParseWiFi(string text, QRPayload payload)
        {
            var parts = text.Replace("WIFI:", "").Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("T:"))
                    payload.Data["security"] = part.Substring(2);
                else if (part.StartsWith("S:"))
                    payload.Data["ssid"] = part.Substring(2);
                else if (part.StartsWith("P:"))
                    payload.Data["password"] = part.Substring(2);
                else if (part.StartsWith("H:"))
                    payload.Data["hidden"] = part.Substring(2);
            }
        }

        private static void ParseEmail(string text, QRPayload payload)
        {
            text = text.Replace("mailto:", "");
            var parts = text.Split('?');
            payload.Data["email"] = parts[0];

            if (parts.Length > 1)
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(parts[1]);
                var subject = queryParams["subject"];
                if (!string.IsNullOrEmpty(subject))
                    payload.Data["subject"] = subject;
                var body = queryParams["body"];
                if (!string.IsNullOrEmpty(body))
                    payload.Data["body"] = body;
            }
        }

        private static void ParseSMS(string text, QRPayload payload)
        {
            text = text.Replace("smsto:", "");
            var parts = text.Split(':');
            if (parts.Length >= 1)
                payload.Data["phone"] = parts[0];
            if (parts.Length >= 2)
                payload.Data["message"] = string.Join(":", parts, 1, parts.Length - 1);
        }

        private static void ParseWhatsApp(string text, QRPayload payload)
        {
            var uri = new Uri(text);
            payload.Data["phone"] = uri.AbsolutePath.Replace("/", "");

            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var message = queryParams["text"];
                if (!string.IsNullOrEmpty(message))
                    payload.Data["message"] = message;
            }
        }

        private static void ParseCalendar(string text, QRPayload payload)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.StartsWith("SUMMARY:"))
                    payload.Data["title"] = line.Substring(8);
                else if (line.StartsWith("DTSTART:"))
                    payload.Data["startdate"] = line.Substring(8);
                else if (line.StartsWith("DTEND:"))
                    payload.Data["enddate"] = line.Substring(6);
                else if (line.StartsWith("LOCATION:"))
                    payload.Data["location"] = line.Substring(9);
                else if (line.StartsWith("DESCRIPTION:"))
                    payload.Data["description"] = line.Substring(12);
            }
        }

        private static void ParseVCard(string text, QRPayload payload)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.StartsWith("FN:"))
                    payload.Data["name"] = line.Substring(3);
                else if (line.StartsWith("TEL:"))
                    payload.Data["phone"] = line.Substring(4);
                else if (line.StartsWith("EMAIL:"))
                    payload.Data["email"] = line.Substring(6);
                else if (line.StartsWith("ORG:"))
                    payload.Data["organization"] = line.Substring(4);
                else if (line.StartsWith("URL:"))
                    payload.Data["url"] = line.Substring(4);
            }
        }

        private static void ParseGeoLocation(string text, QRPayload payload)
        {
            text = text.Replace("geo:", "");
            var parts = text.Split('?');
            var coords = parts[0].Split(',');

            if (coords.Length >= 2)
            {
                payload.Data["latitude"] = coords[0];
                payload.Data["longitude"] = coords[1];
            }

            if (parts.Length > 1)
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(parts[1]);
                var location = queryParams["q"];
                if (!string.IsNullOrEmpty(location))
                    payload.Data["location"] = location;
            }
        }

        private static void ParseUPI(string text, QRPayload payload)
        {
            var uri = new Uri(text);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            payload.Data["upiid"] = queryParams["pa"] ?? "";
            payload.Data["name"] = queryParams["pn"] ?? "";
            payload.Data["description"] = queryParams["tn"] ?? "";
            payload.Data["amount"] = queryParams["am"] ?? "";
        }

        private static void ParseSocialProfile(string text, QRPayload payload)
        {
            payload.Data["url"] = text;

            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
            {
                return;
            }

            string host = uri.Host.ToLowerInvariant();
            string platform = host switch
            {
                var h when h.Contains("instagram") => "Instagram",
                var h when h.Contains("facebook") => "Facebook",
                var h when h.Contains("youtube") => "YouTube",
                var h when h.Contains("x.com") || h.Contains("twitter") => "X (Twitter)",
                var h when h.Contains("linkedin") => "LinkedIn",
                var h when h.Contains("github") => "GitHub",
                var h when h.Contains("telegram") => "Telegram",
                var h when h.Contains("whatsapp") => "WhatsApp",
                var h when h.Contains("snapchat") => "Snapchat",
                _ => "Social"
            };

            payload.Data["platform"] = platform;
            payload.Data["username"] = uri.AbsolutePath.Trim('/');
        }

        private static bool TryDetectSocial(string text, out string platform)
        {
            platform = string.Empty;
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
            {
                return false;
            }

            string host = uri.Host.ToLowerInvariant();
            if (host.Contains("instagram") || host.Contains("facebook") || host.Contains("youtube") ||
                host.Contains("twitter") || host.Contains("x.com") || host.Contains("linkedin") ||
                host.Contains("github") || host.Contains("telegram") || host.Contains("whatsapp") ||
                host.Contains("snapchat"))
            {
                platform = host;
                return true;
            }

            return false;
        }
    }
}
