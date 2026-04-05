using System;
using System.Diagnostics;
<<<<<<< HEAD
=======
using System.Globalization;
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransparentClock
{
    public static class UpdateChecker
    {
        private const string VersionUrl = "https://clock.qlynk.me/version.json";
        private static bool hasChecked;
        private static bool hasPrompted;

        public static void CheckForUpdatesAsync(Form owner)
        {
            if (hasChecked)
            {
                return;
            }

            hasChecked = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(5)
                    };

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
                    using var response = await httpClient.GetAsync(VersionUrl, cts.Token).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return;
                    }

<<<<<<< HEAD
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("version", out var versionEl) ||
                        !root.TryGetProperty("downloadUrl", out var downloadUrlEl))
=======
                    if (!TryParseRemote(json, out var remoteVersion, out var downloadUrl, out var updateNotes))
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                    {
                        return;
                    }

<<<<<<< HEAD
                    string latestVersion = versionEl.GetString() ?? string.Empty;
                    string downloadUrl = downloadUrlEl.GetString() ?? string.Empty;
                    string releaseNotes = root.TryGetProperty("releaseNotes", out var notesEl)
                        ? notesEl.GetString() ?? string.Empty
                        : string.Empty;

                    // Compares the dynamic text version (e.g. V05.04.2026) directly
                    string currentVersion = AppInfo.CurrentVersion;
                    if (string.Equals(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
=======
                    if (!IsNewer(remoteVersion, AppInfo.CurrentVersion))
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                    {
                        return;
                    }

                    if (hasPrompted)
                    {
                        return;
                    }

                    hasPrompted = true;

<<<<<<< HEAD
                    owner.Invoke(new Action(() =>
                    {
                        using var prompt = new UpdatePromptForm(latestVersion, downloadUrl);
                        prompt.SetNotes(releaseNotes);
                        prompt.ShowDialog(owner);
                    }));
                }
                catch
                {
                    // Fail silently in background
=======
                    if (owner == null || owner.IsDisposed)
                    {
                        return;
                    }

                    owner.BeginInvoke(new Action(() => ShowUpdatePrompt(owner, remoteVersion, downloadUrl, updateNotes)));
                }
                catch
                {
                    // Fail silently on any error.
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                }
            });
        }

