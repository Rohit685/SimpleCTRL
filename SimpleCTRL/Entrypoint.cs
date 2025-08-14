using SimpleCTRL.Modules;

[assembly: Rage.Attributes.Plugin("SimpleCTRL", Author = "Venoxity Development", PrefersSingleInstance = true, ShouldTickInPauseMenu = true, SupportUrl = "https://discord.gg/jCEdAF8AQz")]
namespace SimpleCTRL
{
    public class Entrypoint : CommonPlugin
    {
        #region Constants & Fields
        private static readonly Dictionary<string, DecoratorType> decorators = new()
        {
            { "brakeHeat", DecoratorType.Int }
        };

        private static bool isUIVisible;
        private static VehicleControlUI ui;
        #endregion

        #region Entry Point       
        public static void Main()
        {
            Logging.Info("Plugin startup initiated.", "Entrypoint");

            DependencyManager.AddDependency("Venoxity.Common.dll", "1.0.8");
            if (!DependencyManager.CheckDependencies())
            {
                Logging.Error("Missing required dependencies. Plugin will not start.", "Entrypoint");
                return;
            }

            InitializePlugin();
        }
        #endregion

        #region Initialization
        private static void InitializePlugin()
        {
            Logging.Info("Initializing systems and modules...", "Entrypoint");

            try
            {
                Settings.Initialize();
                Decorators.Initialize();
                Decorators.Register(decorators);

                VehicleDamageModule.Start();
                VehicleSystemModule.Start();
                KeybindManager.Start();

                ui = new VehicleControlUI();
                ui.Initialize();
                ui.SetSlotOrder(
                    topOrder: new List<string> { "left_indicator", "hazards", "right_indicator", "empty", "front_hood", "door_1", "door_3", "window_1", "window_3", "seat_1", "seat_2" },
                    bottomOrder: new List<string> { "cruise_control", "headlight_low", "interior_light", "empty", "rear_hood", "door_2", "door_4", "window_2", "window_4", "seat_3", "seat_4" }
                );

                GameFiber.StartNew(UIUpdateLoop);

                Game.RawFrameRender -= OnRawFrameRender;
                Game.RawFrameRender += OnRawFrameRender;

                Logging.Info("All modules successfully initialized.", "Entrypoint");
            }
            catch (Exception ex)
            {
                Logging.Error($"Initialization failed: {ex.Message}\n{ex.StackTrace}", "Entrypoint");
            }
        }
        #endregion

        #region GameFiber Loop
        private static Vehicle previousVehicle = null;

        private static void UIUpdateLoop()
        {
            while (true)
            {
                GameFiber.Yield();
                if (ui == null) continue;

                if (Game.IsKeyDown(Keys.F6)) isUIVisible = !isUIVisible;
                ui.IsInteractive = isUIVisible;

                if (!isUIVisible) continue;

                var veh = Game.LocalPlayer.Character.CurrentVehicle;
                if (veh != null && veh != previousVehicle)
                {
                    previousVehicle = veh;
                    int doorCount = veh.Model.NumberOfSeats;
                    ui.UpdateDynamicSlots(doorCount);
                }

                if (Game.IsKeyDown(Keys.LButton))
                    ui.HandleClick();
            }
        }
        #endregion

        #region Rendering
        private static void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            if (isUIVisible && ui != null)
                ui.Draw(e.Graphics);
        }
        #endregion

        #region Cleanup
        public static void OnUnload(bool isTerminating)
        {
            Logging.Info($"Plugin unloading... Terminating = {isTerminating}", "Entrypoint");

            try
            {
                CleanUp();
                Logging.Info("Plugin successfully unloaded.", "Entrypoint");
            }
            catch (Exception ex)
            {
                Logging.Error($"Error during unload: {ex.Message}\n{ex.StackTrace}", "Entrypoint");
            }
        }

        private static void CleanUp()
        {
            Logging.Info("Cleaning up plugin resources.", "Entrypoint");
            Game.RawFrameRender -= OnRawFrameRender;
        }
        #endregion
    }
}