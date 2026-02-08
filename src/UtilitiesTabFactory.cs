using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Factory for creating the "Utilities" TabPage with full implementations of:
    /// 1. QR Code Generator (13 QR types with customization)
    /// 2. QR Code Scanner (file/drag-drop support)
    /// 3. QuickLink URL Shortener (with API key management)
    /// </summary>
    public static class UtilitiesTabFactory
    {
        private static readonly Color PrimaryButtonBack = Color.FromArgb(58, 122, 224);
        private static readonly Color PrimaryButtonText = Color.White;
        private static readonly Color SecondaryButtonBack = Color.FromArgb(235, 235, 235);
        private static readonly Color SecondaryButtonText = Color.FromArgb(70, 70, 70);
        private static readonly Color DisabledButtonBack = Color.FromArgb(225, 225, 225);
        private static readonly Color DisabledButtonText = Color.FromArgb(140, 140, 140);
        private static readonly Color StatusInfo = Color.FromArgb(90, 90, 90);
        private static readonly Color StatusSuccess = Color.FromArgb(46, 125, 50);
        private static readonly Color StatusError = Color.FromArgb(198, 40, 40);

        public static TabPage CreateUtilitiesTab()
        {
            var tabPage = new TabPage("Utilities")
            {
                Padding = new Padding(14),
                AutoScroll = true
            };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            layout.Controls.Add(CreateQRGeneratorCard());
            layout.Controls.Add(CreateQRScannerCard());
            layout.Controls.Add(CreateQuickLinkShortenerCard());

            tabPage.Controls.Add(layout);
            return tabPage;
        }

        // ========================================
        // QR CODE GENERATOR (Dynamic Fields)
        // ========================================

        private static GroupBox CreateQRGeneratorCard()
        {
            var card = new GroupBox
            {
                Text = string.Empty,
                Width = 900,
                Height = 720,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.White
            };

            // Title
            var titleLabel = new Label
            {
                Text = "QR Code Generator",
                AutoSize = true,
                Location = new Point(12, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            card.Controls.Add(titleLabel);

            // Description
            var descLabel = new Label
            {
                Text = "Dynamic fields based on type. Embedded QuickLink logo with HIGH error correction.",
                AutoSize = true,
                Location = new Point(12, 28),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.GrayText,
                MaximumSize = new Size(500, 30)
            };
            card.Controls.Add(descLabel);

            // Type selector
            var typeLabel = new Label
            {
                Text = "Type:",
                AutoSize = true,
                Location = new Point(12, 55),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(typeLabel);

            var typeCombo = new ComboBox
            {
                Location = new Point(60, 52),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Manually add QR types instead of using DataSource
            foreach (QRType type in Enum.GetValues(typeof(QRType)))
            {
                typeCombo.Items.Add(type);
            }
            typeCombo.SelectedIndex = 0;  // Now safe to set SelectedIndex
            card.Controls.Add(typeCombo);

            // Logo checkbox (always enabled, uses HIGH ECC)
            var logoCheckBox = new CheckBox
            {
                Text = "Embed Logo (uses HIGH ECC)",
                AutoSize = true,
                Location = new Point(240, 54),
                Checked = true,
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(logoCheckBox);

            var logoButton = new Button
            {
                Text = "Choose Logo...",
                Location = new Point(410, 50),
                Width = 120,
                Height = 26,
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(logoButton);
            ApplySecondaryButtonStyle(logoButton);

            var logoPathLabel = new Label
            {
                Text = "Default: QuickLink logo",
                AutoSize = true,
                Location = new Point(12, 78),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.GrayText,
                MaximumSize = new Size(520, 20)
            };
            card.Controls.Add(logoPathLabel);

            var styleLabel = new Label
            {
                Text = "Style:",
                AutoSize = true,
                Location = new Point(12, 100),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            card.Controls.Add(styleLabel);

            var shapeCombo = new ComboBox
            {
                Location = new Point(60, 96),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            shapeCombo.Items.AddRange(Enum.GetNames(typeof(QRCustomization.QRShape)).Cast<object>().ToArray());
            shapeCombo.SelectedIndex = 0;
            card.Controls.Add(shapeCombo);

            var eyeCombo = new ComboBox
            {
                Location = new Point(210, 96),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            eyeCombo.Items.AddRange(Enum.GetNames(typeof(QRCustomization.EyeStyle)).Cast<object>().ToArray());
            eyeCombo.SelectedIndex = 0;
            card.Controls.Add(eyeCombo);

            var fgColorButton = new Button
            {
                Text = "Foreground",
                Location = new Point(360, 96),
                Width = 80,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(fgColorButton);
            ApplySecondaryButtonStyle(fgColorButton);

            var bgColorButton = new Button
            {
                Text = "Background",
                Location = new Point(448, 96),
                Width = 82,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(bgColorButton);
            ApplySecondaryButtonStyle(bgColorButton);

            var gradientCheckBox = new CheckBox
            {
                Text = "Gradient",
                AutoSize = true,
                Location = new Point(12, 128),
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(gradientCheckBox);

            var gradientColorButton = new Button
            {
                Text = "Gradient Color",
                Location = new Point(90, 124),
                Width = 110,
                Height = 24,
                Font = new Font("Segoe UI", 8F),
                Enabled = false
            };
            card.Controls.Add(gradientColorButton);
            ApplySecondaryButtonStyle(gradientColorButton);

            var paddingLabel = new Label
            {
                Text = "Padding:",
                AutoSize = true,
                Location = new Point(210, 128),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            card.Controls.Add(paddingLabel);

            var paddingUpDown = new NumericUpDown
            {
                Location = new Point(270, 124),
                Width = 60,
                Minimum = 0,
                Maximum = 8,
                Value = 2,
                Font = new Font("Segoe UI", 8F)
            };
            card.Controls.Add(paddingUpDown);

            // Input fields container (will be updated dynamically)
            var fieldsPanel = new Panel
            {
                Location = new Point(12, 155),
                Width = 536,
                Height = 180,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            card.Controls.Add(fieldsPanel);

            // Buttons area
            var generateBtn = new Button
            {
                Text = "Generate QR",
                Location = new Point(12, 342),
                Width = 120,
                Height = 36,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(generateBtn);
            ApplyPrimaryButtonStyle(generateBtn);

            var clearBtn = new Button
            {
                Text = "Clear",
                Location = new Point(140, 342),
                Width = 80,
                Height = 36,
                Font = new Font("Segoe UI", 9F)
            };
            card.Controls.Add(clearBtn);
            ApplySecondaryButtonStyle(clearBtn);

            // Preview
            var previewLabel = new Label
            {
                Text = "Preview:",
                AutoSize = true,
                Location = new Point(580, 55),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(previewLabel);

            var previewBox = new PictureBox
            {
                Location = new Point(580, 75),
                Width = 280,
                Height = 280,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            card.Controls.Add(previewBox);

            // Export format label
            var formatLabel = new Label
            {
                Text = "Format:",
                AutoSize = true,
                Location = new Point(580, 370),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(formatLabel);

            // Export format dropdown
            var formatCombo = new ComboBox
            {
                Location = new Point(580, 390),
                Width = 160,
                Height = 24,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            formatCombo.Items.AddRange(Enum.GetNames(typeof(QRExporter.ExportFormat)).Cast<object>().ToArray());
            formatCombo.SelectedIndex = 0; // PNG by default
            card.Controls.Add(formatCombo);

            // Export button
            var exportBtn = new Button
            {
                Text = "Export",
                Location = new Point(580, 420),
                Width = 160,
                Height = 32,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            card.Controls.Add(exportBtn);
            ApplyPrimaryButtonStyle(exportBtn);

            var warningLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(580, 462),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.DarkOrange,
                MaximumSize = new Size(300, 60)
            };
            card.Controls.Add(warningLabel);

            // Message
            var messageLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(12, 615),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.Highlight,
                MaximumSize = new Size(536, 70)
            };
            card.Controls.Add(messageLabel);

            // State
            Bitmap? currentQRImage = null;
            Bitmap? customLogo = null;
            string? customLogoPath = null;
            int previewToken = 0;
            var previewDebounceTimer = new System.Windows.Forms.Timer { Interval = 200 };
            var foregroundColor = Color.Black;
            var backgroundColor = Color.White;
            var gradientColor = Color.Gray;

            fgColorButton.BackColor = foregroundColor;
            bgColorButton.BackColor = backgroundColor;
            gradientColorButton.BackColor = gradientColor;

            QRCustomization BuildCustomization()
            {
                return new QRCustomization
                {
                    Shape = (QRCustomization.QRShape)Enum.Parse(typeof(QRCustomization.QRShape), shapeCombo.SelectedItem?.ToString() ?? "Square"),
                    CornerEyeStyle = (QRCustomization.EyeStyle)Enum.Parse(typeof(QRCustomization.EyeStyle), eyeCombo.SelectedItem?.ToString() ?? "Square"),
                    ForegroundColor = foregroundColor,
                    BackgroundColor = backgroundColor,
                    EyeColor = foregroundColor,
                    UseGradient = gradientCheckBox.Checked,
                    GradientColor = gradientColor,
                    Padding = (int)paddingUpDown.Value,
                    LogoSizePercent = 20
                };
            }

            string? EvaluateScanRisk(QRCustomization customization, bool embedLogo)
            {
                if (embedLogo && customization.LogoSizePercent > 25)
                {
                    return "Warning: Large logos may reduce scan reliability.";
                }

                int contrast = Math.Abs(customization.ForegroundColor.R - customization.BackgroundColor.R) +
                               Math.Abs(customization.ForegroundColor.G - customization.BackgroundColor.G) +
                               Math.Abs(customization.ForegroundColor.B - customization.BackgroundColor.B);
                if (contrast < 220)
                {
                    return "Warning: Low contrast may make the QR hard to scan.";
                }

                if (customization.UseGradient)
                {
                    return "Warning: Gradients can reduce scan accuracy on low-quality cameras.";
                }

                return null;
            }

            async Task GeneratePreviewAsync(bool showStatus)
            {
                int requestId = ++previewToken;

                if (fieldsPanel.Tag is not QRFieldSet fieldSet)
                {
                    return;
                }

                var selectedType = typeCombo.SelectedItem is QRType type
                    ? type
                    : QRType.URL;
                var qrData = BuildQRPayload(selectedType, fieldSet);
                if (qrData == null)
                {
                    if (showStatus)
                    {
                        messageLabel.Text = "❌ Please fill in all required fields correctly";
                        messageLabel.ForeColor = Color.Red;
                    }
                    previewBox.Image?.Dispose();
                    previewBox.Image = null;
                    currentQRImage = null;
                    exportBtn.Enabled = false;
                    return;
                }

                var customization = BuildCustomization();
                var customizationSnapshot = new QRCustomization
                {
                    Shape = customization.Shape,
                    CornerEyeStyle = customization.CornerEyeStyle,
                    ForegroundColor = customization.ForegroundColor,
                    BackgroundColor = customization.BackgroundColor,
                    EyeColor = customization.EyeColor,
                    UseGradient = customization.UseGradient,
                    GradientColor = customization.GradientColor,
                    Padding = customization.Padding,
                    LogoSizePercent = customization.LogoSizePercent
                };
                warningLabel.Text = EvaluateScanRisk(customization, logoCheckBox.Checked) ?? string.Empty;

                if (showStatus)
                {
                    messageLabel.Text = "⏳ Generating QR...";
                    messageLabel.ForeColor = SystemColors.Highlight;
                }

                Bitmap? generated = null;
                Bitmap? logoSnapshot = null;
                bool useDefaultLogo = logoCheckBox.Checked && customLogo == null;
                if (logoCheckBox.Checked && customLogo != null)
                {
                    logoSnapshot = new Bitmap(customLogo);
                }
                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            generated = QRCodeGeneratorWithLogo.GenerateQRCodeWithLogo(
                                qrData,
                                customizationSnapshot,
                                logoCheckBox.Checked ? logoSnapshot : null,
                                useDefaultLogo: useDefaultLogo,
                                pixelsPerModule: 10,
                                eccLevel: logoCheckBox.Checked ? "H" : "M");
                        }
                        finally
                        {
                            logoSnapshot?.Dispose();
                        }
                    });
                }
                catch
                {
                    generated?.Dispose();
                    generated = null;
                }

                if (requestId != previewToken)
                {
                    generated?.Dispose();
                    return;
                }

                previewBox.Image?.Dispose();
                previewBox.Image = generated;
                currentQRImage = generated;
                exportBtn.Enabled = currentQRImage != null;

                if (showStatus)
                {
                    if (generated == null)
                    {
                        SetStatusError(messageLabel, "❌ Failed to generate QR code");
                    }
                    else
                    {
                        SetStatusSuccess(messageLabel, $"✓ {selectedType} QR ready" + (logoCheckBox.Checked ? " with logo" : ""));
                    }
                }
            }

            previewDebounceTimer.Tick += (_, __) =>
            {
                previewDebounceTimer.Stop();
                _ = GeneratePreviewAsync(false);
            };

            void TriggerPreviewUpdate()
            {
                previewDebounceTimer.Stop();
                previewDebounceTimer.Start();
            }

            Bitmap? LoadLogoFromFile(string filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return null;
                }

                if (Path.GetExtension(filePath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        var svg = new Svg.Skia.SKSvg();
                        using var svgStream = new MemoryStream(bytes);
                        svg.Load(svgStream);

                        if (svg.Picture == null)
                        {
                            return null;
                        }

                        using var skBitmap = new SkiaSharp.SKBitmap(256, 256);
                        using var canvas = new SkiaSharp.SKCanvas(skBitmap);
                        canvas.Clear(SkiaSharp.SKColors.Transparent);

                        var bounds = svg.Picture.CullRect;
                        if (bounds.Width <= 0 || bounds.Height <= 0)
                        {
                            return null;
                        }

                        float scaleX = 256f / bounds.Width;
                        float scaleY = 256f / bounds.Height;
                        float scale = Math.Min(scaleX, scaleY);
                        float offsetX = (256f - bounds.Width * scale) / 2f;
                        float offsetY = (256f - bounds.Height * scale) / 2f;

                        canvas.Translate(offsetX, offsetY);
                        canvas.Scale(scale);
                        canvas.DrawPicture(svg.Picture);
                        canvas.Flush();

                        using var image = SkiaSharp.SKImage.FromBitmap(skBitmap);
                        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                        using var stream = new MemoryStream(data.ToArray());
                        using var bitmap = new Bitmap(stream);
                        return new Bitmap(bitmap);
                    }
                    catch
                    {
                        return null;
                    }
                }

                try
                {
                    using var bitmap = new Bitmap(filePath);
                    return new Bitmap(bitmap);
                }
                catch
                {
                    return null;
                }
            }

            fgColorButton.Click += (_, __) =>
            {
                using var dialog = new ColorDialog { Color = foregroundColor };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foregroundColor = dialog.Color;
                    fgColorButton.BackColor = dialog.Color;
                    TriggerPreviewUpdate();
                }
            };

            bgColorButton.Click += (_, __) =>
            {
                using var dialog = new ColorDialog { Color = backgroundColor };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    backgroundColor = dialog.Color;
                    bgColorButton.BackColor = dialog.Color;
                    TriggerPreviewUpdate();
                }
            };

            gradientCheckBox.CheckedChanged += (_, __) =>
            {
                gradientColorButton.Enabled = gradientCheckBox.Checked;
                TriggerPreviewUpdate();
            };

            gradientColorButton.Click += (_, __) =>
            {
                using var dialog = new ColorDialog { Color = gradientColor };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    gradientColor = dialog.Color;
                    gradientColorButton.BackColor = dialog.Color;
                    TriggerPreviewUpdate();
                }
            };

            paddingUpDown.ValueChanged += (_, __) => TriggerPreviewUpdate();
            shapeCombo.SelectedIndexChanged += (_, __) => TriggerPreviewUpdate();
            eyeCombo.SelectedIndexChanged += (_, __) => TriggerPreviewUpdate();
            logoCheckBox.CheckedChanged += (_, __) => TriggerPreviewUpdate();

            logoButton.Click += (_, __) =>
            {
                using var ofd = new OpenFileDialog
                {
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.svg|All Files|*.*"
                };

                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var loaded = LoadLogoFromFile(ofd.FileName);
                if (loaded == null)
                {
                    SetStatusError(messageLabel, "❌ Could not load logo file");
                    return;
                }

                customLogo?.Dispose();
                customLogo = loaded;
                customLogoPath = ofd.FileName;
                logoPathLabel.Text = "Custom: " + Path.GetFileName(customLogoPath);
                TriggerPreviewUpdate();
            };

            // Dynamic fields function
            Action updateFields = () =>
            {
                fieldsPanel.Controls.Clear();
                var selectedType = typeCombo.SelectedItem is QRType type
                    ? type
                    : QRType.URL;

                int yPos = 8;
                Label[] labels = Array.Empty<Label>();
                TextBox[] inputs = Array.Empty<TextBox>();
                ComboBox? socialModeCombo = null;
                ComboBox? socialPlatformCombo = null;
                ComboBox? wifiSecurityCombo = null;
                CheckBox? wifiHiddenCheckBox = null;

                switch (selectedType)
                {
                    case QRType.URL:
                        labels = new[] { new Label { Text = "URL:", AutoSize = true } };
                        inputs = new[] { new TextBox { Width = 500, Height = 24 } };
                        break;

                    case QRType.Text:
                        labels = new[] { new Label { Text = "Text:", AutoSize = true } };
                        inputs = new[] { new TextBox { Width = 500, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical } };
                        break;

                    case QRType.Email:
                        labels = new[] {
                            new Label { Text = "To:", AutoSize = true },
                            new Label { Text = "Subject:", AutoSize = true },
                            new Label { Text = "Body:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical }
                        };
                        break;

                    case QRType.Phone:
                        labels = new[] { new Label { Text = "Phone:", AutoSize = true } };
                        inputs = new[] { new TextBox { Width = 500, Height = 24, Text = "+1" } };
                        break;

                    case QRType.SMS:
                        labels = new[] {
                            new Label { Text = "Number:", AutoSize = true },
                            new Label { Text = "Message:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical }
                        };
                        break;

                    case QRType.WiFi:
                        labels = new[] {
                            new Label { Text = "SSID:", AutoSize = true },
                            new Label { Text = "Password:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24, UseSystemPasswordChar = true }
                        };

                        wifiSecurityCombo = new ComboBox
                        {
                            Width = 500,
                            Height = 24,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        wifiSecurityCombo.Items.AddRange(new[] { "WPA", "WPA2", "WEP", "OPEN" });
                        wifiSecurityCombo.SelectedIndex = 0;

                        wifiHiddenCheckBox = new CheckBox
                        {
                            Text = "Hidden network",
                            AutoSize = true,
                            Font = new Font("Segoe UI", 8F)
                        };
                        break;

                    case QRType.Calendar:
                        labels = new[] {
                            new Label { Text = "Title:", AutoSize = true },
                            new Label { Text = "Start (YYYY-MM-DD HH:MM):", AutoSize = true },
                            new Label { Text = "End (YYYY-MM-DD HH:MM):", AutoSize = true },
                            new Label { Text = "Location:", AutoSize = true },
                            new Label { Text = "Description:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24, Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
                            new TextBox { Width = 500, Height = 24, Text = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm") },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical }
                        };
                        break;

                    case QRType.VCard:
                        labels = new[] {
                            new Label { Text = "Name:", AutoSize = true },
                            new Label { Text = "Phone:", AutoSize = true },
                            new Label { Text = "Email:", AutoSize = true },
                            new Label { Text = "Organization:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 }
                        };
                        break;

                    case QRType.GeoLocation:
                        labels = new[] {
                            new Label { Text = "Latitude (-90 to 90):", AutoSize = true },
                            new Label { Text = "Longitude (-180 to 180):", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24, Text = "0" },
                            new TextBox { Width = 500, Height = 24, Text = "0" }
                        };
                        break;

                    case QRType.UPI:
                        labels = new[] {
                            new Label { Text = "Payee UPI ID:", AutoSize = true },
                            new Label { Text = "Payee Name:", AutoSize = true },
                            new Label { Text = "Amount (₹):", AutoSize = true },
                            new Label { Text = "Note (optional):", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24, Text = "user@bank" },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24 }
                        };
                        break;

                    case QRType.SocialProfile:
                        // Social mode selector
                        var modeLabel = new Label { Text = "Mode:", AutoSize = true, Location = new Point(8, yPos) };
                        fieldsPanel.Controls.Add(modeLabel);
                        yPos += 20;

                        socialModeCombo = new ComboBox
                        {
                            Location = new Point(8, yPos),
                            Width = 500,
                            Height = 24,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        socialModeCombo.Items.AddRange(new[] { "Preset Platform", "Custom Link" });
                        socialModeCombo.SelectedIndex = 0;
                        fieldsPanel.Controls.Add(socialModeCombo);
                        yPos += 28;

                        // Platform selector (shown only in preset mode)
                        var platformLabel = new Label { Text = "Platform:", AutoSize = true, Location = new Point(8, yPos), Tag = "preset" };
                        fieldsPanel.Controls.Add(platformLabel);
                        yPos += 20;

                        socialPlatformCombo = new ComboBox
                        {
                            Location = new Point(8, yPos),
                            Width = 500,
                            Height = 24,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Tag = "preset"
                        };
                        socialPlatformCombo.Items.AddRange(new[] {
                            "Instagram", "Facebook", "YouTube", "X (Twitter)", "LinkedIn",
                            "GitHub", "Telegram", "WhatsApp", "Snapchat"
                        });
                        socialPlatformCombo.SelectedIndex = 0;
                        fieldsPanel.Controls.Add(socialPlatformCombo);
                        yPos += 28;

                        // Username/Link field
                        labels = new[] { new Label { Text = "Username/Link:", AutoSize = true } };
                        inputs = new[] { new TextBox { Width = 500, Height = 24 } };

                        socialModeCombo.SelectedIndexChanged += (_, __) =>
                        {
                            var isPreset = socialModeCombo.SelectedIndex == 0;
                            foreach (Control ctrl in fieldsPanel.Controls)
                            {
                                if (ctrl.Tag?.ToString() == "preset")
                                    ctrl.Visible = isPreset;
                            }
                        };
                        break;
                }

                // Add label/input pairs
                for (int i = 0; i < labels.Length; i++)
                {
                    labels[i].Location = new Point(8, yPos);
                    fieldsPanel.Controls.Add(labels[i]);
                    yPos += 20;

                    inputs[i].Location = new Point(8, yPos);
                    fieldsPanel.Controls.Add(inputs[i]);
                    yPos += inputs[i].Height + 8;
                }

                if (selectedType == QRType.WiFi && wifiSecurityCombo != null && wifiHiddenCheckBox != null)
                {
                    var secLabel = new Label { Text = "Security:", AutoSize = true, Location = new Point(8, yPos) };
                    fieldsPanel.Controls.Add(secLabel);
                    yPos += 20;

                    wifiSecurityCombo.Location = new Point(8, yPos);
                    fieldsPanel.Controls.Add(wifiSecurityCombo);
                    yPos += 28;

                    wifiHiddenCheckBox.Location = new Point(8, yPos);
                    fieldsPanel.Controls.Add(wifiHiddenCheckBox);
                    yPos += 26;
                }


                // Set default values
                if (selectedType == QRType.URL && inputs.Length > 0)
                {
                    inputs[0].Text = "https://qlynk.vercel.app";
                    inputs[0].Focus();
                    inputs[0].SelectAll();
                }
                else if (inputs.Length > 0)
                {
                    inputs[0].Focus();
                }

                // Store for access
                fieldsPanel.Tag = new QRFieldSet
                {
                    Inputs = inputs,
                    SocialMode = socialModeCombo,
                    SocialPlatform = socialPlatformCombo,
                    WifiSecurity = wifiSecurityCombo,
                    WifiHidden = wifiHiddenCheckBox
                };

                foreach (var input in inputs)
                {
                    input.TextChanged += (_, __) => TriggerPreviewUpdate();
                }

                if (socialModeCombo != null)
                {
                    socialModeCombo.SelectedIndexChanged += (_, __) => TriggerPreviewUpdate();
                }

                if (socialPlatformCombo != null)
                {
                    socialPlatformCombo.SelectedIndexChanged += (_, __) => TriggerPreviewUpdate();
                }

                if (wifiSecurityCombo != null)
                {
                    wifiSecurityCombo.SelectedIndexChanged += (_, __) => TriggerPreviewUpdate();
                }

                if (wifiHiddenCheckBox != null)
                {
                    wifiHiddenCheckBox.CheckedChanged += (_, __) => TriggerPreviewUpdate();
                }

                if (selectedType == QRType.WiFi && wifiSecurityCombo != null && inputs.Length > 1)
                {
                    wifiSecurityCombo.SelectedIndexChanged += (_, __) =>
                    {
                        bool isOpen = string.Equals(wifiSecurityCombo.SelectedItem?.ToString(), "OPEN", StringComparison.OrdinalIgnoreCase);
                        inputs[1].Enabled = !isOpen;
                        if (isOpen)
                        {
                            inputs[1].Text = string.Empty;
                        }
                    };
                }

                TriggerPreviewUpdate();
            };

            typeCombo.SelectedIndexChanged += (_, __) => updateFields();
            updateFields(); // Initial setup

            clearBtn.Click += (s, e) =>
            {
                if (fieldsPanel.Tag is QRFieldSet fieldSet)
                {
                    foreach (var input in fieldSet.Inputs)
                    {
                        input.Clear();
                    }
                }
                customLogo?.Dispose();
                customLogo = null;
                customLogoPath = null;
                logoPathLabel.Text = "Default: QuickLink logo";
                messageLabel.Text = "";
                warningLabel.Text = "";
                previewBox.Image?.Dispose();
                previewBox.Image = null;
                exportBtn.Enabled = false;
            };

            generateBtn.Click += async (s, e) =>
            {
                try
                {
                    generateBtn.Enabled = false;
                    await GeneratePreviewAsync(true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    SetStatusError(messageLabel, "❌ Failed to generate QR code");
                }
                finally
                {
                    generateBtn.Enabled = true;
                }
            };

            exportBtn.Click += async (s, e) =>
            {
                if (currentQRImage == null) return;

                var format = (QRExporter.ExportFormat)Enum.Parse(
                    typeof(QRExporter.ExportFormat),
                    formatCombo.SelectedItem?.ToString() ?? "PNG");

                var ext = QRExporter.GetFileExtension(format);

                using (var sfd = new SaveFileDialog
                {
                    Filter = $"{format} Image|*{ext}",
                    FileName = $"qrcode_{DateTime.Now:yyyyMMdd_HHmmss}{ext}"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            exportBtn.Enabled = false;
                            SetStatusInfo(messageLabel, $"Exporting as {format}...");

                            await Task.Run(() =>
                            {
                                QRExporter.ExportQRCode(currentQRImage, sfd.FileName, format);
                            });

                            SetStatusSuccess(messageLabel, $"✓ {Path.GetFileName(sfd.FileName)}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                            SetStatusError(messageLabel, "❌ Export failed. Please try again.");
                        }
                        finally
                        {
                            exportBtn.Enabled = true;
                        }
                    }
                }
            };

            return card;
        }

        private static QRPayload? BuildQRPayload(QRType type, QRFieldSet fieldSet)
        {
            try
            {
                var payload = new QRPayload { Type = type };
            var inputs = fieldSet.Inputs;
            var socialMode = fieldSet.SocialMode;
            var socialPlatform = fieldSet.SocialPlatform;
            var wifiSecurity = fieldSet.WifiSecurity;
            var wifiHidden = fieldSet.WifiHidden;

                switch (type)
                {
                    case QRType.URL:
                        if (!QRFieldValidator.IsValidUrl(inputs[0].Text))
                            return null;
                        var urlValue = inputs[0].Text.StartsWith("http") ? inputs[0].Text : "https://" + inputs[0].Text;
                        payload.Data["url"] = urlValue;
                        break;

                    case QRType.Text:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        payload.Data["text"] = inputs[0].Text;
                        break;

                    case QRType.Email:
                        if (!QRFieldValidator.IsValidEmail(inputs[0].Text))
                            return null;
                        payload.Data["email"] = inputs[0].Text;
                        payload.Data["subject"] = inputs[1].Text;
                        payload.Data["body"] = inputs[2].Text;
                        break;

                    case QRType.Phone:
                        if (!QRFieldValidator.IsValidPhone(inputs[0].Text))
                            return null;
                        payload.Data["phone"] = inputs[0].Text;
                        break;

                    case QRType.SMS:
                        if (!QRFieldValidator.IsValidPhone(inputs[0].Text))
                            return null;
                        payload.Data["phone"] = inputs[0].Text;
                        payload.Data["message"] = inputs[1].Text;
                        break;

                    case QRType.WiFi:
                        if (!QRFieldValidator.IsValidSSID(inputs[0].Text))
                            return null;
                        var sec = wifiSecurity?.SelectedItem?.ToString() ?? "WPA";
                        var pwd = inputs[1].Text;
                        if (!QRFieldValidator.IsValidWiFiPassword(pwd, sec))
                            return null;

                        payload.Data["ssid"] = inputs[0].Text;
                        payload.Data["password"] = pwd;
                        payload.Data["security"] = sec;
                        payload.Data["hidden"] = wifiHidden?.Checked == true ? "true" : "false";
                        break;

                    case QRType.Calendar:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        if (!QRFieldValidator.IsValidDateTime(inputs[1].Text) || !QRFieldValidator.IsValidDateTime(inputs[2].Text))
                            return null;

                        if (!DateTime.TryParse(inputs[1].Text, out var startDate) || !DateTime.TryParse(inputs[2].Text, out var endDate))
                            return null;

                        var start = startDate.ToString("yyyyMMdd'T'HHmmss");
                        var end = endDate.ToString("yyyyMMdd'T'HHmmss");

                        payload.Data["title"] = inputs[0].Text;
                        payload.Data["startdate"] = start;
                        payload.Data["enddate"] = end;
                        payload.Data["location"] = inputs[3].Text;
                        payload.Data["description"] = inputs[4].Text;
                        break;

                    case QRType.VCard:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        payload.Data["name"] = inputs[0].Text;
                        payload.Data["phone"] = inputs[1].Text;
                        payload.Data["email"] = inputs[2].Text;
                        payload.Data["organization"] = inputs[3].Text;
                        break;

                    case QRType.GeoLocation:
                        if (!QRFieldValidator.IsValidLatitude(inputs[0].Text) || !QRFieldValidator.IsValidLongitude(inputs[1].Text))
                            return null;
                        payload.Data["latitude"] = inputs[0].Text;
                        payload.Data["longitude"] = inputs[1].Text;
                        break;

                    case QRType.UPI:
                        if (!QRFieldValidator.IsValidUPI(inputs[0].Text))
                            return null;
                        payload.Data["upiid"] = inputs[0].Text;
                        payload.Data["name"] = inputs[1].Text;
                        payload.Data["amount"] = inputs[2].Text;
                        payload.Data["description"] = inputs[3].Text;
                        break;

                    case QRType.SocialProfile:
                        if (socialMode?.SelectedIndex == 0)
                        {
                            // Preset mode
                            var platform = socialPlatform?.SelectedItem?.ToString() ?? "Instagram";
                            var url = QRFieldValidator.GetSocialProfileUrl(inputs[0].Text, platform);
                            if (string.IsNullOrWhiteSpace(url))
                                return null;
                            payload.Data["profileurl"] = url;
                        }
                        else
                        {
                            // Custom link
                            if (!QRFieldValidator.IsValidUrl(inputs[0].Text))
                                return null;
                            payload.Data["profileurl"] = inputs[0].Text;
                        }
                        break;

                    default:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        payload.Data["text"] = inputs[0].Text;
                        break;
                }

                return payload;
            }
            catch
            {
                return null;
            }
        }

        // ========================================
        // QR CODE SCANNER
        // ========================================

        private static GroupBox CreateQRScannerCard()
        {
            var card = new GroupBox
            {
                Text = string.Empty,
                Width = 540,
                Height = 300,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.White
            };

            // Title
            var titleLabel = new Label
            {
                Text = "QR Code Scanner",
                AutoSize = true,
                Location = new Point(12, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            card.Controls.Add(titleLabel);

            // Description
            var descLabel = new Label
            {
                Text = "Scan QR codes from images. Drag & drop or select file.",
                AutoSize = true,
                Location = new Point(12, 30),
                Font = new Font("Segoe UI", 9F),
                ForeColor = SystemColors.GrayText
            };
            card.Controls.Add(descLabel);

            // Drop zone
            var dropZone = new Panel
            {
                Location = new Point(12, 55),
                Width = 250,
                Height = 80,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            card.Controls.Add(dropZone);

            var dropLabel = new Label
            {
                Text = "Drag & drop QR image here\nor click to browse",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F),
                ForeColor = SystemColors.GrayText
            };
            dropZone.Controls.Add(dropLabel);

            // Browse button
            var browseBtn = new Button
            {
                Text = "Browse...",
                Location = new Point(270, 95),
                Width = 100,
                Height = 32,
                Font = new Font("Segoe UI", 9F)
            };
            card.Controls.Add(browseBtn);
            ApplyPrimaryButtonStyle(browseBtn);

            // Result display
            var resultLabel = new Label
            {
                Text = "Scanned Data:",
                AutoSize = true,
                Location = new Point(12, 145),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(resultLabel);

            var resultTextBox = new TextBox
            {
                Location = new Point(12, 165),
                Width = 358,
                Height = 50,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Courier New", 8F)
            };
            card.Controls.Add(resultTextBox);

            // Copy button
            var copyBtn = new Button
            {
                Text = "Copy",
                Location = new Point(378, 165),
                Width = 70,
                Height = 24,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            card.Controls.Add(copyBtn);
            ApplySecondaryButtonStyle(copyBtn);

            // Preview button
            var previewBtn = new Button
            {
                Text = "Preview",
                Location = new Point(378, 195),
                Width = 70,
                Height = 24,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            card.Controls.Add(previewBtn);
            ApplySecondaryButtonStyle(previewBtn);

            // Status
            var statusLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(12, 225),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.Highlight,
                MaximumSize = new Size(420, 60)
            };
            card.Controls.Add(statusLabel);

            // Drag/drop setup
            dropZone.AllowDrop = true;
            string? scannedData = null;

            var scanImage = (string filePath) =>
            {
                try
                {
                    SetStatusInfo(statusLabel, "Scanning...");

                    scannedData = QRCodeScanner.ScanQrFromImage(filePath);

                    if (string.IsNullOrEmpty(scannedData))
                    {
                        SetStatusError(statusLabel, "No QR code detected in this image");
                        resultTextBox.Text = "";
                        copyBtn.Enabled = false;
                        previewBtn.Enabled = false;
                    }
                    else
                    {
                        resultTextBox.Text = scannedData;
                        var detectedType = QRTypeDetector.DetectQRType(scannedData);
                        SetStatusSuccess(statusLabel, $"✓ QR scanned: {detectedType}");
                        copyBtn.Enabled = true;
                        previewBtn.Enabled = true;
                    }
                }
                catch (Exception)
                {
                    SetStatusError(statusLabel, "No QR code detected in this image");
                    resultTextBox.Text = "";
                    copyBtn.Enabled = false;
                    previewBtn.Enabled = false;
                }
            };

            dropZone.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                    e.Effect = DragDropEffects.Copy;
            };

            dropZone.DragDrop += (s, e) =>
            {
                var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
                if (files?.Length > 0)
                {
                    scanImage(files[0]);
                }
            };

            browseBtn.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog
                {
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*"
                })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                        scanImage(ofd.FileName);
                }
            };

            copyBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(scannedData))
                {
                    Clipboard.SetText(scannedData);
                    SetStatusSuccess(statusLabel, "✓ Copied to clipboard");
                }
            };

            previewBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(scannedData))
                {
                    var type = QRTypeDetector.DetectQRType(scannedData);
                    var payload = QRTypeDetector.ParseQRData(scannedData, type);
                    ShowQRPreview(payload);
                }
            };

            return card;
        }

        // ========================================
        // QUICKLINK URL SHORTENER
        // ========================================

        private static GroupBox CreateQuickLinkShortenerCard()
        {
            var card = new GroupBox
            {
                Text = string.Empty,
                Width = 900,
                Height = 300,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.White
            };

            // Title
            var titleLabel = new Label
            {
                Text = "QuickLink URL Shortener",
                AutoSize = true,
                Location = new Point(12, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            card.Controls.Add(titleLabel);

            // Description
            var descLabel = new Label
            {
                Text = "Shorten long URLs for easy sharing.",
                AutoSize = true,
                Location = new Point(12, 30),
                Font = new Font("Segoe UI", 9F),
                ForeColor = SystemColors.GrayText
            };
            card.Controls.Add(descLabel);

            // API Key Setup Panel
            var apiKeySetupPanel = new Panel
            {
                Location = new Point(12, 55),
                Width = 872,
                Height = 95
            };
            card.Controls.Add(apiKeySetupPanel);

            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                AutoSize = true,
                Location = new Point(0, 5),
                Font = new Font("Segoe UI", 9F)
            };
            apiKeySetupPanel.Controls.Add(apiKeyLabel);

            var apiKeyTextBox = new TextBox
            {
                Location = new Point(70, 2),
                Width = 280,
                Height = 24,
                UseSystemPasswordChar = true
            };
            apiKeySetupPanel.Controls.Add(apiKeyTextBox);

            var getApiKeyBtn = new Button
            {
                Text = "Get API Key",
                Location = new Point(360, 2),
                Width = 100,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            getApiKeyBtn.Click += (_, __) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://qlynk.vercel.app/api-access",
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            apiKeySetupPanel.Controls.Add(getApiKeyBtn);
            ApplySecondaryButtonStyle(getApiKeyBtn);

            var saveApiKeyBtn = new Button
            {
                Text = "Save",
                Location = new Point(460, 2),
                Width = 50,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            apiKeySetupPanel.Controls.Add(saveApiKeyBtn);
            ApplyPrimaryButtonStyle(saveApiKeyBtn);

            var helpLabel = new Label
            {
                Text = "Sign up at qlynk.vercel.app and generate your API key.",
                AutoSize = true,
                Location = new Point(0, 35),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = SystemColors.GrayText,
                MaximumSize = new Size(500, 60)
            };
            apiKeySetupPanel.Controls.Add(helpLabel);

            // URL Shortening Panel
            var shortenPanel = new Panel
            {
                Location = new Point(12, 55),
                Width = 872,
                Height = 210,
                Visible = false
            };
            card.Controls.Add(shortenPanel);

            var longUrlLabel = new Label
            {
                Text = "Long URL:",
                AutoSize = true,
                Location = new Point(0, 5),
                Font = new Font("Segoe UI", 9F)
            };
            shortenPanel.Controls.Add(longUrlLabel);

            var longUrlTextBox = new TextBox
            {
                Location = new Point(0, 25),
                Width = 520,
                Height = 24
            };
            shortenPanel.Controls.Add(longUrlTextBox);

            var aliasLabel = new Label
            {
                Text = "Alias (optional):",
                AutoSize = true,
                Location = new Point(0, 55),
                Font = new Font("Segoe UI", 9F)
            };
            shortenPanel.Controls.Add(aliasLabel);

            var aliasTextBox = new TextBox
            {
                Location = new Point(0, 75),
                Width = 260,
                Height = 24
            };
            shortenPanel.Controls.Add(aliasTextBox);

            var shortenBtn = new Button
            {
                Text = "Shorten",
                Location = new Point(270, 75),
                Width = 100,
                Height = 24,
                Font = new Font("Segoe UI", 9F)
            };
            shortenPanel.Controls.Add(shortenBtn);
            ApplyPrimaryButtonStyle(shortenBtn);

            var logoutBtn = new Button
            {
                Text = "Change Key",
                Location = new Point(380, 75),
                Width = 100,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            shortenPanel.Controls.Add(logoutBtn);
            ApplySecondaryButtonStyle(logoutBtn);

            // Result details panel (right side)
            var resultPanel = new Panel
            {
                Location = new Point(540, 0),
                Width = 320,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            shortenPanel.Controls.Add(resultPanel);

            var resultLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(6)
            };
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++)
            {
                resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            resultPanel.Controls.Add(resultLayout);

            Label MakeKey(string text) => new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };

            var statusValue = new Label { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var shortLink = new LinkLabel { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var directLink = new LinkLabel { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var longLink = new LinkLabel { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var aliasValue = new Label { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var creditsValue = new Label { AutoSize = true, Font = new Font("Segoe UI", 8F) };
            var expiryValue = new Label { AutoSize = true, Font = new Font("Segoe UI", 8F) };

            resultLayout.Controls.Add(MakeKey("Status"), 0, 0);
            resultLayout.Controls.Add(statusValue, 1, 0);
            resultLayout.Controls.Add(MakeKey("Short URL"), 0, 1);
            resultLayout.Controls.Add(shortLink, 1, 1);
            resultLayout.Controls.Add(MakeKey("Direct URL"), 0, 2);
            resultLayout.Controls.Add(directLink, 1, 2);
            resultLayout.Controls.Add(MakeKey("Long URL"), 0, 3);
            resultLayout.Controls.Add(longLink, 1, 3);
            resultLayout.Controls.Add(MakeKey("Alias"), 0, 4);
            resultLayout.Controls.Add(aliasValue, 1, 4);
            resultLayout.Controls.Add(MakeKey("Credits"), 0, 5);
            resultLayout.Controls.Add(creditsValue, 1, 5);
            resultLayout.Controls.Add(MakeKey("Expiry"), 0, 6);
            resultLayout.Controls.Add(expiryValue, 1, 6);

            var seeJsonBtn = new Button
            {
                Text = "See JSON result",
                Location = new Point(0, 105),
                Width = 150,
                Height = 24,
                Font = new Font("Segoe UI", 8F),
                Enabled = false
            };
            shortenPanel.Controls.Add(seeJsonBtn);
            ApplySecondaryButtonStyle(seeJsonBtn);

            var jsonTextBox = new TextBox
            {
                Location = new Point(540, 155),
                Width = 320,
                Height = 50,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Courier New", 8F)
            };
            shortenPanel.Controls.Add(jsonTextBox);

            string lastJson = string.Empty;

            // Status
            var statusLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(12, 205),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.Highlight,
                MaximumSize = new Size(500, 40)
            };
            card.Controls.Add(statusLabel);

            // Load saved API key
            var savedKey = QuickLinkSettings.GetApiKey();
            if (!string.IsNullOrEmpty(savedKey))
            {
                apiKeySetupPanel.Visible = false;
                shortenPanel.Visible = true;
                apiKeyTextBox.Text = savedKey;
            }

            // Event handlers
            saveApiKeyBtn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(apiKeyTextBox.Text))
                {
                    SetStatusError(statusLabel, "❌ Please enter API key");
                    return;
                }

                QuickLinkSettings.SetApiKey(apiKeyTextBox.Text);
                apiKeySetupPanel.Visible = false;
                shortenPanel.Visible = true;
                SetStatusSuccess(statusLabel, "✓ API key saved");
            };

            void SetLink(LinkLabel linkLabel, string? url)
            {
                linkLabel.Text = url ?? string.Empty;
                linkLabel.Links.Clear();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    linkLabel.Links.Add(0, url.Length, url);
                }
            }

            shortLink.LinkClicked += (_, args) =>
            {
                if (args.Link?.LinkData is string url)
                {
                    OpenUrl(url);
                }
            };

            directLink.LinkClicked += (_, args) =>
            {
                if (args.Link?.LinkData is string url)
                {
                    OpenUrl(url);
                }
            };

            longLink.LinkClicked += (_, args) =>
            {
                if (args.Link?.LinkData is string url)
                {
                    OpenUrl(url);
                }
            };

            shortenBtn.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(longUrlTextBox.Text))
                {
                    SetStatusError(statusLabel, "❌ Please enter a long URL");
                    return;
                }

                try
                {
                    shortenBtn.Enabled = false;
                    SetStatusInfo(statusLabel, "Shortening...");

                    var client = new QuickLinkShortenClient();
                    var result = await client.ShortenAsync(
                        apiKeyTextBox.Text,
                        longUrlTextBox.Text,
                        string.IsNullOrWhiteSpace(aliasTextBox.Text) ? null : aliasTextBox.Text
                    );

                    if (result.IsSuccess)
                    {
                        var success = result.Success;
                        statusValue.Text = success?.Status ?? "success";
                        SetLink(shortLink, success?.ShortenedUrl);
                        SetLink(directLink, success?.DirectRedirectUrl);
                        SetLink(longLink, success?.LongUrl);
                        aliasValue.Text = success?.Alias ?? string.Empty;
                        creditsValue.Text = success?.RemainingCredits.ToString() ?? "0";
                        expiryValue.Text = success?.ExpiresAt ?? string.Empty;
                        resultPanel.Visible = true;

                        var jsonPayload = new
                        {
                            status = success?.Status ?? "success",
                            shortenedUrl = success?.ShortenedUrl ?? string.Empty,
                            directRedirectUrl = success?.DirectRedirectUrl ?? string.Empty,
                            longUrl = success?.LongUrl ?? string.Empty,
                            alias = success?.Alias ?? string.Empty,
                            remainingCredits = success?.RemainingCredits ?? 0,
                            expiresAt = success?.ExpiresAt ?? string.Empty
                        };
                        lastJson = System.Text.Json.JsonSerializer.Serialize(jsonPayload, new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        jsonTextBox.Text = string.Empty;
                        seeJsonBtn.Enabled = true;
                        SetStatusSuccess(statusLabel, "✓ Short link created successfully");
                    }
                    else
                    {
                        resultPanel.Visible = false;
                        jsonTextBox.Text = string.Empty;
                        seeJsonBtn.Enabled = false;
                        SetStatusError(statusLabel, result.Error?.Message ?? "Failed to shorten URL");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    SetStatusError(statusLabel, "❌ Unable to shorten right now. Please try again.");
                    resultPanel.Visible = false;
                    jsonTextBox.Text = string.Empty;
                    seeJsonBtn.Enabled = false;
                }
                finally
                {
                    shortenBtn.Enabled = true;
                }
            };

            seeJsonBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(lastJson))
                {
                    jsonTextBox.Text = lastJson;
                }
            };

            logoutBtn.Click += (s, e) =>
            {
                QuickLinkSettings.SetApiKey("");
                apiKeyTextBox.Text = "";
                apiKeySetupPanel.Visible = true;
                shortenPanel.Visible = false;
                resultPanel.Visible = false;
                jsonTextBox.Text = string.Empty;
                seeJsonBtn.Enabled = false;
                SetStatusInfo(statusLabel, "API key cleared");
            };

            return card;
        }

        // ========================================
        // HELPERS
        // ========================================

        private sealed class QRFieldSet
        {
            public TextBox[] Inputs { get; set; } = Array.Empty<TextBox>();
            public ComboBox? SocialMode { get; set; }
            public ComboBox? SocialPlatform { get; set; }
            public ComboBox? WifiSecurity { get; set; }
            public CheckBox? WifiHidden { get; set; }
        }

        private static void ApplyPrimaryButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = PrimaryButtonBack;
            button.ForeColor = PrimaryButtonText;
            button.EnabledChanged += (_, __) =>
            {
                if (button.Enabled)
                {
                    button.BackColor = PrimaryButtonBack;
                    button.ForeColor = PrimaryButtonText;
                }
                else
                {
                    button.BackColor = DisabledButtonBack;
                    button.ForeColor = DisabledButtonText;
                }
            };
        }

        private static void ApplySecondaryButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = SecondaryButtonBack;
            button.ForeColor = SecondaryButtonText;
            button.EnabledChanged += (_, __) =>
            {
                if (button.Enabled)
                {
                    button.BackColor = SecondaryButtonBack;
                    button.ForeColor = SecondaryButtonText;
                }
                else
                {
                    button.BackColor = DisabledButtonBack;
                    button.ForeColor = DisabledButtonText;
                }
            };
        }

        private static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private static void SetStatusInfo(Label label, string text)
        {
            label.Text = text;
            label.ForeColor = StatusInfo;
        }

        private static void SetStatusSuccess(Label label, string text)
        {
            label.Text = text;
            label.ForeColor = StatusSuccess;
        }

        private static void SetStatusError(Label label, string text)
        {
            label.Text = text;
            label.ForeColor = StatusError;
        }

        private static void ShowQRPreview(QRPayload payload)
        {
            var form = new Form
            {
                Text = $"QR Preview - {payload.Type}",
                Width = 500,
                Height = 450,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Info panel
            var infoPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                MaximumSize = new Size(460, 340)
            };

            infoPanel.Controls.Add(new Label
            {
                Text = "Detected Type: " + payload.Type,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(4)
            });

            void AddRow(string label, string? value, bool link = false)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var row = new Panel { Width = 450, Height = 24 };
                var keyLabel = new Label
                {
                    Text = label,
                    AutoSize = true,
                    Location = new Point(0, 4),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                row.Controls.Add(keyLabel);

                if (link)
                {
                    var linkLabel = new LinkLabel
                    {
                        Text = value ?? string.Empty,
                        AutoSize = true,
                        Location = new Point(120, 4)
                    };
                    linkLabel.LinkClicked += (_, __) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = value,
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    };
                    row.Controls.Add(linkLabel);
                }
                else
                {
                    var valueLabel = new Label
                    {
                        Text = value ?? string.Empty,
                        AutoSize = true,
                        Location = new Point(120, 4),
                        Font = new Font("Segoe UI", 9F)
                    };
                    row.Controls.Add(valueLabel);
                }

                infoPanel.Controls.Add(row);
            }

            switch (payload.Type)
            {
                case QRType.URL:
                    AddRow("URL", payload.Data.GetValueOrDefault("url"), link: true);
                    break;

                case QRType.WiFi:
                    AddRow("SSID", payload.Data.GetValueOrDefault("ssid"));
                    AddRow("Password", payload.Data.GetValueOrDefault("password"));
                    AddRow("Security", payload.Data.GetValueOrDefault("security"));
                    break;

                case QRType.Email:
                    AddRow("To", payload.Data.GetValueOrDefault("email"));
                    AddRow("Subject", payload.Data.GetValueOrDefault("subject"));
                    AddRow("Body", payload.Data.GetValueOrDefault("body"));
                    break;

                case QRType.Phone:
                    AddRow("Phone", payload.Data.GetValueOrDefault("phone"));
                    break;

                case QRType.SMS:
                    AddRow("Number", payload.Data.GetValueOrDefault("phone"));
                    AddRow("Message", payload.Data.GetValueOrDefault("message"));
                    break;

                case QRType.GeoLocation:
                    AddRow("Latitude", payload.Data.GetValueOrDefault("latitude"));
                    AddRow("Longitude", payload.Data.GetValueOrDefault("longitude"));
                    AddRow("Location", payload.Data.GetValueOrDefault("location"));
                    break;

                case QRType.Calendar:
                    AddRow("Title", payload.Data.GetValueOrDefault("title"));
                    AddRow("Start", payload.Data.GetValueOrDefault("startdate"));
                    AddRow("End", payload.Data.GetValueOrDefault("enddate"));
                    AddRow("Location", payload.Data.GetValueOrDefault("location"));
                    AddRow("Description", payload.Data.GetValueOrDefault("description"));
                    break;

                case QRType.VCard:
                    AddRow("Name", payload.Data.GetValueOrDefault("name"));
                    AddRow("Phone", payload.Data.GetValueOrDefault("phone"));
                    AddRow("Email", payload.Data.GetValueOrDefault("email"));
                    AddRow("Organization", payload.Data.GetValueOrDefault("organization"));
                    AddRow("URL", payload.Data.GetValueOrDefault("url"), link: true);
                    break;

                case QRType.UPI:
                    AddRow("UPI ID", payload.Data.GetValueOrDefault("upiid"));
                    AddRow("Payee", payload.Data.GetValueOrDefault("name"));
                    AddRow("Amount", payload.Data.GetValueOrDefault("amount"));
                    AddRow("Note", payload.Data.GetValueOrDefault("description"));
                    break;

                case QRType.SocialProfile:
                    AddRow("Platform", payload.Data.GetValueOrDefault("platform"));
                    AddRow("Username", payload.Data.GetValueOrDefault("username"));
                    AddRow("URL", payload.Data.GetValueOrDefault("url"), link: true);
                    break;

                default:
                    foreach (var kv in payload.Data)
                    {
                        AddRow(kv.Key, kv.Value);
                    }
                    break;
            }

            layout.Controls.Add(infoPanel, 0, 0);

            // Close button
            var closeBtn = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 32,
                DialogResult = DialogResult.OK
            };
            layout.Controls.Add(closeBtn, 0, 1);

            form.Controls.Add(layout);
            form.ShowDialog();
        }
    }
}
