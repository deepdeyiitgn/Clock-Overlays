using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TransparentClock
{
    internal static class Program
    {
        public static AppState CurrentState { get; private set; } = AppState.CreateDefault();
        private static System.Threading.Timer? pomodoroAutoSaveTimer;
        public static TransparentClockForm? ClockForm { get; private set; }
        public static DashboardForm? DashboardForm { get; private set; }

        public static readonly IReadOnlyDictionary<string, Color> ClockColors =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                ["White"] = Color.White,
                ["Soft Black"] = Color.FromArgb(30, 30, 30),
                ["Red"] = Color.Red,
                ["Green"] = Color.LimeGreen,
                ["Blue"] = Color.DeepSkyBlue,
                ["Pink"] = Color.HotPink,
                ["Yellow"] = Color.Gold
            };

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            InitializeAppState();
            Application.Run(new AppContext());
        }

        private static void InitializeAppState()
        {
            CurrentState = AppStateStorage.Load();
            CurrentState.Pomodoro ??= PomodoroState.CreateDefault();
            CurrentState.Pomodoro.RestoreAfterLoad(DateTime.UtcNow, CurrentState.PomodoroSettings);

            bool profileHasData = !string.IsNullOrWhiteSpace(CurrentState.UserName)
                && !string.IsNullOrWhiteSpace(CurrentState.Gender);

            if (profileHasData)
            {
                CurrentState.IsProfileCompleted = true;
            }

            if (CurrentState.IsProfileCompleted)
            {
                CurrentState.IsFirstRun = false;
                CurrentState.ShowWelcomeOnStartup = false;
            }

            CurrentState.LastAppLaunch = DateTime.UtcNow;

            pomodoroAutoSaveTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    if (CurrentState.Pomodoro != null && CurrentState.Pomodoro.IsRunning)
                    {
                        AppStateStorage.Save(CurrentState);
                    }
                }
                catch
                {
                    // Fail silently to avoid UI impact.
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Application.ApplicationExit += (_, __) =>
            {
                AppStateStorage.Save(CurrentState);
            };
        }

        public static void ShowMainForm()
        {
            EnsureMainFormCreated();

            if (DashboardForm != null)
            {
                DashboardForm.Show();
                DashboardForm.BringToFront();
            }

            ApplyClockStateToOverlay();
        }

        public static void ToggleDashboard()
        {
            EnsureMainFormCreated();

            if (DashboardForm == null)
            {
                return;
            }

            if (DashboardForm.Visible)
            {
                DashboardForm.Hide();
            }
            else
            {
                DashboardForm.Show();
                DashboardForm.BringToFront();
            }
        }

        public static void ShowPomodoro()
        {
            PomodoroForm? pomodoro = new PomodoroForm();
            pomodoro.Show();
            pomodoro.BringToFront();
        }

        public static void ExitApplication()
        {
            try
            {
                AppStateStorage.Save(CurrentState);
            }
            catch
            {
                // Fail silently to avoid UI impact.
            }

            if (DashboardForm != null && !DashboardForm.IsDisposed)
            {
                DashboardForm.RequestClose();
            }

            if (ClockForm != null && !ClockForm.IsDisposed)
            {
                ClockForm.Close();
                ClockForm.Dispose();
            }

            Application.Exit();
        }

        public static void RegisterClockForm(TransparentClockForm form)
        {
            ClockForm = form;
        }

        public static Color ResolveClockColor(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name) && ClockColors.TryGetValue(name, out var color))
            {
                return color;
            }

            return Color.White;
        }

        public static void ApplyClockStateToOverlay()
        {
            if (ClockForm == null || ClockForm.IsDisposed)
            {
                return;
            }

            ClockForm.ApplyClockColor(ResolveClockColorFromState());
            ClockForm.ApplyClockFontFamily(CurrentState.ClockFontFamily);
            ClockForm.ApplyClockFontSize(CurrentState.ClockFontSize);
            if (CurrentState.ClockUseCustomPosition &&
                CurrentState.ClockCustomPositionX.HasValue &&
                CurrentState.ClockCustomPositionY.HasValue)
            {
                ClockForm.ApplyClockCustomPosition(
                    CurrentState.ClockCustomPositionX.Value,
                    CurrentState.ClockCustomPositionY.Value);
            }
            else
            {
                ClockForm.ApplyClockPosition(CurrentState.ClockPosition);
            }
            ClockForm.ApplyClockEnabled(CurrentState.ClockEnabled);
        }

        public static Color ResolveClockColorFromState()
        {
            if (CurrentState.ClockUseCustomColor && CurrentState.ClockCustomColorArgb.HasValue)
            {
                return Color.FromArgb(CurrentState.ClockCustomColorArgb.Value);
            }

            return ResolveClockColor(CurrentState.ClockColorName);
        }

        public static string GetGreetingText()
        {
            int hour = DateTime.Now.Hour;
            string greeting = hour switch
            {
                >= 5 and <= 11 => "Good Morning",
                >= 12 and <= 16 => "Good Afternoon",
                >= 17 and <= 20 => "Good Evening",
                _ => "Good Night"
            };

            if (!string.IsNullOrWhiteSpace(CurrentState.UserName))
            {
                return $"{greeting}, {CurrentState.UserName}";
            }

            return greeting;
        }

        private sealed class AppContext : ApplicationContext
        {
            public AppContext()
            {
                if (CurrentState.IsFirstRun)
                {
                    var splash = new SplashForm();
                    MainForm = splash;

                    splash.FormClosed += (_, __) =>
                    {
                        using var welcome = new WelcomeForm();
                        welcome.ShowDialog();

                        if (!string.IsNullOrWhiteSpace(CurrentState.UserName) &&
                            !string.IsNullOrWhiteSpace(CurrentState.Gender))
                        {
                            CurrentState.IsProfileCompleted = true;
                            CurrentState.IsFirstRun = false;
                            CurrentState.ShowWelcomeOnStartup = false;
                            AppStateStorage.Save(CurrentState);
                        }

                        ShowMainForm();
                        if (DashboardForm != null)
                        {
                            MainForm = DashboardForm;
                            DashboardForm.RefreshGreeting();
                        }
                    };

                    splash.Show();
                }
                else
                {
                    ShowMainForm();
                    if (DashboardForm != null)
                    {
                        MainForm = DashboardForm;
                    }

                }
            }
        }


        private static void EnsureMainFormCreated()
        {
            if (DashboardForm == null || DashboardForm.IsDisposed)
            {
                DashboardForm = new DashboardForm();
            }
        }
    }
}
