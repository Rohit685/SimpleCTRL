using Common.UI;
using Common.UI.Elements;
using System.Drawing;

namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    public class VehicleHUD : CommonPlugin
    {
        #region Constants
        private const float SpeedPanelWidth = 0.046f;
        private const float SpeedPanelHeight = 0.03f;
        private const float AltitudePanelWidth = 0.046f;
        private const float AltitudePanelHeight = 0.03f;
        private const string SpeedMph = "MPH";
        private const string SpeedKmh = "KM/H";
        #endregion

        #region HUD Drawing
        private static void DrawHud()
        {
            N.HideHudComponentThisFrame(6);
            N.HideHudComponentThisFrame(8);

            if (ClientCurrentVehicle == null || ClientCurrentVehicle.Class == VehicleClass.Cycle || N.IsHudHidden())
                return;

            DrawSpeedometer();
            if (VehicleUtilities.IsAircraft(ClientCurrentVehicle) || ClientCurrentVehicle.IsBlimp)
                DrawAltitude();

            DrawVehicleInfo();
        }

        private static void DrawSpeedometer()
        {
            float speed = ClientCurrentVehicle.Speed * GetSpeedMultiplier();
            Rect.Draw(0.095f, 0.0475f, SpeedPanelWidth, SpeedPanelHeight, 0, 0, 0, 100); // UI: MPH Panel
            Text.Draw(0.84f, -0.125f, 0.6f, $"~w~{Math.Ceiling(speed)}", Color.White, Alignment.Right); // INT: Speed Value

            string speedUnit = Settings.SpeedometerFormat == SpeedKmh ? SpeedKmh : SpeedMph;
            Text.Draw(0.875f, -0.135f, 0.4f, $"~w~{speedUnit}", Color.White); // TXT: Speed Unit
        }

        private static float GetSpeedMultiplier()
        {
            return Settings.SpeedometerFormat == SpeedKmh ? 3.6f : 2.236936f;
        }

        private static void DrawAltitude()
        {
            Rect.Draw(0.095f, 0.17f, AltitudePanelWidth, AltitudePanelHeight, 0, 0, 0, 100); // UI: Altitude Panel
            Text.Draw(0.87f, 0f, 0.6f, $"~w~{Math.Ceiling(ClientCurrentVehicle.HeightAboveGround * 3.2808f)}", Color.White, Alignment.Right); // INT: Altitude Value
            Text.Draw(0.875f, -0.01f, 0.4f, "~w~feet", Color.White); // TXT: Altitude Unit
        }

        private static void DrawVehicleInfo()
        {
            string vehPlate = ClientCurrentVehicle.LicensePlate;
            bool vehBurnout = ClientCurrentVehicle.IsInBurnout;
            bool vehEngineRunning = ClientCurrentVehicle.IsEngineOn;
            float vehEngineHealth = ClientCurrentVehicle.EngineHealth;
            float vehBodyHealth = N.GetVehicleBodyHealth(ClientCurrentVehicle);

            Text.Draw(0.5f, 0.045f, 0.55f, $"~w~{vehPlate}", Color.White, Alignment.Center); // TXT: Plate
            Text.Draw(1f, 0.065f, 0.45f, vehEngineRunning ? "~g~ENG" : "~r~ENG", Color.LightGray, Alignment.Right); // TXT: Engine
            Text.Draw(0.15f, 0.04f, 0.45f, vehBurnout ? "~r~DSC" : "DSC", Color.LightGray); // TXT: DSC

            DrawVehicleHealthInfo(vehBodyHealth, vehEngineHealth);
        }

        private static void DrawVehicleHealthInfo(float bodyHealth, float engineHealth)
        {
            Text.Draw(1f, 0.04f, 0.45f, GetBodyHealthText(bodyHealth), Color.LightGray, Alignment.Right); // TXT: AC Status
            Text.Draw(0.75f, 0.04f, 0.45f, GetEngineFluidText(engineHealth), Color.LightGray); // TXT: Fluid Status
            Text.Draw(0.01f, 0.04f, 0.45f, GetEngineOilText(engineHealth), Color.LightGray); // TXT: Oil Status
        }

        private static string GetBodyHealthText(float bodyHealth)
        {
            return bodyHealth < 310 ? "~r~AC" : bodyHealth < 900 ? "~y~AC" : "AC";
        }

        private static string GetEngineFluidText(float engineHealth)
        {
            return engineHealth < 110 ? "~r~Fluid" : engineHealth < 315 ? "~y~Fluid" : "Fluid";
        }

        private static string GetEngineOilText(float engineHealth)
        {
            return engineHealth < 110 ? "~r~Oil" : engineHealth < 315 ? "~y~Oil" : "Oil";
        }
        #endregion

        #region Initialization and Running
        public static void Start()
        {
            GameFiber.StartNew(Run);
        }

        public static void Run()
        {
            while (true)
            {
                GameFiber.Yield();

                if (ClientPed.IsInAnyVehicle(false))
                {
                    DrawHud();
                }
            }
        }
        #endregion
    }
}