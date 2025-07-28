using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SimpleCTRL.Engine.InternalSystems
{
    internal static class Settings
    {
        #region Fields

        public static int LogLevel = 0;
        public static string SpeedometerFormat = "MPH";
        public static bool AllowShuffle = false;
        public static bool EnableBrakeLights = true;
        public static bool EnableTireRentainment = true;
        public static bool EnableEngineRunOnExit = true;
        public static bool EnableBrakeOverheating = true;
        public static bool PreventAutomaticReversing = true;
        public static bool PreventVehicleRolloverRecovery = true;
        public static bool EngineRunOnExitNotification = true;
        public static bool BrakeOverheatingNotification = true;

        public static VehicleDamageConfig VehicleDamageSettings { get; private set; }

        #endregion

        #region Initialization

        public static void Initialize()
        {
            Logging.Debug("Initializing configuration handler...", "Settings");

            LoadINI("default");
            LoadINI("custom");
            LoadVehicleDamageConfig();

            LogConfig();
        }

        #endregion

        #region INI Loading

        private static bool LoadINI(string filename)
        {
            Logging.Info($"Loading {filename} settings...", "Settings");
            InitializationFile val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini");
            if (!val.Exists())
            {
                val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini.ini");
                if (!val.Exists())
                {
                    Logging.Warning($"Cannot find file for {filename} settings, skipping.", "Settings");
                    return false;
                }
            }
            LogLevel = val.ReadInt32("ADVANCED", "LogLevel", LogLevel);
            SpeedometerFormat = val.ReadString("DISPLAY", "SpeedometerFormat", SpeedometerFormat);
            AllowShuffle = val.ReadBoolean("IMMERSION", "AllowShuffle", AllowShuffle);
            EnableBrakeLights = val.ReadBoolean("IMMERSION", "EnableBrakeLights", EnableBrakeLights);
            EnableTireRentainment = val.ReadBoolean("IMMERSION", "EnableTireRentainment", EnableTireRentainment);
            EnableEngineRunOnExit = val.ReadBoolean("IMMERSION", "EnableEngineRunOnExit", EnableEngineRunOnExit);
            EnableBrakeOverheating = val.ReadBoolean("IMMERSION", "EnableBrakeOverheating", EnableBrakeOverheating);
            PreventAutomaticReversing = val.ReadBoolean("IMMERSION", "PreventAutomaticReversing", PreventAutomaticReversing);
            PreventVehicleRolloverRecovery = val.ReadBoolean("IMMERSION", "PreventVehicleRolloverRecovery", PreventVehicleRolloverRecovery);
            EngineRunOnExitNotification = val.ReadBoolean("NOTIFICATIONS", "EngineRunOnExitNotification", EngineRunOnExitNotification);
            BrakeOverheatingNotification = val.ReadBoolean("NOTIFICATIONS", "BrakeOverheatingNotification", BrakeOverheatingNotification);
            return true;
        }

        #endregion

        #region XML Loading

        private static void LoadVehicleDamageConfig()
        {
            try
            {
                string xmlPath = Path.Combine("plugins", "SimpleCTRL", "data", "VehicleDamageConfig.xml");
                VehicleDamageSettings = VehicleDamageConfig.Load(xmlPath);
                Logging.Info("Vehicle damage config loaded.", "Settings");
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to load vehicle damage config: {ex.Message}", "Settings");
                VehicleDamageSettings = new VehicleDamageConfig(); 
            }
        }

        #endregion

        #region Logging

        private static void LogConfig()
        {
            Logging.Info("================================================================================", "Settings");
            Logging.Info("                             SimpleCTRL Settings", "Settings");
            Logging.Info("================================================================================", "Settings");
            FieldInfo[] fields = typeof(Settings).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fields)
            {
                object value = fieldInfo.GetValue(null);
                Logging.Info($"{fieldInfo.Name,-30} = {value}", "Settings");
            }
            Logging.Info("================================================================================", "Settings");
        }

        #endregion

        #region Utility Methods

        private static Keys GetKeysFromString(string keyString, Keys defaultKeys)
        {
            try
            {
                return (Keys)new KeysConverter().ConvertFromString(keyString);
            }
            catch
            {
                return defaultKeys;
            }
        }

        #endregion
    }
}
