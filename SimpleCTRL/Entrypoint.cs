using SimpleCTRL.Modules;

[assembly: Rage.Attributes.Plugin("SimpleCTRL", Author = "Venoxity Development", PrefersSingleInstance = true, ShouldTickInPauseMenu = true, SupportUrl = "https://discord.gg/jCEdAF8AQz")]
namespace SimpleCTRL
{
    public class Entrypoint : CommonPlugin
    {
        #region Constants
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "brakeHeat", DecoratorType.Int }
        };
        #endregion

        #region Plugin Entry Point       
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

                // VehicleHUD.Start();
                VehicleDamageModule.Start();
                VehicleSystemModule.Start();
                KeybindManager.Start();

                Logging.Info("All modules successfully initialized.", "Entrypoint");
            }
            catch (Exception ex)
            {
                Logging.Error($"Initialization failed: {ex.Message}\n{ex.StackTrace}", "Entrypoint");
            }
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
        }
        #endregion
    }
}
