using Common;
using Common.UI.Elements;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Handlers;
using SimpleCTRL.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleCTRL.Threads
{
    static class SpecialModesManager
    {
        #region Fields
        private static bool notified, hotNotify;
        private static readonly List<VehicleClass> cantRepairOnFootClasses = new List<VehicleClass> { VehicleClass.Boat, VehicleClass.Commercial, VehicleClass.Helicopter, VehicleClass.Industrial, VehicleClass.Motorcycle, VehicleClass.Plane, VehicleClass.Service, VehicleClass.Rail, VehicleClass.Utility };
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

        private static readonly string repairAnimDict = "anim@amb@clubhouse@tutorial@bkr_tut_ig3@";
        private static readonly string repairAnimString = "machinic_loop_mechandplayer";
        #endregion

        #region Functions
        private static void BrakeLights()
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                Vehicle lastVehicle = Game.LocalPlayer.LastVehicle;

                if (currentVehicle.Exists() && currentVehicle.Speed < 0.1)
                {
                    if (Globals.isParked)
                    {
                        N.SetVehicleBrakeLights(currentVehicle, false);
                    }
                    else
                    {
                        N.SetVehicleBrakeLights(currentVehicle, true);
                    }
                }
                else if (lastVehicle.Exists() && lastVehicle.Speed < 0.1)
                {
                    if (Globals.isParked)
                    {
                        N.SetVehicleBrakeLights(lastVehicle, false);
                    }
                    else
                    {
                        N.SetVehicleBrakeLights(lastVehicle, true);
                    }
                }
            }
        }

        private static void HeatBrakes()
        {
            // Unsure if its disabling the brake correctly
            try
            {
                Vehicle veh = Game.LocalPlayer.Character.CurrentVehicle;

                if (!EntityExtensions.Exists(veh) || ignoredClasses.Contains(veh.Class) || veh.Driver != Game.LocalPlayer.Character)
                {
                    return;
                }

                int hotBrakes = 0;
                int oldBrakeValue = -1;
                if (N.DecorExistOn(veh, "brakeHeat"))
                {
                    hotBrakes = N.DecorGetInt(veh, "brakeHeat");
                    oldBrakeValue = hotBrakes;
                }

                if (hotBrakes < 5)
                {
                    notified = hotNotify = false;
                }

                if (veh.Speed > 5f && veh.CurrentGear != 0 && Game.IsControlPressed(0, GameControl.VehicleBrake))
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
                            if (ConfigHandler.BrakeOverheatingNotification == true)
                            {
                                if (!hotNotify)
                                {
                                    Game.DisplayNotification("~y~Your brakes are ~r~REALLY ~y~getting hot!");
                                    notified = true;
                                    hotNotify = true;
                                }
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
                        if (ConfigHandler.BrakeOverheatingNotification == true)
                        {
                            if (!notified)
                            {
                                Game.DisplayNotification("~y~Your brakes are getting hot!");
                                notified = true;
                            }
                        }
                        Game.DisableControlAction(0, GameControl.VehicleBrake, true);
                    }

                    hotBrakes += VehicleHelper.GetBrakePressure(N.GetControlValue(0, 72));
                    if (Game.IsControlPressed(0, GameControl.VehicleAccelerate))
                    {
                        hotBrakes += 5;
                    }

                    N.SetVehicleBrakeLights(veh, true);
                }

                if (Game.IsControlPressed(0, GameControl.VehicleHandbrake) && veh.Speed > 2f && hotBrakes > 1000 && hotBrakes % 4 == 0)
                {
                    Game.DisableControlAction(0, GameControl.VehicleHandbrake, true);
                }

                if (veh.Mods.BrakesModIndex > 1)
                {
                    hotBrakes -= (int)Math.Round((double)VehicleHelper.GetBrakePressure(N.GetControlValue(0, 72)) / 3);
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
                    N.DecorSetInt(veh, "brakeHeat", hotBrakes);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to start and perform an on-foot repair of the last tracked vehicle when the player interacts near the bonnet and the vehicle's damage conditions allow it.
        /// </summary>
        /// <remarks>
        /// Displays context 3D prompts near the vehicle bonnet, validates bonnet bone and player distance, and begins a timed repair animation when the player presses E while on foot. The method enforces preconditions (vehicle exists, repairable class, player able to act, not already repairing, and engine below the degrading threshold) and aborts with user notifications if the vehicle is too damaged, not damaged enough, already repaired, or out of oil. During a successful repair it sets the vehicle driveable when fuel is sufficient, sets engine health to the cascading-failure threshold plus 5, updates max speed, clears/sets internal state flags (isRepairing, prompt, _repairedVehicle, healthEngineLast), and displays success or failure notifications. Early exits occur on missing bonnet bone, distance checks, or if the repair is cancelled mid-animation.
        /// </remarks>
        private static void RepairTick()
        {
            if (!_lastVehicle.Exists() || cantRepairOnFootClasses.Contains(_lastVehicle.Class)) return;
            Ped player = Game.LocalPlayer.Character;
            if (CannotDoAction() || isRepairing || _lastVehicle.EngineHealth > ConfigHandler.DegradingFailureThreshold)
            {
                prompt = false;
                return;
            }

            if (prompt)
            {
                int boneIndex = _lastVehicle.GetBoneIndex("bonnet");

                if (boneIndex == -1)
                {
                    Logging.Warning("Failed to find bonnet bone for vehicle, cannot display repair prompt", "RepairTick");
                }
                
                if (player.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) < 1.8f)
                {
                    prompt = false;
                }

                Text.Draw3D(_lastVehicle.GetBonePosition(boneIndex) + new Vector3(0f, 0f, 0.5f), "Move near here to repair", 0.08f);
            }

            if (_lastVehicle != _repairedVehicle)
            {
                int boneIndex = _lastVehicle.GetBoneIndex("bonnet");

                if (boneIndex == -1)
                {
                    Logging.Warning("Failed to find bonnet bone for vehicle, cannot display repair prompt",
                        "RepairTick");
                    return;
                }
                
                Text.Draw3D(_lastVehicle.GetBonePosition(boneIndex), "Press [E] to repair", 0.065f);
            }

            if (_lastVehicle == _repairedVehicle) return;
            {
                if (!Game.IsKeyDown(Keys.E) ||
                    Game.LocalPlayer.Character.IsInAnyVehicle(false)) return;
                int boneIndex = _lastVehicle.GetBoneIndex("bonnet");

                if (boneIndex == -1)
                {
                    Logging.Warning("Failed to find bonnet bone for vehicle, cannot display repair prompt",
                        "RepairTick");
                    return;
                }
                
                if (player.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) > 2.5f)
                {
                    prompt = true;
                }
                else
                {
                    prompt = false;
                    if (_lastVehicle.OilLevel() <= 0)
                    {
                        Logging.Debug($"Failed to repair: too damaged. {_lastVehicle.OilLevel()}", "SpecialModesManager");
                        Game.DisplayNotification("~r~Your vehicle was too badly damaged. Unable to repair!");
                        return;
                    }

                    if (_lastVehicle.EngineHealth > ConfigHandler.CascadingFailureThreshold + 5)
                    {
                        Logging.Debug($"Failed to repair: not enough damage. {_lastVehicle.EngineHealth}", "SpecialModesManager");
                        Game.DisplayNotification("~y~" + GetRandom(ConfigHandler.NoFixMessages));
                        return;
                    }

                    isRepairing = true;
                    player.Heading = _lastVehicle.Heading - 180f;

                    player.Tasks.ClearImmediately();
                    _lastVehicle.Doors[4].Open(true);
                    player.Tasks.PlayAnimation(repairAnimDict, repairAnimString, 5f, AnimationFlags.UpperBodyOnly);
                    Game.DisplaySubtitle("Attempting to repair vehicle", 6000);
                    // NativeFunction.CallByHash<int>(0x6E13FC662B882D1D, _lastVehicle, 1); // SET_VEHICLE_TYRE_FIXED
                    N.SetVehicleTyreFixed(_lastVehicle, 1);
                    GameFiber.Wait(6000);
                    if (!isRepairing) return;
                    player.Tasks.ClearImmediately();
                    _lastVehicle.Doors[4].Close(true);
                    isRepairing = false;
                    if (_repairedVehicle.Exists() && _repairedVehicle == _lastVehicle)
                    {
                        Logging.Debug("Failed to repair: already repaired this vehicle", "SpecialModesManager");
                        Game.DisplayNotification("~y~" + GetRandom(ConfigHandler.NoFixMessages));
                        return;
                    }
                    if (_lastVehicle.OilLevel() < 2f)
                    {
                        Logging.Debug("Failed to repair: ran oil pan dry", "SpecialModesManager");
                        Game.DisplayNotification("~r~You were unable to repair the vehicle. The oil pan looks all dried up.");
                        return;
                    }
                    if (_lastVehicle.FuelLevel > 1f)
                    {
                        _lastVehicle.IsDriveable = true;
                    }
                    _lastVehicle.EngineHealth = ConfigHandler.CascadingFailureThreshold + 5;
                    healthEngineLast = ConfigHandler.CascadingFailureThreshold + 5;
                    N.SetVehicleMaxSpeed(_lastVehicle, 500.01f);
                    Logging.Debug($"Vehicle repaired! Engine health now {healthEngineLast}", "SpecialModesManager");
                    Game.DisplayNotification("~g~" + GetRandom(ConfigHandler.FixMessages) + ", now get to a mechanic!");
                    _repairedVehicle = _lastVehicle;
                }
            }
        }

        private static bool CannotDoAction()
        {
            Ped player = Game.LocalPlayer.Character;
            // return !player.IsOnFoot || !EntityExtensions.Exists(_lastVehicle) || player.IsCuffed || Extensions.GetDistance(_lastVehicle.Position, player.Position) > 5f || player.IsDead || NativeFunction.CallByHash<int>(0x83F969AA1EE2A664, _lastVehicle, -1) != player.Handle || _lastVehicle.IsDead || N.DecorGetBool(player, "IsDead") || N.DecorGetBool(player, "IsGrabbed");
            return !player.IsOnFoot || !EntityExtensions.Exists(_lastVehicle) || player.IsCuffed || Extensions.Vector3Extensions.GetDistance(_lastVehicle.Position, player.Position) > 5f || player.IsDead || N.GetLastPedInVehicleSeat(_lastVehicle, -1) != player.Handle || _lastVehicle.IsDead || N.DecorGetBool(player, "IsDead") || N.DecorGetBool(player, "IsGrabbed");
        }
        private static bool IsDead() => Game.LocalPlayer.Character.IsDead || N.DecorGetBool(Game.LocalPlayer.Character, "IsDead");
        private static string GetRandom(this List<string> list) => list[new Random().Next(list.Count)];

        /// <summary>
        /// Performs per-frame vehicle safety and state maintenance: interrupts ongoing hood repairs when appropriate, prevents automatic reversing and vehicle flipping, and applies torque or limp-mode limits based on engine health.
        /// </summary>
        /// <remarks>
        /// - Cancels an active repair if the player becomes paused, dead, or moves away from the vehicle's engine bone.
        /// - When enabled, prevents automatic reversing by disabling accelerate/brake inputs at very low speed and applying a slight speed dampening and brake lights.
        /// - When enabled, restricts lateral/vertical steering input if the vehicle roll is extreme while nearly stationary to reduce flipping.
        /// - When enabled, applies an engine torque multiplier or limp-mode speed cap according to current engine health and configured multipliers.
        /// - Side effects: may disable control actions, modify vehicle forward speed, toggle brake lights, change vehicle max speed, and set the current vehicle's engine torque multiplier.
        /// </remarks>
        private static void FlipTick()
        {
            try
            {
                if (_lastVehicle != null)
                {
                    int boneIndex = _lastVehicle.GetBoneIndex("engine");

                    if (boneIndex == -1)
                    {
                        Logging.Warning("Failed to find engine bone for vehicle, cannot display repair prompt",
                            "FlipTick");
                        return;
                    }
                    
                    if (isRepairing && (Game.IsPaused || IsDead() || Game.LocalPlayer.Character.DistanceTo(_lastVehicle.GetBonePosition(boneIndex)) > 1.5f))
                    {
                        isRepairing = false;
                        Game.LocalPlayer.Character.Tasks.Clear();
                        _lastVehicle.Doors[4].Close(true);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                #region Prevent Automatic Reversing
                if (ConfigHandler.PreventAutomaticReversing == true)
                {
                    if (Game.LocalPlayer.Character?.CurrentVehicle?.Class != VehicleClass.Boat)
                    {
                        Vehicle veh = Game.LocalPlayer.Character.CurrentVehicle;

                        if (NativeFunction.CallByHash<Vector3>(0x9A8D700A51CB7B0D, veh, true).Y >= 1f && N.GetControlValue(2, 72) > 127)
                        {
                            isBrakingForward = true;
                        }

                        if (NativeFunction.CallByHash<Vector3>(0x9A8D700A51CB7B0D, veh, true).Y <= -1f && N.GetControlValue(2, 71) > 127)
                        {
                            isBrakingReverse = true;
                        }

                        if (veh.Speed < 1f)
                        {
                            if (isBrakingForward)
                            {
                                Game.DisableControlAction(2, GameControl.VehicleBrake, true);
                                N.SetVehicleForwardSpeed(veh, veh.Speed * 0.98f);
                                N.SetVehicleBrakeLights(veh, true);
                            }
                            if (isBrakingReverse)
                            {
                                Game.DisableControlAction(2, GameControl.VehicleAccelerate, true);
                                N.SetVehicleForwardSpeed(veh, veh.Speed * 0.98f);
                                N.SetVehicleBrakeLights(veh, true);
                            }

                            // We let go of brake
                            if (isBrakingForward && N.GetDisabledControlNormal(2, 72) == 0)
                            {
                                isBrakingForward = false;
                            }
                            if (isBrakingReverse && N.GetDisabledControlNormal(2, 71) == 0)
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
                    // float roll = NativeFunction.CallByHash<float>(0x831E0242595560DF, Game.LocalPlayer.Character.CurrentVehicle); 
                    float roll = N.GetEntityRoll(Game.LocalPlayer.Character.CurrentVehicle);
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
                    N.SetVehicleMaxSpeed(_currentVehicle, 20f);
                }

                _currentVehicle.EngineTorqueMultiplier(factor);
            }
        }

        private static void MainTick()
        {
            // check if certain car is here aka open wheel
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

                float classMultiplier;

                try
                {
                    classMultiplier = ConfigHandler.ClassDamageMultiplier[(int)veh.Class];
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    classMultiplier = 1.0f;
                }

                healthEngineCurrent = veh.EngineHealth;
                if (healthEngineCurrent == 1000f)
                {
                    healthEngineLast = 1000f;
                }

                healthEngineNew = healthEngineCurrent;
                healthEngineDelta = healthEngineLast - healthEngineCurrent;
                healthEngineDeltaScaled = healthEngineDelta * ConfigHandler.DamageFactorEngine * classMultiplier;

                healthBodyCurrent = N.GetVehicleBodyHealth(veh);
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

                if (healthEngineCurrent <= ConfigHandler.EngineSafeGuard && (!ConfigHandler.LimpMode || veh.OilLevel() < 3f) && !N.IsVehicleTyreBurst(veh, 1, true))
                {
                    veh.IsDriveable = false;
                    // NativeFunction.CallByHash<int>(0xEC6A202EE4960385, veh, 1, true, 1000f); 
                    N.SetVehicleTyreBurst(veh, 1, true, 1000f);
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
                        N.SetVehicleMaxSpeed(_currentVehicle, 500.01f);
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
                    // NativeFunction.CallByHash<int>(0xB77D05AC8C78AADB, veh, healthBodyNew); 
                    N.SetVehicleBodyHealth(veh, healthBodyNew);
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

        private static void ShopInteraction()
        {
            RepairShop repairShop = VehicleHelper.GetNearestRepairShop();
            if (repairShop != null)
            {
                if (NativeFunction.CallByHash<float>(0xF271147EB7B40F12, _currentVehicle) < 1000f)
                {
                    if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    {
                        Game.DisplayHelp("Press ~INPUT_CONTEXT~ to repair.");
                    }
                    if (Game.IsControlPressed(0, GameControl.Context))
                    {
                        if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                        {
                            Game.DisplaySubtitle("The mechanics are taking a look at your vehicle", 5000);
                            GameFiber.Wait(5000);
                            Vehicle _currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                            if (EntityExtensions.Exists(_currentVehicle))
                            {
                                if (_currentVehicle.EngineHealth < 200f)
                                {
                                    Game.DisplaySubtitle("The mechanics are repairing your vehicle");
                                    GameFiber.Wait(3000);
                                }
                                if (_currentVehicle.FuelLevel > 1f)
                                {
                                    _currentVehicle.IsDriveable = true;
                                }
                                _currentVehicle.Repair();
                                healthBodyLast = healthEngineLast = healthPetrolTankLast = 1000f;
                                _currentVehicle.IsEngineOn = true;
                                _repairedVehicle = null;
                                NativeFunction.CallByHash<int>(0xBAA045B4E42F3C06, _currentVehicle, 0.0f); // SET_VEHICLE_MAX_SPEED
                                Game.DisplayNotification("~g~The mechanic repaired your car!");
                            }
                        }
                        else
                        {
                            Game.DisplayNotification("You must be in your vehicle for the mechanics to repair it!");
                        }
                    }
                }
            }
        }
        #endregion

        public static void Start()
        {
            Logging.Info("starting...", "SpecialModesManagers");
            GameFiber.StartNew(delegate { Run(); });
        }

        public static void Run()
        {
            while (true)
            {
                GameFiber.Yield();

                BrakeLights();
                HeatBrakes();
                RepairTick();
                FlipTick();
                MainTick();
                ShopInteraction();
            }
        }
    }
}
