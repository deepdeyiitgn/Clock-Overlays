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
                Width = 560,
                Height = 580,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12)
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

            // Input fields container (will be updated dynamically)
            var fieldsPanel = new Panel
            {
                Location = new Point(12, 85),
                Width = 536,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            card.Controls.Add(fieldsPanel);

            // Buttons area
            var generateBtn = new Button
            {
                Text = "Generate QR",
                Location = new Point(12, 290),
                Width = 120,
                Height = 36,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(generateBtn);

            var clearBtn = new Button
            {
                Text = "Clear",
                Location = new Point(140, 290),
                Width = 80,
                Height = 36,
                Font = new Font("Segoe UI", 9F)
            };
            card.Controls.Add(clearBtn);

            // Preview
            var previewLabel = new Label
            {
                Text = "Preview:",
                AutoSize = true,
                Location = new Point(12, 335),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(previewLabel);

            var previewBox = new PictureBox
            {
                Location = new Point(12, 355),
                Width = 140,
                Height = 140,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.White
            };
            card.Controls.Add(previewBox);

            // Export format label
            var formatLabel = new Label
            {
                Text = "Format:",
                AutoSize = true,
                Location = new Point(165, 335),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            card.Controls.Add(formatLabel);

            // Export format dropdown
            var formatCombo = new ComboBox
            {
                Location = new Point(165, 355),
                Width = 120,
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
                Location = new Point(165, 385),
                Width = 120,
                Height = 32,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            card.Controls.Add(exportBtn);

            // Message
            var messageLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Location = new Point(12, 500),
                Font = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.Highlight,
                MaximumSize = new Size(536, 70)
            };
            card.Controls.Add(messageLabel);

            // State
            Bitmap? currentQRImage = null;

            // Dynamic fields function
            Action updateFields = () =>
            {
                fieldsPanel.Controls.Clear();
                var selectedType = (QRType)typeCombo.SelectedItem;

                int yPos = 8;
                TextBox[] inputs = Array.Empty<TextBox>();
                Label[]  labels = Array.Empty<Label>();
                ComboBox? socialModeCombo = null;
                ComboBox? socialPlatformCombo = null;

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
                            new TextBox { Width = 500, Height = 24, Text = "+1" },
                            new TextBox { Width = 500, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical }
                        };
                        break;

                    case QRType.WiFi:
                        labels = new[] {
                            new Label { Text = "SSID:", AutoSize = true },
                            new Label { Text = "Password:", AutoSize = true },
                            new Label { Text = "Security:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24, UseSystemPasswordChar = true },
                            new TextBox { Width = 500, Height = 24, Text = "WPA" }
                        };
                        break;

                    case QRType.Calendar:
                        labels = new[] {
                            new Label { Text = "Title:", AutoSize = true },
                            new Label { Text = "Date/Time (YYYY-MM-DD HH:MM):", AutoSize = true },
                            new Label { Text = "Location:", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24 },
                            new TextBox { Width = 500, Height = 24, Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
                            new TextBox { Width = 500, Height = 24 }
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
                            new Label { Text = "Amount (₹):", AutoSize = true }
                        };
                        inputs = new[] {
                            new TextBox { Width = 500, Height = 24, Text = "user@bank" },
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
                            "Snapchat", "GitHub", "Telegram", "WhatsApp"
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
                fieldsPanel.Tag = (inputs, socialModeCombo, socialPlatformCombo);
            };

            typeCombo.SelectedIndexChanged += (_, __) => updateFields();
            updateFields(); // Initial setup

            clearBtn.Click += (s, e) =>
            {
                var (inputs, _, __) = ((TextBox[], ComboBox?, ComboBox?))fieldsPanel.Tag;
                foreach (var input in inputs)
                    input.Clear();
                messageLabel.Text = "";
                previewBox.Image?.Dispose();
                previewBox.Image = null;
                exportBtn.Enabled = false;
            };

            generateBtn.Click += async (s, e) =>
            {
                try
                {
                    var selectedType = (QRType)typeCombo.SelectedItem;
                    var (inputs, socialModeCombo, socialPlatformCombo) = ((TextBox[], ComboBox?, ComboBox?))fieldsPanel.Tag;

                    // Validate inputs
                    var qrData = BuildQRPayload(selectedType, inputs, socialModeCombo, socialPlatformCombo);
                    if (qrData == null)
                    {
                        messageLabel.Text = "❌ Please fill in all required fields correctly";
                        messageLabel.ForeColor = Color.Red;
                        return;
                    }

                    generateBtn.Enabled = false;
                    messageLabel.Text = "⏳ Generating QR with HIGH error correction...";
                    messageLabel.ForeColor = SystemColors.Highlight;

                    currentQRImage = await Task.Run(() =>
                    {
                        // Generate with HIGH ECC when logo is present
                        return QRCodeGeneratorWithLogo.GenerateQRCodeWithLogo(
                            qrData,
                            useDefaultLogo: logoCheckBox.Checked,
                            pixelsPerModule: 8,
                            eccLevel: logoCheckBox.Checked ? "H" : "M");
                    });

                    previewBox.Image = currentQRImage;
                    exportBtn.Enabled = true;
                    messageLabel.Text = $"✓ {selectedType} QR generated" + (logoCheckBox.Checked ? " with logo (HIGH ECC)" : "");
                    messageLabel.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    messageLabel.Text = $"❌ {ex.Message}";
                    messageLabel.ForeColor = Color.Red;
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
                    formatCombo.SelectedItem.ToString() ?? "PNG");

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
                            messageLabel.Text = $"⏳ Exporting as {format}...";
                            messageLabel.ForeColor = SystemColors.Highlight;

                            await Task.Run(() =>
                            {
                                QRExporter.ExportQRCode(currentQRImage, sfd.FileName, format);
                            });

                            messageLabel.Text = $"✓ {Path.GetFileName(sfd.FileName)}";
                            messageLabel.ForeColor = Color.Green;
                        }
                        catch (Exception ex)
                        {
                            messageLabel.Text = $"❌ {ex.Message}";
                            messageLabel.ForeColor = Color.Red;
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

        private static QRPayload? BuildQRPayload(QRType type, TextBox[] inputs, ComboBox? socialMode, ComboBox? socialPlatform)
        {
            try
            {
                var payload = new QRPayload { Type = type };

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
                        var subj = inputs[1].Text.Replace("\n", "");
                        var body = inputs[2].Text.Replace("\n", "%0A");
                        payload.Data["text"] = $"mailto:{inputs[0].Text}?subject={subj}&body={body}";
                        break;

                    case QRType.Phone:
                        if (!QRFieldValidator.IsValidPhone(inputs[0].Text))
                            return null;
                        payload.Data["text"] = $"tel:{inputs[0].Text}";
                        break;

                    case QRType.SMS:
                        if (!QRFieldValidator.IsValidPhone(inputs[0].Text))
                            return null;
                        payload.Data["text"] = $"smsto:{inputs[0].Text}:{inputs[1].Text}";
                        break;

                    case QRType.WiFi:
                        if (!QRFieldValidator.IsValidSSID(inputs[0].Text))
                            return null;
                        var sec = inputs[2].Text.ToUpperInvariant();
                        var pwd = inputs[1].Text;
                        payload.Data["text"] = $"WIFI:T:{sec};S:{inputs[0].Text};P:{pwd};;";
                        break;

                    case QRType.Calendar:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        payload.Data["text"] = $"BEGIN:VEVENT\nSUMMARY:{inputs[0].Text}\nDTSTART:{inputs[1].Text}\nLOCATION:{inputs[2].Text}\nEND:VEVENT";
                        break;

                    case QRType.VCard:
                        if (!QRFieldValidator.IsValidText(inputs[0].Text))
                            return null;
                        payload.Data["text"] = $"BEGIN:VCARD\nFN:{inputs[0].Text}\nTEL:{inputs[1].Text}\nEMAIL:{inputs[2].Text}\nORG:{inputs[3].Text}\nEND:VCARD";
                        break;

                    case QRType.GeoLocation:
                        if (!QRFieldValidator.IsValidLatitude(inputs[0].Text) || !QRFieldValidator.IsValidLongitude(inputs[1].Text))
                            return null;
                        payload.Data["text"] = $"geo:{inputs[0].Text},{inputs[1].Text}";
                        break;

                    case QRType.UPI:
                        if (!QRFieldValidator.IsValidUPI(inputs[0].Text))
                            return null;
                        var amt = string.IsNullOrWhiteSpace(inputs[2].Text) ? "" : $"&am={inputs[2].Text}";
                        payload.Data["text"] = $"upi://pay?pa={inputs[0].Text}&pn={inputs[1].Text}{amt}";
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
                Margin = new Padding(0, 0, 0, 12)
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
                    statusLabel.Text = "⏳ Scanning...";
                    statusLabel.ForeColor = SystemColors.Highlight;

                    scannedData = QRCodeScanner.ScanQrFromImage(filePath);

                    if (string.IsNullOrEmpty(scannedData))
                    {
                        statusLabel.Text = QRCodeScanner.LastErrorMessage ?? "No QR code detected in this image";
                        statusLabel.ForeColor = Color.Red;
                        resultTextBox.Text = "";
                        copyBtn.Enabled = false;
                        previewBtn.Enabled = false;
                    }
                    else
                    {
                        resultTextBox.Text = scannedData;
                        var detectedType = QRTypeDetector.DetectQRType(scannedData);
                        statusLabel.Text = $"✓ QR scanned: {detectedType}";
                        statusLabel.ForeColor = Color.Green;
                        copyBtn.Enabled = true;
                        previewBtn.Enabled = true;
                    }
                }
                catch (Exception)
                {
                    statusLabel.Text = QRCodeScanner.LastErrorMessage ?? "No QR code detected in this image";
                    statusLabel.ForeColor = Color.Red;
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
                    statusLabel.Text = "✓ Copied to clipboard";
                    statusLabel.ForeColor = Color.Green;
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
                Width = 540,
                Height = 250,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12)
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
                Width = 516,
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

            var saveApiKeyBtn = new Button
            {
                Text = "Save",
                Location = new Point(460, 2),
                Width = 50,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            apiKeySetupPanel.Controls.Add(saveApiKeyBtn);

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
                Width = 516,
                Height = 150,
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
                Width = 516,
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
                Width = 250,
                Height = 24
            };
            shortenPanel.Controls.Add(aliasTextBox);

            var shortenBtn = new Button
            {
                Text = "Shorten",
                Location = new Point(260, 75),
                Width = 100,
                Height = 24,
                Font = new Font("Segoe UI", 9F)
            };
            shortenPanel.Controls.Add(shortenBtn);

            var logoutBtn = new Button
            {
                Text = "Change Key",
                Location = new Point(370, 75),
                Width = 100,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            shortenPanel.Controls.Add(logoutBtn);

            // Result panel
            var resultPanel = new Panel
            {
                Location = new Point(0, 105),
                Width = 516,
                Height = 45,
                Visible = false
            };
            shortenPanel.Controls.Add(resultPanel);

            var resultLabel = new Label
            {
                Text = "Short URL:",
                AutoSize = true,
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 8F)
            };
            resultPanel.Controls.Add(resultLabel);

            var resultTextBox = new TextBox
            {
                Location = new Point(0, 18),
                Width = 410,
                Height = 24,
                ReadOnly = true,
                Font = new Font("Courier New", 8F)
            };
            resultPanel.Controls.Add(resultTextBox);

            var copyResultBtn = new Button
            {
                Text = "Copy",
                Location = new Point(420, 18),
                Width = 96,
                Height = 24,
                Font = new Font("Segoe UI", 8F)
            };
            resultPanel.Controls.Add(copyResultBtn);

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
                    statusLabel.Text = "❌ Please enter API key";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                QuickLinkSettings.SetApiKey(apiKeyTextBox.Text);
                apiKeySetupPanel.Visible = false;
                shortenPanel.Visible = true;
                statusLabel.Text = "✓ API key saved";
                statusLabel.ForeColor = Color.Green;
            };

            shortenBtn.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(longUrlTextBox.Text))
                {
                    statusLabel.Text = "❌ Please enter a long URL";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                try
                {
                    shortenBtn.Enabled = false;
                    statusLabel.Text = "⏳ Shortening...";
                    statusLabel.ForeColor = SystemColors.Highlight;

                    var client = new QuickLinkShortenClient();
                    var result = await client.ShortenAsync(
                        apiKeyTextBox.Text,
                        longUrlTextBox.Text,
                        string.IsNullOrWhiteSpace(aliasTextBox.Text) ? null : aliasTextBox.Text
                    );

                    if (result.IsSuccess)
                    {
                        resultTextBox.Text = result.Success?.ShortenedUrl ?? "";
                        resultPanel.Visible = true;
                        statusLabel.Text = "✓ Short link created successfully 🎉";
                        statusLabel.ForeColor = Color.Green;
                    }
                    else
                    {
                        resultPanel.Visible = false;
                        statusLabel.Text = result.Error?.Message ?? "Failed to shorten URL";
                        statusLabel.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    statusLabel.Text = $"❌ Error: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                    resultPanel.Visible = false;
                }
                finally
                {
                    shortenBtn.Enabled = true;
                }
            };

            copyResultBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(resultTextBox.Text))
                {
                    Clipboard.SetText(resultTextBox.Text);
                    statusLabel.Text = "✓ Copied to clipboard";
                    statusLabel.ForeColor = Color.Green;
                }
            };

            logoutBtn.Click += (s, e) =>
            {
                QuickLinkSettings.SetApiKey("");
                apiKeyTextBox.Text = "";
                apiKeySetupPanel.Visible = true;
                shortenPanel.Visible = false;
                resultPanel.Visible = false;
                statusLabel.Text = "API key cleared";
                statusLabel.ForeColor = Color.Orange;
            };

            return card;
        }

        // ========================================
        // HELPERS
        // ========================================

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
            var infoPanel = new Panel { AutoSize = true };
            var infoText = "Detected Type: " + payload.Type + "\n\n";

            foreach (var kv in payload.Data)
            {
                infoText += $"{kv.Key}: {kv.Value}\n";
            }

            var infoLabel = new Label
            {
                Text = infoText,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                MaximumSize = new Size(460, 300),
                Padding = new Padding(6)
            };
            infoPanel.Controls.Add(infoLabel);

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
