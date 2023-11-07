using Common.Client;
using Common.Client.Elements;
using Common.Client.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Handlers;
using System;
using System.Collections.Generic;

namespace SimpleCTRL.Threads
{
    static class SpecialModesManager
    {
        #region Variables
        private static bool notified, hotNotify;
        private static readonly List<VehicleClass> ignoredClasses = new List<VehicleClass> { VehicleClass.Boat, VehicleClass.Helicopter, VehicleClass.Plane, VehicleClass.Cycle, VehicleClass.Military, VehicleClass.Rail, VehicleClass.Utility };

        private static bool pedInSameVehicleLast, isBrakingForward, isBrakingReverse, isRepairing, prompt;
        public static Vehicle _currentVehicle, _lastVehicle, _repairedVehicle;
        private static float _fCollisionDamageMult, _fDeformationDamageMult, _fEngineDamageMult = 0f;
        private static float _fBrakeForce = 1f;

        public static float healthEngineLast, healthEngineCurrent, healthEngineNew = 1000f;
        private static float healthEngineDelta, healthEngineDeltaScaled = 0f;

        public static float healthBodyLast, healthBodyCurrent, healthBodyNew = 1000f;
        private static float healthBodyDelta, healthBodyDeltaScaled = 0f;

        public static float healthPetrolTankLast, healthPetrolTankCurrent, healthPetrolTankNew = 1000f;
        private static float healthPetrolTankDelta, healthPetrolTankDeltaScaled = 0f;

        public static List<Blip> mechanicBlips = new List<Blip>();

        private static readonly string repairAnimDict = "anim@amb@clubhouse@tutorial@bkr_tut_ig3@";
        private static readonly string repairAnimString = "machinic_loop_mechandplayer";
        #endregion

