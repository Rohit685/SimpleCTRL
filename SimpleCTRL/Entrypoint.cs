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
            Logging.Info("SimpleCTRL plugin lifecycle started.", "EntryPoint");

            DependencyManager.AddDependency("Venoxity.Common.dll", "1.0.8");
            if (!DependencyManager.CheckDependencies()) return;

            InitializePlugin();
        }
        #endregion

        #region Initialization
        private static void InitializePlugin()
        {
            Logging.Info("Initializing SimpleCTRL...", "EntryPoint");

            try
            {
                Settings.Initialize();
                Decorators.Initialize();
                Decorators.Register(decorators);

                VehicleDamageModule.Start();
                VehicleSystemModule.Start();
                KeybindManager.Start();

                Logging.Info("SimpleCTRL successfully initialized.", "EntryPoint");
            }
            catch (Exception ex)
            {
                Logging.Error($"Initialization failed: {ex.Message}", "EntryPoint");
            }
        }
        #endregion

        #region Cleanup
        public static void OnUnload(bool isTerminating)
        {
            Logging.Info($"Plugin unloading initiated.", "EntryPoint");

            try
            {
                CleanUp();
                Logging.Info("Plugin successfully unloaded.", "EntryPoint");
            }
            catch (Exception ex)
            {
                Logging.Error($"Error during unload: {ex.Message}", "EntryPoint");
            }
        }

        private static void CleanUp()
        {
            Logging.Info("Cleaning up plugin resources.", "Entrypoint");
        }
        #endregion
    }
}
