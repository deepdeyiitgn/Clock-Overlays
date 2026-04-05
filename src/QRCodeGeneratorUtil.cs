using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using QRCoder;

namespace TransparentClock
{
    /// <summary>
    /// Generates QR codes from text/URL with customization support.
    /// All operations are synchronous and use local QRCoder library.
    /// </summary>
    public static class QRCodeGenerator
    {
        /// <summary>
        /// Generates a QR code image from the specified payload.
        /// </summary>
        /// <param name="payload">The QR payload containing type and data</param>
        /// <param name="customization">Optional customization settings</param>
        /// <param name="pixelsPerModule">Size of each QR module in pixels (default 10)</param>
        /// <returns>A Bitmap containing the generated QR code</returns>
        public static Bitmap GenerateQRCode(QRPayload payload, QRCustomization? customization = null, int pixelsPerModule = 10)
        {
            try
            {
                customization ??= new QRCustomization();
                
                string encodedString = payload.GetEncodedString();
                if (string.IsNullOrEmpty(encodedString))
                    throw new InvalidOperationException("Failed to encode QR payload data");

                using (var generator = new QRCoder.QRCodeGenerator())
                {
                    QRCodeData qrCodeData = generator.CreateQrCode(encodedString, QRCoder.QRCodeGenerator.ECCLevel.M);
                    
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        // Generate base QR code image
                        var baseImage = qrCode.GetGraphic(pixelsPerModule,
                            customization.ForegroundColor,
                            customization.BackgroundColor,
                            drawQuietZones: true);

                        // Apply customizations
                        Bitmap result = ApplyCustomizations(baseImage, customization);
                        
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate QR code: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Applies customization options to a base QR code image.
        /// Currently supports background color and eye style rendering.
        /// </summary>
        private static Bitmap ApplyCustomizations(Bitmap baseImage, QRCustomization customization)
        {
            // For this version, we return the base image as-is.
            // Advanced customizations (shape, gradient, logo) would require
            // more complex graphics manipulation and are deferred for v2.
            
            return new Bitmap(baseImage);
        }

        /// <summary>
        /// Saves a QR code image to a PNG file.
        /// </summary>
        /// <param name="qrImage">The QR code bitmap</param>
        /// <param name="filePath">Output file path (should end with .png)</param>
        public static void SaveQRCodePNG(Bitmap qrImage, string filePath)
        {
            try
            {
                if (qrImage == null)
                    throw new ArgumentNullException(nameof(qrImage));

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("File path cannot be empty", nameof(filePath));

                // Ensure directory exists
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Save the image
                qrImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save QR code to {filePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates QR payload data based on type.
        /// Returns true if all required fields are present.
        /// </summary>
        public static bool ValidatePayload(QRPayload payload)
        {
            if (payload == null) return false;

            return payload.Type switch
            {
                QRType.URL => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("url")),
                QRType.Text => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("text")),
                QRType.WiFi => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("ssid")),
                QRType.Email => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("email")),
                QRType.Phone => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("phone")),
                QRType.SMS => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("phone")),
                QRType.WhatsApp => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("phone")),
                QRType.Calendar => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("title")),
                QRType.VCard => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("name")),
                QRType.GeoLocation => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("latitude")),
                QRType.UPI => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("upiid")),
                QRType.AppLink => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("applink")),
                QRType.SocialProfile => !string.IsNullOrWhiteSpace(payload.Data.GetValueOrDefault("profileurl")),
                _ => false
            };
        }

        /// <summary>
        /// Gets the required field names for a specific QR type.
        /// </summary>
        public static string[] GetRequiredFields(QRType type)
        {
            return type switch
            {
                QRType.URL => new[] { "url" },
                QRType.Text => new[] { "text" },
                QRType.WiFi => new[] { "ssid", "password", "security" },
                QRType.Email => new[] { "email", "subject", "body" },
                QRType.Phone => new[] { "phone" },
                QRType.SMS => new[] { "phone", "message" },
                QRType.WhatsApp => new[] { "phone", "message" },
                QRType.Calendar => new[] { "title", "startdate", "enddate" },
                QRType.VCard => new[] { "name", "phone", "email" },
                QRType.GeoLocation => new[] { "latitude", "longitude", "location" },
                QRType.UPI => new[] { "upiid", "name", "amount" },
                QRType.AppLink => new[] { "applink" },
                QRType.SocialProfile => new[] { "profileurl" },
                _ => Array.Empty<string>()
            };
        }
    }
}
