using Common.Elements;
using Common.Native;
using Common.UI;
using Rage;
using SimpleCTRL.Handlers;
using SimpleCTRL.Utils;
using System;
using System.Drawing;

namespace SimpleCTRL.UI
{
    internal static class VehicleUI
    {
        private static Vehicle _playerVehicle = null;

        private static void UpdateCache()
        {
            _playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;
        }

        private static void DrawHud()
        {
            N.HideHudComponentThisFrame(6);
            N.HideHudComponentThisFrame(8);

            #region Car Hud
            if (_playerVehicle != null && _playerVehicle.Class != VehicleClass.Cycle && !N.IsHudHidden())
            {
                string vehPlate = _playerVehicle.LicensePlate;

                bool vehBurnout = _playerVehicle.IsInBurnout;
                bool vehEngineRunning = _playerVehicle.IsEngineOn;

                float vehEngineHealth = _playerVehicle.EngineHealth;
                float vehBodyHealth = N.GetVehicleBodyHealth(_playerVehicle);
                float vehSpeedMph = _playerVehicle.Speed * 2.236936f;

                float speedPanelWidth = 0.046f;
                float speedPanelHeight = 0.03f;

                if (ConfigHandler.SpeedometerEnabled == true)
                {
                    Rect.Draw(0.095f, 0.0475f, speedPanelWidth, speedPanelHeight, 0, 0, 0, 100); // UI: MPH Panel
                    Text.Draw(.84f, -0.125f, .6f, $"~w~{Math.Ceiling(vehSpeedMph)}", Color.White, Alignment.Right); // INT: Speed Value
                    Text.Draw(0.875f, -0.135f, .4f, $"~w~MPH", Color.White); // TXT: Speed Unit
                }

                if (_playerVehicle.IsAircraft() || _playerVehicle.IsBlimp)
                {
                    Rect.Draw(0.095f, 0.17f, speedPanelWidth, speedPanelHeight, 0, 0, 0, 100); // UI: Altitude Panel
                    Text.Draw(.87f, 0f, .6f, $"~w~{Math.Ceiling(_playerVehicle.HeightAboveGround * 3.2808f)}", Color.White, Alignment.Right); // INT: Altitude Value
                    Text.Draw(0.875f, -0.01f, .4f, $"~w~feet", Color.White); // TXT: Altitude Unit
                }

                if (ConfigHandler.LicensePlateEnabled) Text.Draw(.5f, .045f, 0.55f, $"~w~{vehPlate}", Color.White, Alignment.Center);  // TXT: Plate

                if (ConfigHandler.EngStatusEnabled)
                {
                    if (ConfigHandler.ParkIndicatorEnabled && !Globals.disallowedClasses.Contains(_playerVehicle.Class))
                    {
                        Text.Draw(0.87f, .065f, .45f, vehEngineRunning ? "~g~ENG" : "~r~ENG", Color.LightGray, Alignment.Right); // TXT: Engine
                    }
                    else
                    {
                        Text.Draw(1f, .065f, .45f, vehEngineRunning ? "~g~ENG" : "~r~ENG", Color.LightGray, Alignment.Right); // TXT: Engine
                    }
                }

                Text.Draw(.15f, .04f, .45f, vehBurnout ? "~r~DSC" : "DSC", Color.LightGray); // TXT: DSC

                if (vehBodyHealth < 310)
                {
                    Text.Draw(1f, .04f, .45f, "~r~AC", Color.LightGray, Alignment.Right); // TXT: AC Damaged
                }
                else if (vehBodyHealth < 900)
                {
                    Text.Draw(1f, .04f, .45f, "~y~AC", Color.LightGray, Alignment.Right); // TXT: AC Slightly Damaged
                }
                else
                {
                    Text.Draw(1f, .04f, .45f, "AC", Color.LightGray, Alignment.Right); // TXT: AC 
                }

                if (ConfigHandler.ParkIndicatorEnabled)
                {
                    Text.Draw(0.99f, .065f, .45f, Globals.isParked ? "~r~P" : "", Color.LightGray, Alignment.Right); // TXT: Current Gear
                }

                if (vehEngineHealth < 110)
                {
                    Text.Draw(.75f, .04f, .45f, "~r~Fluid", Color.LightGray); // TXT: Fluid Damaged
                    Text.Draw(.01f, .04f, .45f, "~r~Oil", Color.LightGray); // TXT : Oil Damaged
                }
                else if (vehEngineHealth < 315)
                {
                    Text.Draw(.75f, .04f, .45f, "~r~Fluid", Color.LightGray); // TXT: Fluid Damaged
                    Text.Draw(.01f, .04f, .45f, "~y~Oil", Color.LightGray); // TXT : Oil Slightly Damaged
                }
                else if (vehEngineHealth < 900)
                {
                    Text.Draw(.75f, .04f, .45f, "~y~Fluid", Color.LightGray); // TXT: Fluid Slightly Damaged
                    Text.Draw(.01f, .04f, .45f, "Oil", Color.LightGray); // TXT : Oil 
                }
                else
                {
                    Text.Draw(.75f, .04f, .45f, "Fluid", Color.LightGray); // TXT: Fluid
                    Text.Draw(.01f, .04f, .45f, "Oil", Color.LightGray); // TXT: Oil 
                }
            }
            #endregion
        }

        public static void Start()
        {
            UpdateCache();
            DrawHud();
        }
    }
}
