using Common.Client;
using Common.Client.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Handlers;
using System;
using System.Collections.Generic;

namespace SimpleCTRL.Threads
{
    internal class PlayerController : CommonPlugin
    {
        #region Variables
        private static readonly IReadOnlyList<VehicleClass> _ignoredVehicleClasses = new List<VehicleClass>
        { VehicleClass.Cycle, VehicleClass.Motorcycle, VehicleClass.Plane, VehicleClass.Helicopter, VehicleClass.Boat, VehicleClass.Rail };
        private static readonly IReadOnlyList<int> _tireIndex = new List<int> { 0, 1, 2, 3, 4, 5, 45, 47 };
        private static bool isDisabled = false;
        private static VehicleIndicatorLightsStatus status = (VehicleIndicatorLightsStatus)0;
        private static VehicleIndicatorLightsStatus intendedStatus = (VehicleIndicatorLightsStatus)0;
        private static float initialHeading = 0f;
        private static uint turnOffAt = 0u;
        protected static float _steeringAngle;
        private static Vehicle _steeringVeh = null;
        #endregion

        #region Engine Control
        private static void EngineControl()
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                if (Game.IsKeyDownRightNow(ConfigHandler.EngineKey) && Game.IsKeyDownRightNow(ConfigHandler.EngineModifierKey) && !isDisabled)
                {
                    if (Game.LocalPlayer.Character.CurrentVehicle.Speed < 5f)
                    {
                        API.SetVehicleEngineOn(Game.LocalPlayer.Character.CurrentVehicle, false, false, true);
                    }
                    isDisabled = true;
                }
                else if (Game.IsControlPressed(0, GameControl.VehicleAccelerate) && isDisabled)
                {
                    API.SetVehicleEngineOn(Game.LocalPlayer.Character.CurrentVehicle, true, false, false);
                    isDisabled = false;
                }
            }
        }
        #endregion

        #region Vehicle Indicators
        private static void KeyPolling()
        {
            if (Game.IsKeyDownRightNow(ConfigHandler.RightBlinkerKey) && Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
            {
                intendedStatus = (VehicleIndicatorLightsStatus)1;
            }
            if (Game.IsKeyDownRightNow(ConfigHandler.LeftBlinkerKey) && Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
            {
                intendedStatus = (VehicleIndicatorLightsStatus)2;
            }
            if (Game.IsKeyDownRightNow(ConfigHandler.HazardKey) && Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
            {
                intendedStatus = (VehicleIndicatorLightsStatus)3;
            }
        }

        private static void VehicleIndicators()
        {
            try
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (EntityExtensions.Exists((IHandleable)(object)currentVehicle))
                {
                    if ((int)intendedStatus == 1 && currentVehicle.IsEngineOn)
                    {
                        turnOffAt = 0u;
                        if ((int)status == 1)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)1;
                            initialHeading = ((Entity)currentVehicle).Heading;
                        }
                        currentVehicle.IndicatorLightsStatus = status;
                    }
                    else if ((int)intendedStatus == 2 && currentVehicle.IsEngineOn)
                    {
                        turnOffAt = 0u;
                        if ((int)status == 2)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)2;
                            initialHeading = ((Entity)currentVehicle).Heading;
                        }
                        currentVehicle.IndicatorLightsStatus = status;
                    }
                    else if ((int)intendedStatus == 3)
                    {
                        if ((int)status == 3)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)3;
                        }
                        currentVehicle.IndicatorLightsStatus = status;
                    }
                    if ((int)status != 3)
                    {
                        if (turnOffAt == 0)
                        {
                            if ((int)status != 0 && Math.Abs(((Entity)currentVehicle).Heading - initialHeading) > 60f)
                            {
                                turnOffAt = Game.GameTime + 1500;
                            }
                        }
                        else if (Game.GameTime >= turnOffAt)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                            currentVehicle.IndicatorLightsStatus = status;
                        }
                    }
                }
                intendedStatus = (VehicleIndicatorLightsStatus)0;
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }
        #endregion

        #region Tire Rentainment
        private static void TireRentainment()
        {
            Ped player = Game.LocalPlayer.Character;
            if (EntityExtensions.Exists(player) && EntityExtensions.Exists(Game.LocalPlayer.Character.CurrentVehicle) && Game.LocalPlayer.Character.CurrentVehicle.Driver == player && Game.LocalPlayer.Character.CurrentVehicle.IsAlive && !Game.LocalPlayer.Character.CurrentVehicle.Model.IsBicycle && (Game.LocalPlayer.Character.CurrentVehicle.Model.IsCar || Game.LocalPlayer.Character.CurrentVehicle.Model.IsBike || Game.LocalPlayer.Character.CurrentVehicle.Model.IsQuadBike))
            {
                _steeringVeh = Game.LocalPlayer.Character.CurrentVehicle;
                if (Game.LocalPlayer.Character.CurrentVehicle.SteeringAngle > 20)
                {
                    _steeringAngle = 40;
                }
                else if (Game.LocalPlayer.Character.CurrentVehicle.SteeringAngle < -20)
                {
                    _steeringAngle = -40;
                }
                else if (Game.LocalPlayer.Character.CurrentVehicle.SteeringAngle > 5 || Game.LocalPlayer.Character.CurrentVehicle.SteeringAngle < -5)
                {
                    _steeringAngle = Game.LocalPlayer.Character.CurrentVehicle.SteeringAngle;
                }
            }

            if (EntityExtensions.Exists(_steeringVeh) && (player.IsOnFoot || NativeFunction.CallByHash<bool>(0x5721B434AD84D57A, _steeringVeh)))
            {
                _steeringVeh.SteeringAngle = _steeringAngle;
            }
        }
        #endregion

        public static void Process()
        {
            while (true)
            {
                GameFiber.Yield();

                EngineControl();
                if (ConfigHandler.BlinkersEnabled == true)
                {
                    KeyPolling();
                    VehicleIndicators();
                }
                if (ConfigHandler.TireRentainment == true) TireRentainment();
            }
        }
    }
}