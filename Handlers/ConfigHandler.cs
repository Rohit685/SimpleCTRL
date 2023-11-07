using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static SimpleCTRL.Threads.SpecialModesManager;

namespace SimpleCTRL.Handlers
{
    internal static class ConfigHandler
    {
        #region Variables
        public static int LogLevel = 0;

        public static string PluginPath = AppDomain.CurrentDomain.BaseDirectory + "/plugins/SimpleCTRL";
        public static string AudioPath = PluginPath + "/audio";
        public static string TexturePath = PluginPath + "/textures";

        public static Keys HazardKey = Keys.None;
        public static Keys LeftBlinkerKey = Keys.None;
        public static Keys RightBlinkerKey = Keys.None;
        public static Keys BlinkerModifierKey = Keys.None;
        public static bool BlinkersEnabled = true;

        public static Keys EngineKey = Keys.None;
        public static Keys EngineModifierKey = Keys.None;
        public static bool EngineEnabled = true;

        public static bool TireRentainment = true;
        public static bool PreventVehicleFlip = true;
        public static bool PreventAutomaticReversing = true;
        public static bool TorqueMultiplierEnable = true;
        public static bool LimpMode = true;

        public static bool SpeedometerEnabled = true;

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

        public static float LimpModeMultiplier { get; set; }    // The torque multiplier to use when vehicle is limping. Sane values are 0.05 to 0.25

        public static List<float> ClassDamageMultiplier { get; set; }

        // Repair Cfg
        public static List<RepairShop> RepairShops { get; set; }

        public static List<string> FixMessages { get; set; }

        public static List<string> NoFixMessages { get; set; }
        #endregion

        public static void Initialize()
        {
            LoadINI("SimpleCTRL");
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
            LogLevel = val.ReadInt32("Advanced", "LogLevel", LogLevel);
            HazardKey = GetKeysFromString(val.ReadString("BLINKERS", "HazardKey", ""), HazardKey);
            LeftBlinkerKey = GetKeysFromString(val.ReadString("BLINKERS", "LeftBlinkerKey", ""), LeftBlinkerKey);
            RightBlinkerKey = GetKeysFromString(val.ReadString("BLINKERS", "RightBlinkerKey", ""), RightBlinkerKey);
            BlinkerModifierKey = GetKeysFromString(val.ReadString("BLINKERS", "BlinkerModifierKey", ""), BlinkerModifierKey);
            BlinkersEnabled = val.ReadBoolean("BLINKERS", "BlinkersEnabled", BlinkersEnabled);
            EngineKey = GetKeysFromString(val.ReadString("ENGINE", "EngineKey", ""), EngineKey);
            EngineModifierKey = GetKeysFromString(val.ReadString("ENGINE", "EngineModifierKey", ""), EngineModifierKey);
            EngineEnabled = val.ReadBoolean("ENGINE", "EngineEnabled", EngineEnabled);
            TireRentainment = val.ReadBoolean("GENERAL", "TireRentainment", TireRentainment);
            PreventVehicleFlip = val.ReadBoolean("GENERAL", "PreventVehicleFlip", PreventVehicleFlip);
            PreventAutomaticReversing = val.ReadBoolean("GENERAL", "PreventAutomaticReversing", PreventAutomaticReversing);
            TorqueMultiplierEnable = val.ReadBoolean("GENERAL", "TorqueMultiplierEnable", TorqueMultiplierEnable);
            LimpMode = val.ReadBoolean("GENERAL", "LimpMode", LimpMode);
            SpeedometerEnabled = val.ReadBoolean("SPEEDOMETER", "SpeedometerEnabled", SpeedometerEnabled);

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
            LimpModeMultiplier = 0.15f;
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