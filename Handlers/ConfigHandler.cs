using Common;
using Common.Models;
using Newtonsoft.Json;
using Rage;
using SimpleCTRL.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SimpleCTRL.Handlers
{
    internal static class ConfigHandler
    {
        public static int DeformationMultiplier = -1;
        public static float DeformationExponent = 1f;
        public static float CollisionDamageExponent = 1f;
        public static float DamageFactorEngine = 5.1f;
        public static float DamageFactorBody = 5.1f;
        public static float DamageFactorPetrolTank = 61f;
        public static float EngineDamageExponent = 1f;
        public static float WeaponsDamageMultiplier = 0.124f;
        public static float DegradingHealthSpeedFactor = 3.0f;
        public static float CascadingFailureSpeedFactor = 1.5f;
        public static float DegradingFailureThreshold = 677f;
        public static float CascadingFailureThreshold = 310f;
        public static float EngineSafeGuard = 100f;
        public static bool TorqueMultiplierEnable = true;
        public static bool LimpMode = true;
        public static float LimpModeMultiplier = 0.15f;
        public static int LogLevel = 0;

        public static string PluginPath = AppDomain.CurrentDomain.BaseDirectory + "/plugins/SimpleCTRL";
        public static string AudioPath = PluginPath + "/audio";
        public static string TexturePath = PluginPath + "/textures";

        public static Keys HazardKey = Keys.None;
        public static Keys LeftBlinkerKey = Keys.None;
        public static Keys RightBlinkerKey = Keys.None;
        public static Keys BlinkerModifierKey = Keys.None;
        public static Keys EngineKey = Keys.None;
        public static Keys EngineModifierKey = Keys.None;

        public static ControllerButtons HazardControllerButton = (ControllerButtons)0;
        public static ControllerButtons LeftBlinkerControllerButton = (ControllerButtons)0;
        public static ControllerButtons RightBlinkerControllerButton = (ControllerButtons)0;
        public static ControllerButtons BlinkerModifierControllerButton = (ControllerButtons)0;
        public static ControllerButtons EngineControllerButton = (ControllerButtons)0;
        public static ControllerButtons EngineModifierControllerButton = (ControllerButtons)0;

        public static bool BlinkersEnabled = true;
        public static bool EngineEnabled = true;
        public static bool SpeedometerEnabled = true;

        public static bool PreventAutomaticReversing = true;
        public static bool PreventVehicleFlip = true;
        public static bool TireRentainment = true;

        public static bool LeaveEngineOnNotification = true;
        public static bool BrakeOverheatingNotification = true;

        public static float AircraftLowFuelWarning = 25f;
        public static bool AircraftUseAirportPumps = true;
        public static bool AircraftUseFuelTankers = false;
        public static List<string> AircraftFuelTankers;

        public static List<float> ClassDamageMultiplier { get; set; }

        // Repair Cfg
        public static List<RepairShop> RepairShops { get; set; }

        public static List<string> FixMessages { get; set; }

        public static List<string> NoFixMessages { get; set; }

        public static void Initialize()
        {
            Logging.Debug("initializing...", "ConfigHandler");
            LoadINI("SimpleCTRL");
            LoadVehicleData();
            LoadStations();
            LoadLocalStations();
            LoadAircraftFuelPumps();
            LoadPumps();
            LogConfig();
        }

        private static bool LoadINI(string filename)
        {
            InitializationFile val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini");
            if (!val.Exists())
            {
                val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini.ini");
                if (!val.Exists())
                {
                    return false;
                }
            }
            LogLevel = val.ReadInt32("ADVANCED", "LogLevel", LogLevel);

            HazardKey = GetKeysFromString(val.ReadString("CONTROLS", "HazardKey", ""), HazardKey);
            LeftBlinkerKey = GetKeysFromString(val.ReadString("CONTROLS", "LeftBlinkerKey", ""), LeftBlinkerKey);
            RightBlinkerKey = GetKeysFromString(val.ReadString("CONTROLS", "RightBlinkerKey", ""), RightBlinkerKey);
            BlinkerModifierKey = GetKeysFromString(val.ReadString("CONTROLS", "BlinkerModifierKey", ""), BlinkerModifierKey);
            EngineKey = GetKeysFromString(val.ReadString("CONTROLS", "EngineKey", ""), EngineKey);
            EngineModifierKey = GetKeysFromString(val.ReadString("CONTROLS", "EngineModifierKey", ""), EngineModifierKey);

            HazardControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "HazardControllerButton", HazardControllerButton);
            LeftBlinkerControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "LeftBlinkerControllerButton", LeftBlinkerControllerButton);
            RightBlinkerControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "RightBlinkerControllerButton", RightBlinkerControllerButton);
            BlinkerModifierControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "BlinkerModifierControllerButton", BlinkerModifierControllerButton);
            EngineControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "EngineControllerButton", EngineControllerButton);
            EngineModifierControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "EngineModifierControllerButton", EngineModifierControllerButton);

            BlinkersEnabled = val.ReadBoolean("GENERAL", "BlinkersEnabled", BlinkersEnabled);
            EngineEnabled = val.ReadBoolean("GENERAL", "EngineEnabled", EngineEnabled);;
            SpeedometerEnabled = val.ReadBoolean("GENERAL", "SpeedometerEnabled", SpeedometerEnabled);

            PreventAutomaticReversing = val.ReadBoolean("IMMERSION", "PreventAutomaticReversing", PreventAutomaticReversing);
            PreventVehicleFlip = val.ReadBoolean("IMMERSION", "PreventVehicleFlip", PreventVehicleFlip);
            TireRentainment = val.ReadBoolean("GENERAL", "TireRentainment", TireRentainment);

            LeaveEngineOnNotification = val.ReadBoolean("NOTIFICATIONS", "LeaveEngineOnNotification", LeaveEngineOnNotification);
            BrakeOverheatingNotification = val.ReadBoolean("NOTIFICATIONS", "BrakeOverheatingNotification", BrakeOverheatingNotification);

            AircraftLowFuelWarning = Calc.Clamp(Convert.ToSingle(val.ReadDouble("OTHER", "AircraftLowFuelWarning", (double)AircraftLowFuelWarning)), 1f, 100f);
            AircraftUseAirportPumps = val.ReadBoolean("OTHER", "AircraftUseAirportPumps", AircraftUseAirportPumps);
            AircraftUseFuelTankers = val.ReadBoolean("OTHER", "AircraftUseFuelTankers", AircraftUseFuelTankers);
            AircraftFuelTankers = new List<string>(); // figure out how to read a list inside ini file

            ClassDamageMultiplier = new List<float>
            {
                1.0f, 1.0f, 1.0f, 0.95f, 1.0f, 0.95f, 0.95f, 0.95f, 0.27f, 0.7f, 0.25f, 0.35f, 0.85f, 1.0f, 0.4f, 0.7f, 0.7f, 0.75f, 0.05f, 0.67f, 0.43f, 1.0f
            };

            RepairShops = new List<RepairShop> {
                new RepairShop(-337f, -135f, 39f, 25f, "LSC Burton"),
                new RepairShop(-1155f, -2007f, 13f, 25f, "LSC by airport"),
                new RepairShop(734f, -1085f, 22f, 25f, "LSC La Mesa"),
                new RepairShop(1177f, 2649f, 37f, 25f, "LSC Harmony"),
                new RepairShop(108f, 6624f, 31f, 25f, "LSC Paleto Bay"),
                new RepairShop(538f, -183f, 54f, 18f, "Mechanic Hawic"),
                new RepairShop(1774f, 3333f, 41f, 15f, "Mechanic Sandy Shores Airfield"),
                new RepairShop(1143f, -776f, 57f, 15f, "Mechanic Mirror Park"),
                new RepairShop(2508f, 4103f, 38f, 30f, "Mechanic East Joshua Rd."),
                new RepairShop(2006f, 3792f, 32f, 16f, "Mechanic Sandy SHores Gas Station"),
                new RepairShop(484f, -1316f, 29f, 25f, "Hayes Auto, Little Bighorn Ave."),
                new RepairShop(-1419, -450f, 36f, 33f, "Hayes Auto Body Shop, Del Perro"),
                new RepairShop(268f, -1810f, 27f, 33f, "Hayes Auto Body Shop, Davis"),
                new RepairShop(1915f, 3729f, 32f, 27f, "Otto's Auto Parts, Sandy Shores"),
                new RepairShop(-29f, -1665f, 29f, 45f, "Mosley Auto Service, Strawberry"),
                new RepairShop(-212f, -1378f, 31f, 44f, "Glass Heroes, Strawberry"),
                new RepairShop(258f, 2594f, 44f, 33f, "Mechanic Harmony"),
                new RepairShop(-32f, -1090f, 26f, 18f, "Simeons"),
                new RepairShop(-211f, -1325f, 31f, 25f, "Bennys"),
                new RepairShop(903f, 3563f, 34f, 25f, "Auto Repair, Grand Senora Desert"),
                new RepairShop(437f, 3568f, 38f, 25f, "Auto Shop, Grand Senora Desert"),
            };

            FixMessages = new List<string>
            {
                "You put the oil plug back in.",
                "You stopped the oil leak using chewing gum.",
                "You repaired the oil tube with gaffer tape.",
                "You tightened the oil pan screw and stopped the dripping.",
                "You kicked the engine and it magically came back to life.",
                "You removed some rust from the spark tube.",
                "You yelled at your vehicle, and it somehow had an effect."
            };

            NoFixMessages = new List<string>
            {
                "You checked the oil plug. It's still there.",
                "You looked at your engine, it seemed fine.",
                "You made sure that the gaffer tape was still holding the engine together.",
                "You turned up the radio volume. It just drowned out the weird engine noises.",
                "You added rust-preventer to the spark tube. It made no difference.",
                "Never fix something that ain't broken they said. You didn't listen. At least it didn't get worse."
            };
            return true;
        }

        private static void LoadVehicleData()
        {
            string json = null;
            try
            {
                json = File.ReadAllText("plugins/SimpleCTRL/AircraftSpecs.json") ?? "[]";

                if (!string.IsNullOrWhiteSpace(json))
                {
                    VehicleProperties.AircraftSpecs = JsonConvert.DeserializeObject<List<AircraftFuelSpecs>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LoadStations()
        {
            string json = null;
            try
            {
                json = File.ReadAllText("plugins/SimpleCTRL/GasStations.json") ?? "[]";

                if (!string.IsNullOrWhiteSpace(json))
                {
                    Globals.GasStations.AddRange(JsonConvert.DeserializeObject<List<GasStation>>(json));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LoadPumps()
        {
            string json = null;
            try
            {
                json = File.ReadAllText("plugins/SimpleCTRL/DepartmentPumps.json") ?? "[]";

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<GasStation> stations = JsonConvert.DeserializeObject<List<GasStation>>(json);
                    Globals.GasStations.AddRange(stations);
                    {
                        foreach (GasStation station in stations)
                        {
                            Globals.DepartmentPumps.AddRange(station.Pumps);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LoadLocalStations()
        {
            string json = null;
            try
            {
                json = File.ReadAllText("plugins/SimpleCTRL/GasStations.Local.json") ?? "[]";

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<GasStation> local = JsonConvert.DeserializeObject<List<GasStation>>(json);
                    Globals.GasStations.AddRange(local);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LoadAircraftFuelPumps()
        {
            string json = null;
            try
            {
                json = File.ReadAllText("plugins/SimpleCTRL/GasStations.Aircraft.json") ?? "[]";

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<AirportFuelPump> list = JsonConvert.DeserializeObject<List<AirportFuelPump>>(json);
                    Globals.AirportFuelPumps.AddRange(list);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LogConfig()
        {
            Logging.Info("================================================================================", "ConfigHandler");
            Logging.Info("                             SimpleCTRL Settings", "ConfigHandler");
            Logging.Info("================================================================================", "ConfigHandler");
            FieldInfo[] fields = typeof(ConfigHandler).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fields)
            {
                object value = fieldInfo.GetValue(null);
                Logging.Info($"{fieldInfo.Name,-30} = {value}", "ConfigHandler");
            }
            Logging.Info("================================================================================", "ConfigHandler");
        }

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
    }
}