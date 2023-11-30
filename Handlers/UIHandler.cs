using Rage;
using SimpleCTRL.UI;
using SimpleCTRL.Utils;

namespace SimpleCTRL.Handlers
{
    internal static class UIHandler
    {
        private static bool IsRunning = false;

        public static void Start()
        {
            Logging.Info("starting...", "UIHandler");
            SettingsMenuUI.Initialize();
            GameFiber.StartNew(delegate { Run(); });
        }

        private static void Run()
        {
            IsRunning = true;
            while (IsRunning)
            {
                GameFiber.Yield();
                VehicleUI.Start();
                SettingsMenuUI.ProcessMenus();
            }
        }
    }
}
