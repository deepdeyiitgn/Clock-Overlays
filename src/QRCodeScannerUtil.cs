using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
                LastErrorMessage = "Please select a valid image file.";
                return null;
            }

            if (!File.Exists(filePath))
            {
                LastErrorMessage = "Image file not found.";
                return null;
            }

            try
            {
                return ScanQrFromImageOnline(filePath);
            }
            catch (HttpRequestException)
            {
                LastErrorMessage = "Network connection required. Please connect to the internet and try again.";
                return null;
            }
            catch (TaskCanceledException)
            {
                LastErrorMessage = "Network timeout. Please check your connection and try again.";
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Scan Error: {ex.Message}");
                LastErrorMessage = "Unable to scan QR code. Please try another image.";
                return null;
            }
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
                LastErrorMessage = "Online QR service error. Please try again later.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("["))
            {
                LastErrorMessage = "Online QR service returned an invalid response.";
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                {
                    LastErrorMessage = "No QR code detected in this image";
                    return null;
                }

                var symbolArray = root[0].GetProperty("symbol");
                if (symbolArray.ValueKind != JsonValueKind.Array || symbolArray.GetArrayLength() == 0)
                {
                    LastErrorMessage = "No QR code detected in this image";
                    return null;
                }

                var data = symbolArray[0].GetProperty("data").GetString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    LastErrorMessage = "No QR code detected in this image";
                    return null;
                }

                return data;
            }
            catch (JsonException)
            {
                LastErrorMessage = "Online QR service returned invalid data.";
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
                        payload.Data["url"] = decodedText;
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
                if (!string.IsNullOrEmpty(queryParams["subject"]))
                    payload.Data["subject"] = queryParams["subject"];
                if (!string.IsNullOrEmpty(queryParams["body"]))
                    payload.Data["body"] = queryParams["body"];
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
                if (!string.IsNullOrEmpty(queryParams["text"]))
                    payload.Data["message"] = queryParams["text"];
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
                if (!string.IsNullOrEmpty(queryParams["q"]))
                    payload.Data["location"] = queryParams["q"];
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
    }
}
