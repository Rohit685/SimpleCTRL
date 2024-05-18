using Common;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.UI;
using System.Collections.Generic;

namespace SimpleCTRL.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Ped"/> class.
    /// </summary>
    internal static class PedExtensions
    {
        #region Refueling Methods
        /// <summary>
        /// Allows the player to manually refuel the vehicle.
        /// </summary>
        /// <param name="playerPed">The player's <see cref="Ped"/>.</param>
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
        #endregion

        #region Vehicle Control Methods
        /// <summary>
        /// Checks if the player is currently driving a vehicle.
        /// </summary>
        /// <param name="playerPed">The player's <see cref="Ped"/>.</param>
        /// <returns><c>true</c> if the player is currently driving a vehicle; otherwise, <c>false</c>.</returns>
        public static bool IsDrivingAVehicle(this Ped playerPed)
        {
            Vehicle v = playerPed.CurrentVehicle;
            return !(!EntityExtensions.Exists(playerPed) || playerPed.IsDead || !EntityExtensions.Exists(v)
) && v.Driver == playerPed && !new List<VehicleClass> { VehicleClass.Plane, VehicleClass.Helicopter, VehicleClass.Cycle, VehicleClass.Rail }.Contains(v.Class);
        }

        /// <summary>
        /// Checks if the player is driving a road vehicle.
        /// </summary>
        /// <param name="playerPed">The player's <see cref="Ped"/>.</param>
        /// <returns><c>true</c> if the player is driving a road vehicle; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the player is flying an aircraft.
        /// </summary>
        /// <param name="playerPed">The player's <see cref="Ped"/>.</param>
        /// <returns><c>true</c> if the player is flying an aircraft; otherwise, <c>false</c>.</returns>
        internal static bool IsFlyingAnAircraft(this Ped playerPed)
        {
            if (EntityExtensions.Exists(playerPed) && NativeFunction.CallByHash<bool>(0x9134873537FA419C, playerPed) && playerPed.CurrentVehicle.IsAircraft() && playerPed.CurrentVehicle.GetPedOnSeat((int)(VehicleSeat)(-1)) == playerPed)
            {
                return playerPed.CurrentVehicle.IsAlive;
            }
            return false;
        }
        #endregion
    }
}
