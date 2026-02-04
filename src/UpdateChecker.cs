using System;
using System.Diagnostics;
using System.Globalization;
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

                    if (!TryParseRemote(json, out var remoteVersion, out var downloadUrl, out var updateNotes))
                    {
                        return;
                    }

                    if (!IsNewer(remoteVersion, AppInfo.CurrentVersion))
                    {
                        return;
                    }

                    if (hasPrompted)
                    {
                        return;
                    }

                    hasPrompted = true;

                    if (owner == null || owner.IsDisposed)
                    {
                        return;
                    }

                    owner.BeginInvoke(new Action(() => ShowUpdatePrompt(owner, remoteVersion, downloadUrl, updateNotes)));
                }
                catch
                {
                    // Fail silently on any error.
                }
            });
        }

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
            }
        }

        private sealed class UpdatePromptForm : Form
        {
            private readonly Label notesLabel;

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
                };

                var subtitle = new Label
                {
                    Text = "A newer version is available.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(16, 44)
                };

                notesLabel = new Label
                {
                    Text = string.Empty,
                    AutoSize = false,
                    Location = new System.Drawing.Point(16, 72),
                    Size = new System.Drawing.Size(388, 60)
                };

                var instruction = new Label
                {
                    Text = "Please go to below and download exe file.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(16, 136)
                };

                var downloadButton = new Button
                {
                    Text = "Download Update",
                    Width = 140,
                    Height = 30,
                    Location = new System.Drawing.Point(16, 168)
                };
                downloadButton.Click += (_, __) =>
                {
                    OpenUrl(downloadUrl);
                    Close();
                };

                var laterButton = new Button
                {
                    Text = "Later",
                    Width = 80,
                    Height = 30,
                    Location = new System.Drawing.Point(170, 168)
                };
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
                    notesLabel.Text = string.Empty;
                    return;
                }

                notesLabel.Text = notes;
            }
        }
    }
}
