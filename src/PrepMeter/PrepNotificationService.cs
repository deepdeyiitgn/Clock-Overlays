using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Service for managing smart, anti-spam Windows notifications for the Prep Meter feature.
    /// Provides contextual reminders based on time of day and user commitments.
    /// </summary>
    public static class PrepNotificationService
    {
        private static System.Windows.Forms.Timer? _timer;
        private static NotifyIcon? _notifyIcon;
        private static NotificationState _state;

        private static readonly string StateFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TransparentClock", "PrepNotificationState.json");

        // Time windows (24-hour format)
        private const int MorningStartHour = 8;
        private const int MorningEndHour = 8;
        private const int EveningStartHour = 20;
        private const int EveningEndHour = 20;
        private const int NightStartHour = 22;
        private const int NightStartMinute = 30;
        private const int NightEndHour = 23;
        private const int NightEndMinute = 59;

        static PrepNotificationService()
        {
            LoadState();
        }

        /// <summary>
        /// Starts the notification service with timer and system tray icon.
        /// Call this method from the main application entry point.
        /// </summary>
        public static void Start()
        {
            if (_timer != null)
                return; // Already started

            try
            {
                // Initialize notify icon
                _notifyIcon = new NotifyIcon
                {
                    Icon = SystemIcons.Information, // Use system icon as fallback
                    Visible = false, // Don't show in system tray permanently
                    BalloonTipTitle = "Prep Meter"
                };

                // Try to use app icon if available
                try
                {
                    var appIcon = Program.GetAppIcon();
                    if (appIcon != null)
                    {
                        _notifyIcon.Icon = appIcon;
                    }
                }
                catch
                {
                    // Keep system icon as fallback
                }

                // Initialize timer (1 minute intervals)
                _timer = new System.Windows.Forms.Timer
                {
                    Interval = 60000 // 1 minute
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();

                // Check immediately on start
                CheckAndSendNotifications();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                Console.WriteLine($"Failed to start Prep Notification Service: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the notification service and cleans up resources.
        /// </summary>
        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }

        private static void Timer_Tick(object? sender, EventArgs e)
        {
            CheckAndSendNotifications();
        }

        private static void CheckAndSendNotifications()
        {
            DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;

            // Morning notification (8:00-8:59 AM)
            if (now.Hour == MorningStartHour && now.Minute >= 0 && now.Minute <= 59)
            {
                if (_state.LastMorningDate != today)
                {
                    SendMorningNotification();
                    _state.LastMorningDate = today;
                    SaveState();
                }
            }
            // Evening notification (8:00-8:59 PM)
            else if (now.Hour == EveningStartHour && now.Minute >= 0 && now.Minute <= 59)
            {
                if (_state.LastEveningDate != today)
                {
                    SendEveningNotification();
                    _state.LastEveningDate = today;
                    SaveState();
                }
            }
            // Night notification (10:30 PM - 11:59 PM)
            else if ((now.Hour == NightStartHour && now.Minute >= NightStartMinute) ||
                     (now.Hour == NightEndHour && now.Minute <= NightEndMinute))
            {
                if (_state.LastNightDate != today)
                {
                    SendNightNotification();
                    _state.LastNightDate = today;
                    SaveState();
                }
            }
        }

        private static void SendMorningNotification()
        {
            try
            {
                var latestCommitment = PrepMeterStorage.LoadLatestCommitment();
                string goalText = latestCommitment?.TomorrowGoal ?? "Review your study plan and set clear goals";

                string message = $"🌅 Mission Today: {goalText}. Execution starts now.";

                ShowNotification("Good Morning!", message, ToolTipIcon.Info, 10000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send morning notification: {ex.Message}");
            }
        }

        private static void SendEveningNotification()
        {
            const string message = "⏳ Day is ending. Push for the final hours!";
            ShowNotification("Evening Push", message, ToolTipIcon.Warning, 8000);
        }

        private static void SendNightNotification()
        {
            const string message = "📓 Time to log your Commitment Diary. Maintain the streak!";
            ShowNotification("Night Check-in", message, ToolTipIcon.Info, 12000);
        }

        private static void ShowNotification(string title, string message, ToolTipIcon icon, int durationMs)
        {
            if (_notifyIcon == null)
                return;

            try
            {
                _notifyIcon.BalloonTipTitle = title;
                _notifyIcon.BalloonTipText = message;
                _notifyIcon.BalloonTipIcon = icon;
                _notifyIcon.ShowBalloonTip(durationMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to show notification: {ex.Message}");
            }
        }

        private static void LoadState()
        {
            try
            {
                if (File.Exists(StateFilePath))
                {
                    string json = File.ReadAllText(StateFilePath);
                    var loadedState = JsonSerializer.Deserialize<NotificationState>(json);
                    if (loadedState != null)
                    {
                        _state = loadedState;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load notification state: {ex.Message}");
            }

            // Initialize with default state
            _state = new NotificationState();
        }

        private static void SaveState()
        {
            try
            {
                string directory = Path.GetDirectoryName(StateFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(_state, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(StateFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save notification state: {ex.Message}");
            }
        }

        /// <summary>
        /// Internal class to track notification state for anti-spam logic.
        /// </summary>
        private class NotificationState
        {
            public DateTime LastMorningDate { get; set; } = DateTime.MinValue;
            public DateTime LastEveningDate { get; set; } = DateTime.MinValue;
            public DateTime LastNightDate { get; set; } = DateTime.MinValue;
        }
    }
}
