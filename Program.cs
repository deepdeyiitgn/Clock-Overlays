// Check if the application is already running
using System.Threading;

static void Main(string[] args)
{
    using (Mutex mutex = new Mutex(false, "TransparentClockAppMutex"))
    {
        if (!mutex.WaitOne(0, false))
        {
            // App is already running, exit
            return;
        }

        // Show splash screen if this is a fresh launch
        if (IsFreshLaunch())
        {
            using (var splash = new SplashForm())
            {
                splash.Show();
                Thread.Sleep(6000); // Show for ~5-7 seconds
                splash.Close();
            }
        }

        // Proceed to show WelcomeForm or Dashboard
        Application.Run(new WelcomeForm());
    }
}

private static bool IsFreshLaunch()
{
    // Logic to determine if this is a fresh launch
    // This could involve checking a flag in AppState or similar
    return true; // Placeholder for actual logic
}