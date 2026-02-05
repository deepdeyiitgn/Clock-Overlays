using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;
using SkiaSharp;

namespace TransparentClock
{
    /// <summary>
    /// Enhanced QR code generation with logo embedding support.
    /// Embeds logos at the center of QR codes with safe scaling.
    /// </summary>
    public static class QRCodeGeneratorWithLogo
    {
        /// <summary>
        /// Generates a QR code with optional logo embedding.
        /// By default, embeds the QuickLink logo if no custom logo is provided.
        /// Uses HIGH ECC level when logo is present to ensure scanability with embedded logo.
        /// </summary>
        public static Bitmap GenerateQRCodeWithLogo(
            QRPayload payload,
            QRCustomization? customization = null,
            Bitmap? customLogo = null,
            bool useDefaultLogo = true,
            int pixelsPerModule = 10,
            string eccLevel = "H")
        {
            try
            {
                customization ??= new QRCustomization();

                string encodedString = payload.GetEncodedString();
                if (string.IsNullOrEmpty(encodedString))
                    throw new InvalidOperationException("Failed to encode QR payload data");

                // Generate base QR code with specified ECC level (H for logo, M otherwise)
                Bitmap baseQR = GenerateBaseQRCode(encodedString, pixelsPerModule, eccLevel);

                // Embed logo if provided or if using default
                if (customLogo != null)
                {
                    return EmbedLogo(baseQR, customLogo, logoSizePercent: customization.LogoSizePercent);
                }

                if (useDefaultLogo)
                {
                    // Try to get QuickLink logo asynchronously
                    var quickLinkLogo = GetQuickLinkLogoSync();
                    if (quickLinkLogo != null)
                    {
                        return EmbedLogo(baseQR, quickLinkLogo, logoSizePercent: 20);
                    }
                }

                return baseQR;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate QR code with logo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Synchronously gets the QuickLink logo by running async task on thread pool.
        /// Used when called from UI event handlers.
        /// </summary>
        private static Bitmap? GetQuickLinkLogoSync()
        {
            try
            {
                var task = LogoManager.GetQuickLinkLogoAsync();
                task.Wait(TimeSpan.FromSeconds(5));
                return task.Result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generates the base QR code image (without logo).
        /// </summary>
        private static Bitmap GenerateBaseQRCode(string encodedString, int pixelsPerModule, string eccLevel = "H")
        {
            using (var generator = new QRCoder.QRCodeGenerator())
            {
                // Parse ECC level: H (High), M (Medium), L (Low), Q (Quartile)
                var ecc = eccLevel.ToUpperInvariant() switch
                {
                    "L" => QRCoder.QRCodeGenerator.ECCLevel.L,
                    "M" => QRCoder.QRCodeGenerator.ECCLevel.M,
                    "Q" => QRCoder.QRCodeGenerator.ECCLevel.Q,
                    "H" => QRCoder.QRCodeGenerator.ECCLevel.H,
                    _ => QRCoder.QRCodeGenerator.ECCLevel.H
                };

                QRCodeData qrCodeData = generator.CreateQrCode(encodedString, ecc);

                using (var qrCode = new QRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(pixelsPerModule,
                        Color.Black,
                        Color.White,
                        drawQuietZones: true);
                }
            }
        }

        /// <summary>
        /// Embeds a logo at the center of a QR code.
        /// Logo is scaled to specified percentage without breaking scannability.
        /// </summary>
        private static Bitmap EmbedLogo(Bitmap baseQR, Bitmap logo, int logoSizePercent = 20)
        {
            try
            {
                int qrWidth = baseQR.Width;
                int qrHeight = baseQR.Height;

                // Calculate logo size (percentage of QR code)
                int logoSize = (int)((qrWidth * logoSizePercent) / 100f);
                
                // Ensure logo size is reasonable
                if (logoSize < 20) logoSize = 20;
                if (logoSize > qrWidth / 3) logoSize = qrWidth / 3;

                // Resize logo
                Bitmap resizedLogo = new Bitmap(logo, new Size(logoSize, logoSize));

                // Create white background for logo (for contrast)
                Bitmap result = new Bitmap(baseQR);
                
                // Draw white rectangle at center
                int logoX = (qrWidth - logoSize) / 2;
                int logoY = (qrHeight - logoSize) / 2;
                
                using (var graphics = Graphics.FromImage(result))
                {
                    // White border around logo (2px)
                    graphics.FillRectangle(Brushes.White,
                        logoX - 2, logoY - 2,
                        logoSize + 4, logoSize + 4);

                    // Draw logo
                    graphics.DrawImage(resizedLogo, logoX, logoY, logoSize, logoSize);
                    graphics.Flush();
                }

                resizedLogo.Dispose();
                return result;
            }
            catch
            {
                // If logo embedding fails, return base QR
                return baseQR;
            }
        }
    }
}
