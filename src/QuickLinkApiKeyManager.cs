using System;
using System.IO;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Manages QuickLink API key storage and retrieval.
    /// Stores API key in plain text in the local application data folder.
    /// Suitable for personal use; not recommended for multi-user or enterprise scenarios.
    /// </summary>
    public static class QuickLinkSettings
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays"
        );

        private static readonly string ApiKeyFilePath = Path.Combine(AppDataPath, "quicklink_api_key.txt");

        /// <summary>
        /// Retrieves the stored QuickLink API key.
        /// </summary>
        /// <returns>The stored API key, or empty string if not found.</returns>
        public static string GetApiKey()
        {
            try
            {
                if (!File.Exists(ApiKeyFilePath))
                    return string.Empty;

                string key = File.ReadAllText(ApiKeyFilePath).Trim();
                return key;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Stores the QuickLink API key locally.
        /// Creates the application data directory if it does not exist.
        /// </summary>
        /// <param name="apiKey">The API key to store. If null or empty, deletes the stored key.</param>
        public static void SetApiKey(string apiKey)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    // Delete the file if clearing the key
                    if (File.Exists(ApiKeyFilePath))
                        File.Delete(ApiKeyFilePath);
                }
                else
                {
                    // Store the key
                    File.WriteAllText(ApiKeyFilePath, apiKey.Trim());
                }
            }
            catch
            {
                // Silently fail on write errors
            }
        }

        /// <summary>
        /// Clears the stored API key.
        /// </summary>
        public static void ClearApiKey()
        {
            SetApiKey(string.Empty);
        }
    }

    /// <summary>
    /// WinForms panel for managing QuickLink API key configuration.
    /// Provides TextBox input, buttons to fetch/view API documentation,
    /// and integrates with QuickLinkSettings for persistence.
    /// </summary>
    public static class QuickLinkApiKeyPanel
    {
        /// <summary>
        /// Creates a configured Panel with API key management controls.
        /// 
        /// The panel contains:
        /// - Title label "QuickLink API Configuration"
        /// - Description text
        /// - TextBox for API key input (auto-populated from settings)
        /// - "Get API Key" button (opens https://qlynk.vercel.app/api-access)
        /// - "API Docs" LinkLabel (opens https://qlynk.vercel.app/api/v1/st)
        /// - Save button to persist the API key
        /// </summary>
        /// <returns>A configured Panel ready to add to a form or container.</returns>
        public static Panel CreateApiKeyPanel()
        {
            const int panelWidth = 500;
            const int panelHeight = 250;

            var panel = new Panel
            {
                Width = panelWidth,
                Height = panelHeight,
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Title label
            var titleLabel = new Label
            {
                Text = "QuickLink API Configuration",
                Location = new System.Drawing.Point(12, 12),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64)
            };
            panel.Controls.Add(titleLabel);

            // Description label
            var descriptionLabel = new Label
            {
                Text = "Enter your QuickLink API key to shorten URLs. Get one at the link below.",
                Location = new System.Drawing.Point(12, 36),
                Width = panelWidth - 36,
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.FromArgb(110, 110, 110)
            };
            panel.Controls.Add(descriptionLabel);

            // API Key label
            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                Location = new System.Drawing.Point(12, 70),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
            };
            panel.Controls.Add(apiKeyLabel);

            // API Key TextBox
            var apiKeyTextBox = new TextBox
            {
                Location = new System.Drawing.Point(12, 92),
                Width = panelWidth - 36,
                Height = 28,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                UseSystemPasswordChar = true
            };
            
            // Load stored API key
            string storedKey = QuickLinkSettings.GetApiKey();
            if (!string.IsNullOrEmpty(storedKey))
                apiKeyTextBox.Text = storedKey;

            panel.Controls.Add(apiKeyTextBox);

            // "Get API Key" button
            var getKeyButton = new Button
            {
                Text = "Get API Key",
                Location = new System.Drawing.Point(12, 128),
                Width = 110,
                Height = 28,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            getKeyButton.Click += (sender, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://qlynk.vercel.app/api-access",
                            UseShellExecute = true
                        }
                    );
                }
                catch
                {
                    MessageBox.Show(
                        "Failed to open browser. Please visit: https://qlynk.vercel.app/api-access",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
            panel.Controls.Add(getKeyButton);

            // "API Docs" LinkLabel
            var apiDocsLink = new LinkLabel
            {
                Text = "View API Docs",
                Location = new System.Drawing.Point(128, 133),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                LinkColor = System.Drawing.Color.FromArgb(0, 102, 204),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            apiDocsLink.LinkClicked += (sender, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://qlynk.vercel.app/api/v1/st",
                            UseShellExecute = true
                        }
                    );
                }
                catch
                {
                    MessageBox.Show(
                        "Failed to open browser. Please visit: https://qlynk.vercel.app/api/v1/st",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
            panel.Controls.Add(apiDocsLink);

            // Save button
            var saveButton = new Button
            {
                Text = "Save API Key",
                Location = new System.Drawing.Point(12, 168),
                Width = 110,
                Height = 28,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            saveButton.Click += (sender, e) =>
            {
                string apiKey = apiKeyTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    MessageBox.Show(
                        "API key cannot be empty.",
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                QuickLinkSettings.SetApiKey(apiKey);
                MessageBox.Show(
                    "API key saved successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            panel.Controls.Add(saveButton);

            // Clear button
            var clearButton = new Button
            {
                Text = "Clear",
                Location = new System.Drawing.Point(128, 168),
                Width = 80,
                Height = 28,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = System.Windows.Forms.Cursors.Hand,
                ForeColor = System.Drawing.Color.FromArgb(192, 0, 0)
            };
            clearButton.Click += (sender, e) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear the stored API key?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    QuickLinkSettings.ClearApiKey();
                    apiKeyTextBox.Clear();
                    MessageBox.Show(
                        "API key cleared.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            };
            panel.Controls.Add(clearButton);

            // Status label
            var statusLabel = new Label
            {
                Text = storedKey != string.Empty ? "✓ API key is configured" : "✗ No API key configured",
                Location = new System.Drawing.Point(12, 210),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic),
                ForeColor = storedKey != string.Empty 
                    ? System.Drawing.Color.FromArgb(0, 128, 0)
                    : System.Drawing.Color.FromArgb(192, 0, 0)
            };
            panel.Controls.Add(statusLabel);

            return panel;
        }

        /// <summary>
        /// Validates that an API key is configured before making API requests.
        /// Shows an error message if no key is found.
        /// </summary>
        /// <returns>True if API key is configured; false otherwise.</returns>
        public static bool ValidateApiKeyConfigured()
        {
            string apiKey = QuickLinkSettings.GetApiKey();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show(
                    "No QuickLink API key configured. Please enter your API key in Settings.",
                    "API Key Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            return true;
        }
    }
}
