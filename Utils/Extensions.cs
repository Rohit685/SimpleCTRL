using Common;
using Common.Native;
using Rage;
using Rage.Native;
using RAGENativeUI;
using SimpleCTRL.API;
using SimpleCTRL.Handlers;
using SimpleCTRL.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SimpleCTRL.Utils
{
    internal static class Extensions
    {
        public static RepairShop IsNearMechanic()
        {
            foreach (RepairShop repairShop in ConfigHandler.RepairShops)
            {
                if (Game.LocalPlayer.Character.DistanceTo(repairShop.Position) < repairShop.UseRange)
                {
                    return repairShop;
                }
            }
            return null;
        }

        public static int GetBrakePressure(int pressure)
        {
            if (pressure > 230)
            {
                return 10;
            }
            else if (pressure > 205)
            {
                return 8;
            }
            else if (pressure > 180)
            {
                return 6;
            }
            else if (pressure > 155)
            {
                return 4;
            }
            return 2;
        }

        public static bool IsDrivingAVehicle(this Ped playerPed)
        {
            Vehicle v = playerPed.CurrentVehicle;
            return !(!EntityExtensions.Exists(playerPed) || playerPed.IsDead || !EntityExtensions.Exists(v)
) && v.Driver == playerPed && !new List<VehicleClass> { VehicleClass.Plane, VehicleClass.Helicopter, VehicleClass.Cycle, VehicleClass.Rail }.Contains(v.Class);
        }

        internal static int ToInt32(this string text, [CallerMemberName] string callingMethod = null)
        {
            int i = 0;
            try
            {
                i = Convert.ToInt32(text);
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.Message);
            }
            return i;
        }




















        public static void LockTransmission(Vehicle playerVeh, bool toggle)
        {
            NativeFunction.CallByHash<int>(0x684785568EF26A22, playerVeh, toggle); // SET_VEHICLE_HANDBRAKE
        }

        public static void CreateVehicleBlip(Vehicle playerVeh)
        {
            int parkedVehicleBlip = NativeFunction.CallByHash<int>(0xBC8DBDCA2436F7E8, playerVeh); // GET_BLIP_FROM_ENTITY
            if (!NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST
            {
                parkedVehicleBlip = NativeFunction.CallByHash<int>(0x5CDE92C702A8FCE7, playerVeh); // ADD_BLIP_FOR_ENTITY

                if (NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST
                {
                    NativeFunction.CallByHash<int>(0xDF735600A4696DAF, parkedVehicleBlip, 326); // SET_BLIP_SPRITE
                    NativeFunction.CallByHash<int>(0xD38744167B2FA257, parkedVehicleBlip, 0.7f); // SET_BLIP_SCALE
                    NativeFunction.CallByHash<int>(0xF9113A30DE5C6670, "STRING"); // BEGIN_TEXT_COMMAND_SET_BLIP_NAME
                    NativeFunction.CallByHash<int>(0x6C188BE134E074AA, "Personal Vehicle"); // ADD_​TEXT_​COMPONENT_​SUBSTRING_​PLAYER_​NAME
                    NativeFunction.CallByHash<int>(0xBC38B49BCB83BC9B, parkedVehicleBlip); // END_TEXT_COMMAND_SET_BLIP_NAME
                    NativeFunction.CallByHash<int>(0x6F6F290102C02AB4, parkedVehicleBlip, true); // SET_BLIP_AS_FRIENDLY
                }
            }
        }

        public static void DeleteVehicleBlip(Vehicle playerVeh)
        {
            int parkedVehicleBlip = NativeFunction.CallByHash<int>(0xBC8DBDCA2436F7E8, playerVeh); // GET_BLIP_FROM_ENTITY
            if (NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST 
            {
                unsafe
                {
                    NativeFunction.CallByHash<int>(0x86A652570E5F25DD, &parkedVehicleBlip); // REMOVE_BLIP
                }
            }
        }



        #region Indicator Modes
        public static void HandleNormalMode(ref VehicleIndicatorLightsStatus intendedStatus, ref VehicleIndicatorLightsStatus status)
        {
            try
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (EntityExtensions.Exists((IHandleable)(object)currentVehicle))
                {
                    if ((int)intendedStatus == 1 && currentVehicle.IsEngineOn)
                    {
                        if ((int)status == 1)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)1;
                        }
                        currentVehicle.IndicatorLightsStatus = status;
                    }
                    else if ((int)intendedStatus == 2 && currentVehicle.IsEngineOn)
                    {
                        if ((int)status == 2)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)2;
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
                }
                intendedStatus = (VehicleIndicatorLightsStatus)0;
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }

        public static void HandleTurnOffAtTurnMode(ref VehicleIndicatorLightsStatus intendedStatus, ref uint turnOffAt, ref VehicleIndicatorLightsStatus status, ref float initialHeading)
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

        // WIP Function

        public static void HandleAutomaticTurnMode(float initialHeading)
        {
            try
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (EntityExtensions.Exists((IHandleable)(object)currentVehicle))
                {
                    initialHeading = ((Entity)currentVehicle).Heading;

                    Game.LogTrivial("initialHeading: " + initialHeading);

                    float headingChange = Math.Abs(((Entity)currentVehicle).Heading - initialHeading);

                    Game.LogTrivial("headingChange: " + headingChange);

                    float turnThreshold = 10f; 

                    if (headingChange > turnThreshold)
                    {
                        if (currentVehicle.SteeringAngle < 0)
                        {
                            Game.LogTrivial("Turn detected: Left turn");
                        }
                        else if (currentVehicle.SteeringAngle > 0)
                        {
                            Game.LogTrivial("Turn detected: Right turn");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }
        #endregion

        public static void InitFuel(this Vehicle vehicle)
        {
            Current.VehicleFuelLevelInitialized = true;
            Current.VehicleFuelCapacity = MaxFuelLevel(vehicle);
            if (!N.DecorExistOn(vehicle, "_Fuel_Level"))
            {
                if (IsAircraft(vehicle))
                {
                    N.DecorSetFloat(vehicle, "_Fuel_Level", Current.VehicleFuelCapacity);
                }
                else
                {
                    N.DecorSetFloat(vehicle, "_Fuel_Level", RandomizeFuelLevel(vehicle, Current.VehicleFuelCapacity));
                }
            }
            vehicle.FuelLevel = N.DecorGetFloat(vehicle, "_Fuel_Level");
        }

        public static bool IsBoat(this Vehicle vehicle)
        {
            if (!vehicle.Model.IsBoat && !(vehicle.DisplayName() == "SUBMERS"))
            {
                return vehicle.DisplayName() == "SUBMERS2";
            }
            return true;
        }

        public static bool IsElectric(this Vehicle vehicle)
        {
            List<string> electricVehicles = new List<string>
            {
                "airtug", "caddy", "caddy2", "caddy3", "cyclone", "dilettan", "khamel", "neon", "raiden", "surge",
                "tezeract", "voltic"
            };
            return electricVehicles.Contains(vehicle.DisplayName().ToLower());
        }

        public static float RandomizeFuelLevel(this Vehicle veh, float fuelCapacity)
        {
            float randomizedFuelLevel;
            if (new Random().Next(0, 4) != 0) 
            {
                float min = fuelCapacity / 4f;
                float max = fuelCapacity / 2f;
                randomizedFuelLevel = (float)(new Random().NextDouble() * (double)(max - min) + (double)min);
            }
            else
            {
                randomizedFuelLevel = fuelCapacity; 
            }
            return randomizedFuelLevel;
        }


        public static void SetFuelLevel(this Vehicle vehicle, float fuelLevel)
        {
            float max = MaxFuelLevel(vehicle);
            if (fuelLevel > max)
            {
                fuelLevel = max;
            }
            vehicle.FuelLevel = fuelLevel;
            N.DecorSetFloat(vehicle, "_Fuel_Level", fuelLevel);
        }

        public static float GetFuelLevel(this Vehicle vehicle)
        {
            if (N.DecorExistOn(vehicle, "_Fuel_Level"))
            {
                return N.DecorGetFloat(vehicle, "_Fuel_Level");
            }
            return 65f;
        }

        public static float MaxFuelLevel(this Vehicle vehicle)
        {
            float maxFuel = vehicle.HandlingData.PetrolTankVolume;
            if (maxFuel == 0f)
            {
                return 65f;
            }
            return maxFuel;
        }

        public static AircraftFuelSpecs GetAircraftFuelSpecs(this Vehicle vehicle)
        {
            AircraftFuelSpecs afs = null;
            if (IsAircraft(vehicle))
            {
                afs = VehicleProperties.AircraftSpecs.FirstOrDefault((AircraftFuelSpecs x) => x.Models.Any((string y) => y.ToUpper() == vehicle.DisplayName().ToUpper()));
                if (afs == null)
                {
                    afs = new AircraftFuelSpecs();
                    afs.Models.Add(vehicle.DisplayName());
                }
            }
            return afs;
        }

        public static void ControlAircraftEngine(this Vehicle vehicle)
        {
            if (Game.IsControlJustPressed(0, GameControl.VehicleFlyUnderCarriage))
            {
                ToggleEngine(vehicle);
            }
        }

        public static void ToggleEngine(this Vehicle vehicle)
        {
            SetEngine(vehicle, !vehicle.IsEngineOn);
        }

        public static void SetEngine(this Vehicle vehicle, bool engineOn)
        {
            vehicle.IsDriveable = engineOn;
            N.SetVehicleEngineOn(vehicle, engineOn, false, true);
            Current.AircraftEngineOn = engineOn;
        }

        internal static void ConsumeRoadVehicleFuel(this Vehicle vehicle)
        {
            float fuel = GetFuelLevel(vehicle);
            if (fuel > 0f && vehicle.IsEngineOn)
            {
                if (!Current.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                {
                    Current.TripInfos[vehicle.Handle.ToInt32()] = new TripInfo();
                }
                if (Current.TripInfos[vehicle.Handle.ToInt32()].LastPosition == Vector3.Zero)
                {
                    Current.TripInfos[vehicle.Handle.ToInt32()].LastPosition = vehicle.Position;
                }
                float distance = Vector3.Distance(Current.TripInfos[vehicle.Handle.ToInt32()].LastPosition, vehicle.Position);
                float kmTravelled = Math.Abs(distance / 1000f) * 5.2f;
                float fuelUsed = Functions.ConsumeCarFuel(vehicle, kmTravelled);
                Current.TripInfos[vehicle.Handle.ToInt32()].DistanceTraveledKM += kmTravelled;
                Current.TripInfos[vehicle.Handle.ToInt32()].LastPosition = vehicle.Position;
                Current.TripInfos[vehicle.Handle.ToInt32()].FuelConsumed += fuelUsed;
                fuel -= fuelUsed;
                if (fuel < 0.2f && IsElectric(vehicle) && vehicle.IsDriveable)
                {
                    Game.DisplayNotification("Your battery has run out of juice!");
                    vehicle.IsDriveable = false;
                }
                fuel = ((fuel < 0f) ? 0f : fuel);
            }
            fuel = ProcessRefuelingCar(vehicle, fuel);
            SetFuelLevel(vehicle, fuel);
        }

        internal static float GetDistance(Vector3 position1, Vector3 position2)
        {
            return DistanceTo(position1, position2, useZ: true);
        }

        private static float ProcessRefuelingCar(this Vehicle vehicle, float fuel)
        {
            if (Current.GasStation != null && IsVehicleNearAnyPump(vehicle))
            {
                if (Game.LocalPlayer.Character.CurrentVehicle != null && IsPlayerDriving(vehicle))
                {
                    CustomUI.InstructToggleEngine();
                }
                if (Globals.RefuelingAllowed)
                {
                    if (fuel >= Current.VehicleFuelCapacity)
                    {
                        // CustomUI.InstructFullOrEmpty("Fuel tank full")
                        CustomUI.HideRefuel();
                    }
                    else
                    {
                        CustomUI.InstructRefuel();
                    }
                    // if (Game.IsControlPressed(0, GameControl.Context))
                    if (Controls.IsControlDownWithModifier(Controls.SimpleControls.REFUEL))
                    {
                        if (fuel < Current.VehicleFuelCapacity)
                        {
                            fuel += 0.045f;
                            Current.FuelAmountPumped += 0.045f;
                            if (!NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 3))
                            {
                                NativeFunction.CallByHash<int>(0xEA47FE3719165B94, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 2f, 8f, -1, 49, 0f);
                            }
                        }
                        else
                        {
                            if (NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 3))
                            {
                                Game.LocalPlayer.Character.Tasks.ClearSecondary();
                            }
                        }
                    }
                    // Game.IsControlJustReleased(0, GameControl.Context)
                    if ((!Globals.RefuelingAllowed || !Controls.IsControlDownWithModifier(Controls.SimpleControls.REFUEL)) && Current.FuelAmountPumped > 0f)
                    {
                        if (!IsElectric(vehicle))
                        {
                            if (ConfigHandler.RefuelNotification == true)
                            {
                                float gallonsPumped = Common.API.Math.ConvertLitresToGallons(Current.FuelAmountPumped);
                                string fuelMsg = $"Pumped {Math.Round(Current.FuelAmountPumped, 1)} L // {Math.Round(gallonsPumped, 1)} gallons";
                                Game.DisplayNotification("~o~[FUEL] ~w~" + fuelMsg);
                            }
                            if (Current.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                            {
                                TripInfo t = Current.TripInfos[vehicle.Handle.ToInt32()];
                                if (t.DistanceTraveledKM > 0f)
                                {
                                    if (ConfigHandler.RefuelNotification == true)
                                    {
                                        string fuelEcon = $"Average: {Math.Round(t.FuelEconomyInLPer100Km, 1)} L/100 km // {Math.Round(t.FuelEconomyInMPG, 1)} MPG";
                                        Game.DisplayNotification("~o~[FUEL] ~w~" + fuelEcon);
                                    }
                                }
                            }
                            if (Current.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                            {
                                Current.TripInfos[vehicle.Handle.ToInt32()].Reset(((Entity)vehicle).Position);
                            }
                            Current.FuelAmountPumped = 0f;
                        }
                        Game.LocalPlayer.Character.Tasks.ClearSecondary();
                    }
                }
                else
                {
                    if (NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 3))
                    {
                        Game.LocalPlayer.Character.Tasks.ClearSecondary();
                    }
                    // Game.IsControlJustPressed(0, GameControl.Context)
                    if (!Game.LocalPlayer.Character.IsOnFoot && Controls.IsControlDownWithModifier(Controls.SimpleControls.REFUEL) && IsPlayerDriving(vehicle))
                    {
                        Game.DisplayNotification("You must be on foot in order to refuel.");
                    }
                }
                if ((Game.LocalPlayer.Character.CurrentVehicle != null && IsPlayerDriving(vehicle)) || Globals.RefuelingAllowed)
                {
                    CustomUI.RenderInstructions();
                    if (!Globals.HudActive) 
                    {
                        NativeFunction.CallByHash<int>(0x67C540AA08E4A6F5, -1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", 1);
                    }
                    Globals.HudActive = true;
                }
            }
            else if (!Globals.RefuelingAllowed)
            {
                Globals.HudActive = false;
            }
            return fuel;
        }

        public static void ConsumeAircraftFuel(this Vehicle vehicle)
        {
            float fuel = GetFuelLevel(vehicle);
            if (fuel > 0f && vehicle.IsEngineOn)
            {
                TimeSpan timeElapsed = DateTime.UtcNow - Current.LastWorldTime;
                float fuelUsed = Functions.ConsumeFuelAircraft(vehicle, timeElapsed);
                fuel -= fuelUsed;
                fuel = ((fuel < 0f) ? 0f : fuel);
            }
            fuel = ProcessRefuelingAircraft(vehicle, fuel);
            SetFuelLevel(vehicle, fuel);
        }

        private static float ProcessRefuelingAircraft(this Vehicle vehicle, float fuel)
        {
            if (vehicle.IsInAir)
            {
                return fuel;
            }
            if (vehicle.Speed < 2f)
            {
                ControlAircraftEngine(vehicle);
            }
            if (IsVehicleNearAnyTankerOrFuelPump(vehicle))
            {
                CustomUI.InstructRefuel();
                // Game.IsControlPressed(0, GameControl.Context)
                if (Controls.IsControlDownWithModifier(Controls.SimpleControls.REFUEL))
                {
                    float pumpRate = MaxFuelLevel(vehicle) * 0.001f;

                    if (fuel + pumpRate <= MaxFuelLevel(vehicle))
                    {
                        fuel += pumpRate;
                        Current.FuelAmountPumped += pumpRate;
                    }
                }
                // Game.IsControlJustReleased(0, GameControl.Context)
                if (!Controls.IsControlDownWithModifier(Controls.SimpleControls.REFUEL) && Current.FuelAmountPumped > 0f)
                {
                    if (ConfigHandler.RefuelNotification == true)
                    {
                        float gallonsPumped = Common.API.Math.ConvertLitresToGallons(Current.FuelAmountPumped);
                        string fuelMsg = $"Pumped {Math.Round(Current.FuelAmountPumped, 1)} L // {Math.Round(gallonsPumped, 1)} gallons";
                        Game.DisplayNotification("~o~[FUEL] ~w~" + fuelMsg);
                    }
                    Current.FuelAmountPumped = 0f;
                }
                if (Game.LocalPlayer.Character.CurrentVehicle != null)
                {
                    CustomUI.RenderInstructions();
                    if (!Globals.HudActive) //  Due to fix sound check only being called once per load plugin
                    {
                        NativeFunction.CallByHash<int>(0x67C540AA08E4A6F5, -1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", 1);
                    }
                    Globals.HudActive = true;
                }
            } 
            return fuel;
        }





        public static int ToInt32(this PoolHandle poolHandle)
        {
            return (int)poolHandle.Value;
        }

        public static string FormatKeyBinding(ControllerButtons key) => $"{key.GetInstructionalId()}";

        public static string FormatKeyBinding(Keys key) => $"{key.GetInstructionalId()}";

        #region Ped Extensions
        internal static void ManualRefuel(this Ped playerPed)
        {
            if (playerPed.LastVehicle.Exists())
            {
                Vector3 pos = playerPed.Position;
                Vehicle vehicle = playerPed.LastVehicle;
                Vector3 position = playerPed.LastVehicle.Position;
                if (!(position.DistanceToSquared(pos) <= 10f) || !(N.DecorExistOn(vehicle, "_Fuel_Level") || !vehicle.IsRoadVehicle() || vehicle.IsElectric()))
                {
                    return;
                }
                if (!Current.VehicleFuelLevelInitialized)
                {
                    vehicle.InitFuel();
                }
                float max = vehicle.MaxFuelLevel();
                float fuel = vehicle.GetFuelLevel();
                if (max - fuel < 0.2f)
                {
                    // CustomUI.InstructFullOrEmpty("Fuel tank full");
                }
                else if (fuel == 0f)
                {
                    // CustomUI.InstructFullOrEmpty("Fuel tank empty");
                }
                else
                {
                    return; // finsih this function later
                }
                Game.DisableControlAction(0, GameControl.Attack, true);
                Game.DisableControlAction(0, GameControl.Aim, true);
                Game.DisableControlAction(0, GameControl.AccurateAim, true);
                if (Game.IsControlPressed(0, GameControl.Attack) && !Game.IsControlPressed(0, GameControl.Aim))
                {
                    if (fuel < max)
                    {
                        // jerry can animation here
                        Current.FuelAmountPumped += 0.023f;
                        vehicle.SetFuelLevel(fuel + 0.023f);
                    }
                }
                else if (Game.IsControlPressed(0, GameControl.Aim) && !N.IsDisabledControlPressed((int)GameControl.Aim, 0))
                {
                    if (fuel > 0f)
                    {
                        //if (!API.IsEntityPlayingAnim(((PoolObject)playerPed).Handle, Globals.DictSiphoning, Globals.AnimSiphoning, 3))
                        //{
                        //    playerPed.Task.PlayAnimation(Globals.DictSiphoning, Globals.AnimSiphoning, 2f, 8f, -1, (AnimationFlags)1, 0f);
                        //    Current.FuelAmountSiphoned += 0.00125f;
                        //    vehicle.SetFuelLevel(fuel - 0.00125f);
                        //}
                        //else
                        //{
                        //    Current.FuelAmountSiphoned += 0.00125f;
                        //    vehicle.SetFuelLevel(fuel - 0.00125f);
                        //}
                    }
                    else
                    {
                        vehicle.SetFuelLevel(0f);
                        // playerPed.Task.ClearAnimation(Globals.DictSiphoning, Globals.AnimSiphoning);
                    }
                }
                if (Game.IsControlJustReleased(0, GameControl.VehicleAttack) && fuel >= max)
                {
                    vehicle.SetFuelLevel(max);
                    // Globals.JerryCanAnimation.RewindAndStop(playerPed);
                }
                if (Game.IsControlJustReleased(9, GameControl.Attack))
                {
                    // Globals.JerryCanAnimation.RewindAndStop(playerPed);
                }
                if (Game.IsControlJustReleased(0, GameControl.VehicleAim) && fuel <= 0f)
                {
                    vehicle.SetFuelLevel(0f);
                    // playerPed.Task.ClearAnimation(Globals.DictSiphoning, Globals.AnimSiphoning);
                }
                if (Game.IsControlJustReleased(9, GameControl.Aim))
                {
                    Game.DisableControlAction(0, GameControl.Attack, true);
                    Game.DisableControlAction(0, GameControl.Attack2, true);
                    // playerPed.Task.ClearAnimation(Globals.DictSiphoning, Globals.AnimSiphoning);
                }
                CustomUI.RenderInstructions();
                //if (!Globals.HudActive) //  Due to fix sound check only being called once per load plugin
                //{
                //    NativeFunction.CallByHash<int>(0x67C540AA08E4A6F5, -1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", 1);
                //}
                //Globals.HudActive = true;
            }
        }

        internal static bool IsDrivingRoadVehicle(this Ped playerPed)
        {
            Vehicle v = playerPed.CurrentVehicle;
            if (v == null)
            {
                v = playerPed.LastVehicle;
            }
            if (v == null)
            {
                return false;
            }
            if (EntityExtensions.Exists(playerPed) && !v.Model.IsBicycle && v.IsRoadVehicle() && (v.GetPedOnSeat((int)(VehicleSeat)(-1)) == playerPed || NativeFunction.CallByHash<int>(0x83F969AA1EE2A664, v, -1) == playerPed.Handle))
            {
                return v.IsAlive;
            }
            return false;
        }

        internal static bool IsFlyingAnAircraft(this Ped playerPed)
        {
            if (EntityExtensions.Exists(playerPed) && NativeFunction.CallByHash<bool>(0x9134873537FA419C, playerPed) && playerPed.CurrentVehicle.IsAircraft() && playerPed.CurrentVehicle.GetPedOnSeat((int)(VehicleSeat)(-1)) == playerPed)
            {
                return playerPed.CurrentVehicle.IsAlive;
            }
            return false;
        }
        #endregion

        #region Vector3Extensions
        public static float DistanceTo(this Vector3 v1, Vector3 v2)
        {
            return DistanceTo(v1, v2, useZ: true);
        }

        public static float DistanceTo2D(this Vector3 v1, Vector3 v2)
        {
            return DistanceTo(v1, v2, useZ: false);
        }

        public static float DistanceToSquared(this Vector3 point1, Vector3 point2)
        {
            float dx = point1.X - point2.X;
            float dy = point1.Y - point2.Y;
            float dz = point1.Z - point2.Z;

            return dx * dx + dy * dy + dz * dz;
        }

        private static float DistanceTo(Vector3 v1, Vector3 v2, bool useZ)
        {
            return NativeFunction.CallByHash<float>(0xF1B760881820C952, v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, useZ);
        }
        #endregion

        #region Vehicle Extensions
        internal static bool IsVehicleNearAnyPump(this Vehicle vehicle)
        {
            Vector3 fuelTankPos = GetVehicleTankPos(vehicle);
            if (Current.GasStation != null)
            {
                return Current.GasStation.Pumps.Any((GasPump x) => Vector3.DistanceSquared(x.Position, fuelTankPos) <= 20f);
            }
            return false;
        }

        private static Vector3 GetVehicleTankPos(Vehicle vehicle)
        {
            string[] vehicleFuelTankBones = Constants.VehicleFuelTankBones;
            int foundBoneIndex = -1;
            foreach (string boneName in vehicleFuelTankBones)
            {
                try
                {
                    int boneIndex = vehicle.GetBoneIndex(boneName);
                    if (boneIndex != -1 && vehicle.HasBone(boneIndex))
                    {
                        foundBoneIndex = boneIndex;
                        break;
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            return vehicle.GetBonePosition(foundBoneIndex); 
        }

        internal static bool IsVehicleNearAnyTankerOrFuelPump(this Vehicle vehicle)
        {
            if (ConfigHandler.AircraftUseAirportPumps)
            {
                AirportFuelPump airport = Functions.GetAirportFuelPumpInRange(vehicle.Position, 100f);
                if (airport != null && airport.Position != Vector3.Zero)
                {
                    return true;
                }
            }
            if (ConfigHandler.AircraftUseFuelTankers)
            {
                Vehicle tanker = FindNearbyTanker(vehicle);
                if (tanker != null && EntityExtensions.Exists(tanker))
                {
                    return vehicle.Position.DistanceTo(tanker.Position) <= 100f;
                }
                return false;
            }
            return false;
        }

        private static Vehicle FindNearbyTanker(Vehicle v)
        {
            Vehicle tanker = null;

            if (ConfigHandler.AircraftUseFuelTankers)
            {
                var nearbyEntities = World.GetEntities(v.Position, 100f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle);

                if (nearbyEntities.Any())
                {
                    tanker = nearbyEntities
                        .OfType<Vehicle>()
                        .FirstOrDefault(entity => entity != null &&
                           entity.Exists() &&
                           ConfigHandler.AircraftFuelTankers
                               .Any(x => x.Equals(entity.Model.Hash)));
                }
            }

            return tanker;
        }

        public static bool IsPlayerDriving(this Vehicle vehicle)
        {
            bool driving = false;
            if (Game.LocalPlayer.Character != null && EntityExtensions.Exists(Game.LocalPlayer.Character))
            {
                driving = !Game.LocalPlayer.Character.IsOnFoot && vehicle.Driver != null && EntityExtensions.Exists(vehicle.Driver) && vehicle.Driver.Handle == Game.LocalPlayer.Character.Handle;
            }
            return driving;
        }

        internal static bool IsRoadVehicle(this Vehicle vehicle)
        {
            if (!vehicle.Model.IsBicycle)
            {
                if (!vehicle.Model.IsCar && !vehicle.Model.IsBike)
                {
                    return vehicle.Model.IsQuadBike;
                }
                return true;
            }
            return false;
        }

        internal static bool IsAircraft(this Vehicle vehicle) => vehicle != null && (vehicle.Model.IsHelicopter || vehicle.Model.IsPlane);
        #endregion
    }
}