        #region Brake Lights
        private static void BrakeLights()
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false)) 
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                Vehicle lastVehicle = Game.LocalPlayer.LastVehicle;

                if (currentVehicle.Exists() && currentVehicle.Speed < 0.1)
                {
                    API.SetVehicleBrakeLights(currentVehicle, true);
                }
                else if (lastVehicle.Exists() && lastVehicle.Speed < 0.1)
                {
                    API.SetVehicleBrakeLights(currentVehicle, true);
                }
            }
        }
        #endregion

        #region Heat Brakes
        public static void RegisterDecor()
        {
            Decorators.Initialize();
            Decorators.Register("brakeHeat", DecoratorType.Int);
        }

        private static void HeatBrakes()
        {
            // Unsure if its disabling the brake correctly
            var process = new GameFiber(delegate
            {
                try
                {
                    Vehicle veh = Game.LocalPlayer.Character.CurrentVehicle;

                    if (!EntityExtensions.Exists(veh) || ignoredClasses.Contains(veh.Class) || veh.Driver != Game.LocalPlayer.Character)
                    {
                        GameFiber.Sleep(2000);
                        return;
                    }

                    int hotBrakes = 0;
                    int oldBrakeValue = -1;
                    if (API.DecorExistOn(veh, "brakeHeat"))
                    {
                        hotBrakes = API.DecorGetInt(veh, "brakeHeat");
                        oldBrakeValue = hotBrakes;
                    }

                    if (hotBrakes < 5)
                    {
                        notified = hotNotify = false;
                    }

                    if (veh.Speed > 5f && veh.CurrentGear != 0 && Controls.IsControlPressed(GameControl.VehicleBrake))
                    {
                        if (hotBrakes > 10000)
                        {
                            Game.DisplayHelp("~r~Your brakes are disabled due to being too hot.");
                            Game.DisableControlAction(0, GameControl.VehicleBrake, true);
                        }
                        else if (hotBrakes > 5000)
                        {
                            if (hotBrakes % 2 == 0)
                            {
                                if (!hotNotify)
                                {
                                    Game.DisplayNotification("~y~Your brakes are ~r~REALLY ~y~getting hot!");
                                    notified = true;
                                    hotNotify = true;
                                }
                                Game.DisableControlAction(0, GameControl.VehicleBrake, true);
                            }
                        }
                        else if (hotBrakes > 3500)
                        {
                            if (hotBrakes % 4 == 0)
                            {
                                Game.DisableControlAction(0, GameControl.VehicleBrake, true);
                            }
                        }
                        else if (hotBrakes > 2500 && hotBrakes % 10 == 0)
                        {
                            if (!notified)
                            {
                                Game.DisplayNotification("~y~Your brakes are getting hot!");
                                notified = true;
                            }
                            Game.DisableControlAction(0, GameControl.VehicleBrake, true);
                        }

                        hotBrakes += GetBrakePressure(API.GetControlValue(0, 72));
                        if (Controls.IsControlPressed(GameControl.VehicleAccelerate))
                        {
                            hotBrakes += 5;
                        }

                        API.SetVehicleBrakeLights(veh, true);
                    }

                    if (Controls.IsControlPressed(GameControl.VehicleHandbrake) && veh.Speed > 2f && hotBrakes > 1000 && hotBrakes % 4 == 0)
                    {
                        Game.DisableControlAction(0, GameControl.VehicleHandbrake, true);
                    }

                    if (veh.Mods.BrakesModIndex > 1)
                    {
                        hotBrakes -= (int)Math.Round((double)GetBrakePressure(API.GetControlValue(0, 72)) / 3);
                    }

                    if (veh.IsInWater && hotBrakes < 200)
                    {
                        hotBrakes -= 25;
                    }

                    if (hotBrakes > 0)
                    {
                        if (new Random(100).Next() < 34)
                        {
                            hotBrakes -= 4;
                        }
                        hotBrakes -= 1;
                    }

                    if (hotBrakes < 0)
                    {
                        hotBrakes = 0;
                    }

                    // Basically ignores updating the decor if the brakes are cold and unused this tick
                    if (oldBrakeValue != hotBrakes)
                    {
                        API.DecorSetInt(veh, "brakeHeat", hotBrakes);
                    }
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"An exception occurred: {ex.Message}");
                }
            }, "");
            process.Start();
        }

        private static int GetBrakePressure(int pressure)
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
        #endregion

        #region Repair Tick
        private static void RepairTick()
        {
            if (_lastVehicle != null)
            {
                Ped player = Game.LocalPlayer.Character;
                if (isRepairing || _lastVehicle.EngineHealth > ConfigHandler.DegradingFailureThreshold)
                {
                    prompt = false;
                    return;
                }

                if (prompt)
                {
                    int boneIndex = _lastVehicle.GetBoneIndex("bonnet");
                    if (player.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) < 1.2f)
                    {
                        prompt = false;
                    }

                    Text.Draw3D(_lastVehicle.GetBonePosition(boneIndex) + new Vector3(0f, 0f, 0.5f), "Move near here to repair", 0.08f);
                }

                if (_lastVehicle != _repairedVehicle && !Game.LocalPlayer.Character.IsInAnyVehicle(false))
                {
                    int boneIndex = _lastVehicle.GetBoneIndex("bonnet");
                    Text.Draw3D(_lastVehicle.GetBonePosition(boneIndex), "Press [E] to repair", 0.065f);
                }

                if (Game.IsKeyDown(System.Windows.Forms.Keys.E) && !Game.LocalPlayer.Character.IsInAnyVehicle(false))
                {
                    int boneIndex = _lastVehicle.GetBoneIndex("bonnet");
                    if (player.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) > 1.6f)
                    {
                        Game.LogTrivial($"Repair key pressed, but distance from engine is too great ({Math.Round(player.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)), 3)}");
                        prompt = true;
                    }
                    else
                    {
                        prompt = false;
                        if (_lastVehicle.OilLevel() <= 0)
                        {
                            Game.LogTrivial($"Failed to repair: too damaged. {_lastVehicle.OilLevel()}");
                            Game.DisplayNotification("~r~Your vehicle was too badly damaged. Unable to repair!");
                            return;
                        }
                        else if (_lastVehicle.EngineHealth > ConfigHandler.CascadingFailureThreshold + 5)
                        {
                            Game.LogTrivial($"Failed to repair: not enough damage. {_lastVehicle.EngineHealth}");
                            Game.DisplayNotification("~y~" + GetRandom(ConfigHandler.NoFixMessages));
                            return;
                        }

                        Game.LogTrivial("Attempting to repair");
                        isRepairing = true;
                        player.Heading = _lastVehicle.Heading - 180f;

                        player.Tasks.ClearImmediately();
                        _lastVehicle.Doors[4].Open(true);
                        player.Tasks.PlayAnimation(repairAnimDict, repairAnimString, 5f, AnimationFlags.UpperBodyOnly);
                        Game.DisplaySubtitle("Attempting to repair vehicle", 6000);
                        NativeFunction.CallByHash<int>(0x6E13FC662B882D1D, _lastVehicle, 1); // SET_VEHICLE_TYRE_FIXED
                        GameFiber.Wait(6000);
                        if (isRepairing)
                        {
                            player.Tasks.ClearImmediately();
                            _lastVehicle.Doors[4].Close(true);
                            isRepairing = false;
                            if (EntityExtensions.Exists(_repairedVehicle) && _repairedVehicle == _lastVehicle)
                            {
                                Game.LogTrivial("Failed to repair: already repaired this vehicle");
                                Game.DisplayNotification("~y~" + GetRandom(ConfigHandler.NoFixMessages));
                                return;
                            }
                            if (_lastVehicle.OilLevel() < 2f)
                            {
                                Game.LogTrivial("Failed to repair: ran oil pan dry");
                                Game.DisplayNotification("~r~You were unable to repair the vehicle. The oil pan looks all dried up.");
                                return;
                            }
                            if (_lastVehicle.FuelLevel > 1f)
                            {
                                _lastVehicle.IsDriveable = true;
                            }
                            _lastVehicle.EngineHealth = ConfigHandler.CascadingFailureThreshold + 5;
                            healthEngineLast = ConfigHandler.CascadingFailureThreshold + 5;
                            API.SetVehicleMaxSpeed(_lastVehicle, 500.01f);
                            Game.LogTrivial($"Vehicle repaired! Engine health now {healthEngineLast}");
                            Game.DisplayNotification("~g~" + GetRandom(ConfigHandler.FixMessages) + ", now get to a mechanic!");
                            _repairedVehicle = _lastVehicle;
                        }
                    }
                }
            }
        }

        private static bool IsDead() => Game.LocalPlayer.Character.IsDead || API.DecorGetBool(Game.LocalPlayer.Character, "IsDead");
        private static string GetRandom(this List<string> list) => list[new Random().Next(list.Count)];
        #endregion

        #region Flip Tick
        private static void FlipTick()
        {
            if (_lastVehicle != null)
            {

                int boneIndex = _lastVehicle.GetBoneIndex("bonnet");
                if (isRepairing && (Game.IsPaused || IsDead() || Game.LocalPlayer.Character.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) > 1.5f))
                {
                    isRepairing = false;
                    Game.LocalPlayer.Character.Tasks.Clear();
                    _lastVehicle.Doors[4].Close(true);
                    return;
                }
            }

            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                #region Prevent Automatic Reversing
                if (ConfigHandler.PreventAutomaticReversing == true)
                {
                    if (Game.LocalPlayer.Character?.CurrentVehicle?.Class != VehicleClass.Boat)
                    {
                        Vehicle veh = Game.LocalPlayer.Character.CurrentVehicle;

                        if (NativeFunction.CallByHash<Vector3>(0x9A8D700A51CB7B0D, veh, true).Y >= 1f && API.GetControlValue(2, 72) > 127)
                        {
                            isBrakingForward = true;
                        }

                        if (NativeFunction.CallByHash<Vector3>(0x9A8D700A51CB7B0D, veh, true).Y <= -1f && API.GetControlValue(2, 71) > 127)
                        {
                            isBrakingReverse = true;
                        }

                        if (veh.Speed < 1f)
                        {
                            if (isBrakingForward)
                            {
                                Game.DisableControlAction(2, GameControl.VehicleBrake, true);
                                API.SetVehicleForwardSpeed(veh, veh.Speed * 0.98f);
                                API.SetVehicleBrakeLights(veh, true);
                            }
                            if (isBrakingReverse)
                            {
                                Game.DisableControlAction(2, GameControl.VehicleAccelerate, true);
                                API.SetVehicleForwardSpeed(veh, veh.Speed * 0.98f);
                                API.SetVehicleBrakeLights(veh, true);
                            }

                            // We let go of brake
                            if (isBrakingForward && API.GetDisabledControlNormal(2, 72) == 0)
                            {
                                isBrakingForward = false;
                            }
                            if (isBrakingReverse && API.GetDisabledControlNormal(2, 71) == 0)
                            {
                                isBrakingReverse = false;
                            }
                        }
                    }
                }
                #endregion

                #region Prevent Vehicle Flip
                if (ConfigHandler.PreventVehicleFlip == true)
                {
                    float roll = NativeFunction.CallByHash<float>(0x831E0242595560DF, Game.LocalPlayer.Character.CurrentVehicle); // GET_ENTITY_ROLL
                    if ((roll > 75f || roll < -75f) && Game.LocalPlayer.Character.CurrentVehicle.Speed < 2f)
                    {
                        Game.DisableControlAction(2, GameControl.VehicleMoveLeftRight, true);
                        Game.DisableControlAction(2, GameControl.VehicleMoveUpDown, true);
                    }
                }
                #endregion
            }

            if (!ConfigHandler.TorqueMultiplierEnable && !ConfigHandler.LimpMode && !ConfigHandler.PreventVehicleFlip)
            {
                return;
            }

            if (ConfigHandler.TorqueMultiplierEnable || ConfigHandler.LimpMode)
            {
                if (!pedInSameVehicleLast)
                {
                    return;
                }

                float factor = 1f;
                if (ConfigHandler.TorqueMultiplierEnable && healthEngineNew < 900)
                {
                    factor = (healthEngineNew + 200f) / 1100;
                }

                if (ConfigHandler.LimpMode && healthEngineNew < (ConfigHandler.EngineSafeGuard + 5))
                {
                    factor = ConfigHandler.LimpModeMultiplier;
                    API.SetVehicleMaxSpeed(_currentVehicle, 20f);
                }

                _currentVehicle.EngineTorqueMultiplier(factor);
            }
        }
        #endregion

        #region Main Tick
        private static void MainTick()
        {
            if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                if (pedInSameVehicleLast)
                {
                    _lastVehicle = Game.LocalPlayer.LastVehicle;

                    if (EntityExtensions.Exists(_lastVehicle))
                    {
                        if (ConfigHandler.DeformationMultiplier != -1)
                        {
                            _lastVehicle.HandlingData.DeformationDamageMultiplier = _fDeformationDamageMult; // Restore deformation multiplier
                        }

                        _lastVehicle.HandlingData.BrakeForce = _fBrakeForce; // Restore Brake Force multiplier

                        if (ConfigHandler.WeaponsDamageMultiplier != 1)
                        {
                            _lastVehicle.HandlingData.WeaponDamageMultiplier = ConfigHandler.WeaponsDamageMultiplier; // Since we are out of the vehicle, we should no longer compensate for bodyDamageFactor
                        }
                        _lastVehicle.HandlingData.CollisionDamageMultiplier = _fCollisionDamageMult; // Restore the original CollisionDamageMultiplier
                        _lastVehicle.HandlingData.EngineDamageMultiplier = _fEngineDamageMult; // Restore the original EngineDamageMultiplier
                    }

                }
                pedInSameVehicleLast = false;
                return;
            }


            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                _currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                Vehicle veh = _currentVehicle;

                float classMultiplier = ConfigHandler.ClassDamageMultiplier[(int)veh.Class];

                healthEngineCurrent = veh.EngineHealth;
                if (healthEngineCurrent == 1000f)
                {
                    healthEngineLast = 1000f;
                }

                healthEngineNew = healthEngineCurrent;
                healthEngineDelta = healthEngineLast - healthEngineCurrent;
                healthEngineDeltaScaled = healthEngineDelta * ConfigHandler.DamageFactorEngine * classMultiplier;

                healthBodyCurrent = API.GetVehicleBodyHealth(veh);
                if (healthBodyCurrent == 1000f)
                {
                    healthBodyLast = 1000f;
                }
                healthBodyNew = healthBodyCurrent;
                healthBodyDelta = healthBodyLast - healthBodyCurrent;
                healthBodyDeltaScaled = healthBodyDelta * ConfigHandler.DamageFactorBody * classMultiplier;

                healthPetrolTankCurrent = veh.FuelTankHealth;
                if (healthPetrolTankCurrent == 1000f)
                {
                    healthPetrolTankLast = 1000f;
                }
                healthPetrolTankNew = healthPetrolTankCurrent;
                healthPetrolTankDelta = healthPetrolTankLast - healthPetrolTankCurrent;
                healthPetrolTankDeltaScaled = healthPetrolTankDelta * ConfigHandler.DamageFactorPetrolTank * classMultiplier;

                if (healthEngineCurrent > ConfigHandler.EngineSafeGuard + 1 && veh.FuelLevel > 1f)
                {
                    veh.IsDriveable = true;
                }

                if (healthEngineCurrent <= ConfigHandler.EngineSafeGuard && (!ConfigHandler.LimpMode || veh.OilLevel() < 3f) && !API.IsVehicleTyreBurst(veh, 1, true))
                {
                    veh.IsDriveable = false;
                    NativeFunction.CallByHash<int>(0xEC6A202EE4960385, veh, 1, true, 1000f); // SET_VEHICLE_TYRE_BURST
                }

                if (_currentVehicle != _lastVehicle)
                {
                    pedInSameVehicleLast = false;
                }

                if (pedInSameVehicleLast)
                {
                    if (healthEngineCurrent != 1000f || healthBodyCurrent != 1000f || healthPetrolTankCurrent != 1000f)
                    {
                        // Combine the delta values (Get the largest of the three)
                        float healthEngineCombinedDelta = Math.Max(healthEngineDeltaScaled, Math.Max(healthBodyDeltaScaled, healthPetrolTankDeltaScaled));

                        // If huge damage, scale back a bit
                        if (healthEngineCombinedDelta > (healthEngineCurrent - ConfigHandler.EngineSafeGuard))
                        {
                            healthEngineCombinedDelta *= 0.7f;
                        }

                        // If complete damage, but not catastrophic(ie.explosion territory) pull back a bit, to give a couple seconds of engine runtime before dying
                        if (healthEngineCombinedDelta > healthEngineCurrent)
                        {
                            healthEngineCombinedDelta = healthEngineCurrent - (ConfigHandler.CascadingFailureThreshold / 5);
                        }

                        // ======= Calculate new value =======
                        healthEngineNew = healthEngineLast - healthEngineCombinedDelta;

                        // ======= Sanity Check on new values and further manipulations
                        //  If somewhat damaged, slowly degrade until slightly before cascading failure sets in, then stop

                        if (healthEngineNew > (ConfigHandler.DegradingFailureThreshold + 5) && (veh.Class == VehicleClass.Emergency ? healthEngineNew < 850f : healthEngineNew < 950f) && veh.IsEngineOn && veh.Speed > 2f)
                        {
                            healthEngineNew -= (0.02f * ConfigHandler.DegradingHealthSpeedFactor);
                        }

                        // If Damage is near catastrophic, cascade the failure
                        if (healthEngineNew < ConfigHandler.CascadingFailureThreshold && veh.IsEngineOn && veh.Speed > 2f)
                        {
                            healthEngineNew -= (0.05f * ConfigHandler.CascadingFailureSpeedFactor);
                        }

                        // Prevent Engine going to or below zero. Ensures you can reenter a damaged car.
                        if (healthEngineNew < ConfigHandler.EngineSafeGuard)
                        {
                            healthEngineNew = ConfigHandler.EngineSafeGuard;

                        }

                        if (healthBodyNew < 0f)
                        {
                            healthBodyNew = 0f;
                        }
                    }
                    else
                    {
                        // Vehicle is fixed?
                        _repairedVehicle = null;
                        API.SetVehicleMaxSpeed(_currentVehicle, 500.01f);
                    }
                }
                else
                {
                    // Just got into a vehicle. Damage cannot be multipled this round

                    // Set vehicle handling meta
                    _fDeformationDamageMult = veh.HandlingData.DeformationDamageMultiplier;
                    _fBrakeForce = veh.HandlingData.BrakeForce;
                    if (ConfigHandler.DeformationMultiplier != -1)
                    {
                        veh.HandlingData.DeformationDamageMultiplier = (float)Math.Pow(_fDeformationDamageMult, ConfigHandler.DeformationExponent) * ConfigHandler.DeformationMultiplier; // Multiply by our factor
                    }

                    if (ConfigHandler.WeaponsDamageMultiplier != -1)
                    {
                        veh.HandlingData.WeaponDamageMultiplier = ConfigHandler.WeaponsDamageMultiplier / ConfigHandler.DamageFactorBody; // Set weaponsDamageMultiplier and compensate for damageFactorBody
                    }

                    _fCollisionDamageMult = veh.HandlingData.CollisionDamageMultiplier;
                    // Modify it by pulling all numbers to 1f
                    veh.HandlingData.CollisionDamageMultiplier = (float)Math.Pow(_fCollisionDamageMult, ConfigHandler.CollisionDamageExponent);

                    _fEngineDamageMult = veh.HandlingData.EngineDamageMultiplier;
                    veh.HandlingData.EngineDamageMultiplier = (float)Math.Pow(_fEngineDamageMult, ConfigHandler.EngineDamageExponent);

                    // If body damage catastrophic, reset somewhat so we can get new damage to multiply
                    if (healthBodyCurrent < ConfigHandler.CascadingFailureThreshold)
                    {
                        healthBodyNew = ConfigHandler.CascadingFailureThreshold;
                    }

                    pedInSameVehicleLast = true;
                }

                // Set the actual values
                if (healthEngineNew != healthEngineCurrent)
                {
                    veh.EngineHealth = healthEngineNew;
                }
                if (healthBodyNew != healthBodyCurrent)
                {
                    NativeFunction.CallByHash<int>(0xB77D05AC8C78AADB, veh, healthBodyNew); // SET_VEHICLE_BODY_HEALTH
                }
                if (healthPetrolTankNew != healthPetrolTankCurrent)
                {
                    veh.FuelTankHealth = healthPetrolTankNew;
                }

                // Store current values, so we can calculate delta next time
                healthEngineLast = healthEngineNew;
                healthBodyLast = healthBodyNew;
                healthPetrolTankLast = healthPetrolTankNew;
                _lastVehicle = _currentVehicle;
            }
        }
        #endregion

        #region CreateMechanicBlips
        private static void CreateMechanicBlips()
        {
            foreach (RepairShop repairShop in ConfigHandler.RepairShops)
            {
                try
                {
                    Blip blip = new Blip(repairShop.Position);
                    blip.Sprite = (BlipSprite)446;
                    blip.Scale = 1f;
                    NativeFunction.CallByHash<int>(0xBE8BE4FE60E27B72, blip, true); // SET_BLIP_AS_SHORT_RANGE
                    blip.Name = "Repair Shop";

                    mechanicBlips.Add(blip);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial("Error creating reapir shop blips: " + ex.Message);
                    Game.LogTrivial(ex.StackTrace);
                }
            }
        }
        #endregion

        #region IsNearMechanic
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
        #endregion

        #region ProccessPlayer
        public static void ProcessPlayer()
        {
            RegisterDecor();
            CreateMechanicBlips();

            while (true)
            {
                GameFiber.Yield();

                BrakeLights();
                HeatBrakes();
                RepairTick();
                FlipTick();
                MainTick();
            }
        }
        #endregion

        #region Models
        public class RepairShop
        {
            public string Location { get; set; }
            public Vector3 Position { get; set; }
            public float UseRange { get; set; }

            public RepairShop(float x, float y, float z, float t, string description)
            {
                Location = description;
                Position = new Vector3(x, y, z);
                UseRange = t;
            }
        }
        #endregion
    }
}