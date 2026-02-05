using System;
using System.Text.RegularExpressions;

namespace TransparentClock
{
    /// <summary>
    /// Validates and formats QR code input data based on the selected QR type.
    /// Ensures data integrity before QR generation.
    /// </summary>
    public static class QRFieldValidator
    {
        /// <summary>
        /// Validates URL format.
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Accept URLs with or without protocol
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Validates phone number (basic international format).
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var cleaned = Regex.Replace(phone, @"[^\d+\-\(\) ]", "");
            return cleaned.Length >= 7;
        }

        /// <summary>
        /// Validates email address.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates WiFi SSID (not empty).
        /// </summary>
        public static bool IsValidSSID(string ssid)
        {
            return !string.IsNullOrWhiteSpace(ssid);
        }

        /// <summary>
        /// Validates WiFi password (min 8 chars for WPA, any for open).
        /// </summary>
        public static bool IsValidWiFiPassword(string password, string securityType)
        {
            if (securityType.Equals("OPEN", StringComparison.OrdinalIgnoreCase))
                return true;

            return !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
        }

        /// <summary>
        /// Validates latitude (-90 to 90).
        /// </summary>
        public static bool IsValidLatitude(string lat)
        {
            if (double.TryParse(lat, out var latValue))
                return latValue >= -90 && latValue <= 90;
            return false;
        }

        /// <summary>
        /// Validates longitude (-180 to 180).
        /// </summary>
        public static bool IsValidLongitude(string lon)
        {
            if (double.TryParse(lon, out var lonValue))
                return lonValue >= -180 && lonValue <= 180;
            return false;
        }

        /// <summary>
        /// Validates UPI ID format (payee@bank).
        /// </summary>
        public static bool IsValidUPI(string upiId)
        {
            if (string.IsNullOrWhiteSpace(upiId))
                return false;

            return Regex.IsMatch(upiId, @"^[a-zA-Z0-9._-]+@[a-zA-Z]{3,}$");
        }

        /// <summary>
        /// Validates that text is not empty.
        /// </summary>
        public static bool IsValidText(string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// Normalizes a social media username (removes @ if present).
        /// </summary>
        public static string NormalizeSocialUsername(string username, string platform)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "";

            username = username.Trim();

            // Remove @ if user entered it
            if (username.StartsWith("@"))
                username = username.Substring(1);

            return username;
        }

        /// <summary>
        /// Gets the social profile URL for a given platform and username.
        /// </summary>
        public static string GetSocialProfileUrl(string username, string platform)
        {
            username = NormalizeSocialUsername(username, platform);

            if (string.IsNullOrWhiteSpace(username))
                return "";

            return platform.ToLowerInvariant() switch
            {
                "instagram" => $"https://instagram.com/{username}",
                "facebook" => $"https://facebook.com/{username}",
                "youtube" => $"https://youtube.com/@{username}",
                "twitter" or "x" => $"https://x.com/{username}",
                "linkedin" => $"https://linkedin.com/in/{username}",
                "snapchat" => $"https://snapchat.com/add/{username}",
                "github" => $"https://github.com/{username}",
                "telegram" => $"https://t.me/{username}",
                "whatsapp" => $"https://wa.me/{username.Replace("+", "")}",
                _ => username
            };
        }

        /// <summary>
        /// Validates a social profile URL or username.
        /// </summary>
        public static bool IsValidSocialProfile(string input, string mode)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (mode == "preset")
            {
                // Username mode - just needs to be non-empty and alphanumeric-ish
                return Regex.IsMatch(input, @"^[a-zA-Z0-9._\-@]{1,}$");
            }
            else
            {
                // Custom link mode - should be a valid URL
                return IsValidUrl(input);
            }
        }
    }
}
