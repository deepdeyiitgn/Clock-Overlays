using System;
using System.Collections.Generic;

namespace TransparentClock
{
    /// <summary>
    /// Represents different types of QR codes that can be generated.
    /// </summary>
    public enum QRType
    {
        URL,
        Text,
        WiFi,
        Email,
        Phone,
        SMS,
        WhatsApp,
        Calendar,
        VCard,
        GeoLocation,
        UPI,
        AppLink,
        SocialProfile
    }

    /// <summary>
    /// Represents the payload data for QR code generation.
    /// Supports multiple QR code types with flexible data structures.
    /// </summary>
    public class QRPayload
    {
        public QRType Type { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public QRPayload()
        {
            Data = new Dictionary<string, string>();
        }

        /// <summary>
        /// Generates the encoded string for the QR code based on type.
        /// </summary>
        public string GetEncodedString()
        {
            return Type switch
            {
                QRType.URL => Data.ContainsKey("url") ? Data["url"] : string.Empty,
                QRType.Text => Data.ContainsKey("text") ? Data["text"] : string.Empty,
                QRType.WiFi => GenerateWiFiString(),
                QRType.Email => GenerateEmailString(),
                QRType.Phone => $"tel:{Data.GetValueOrDefault("phone", "")}",
                QRType.SMS => GenerateSMSString(),
                QRType.WhatsApp => GenerateWhatsAppString(),
                QRType.Calendar => GenerateCalendarString(),
                QRType.VCard => GenerateVCardString(),
                QRType.GeoLocation => GenerateGeoLocationString(),
                QRType.UPI => GenerateUPIString(),
                QRType.AppLink => Data.ContainsKey("applink") ? Data["applink"] : string.Empty,
                QRType.SocialProfile => Data.ContainsKey("profileurl") ? Data["profileurl"] : string.Empty,
                _ => string.Empty
            };
        }

        private string GenerateWiFiString()
        {
            // WIFI:T:WPA;S:SSID;P:PASSWORD;H:HIDDEN;;
            string ssid = Data.GetValueOrDefault("ssid", "");
            string password = Data.GetValueOrDefault("password", "");
            string security = Data.GetValueOrDefault("security", "WPA");
            string hidden = Data.GetValueOrDefault("hidden", "false");

            return $"WIFI:T:{security};S:{ssid};P:{password};H:{(hidden == "true" ? "true" : "false")};;";
        }

        private string GenerateEmailString()
        {
            // mailto:email?subject=Subject&body=Body
            string email = Data.GetValueOrDefault("email", "");
            string subject = Data.GetValueOrDefault("subject", "");
            string body = Data.GetValueOrDefault("body", "");

            if (string.IsNullOrEmpty(email)) return string.Empty;
            return $"mailto:{email}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
        }

        private string GenerateSMSString()
        {
            // smsto:PHONE:MESSAGE
            string phone = Data.GetValueOrDefault("phone", "");
            string message = Data.GetValueOrDefault("message", "");
            return $"smsto:{phone}:{message}";
        }

        private string GenerateWhatsAppString()
        {
            // https://wa.me/PHONENUMBER?text=MESSAGE
            string phone = Data.GetValueOrDefault("phone", "");
            string message = Data.GetValueOrDefault("message", "");
            return $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";
        }

        private string GenerateCalendarString()
        {
            // BEGIN:VEVENT\nDTSTART:...\nDTEND:...\nSUMMARY:...\nLOCATION:...\nEND:VEVENT
            string title = Data.GetValueOrDefault("title", "");
            string startDate = Data.GetValueOrDefault("startdate", "");
            string endDate = Data.GetValueOrDefault("enddate", "");
            string location = Data.GetValueOrDefault("location", "");
            string description = Data.GetValueOrDefault("description", "");

            return $"BEGIN:VEVENT\n" +
                   $"DTSTART:{startDate}\n" +
                   $"DTEND:{endDate}\n" +
                   $"SUMMARY:{title}\n" +
                   $"LOCATION:{location}\n" +
                   $"DESCRIPTION:{description}\n" +
                   $"END:VEVENT";
        }

        private string GenerateVCardString()
        {
            // BEGIN:VCARD\nVERSION:3.0\nFN:Name\nTEL:Phone\nEMAIL:Email\nEND:VCARD
            string name = Data.GetValueOrDefault("name", "");
            string phone = Data.GetValueOrDefault("phone", "");
            string email = Data.GetValueOrDefault("email", "");
            string organization = Data.GetValueOrDefault("organization", "");
            string url = Data.GetValueOrDefault("url", "");

            string vcard = $"BEGIN:VCARD\n" +
                          $"VERSION:3.0\n" +
                          $"FN:{name}\n";

            if (!string.IsNullOrEmpty(phone))
                vcard += $"TEL:{phone}\n";
            if (!string.IsNullOrEmpty(email))
                vcard += $"EMAIL:{email}\n";
            if (!string.IsNullOrEmpty(organization))
                vcard += $"ORG:{organization}\n";
            if (!string.IsNullOrEmpty(url))
                vcard += $"URL:{url}\n";

            vcard += "END:VCARD";
            return vcard;
        }

        private string GenerateGeoLocationString()
        {
            // geo:latitude,longitude?q=location+name
            string latitude = Data.GetValueOrDefault("latitude", "");
            string longitude = Data.GetValueOrDefault("longitude", "");
            string location = Data.GetValueOrDefault("location", "");

            return $"geo:{latitude},{longitude}?q={Uri.EscapeDataString(location)}";
        }

        private string GenerateUPIString()
        {
            // upi://pay?pa=UPI&pn=NAME&tn=DESCRIPTION&am=AMOUNT&tr=REFERENCE
            string upiId = Data.GetValueOrDefault("upiid", "");
            string name = Data.GetValueOrDefault("name", "");
            string amount = Data.GetValueOrDefault("amount", "");
            string description = Data.GetValueOrDefault("description", "");

            return $"upi://pay?pa={upiId}&pn={Uri.EscapeDataString(name)}&tn={Uri.EscapeDataString(description)}&am={amount}";
        }
    }

    /// <summary>
    /// Represents customization options for QR code generation.
    /// </summary>
    public class QRCustomization
    {
        public enum QRShape { Square, Dots, Rounded, ExtraRounded }
        public enum EyeStyle { Square, Circle, Diamond }

        public QRShape Shape { get; set; } = QRShape.Square;
        public System.Drawing.Color ForegroundColor { get; set; } = System.Drawing.Color.Black;
        public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.White;
        public EyeStyle CornerEyeStyle { get; set; } = EyeStyle.Square;
        public System.Drawing.Color EyeColor { get; set; } = System.Drawing.Color.Black;
        public bool UseGradient { get; set; } = false;
        public bool EmbedLogo { get; set; } = false;
        public int Padding { get; set; } = 0;
        public int LogoSizePercent { get; set; } = 20;

        // For gradient (secondary color)
        public System.Drawing.Color GradientColor { get; set; } = System.Drawing.Color.Gray;
    }
}
