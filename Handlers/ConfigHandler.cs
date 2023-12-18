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
        public static int DeformationMultiplier { get; set; }  // How much should the vehicle visually deform from a collision. Range 0.0 to 10.0 Where 0.0 is no deformation and 10.0 is 10x deformation. -1 = Don't touch
        public static float DeformationExponent { get; set; }  //How much should the handling file deformation setting be compressed toward 1.0. (Make cars more similar). A value of 1=no change. Lower values will compress more, values above 1 it will expand. Dont set to zero or negative.
        public static float CollisionDamageExponent { get; set; }  // How much should the handling file deformation setting be compressed toward 1.0. (Make cars more similar). A value of 1=no change. Lower values will compress more, values above 1 it will expand. Dont set to zero or negative.

        public static float DamageFactorEngine { get; set; } // Sane values are 1 to 100. Higher values means more damage to vehicle. A good starting point is 10
        public static float DamageFactorBody { get; set; }   // Sane values are 1 to 100. Higher values means more damage to vehicle. A good starting point is 10
        public static float DamageFactorPetrolTank { get; set; }  // Sane values are 1 to 100. Higher values means more damage to vehicle. A good starting point is 64
        public static float EngineDamageExponent { get; set; } // How much should the handling file engine damage setting be compressed toward 1.0. (Make cars more similar). A value of 1 = no change. Lower values will compress more, values above 1 it will expand. Dont set to zero or negative.
        public static float WeaponsDamageMultiplier { get; set; }  // How much damage should the vehicle get from weapons fire. Range 0.0 to 10.0, where 0.0 is no damage and 10.0 is 10x damage. -1 = don't touch
        public static float DegradingHealthSpeedFactor { get; set; } // Speed of slowly degrading health, but not failure. Value of 10 means that it will take about 0.25 second per health point, so degradation from 800 to 305 will take about 2 minutes of clean driving. Higher values means faster degradation
        public static float CascadingFailureSpeedFactor { get; set; }    // Sane values are 1 to 100. When vehicle health drops below a certain point, cascading failure sets in, and the health drops rapidly until the vehicle dies. Higher values means faster failure. A good starting point is 8

        public static float DegradingFailureThreshold { get; set; }  // Below this value, slow health degradation will set in
        public static float CascadingFailureThreshold { get; set; }  // Below this value, slow health cascading will set in
        public static float EngineSafeGuard { get; set; }    // Final failure value. Set it too high, and the vehicle won't smoke when disabled. Set too low, and the car will catch fire from a single bullet to the engine. At health 100 a typical car can take 3-4 bullets to the engine before catching fire.

        public static bool TorqueMultiplierEnable { get; set; }  // Decrease engine torge as engine gets more and more damaged

        public static bool LimpMode { get; set; }   // If true, the engine never fails completely, so you will always be able to get to a mechanic unless you flip your vehicle and preventVehicleFlip is set to true
        public static float LimpModeMultiplier { get; set; }    // The torque multiplier to use when vehicle is limping. Sane values are 0.05 to 0.25

        public static bool DisableMenuMouse = true;
        public static int LogLevel = 0;

        public static string PluginPath = AppDomain.CurrentDomain.BaseDirectory + "/plugins/SimpleCTRL";
        public static string AudioPath = PluginPath + "/audio";

        public static Keys HazardKey = Keys.None;
        public static Keys LeftBlinkerKey = Keys.None;
        public static Keys RightBlinkerKey = Keys.None;
        public static Keys BlinkerModifierKey = Keys.None;
        public static Keys EngineKey = Keys.None;
        public static Keys RefuelKey = Keys.None;

        public static ControllerButtons HazardControllerButton = (ControllerButtons)0;
        public static ControllerButtons LeftBlinkerControllerButton = (ControllerButtons)0;
        public static ControllerButtons RightBlinkerControllerButton = (ControllerButtons)0;
        public static ControllerButtons BlinkerModifierControllerButton = (ControllerButtons)0;
        public static ControllerButtons EngineControllerButton = (ControllerButtons)0;
        public static ControllerButtons RefuelControllerButton = (ControllerButtons)0;

        public static bool SpeedometerEnabled = true;

        public static bool PreventAutomaticReversing = true;
        public static bool GlobalPositioningSystem = false;
        public static bool PreventVehicleFlip = true;
        public static bool VehicleIndicators = true;
        public static bool TireRentainment = true;
        public static bool FuelSystem = true;

        public static bool LeaveEngineOnNotification = true;
        public static bool BrakeOverheatingNotification = true;
        public static bool RefuelNotification = false;

        public static float AircraftLowFuelWarning = 25f;
        public static bool AircraftUseAirportPumps = true;
        public static bool AircraftUseFuelTankers = false;
        public static List<uint> AircraftFuelTankers;

        public static List<float> ClassDamageMultiplier { get; set; }

        // Repair Cfg
        public static List<RepairShop> RepairShops { get; set; }

        public static List<string> FixMessages { get; set; }

        public static List<string> NoFixMessages { get; set; }

        public static void Initialize()
        {
            Logging.Debug("initializing...", "ConfigHandler");
            LoadINI("default");
            LoadINI("custom");
            LoadVehicleData();
            LoadStations();
            LoadLocalStations();
            LoadAircraftFuelPumps();
            LoadPumps();
            LogConfig();
        }

        private static bool LoadINI(string filename)
        {
            Logging.Info("loading " + filename + " settings...", "ConfigHandler");
            InitializationFile val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini");
            if (!val.Exists())
            {
                val = new InitializationFile("plugins/SimpleCTRL/" + filename + ".ini.ini");
                if (!val.Exists())
                {
                    Logging.Warning("cannot find file for " + filename + " settings, skipping", "ConfigHandler");
                    return false;
                }
            }

            DeformationMultiplier = -1;
            DeformationExponent = 1f;
            CollisionDamageExponent = 1f;
            DamageFactorEngine = 5.1f;
            DamageFactorBody = 5.1f;
            DamageFactorPetrolTank = 61f;
            EngineDamageExponent = 1f;
            WeaponsDamageMultiplier = 0.124f;
            DegradingHealthSpeedFactor = 3.0f;
            CascadingFailureSpeedFactor = 1.5f;
            DegradingFailureThreshold = 677f;
            CascadingFailureThreshold = 310f;
            EngineSafeGuard = 100f;
            TorqueMultiplierEnable = true;
            LimpMode = true;
            LimpModeMultiplier = 0.15f;
            DisableMenuMouse = val.ReadBoolean("ADVANCED", "DisableMenuMouse", DisableMenuMouse);
            LogLevel = val.ReadInt32("ADVANCED", "LogLevel", LogLevel);

            HazardKey = GetKeysFromString(val.ReadString("CONTROLS", "HazardKey", ""), HazardKey);
            LeftBlinkerKey = GetKeysFromString(val.ReadString("CONTROLS", "LeftBlinkerKey", ""), LeftBlinkerKey);
            RightBlinkerKey = GetKeysFromString(val.ReadString("CONTROLS", "RightBlinkerKey", ""), RightBlinkerKey);
            BlinkerModifierKey = GetKeysFromString(val.ReadString("CONTROLS", "BlinkerModifierKey", ""), BlinkerModifierKey);
            EngineKey = GetKeysFromString(val.ReadString("CONTROLS", "EngineKey", ""), EngineKey);
            RefuelKey = GetKeysFromString(val.ReadString("CONTROLS", "RefuelKey", ""), RefuelKey);

            HazardControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "HazardControllerButton", HazardControllerButton);
            LeftBlinkerControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "LeftBlinkerControllerButton", LeftBlinkerControllerButton);
            RightBlinkerControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "RightBlinkerControllerButton", RightBlinkerControllerButton);
            BlinkerModifierControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "BlinkerModifierControllerButton", BlinkerModifierControllerButton);
            EngineControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "EngineControllerButton", EngineControllerButton);
            RefuelControllerButton = val.ReadEnum<ControllerButtons>("BUTTONS", "RefuelControllerButton",RefuelControllerButton);

            SpeedometerEnabled = val.ReadBoolean("DISPLAY", "SpeedometerEnabled", SpeedometerEnabled);

            PreventAutomaticReversing = val.ReadBoolean("IMMERSION", "PreventAutomaticReversing", PreventAutomaticReversing);
            GlobalPositioningSystem = val.ReadBoolean("IMMERSION", "GlobalPositioningSystem", GlobalPositioningSystem);
            PreventVehicleFlip = val.ReadBoolean("IMMERSION", "PreventVehicleFlip", PreventVehicleFlip);
            VehicleIndicators = val.ReadBoolean("IMMERSION", "VehicleIndicators", VehicleIndicators);
            TireRentainment = val.ReadBoolean("IMMERSION", "TireRentainment", TireRentainment);
            FuelSystem = val.ReadBoolean("IMMERSION", "FuelSystem", FuelSystem);

            LeaveEngineOnNotification = val.ReadBoolean("NOTIFICATIONS", "LeaveEngineOnNotification", LeaveEngineOnNotification);
            BrakeOverheatingNotification = val.ReadBoolean("NOTIFICATIONS", "BrakeOverheatingNotification", BrakeOverheatingNotification);
            RefuelNotification = val.ReadBoolean("NOTIFICATIONS", "RefuelNotification", RefuelNotification);

            AircraftLowFuelWarning = Calc.Clamp(Convert.ToSingle(val.ReadDouble("OTHER", "AircraftLowFuelWarning", (double)AircraftLowFuelWarning)), 1f, 100f);
            AircraftUseAirportPumps = val.ReadBoolean("OTHER", "AircraftUseAirportPumps", AircraftUseAirportPumps);
            AircraftUseFuelTankers = val.ReadBoolean("OTHER", "AircraftUseFuelTankers", AircraftUseFuelTankers);

            List<string> tempAircraftFuelTankers = val.ReadString("OTHER", "AircraftFuelTankers", "")
                .Split(',')
                .Select(s => s.Trim())
                .ToList();

            AircraftFuelTankers = tempAircraftFuelTankers
                .Select(modelName => Game.GetHashKey(modelName))
                .ToList();

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