<<<<<<< HEAD
        private static void OpenUrl(string url)
        {
=======
        private static void ShowUpdatePrompt(Form owner, string remoteVersion, string downloadUrl, string updateNotes)
        {
            try
            {
                var form = new UpdatePromptForm(remoteVersion, downloadUrl)
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                form.SetNotes(updateNotes);
                form.Show(owner);
            }
            catch
            {
                // Fail silently.
            }
        }

        private static bool TryParseRemote(string json, out string remoteVersion, out string downloadUrl, out string updateNotes)
        {
            remoteVersion = string.Empty;
            downloadUrl = string.Empty;
            updateNotes = string.Empty;

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("latestVersion", out var versionElement))
                {
                    remoteVersion = versionElement.GetString() ?? string.Empty;
                }
                else if (root.TryGetProperty("latest_version", out var legacyVersion))
                {
                    remoteVersion = legacyVersion.GetString() ?? string.Empty;
                }
                else if (root.TryGetProperty("version", out var olderVersion))
                {
                    remoteVersion = olderVersion.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("releaseNotesUrl", out var releaseNotesElement))
                {
                    downloadUrl = releaseNotesElement.GetString() ?? string.Empty;
                }
                else if (root.TryGetProperty("downloadUrl", out var downloadElement))
                {
                    downloadUrl = downloadElement.GetString() ?? string.Empty;
                }
                else if (root.TryGetProperty("download_url", out var legacyDownload))
                {
                    downloadUrl = legacyDownload.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("updateNotes", out var notesElement))
                {
                    updateNotes = notesElement.GetString() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(remoteVersion) || string.IsNullOrWhiteSpace(downloadUrl))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsNewer(string remoteVersion, string localVersion)
        {
            if (!TryParseVersionDate(remoteVersion, out var remoteDate))
            {
                return false;
            }

            if (!TryParseVersionDate(localVersion, out var localDate))
            {
                return false;
            }

            return remoteDate > localDate;
        }

        private static bool TryParseVersionDate(string version, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            string trimmed = version.Trim();
            if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(1);
            }

            return DateTime.TryParseExact(
                trimmed,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
<<<<<<< HEAD
                // Fallback or ignore
=======
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
            }
        }

        private sealed class UpdatePromptForm : Form
        {
            private readonly Label notesLabel;

<<<<<<< HEAD
            public UpdatePromptForm(string latestVersion, string downloadUrl)
            {
                Text = "Mission Update Available";
                Size = new System.Drawing.Size(420, 260);
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250); // Premium light background
                Font = new System.Drawing.Font("Segoe UI", 9.5F);

                var title = new Label
                {
                    Text = $"New Tactical Update: {latestVersion}",
                    Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(33, 37, 41),
                    AutoSize = true,
                    Location = new System.Drawing.Point(15, 15)
=======
            public UpdatePromptForm(string remoteVersion, string downloadUrl)
            {
                Text = AppInfo.AppName;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new System.Drawing.Size(420, 220);

                var title = new Label
                {
                    Text = $"New update available ({remoteVersion})",
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                    Location = new System.Drawing.Point(16, 16)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                };

                var subtitle = new Label
                {
<<<<<<< HEAD
                    Text = "A new version of the War-Room Tracker is ready for deployment.",
                    AutoSize = true,
                    ForeColor = System.Drawing.Color.FromArgb(80, 80, 80),
                    Location = new System.Drawing.Point(18, 45)
=======
                    Text = "A newer version is available.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(16, 44)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                };

                notesLabel = new Label
                {
<<<<<<< HEAD
                    Location = new System.Drawing.Point(18, 75),
                    Size = new System.Drawing.Size(370, 50),
                    ForeColor = System.Drawing.Color.FromArgb(0, 102, 204),
                    Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic)
=======
                    Text = string.Empty,
                    AutoSize = false,
                    Location = new System.Drawing.Point(16, 72),
                    Size = new System.Drawing.Size(388, 60)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                };

                var instruction = new Label
                {
<<<<<<< HEAD
                    Text = "Click below to download the latest setup file.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 140)
=======
                    Text = "Please go to below and download exe file.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(16, 136)
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                };

                var downloadButton = new Button
                {
                    Text = "Download Update",
<<<<<<< HEAD
                    Width = 150,
                    Height = 40,
                    Location = new System.Drawing.Point(20, 170),
                    BackColor = System.Drawing.Color.FromArgb(0, 123, 255),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                downloadButton.FlatAppearance.BorderSize = 0;
=======
                    Width = 140,
                    Height = 30,
                    Location = new System.Drawing.Point(16, 168)
                };
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                downloadButton.Click += (_, __) =>
                {
                    OpenUrl(downloadUrl);
                    Close();
                };

                var laterButton = new Button
                {
<<<<<<< HEAD
                    Text = "Dismiss",
                    Width = 90,
                    Height = 40,
                    Location = new System.Drawing.Point(180, 170),
                    BackColor = System.Drawing.Color.FromArgb(220, 220, 220),
                    ForeColor = System.Drawing.Color.Black,
                    Font = new System.Drawing.Font("Segoe UI", 10F),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                laterButton.FlatAppearance.BorderSize = 0;
=======
                    Text = "Later",
                    Width = 80,
                    Height = 30,
                    Location = new System.Drawing.Point(170, 168)
                };
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                laterButton.Click += (_, __) => Close();

                Controls.Add(title);
                Controls.Add(subtitle);
                Controls.Add(notesLabel);
                Controls.Add(instruction);
                Controls.Add(downloadButton);
                Controls.Add(laterButton);
            }

            public void SetNotes(string notes)
            {
                if (string.IsNullOrWhiteSpace(notes))
                {
<<<<<<< HEAD
                    notesLabel.Text = "Standard performance improvements and bug fixes.";
=======
                    notesLabel.Text = string.Empty;
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
                    return;
                }

                notesLabel.Text = notes;
            }
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> fb78e9a755f0d7248d3204a59be8ab7f18eca15a
