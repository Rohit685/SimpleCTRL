namespace SimpleCTRL.Engine.InternalSystems
{
    internal class KeybindManager : CommonPlugin
    {
        public static void Start()
        {
            Logging.Info("Initializing input and keybind monitoring threads...", "KeybindManager");
            GameFiber.StartNew(Run, "SimpleCTRL - Keybind Monitor");
            Logging.Info("User input processing thread successfully initialized.", "KeybindManager");
            Logging.Info("Keybind monitoring thread started.", "KeybindManager");
        }

        public static void Run()
        {
            while (true)
            {
                MainLoop();
                GameFiber.Yield();
            }
        }

        private static void MainLoop()
        {
            // This section intentionally left blank.
        }
    }
}
