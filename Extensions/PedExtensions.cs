using Common;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Core.Models.UI;
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
            if (playerPed.Inventory.EquippedWeapon == null || playerPed.Inventory.EquippedWeapon.Hash != WeaponHash.PetrolCan)
            {
                Globals.HudActive = false; // should be fine for here for now. but in theory it should be when you have it out and keep walking towards the prompt yk so it should be in position based
                return;
            }
            Vector3 pos = ((Entity)playerPed).Position;
            HitResult raycastResult = World.TraceCapsule(pos, ((Entity)playerPed).ForwardVector, 10f, TraceFlags.IntersectVehicles, (Entity)(object)playerPed);
            //if (!raycastResult.Hit) // dont knoiw if should be dithitentity
            //{
            //    return;
            //}
            Entity hitEntity = raycastResult.HitEntity;
            Vehicle vehicle = (Vehicle)(object)((hitEntity is Vehicle) ? hitEntity : null);
            if (vehicle == null)
            {
                return;
            }
            Vector3 position = ((Entity)vehicle).Position;
            if (!(position.DistanceToSquared(pos) <= 10f) || !(N.DecorExistOn(vehicle, "_Fuel_Level") || !vehicle.IsRoadVehicle() || vehicle.IsElectric()))
            {
                return;
            }
            if (!Managed.VehicleFuelLevelInitialized)
            {
                vehicle.InitFuel();
            }
            float max = vehicle.MaxFuelLevel();
            float fuel = vehicle.GetFuelLevel();
            if (max - fuel < 0.2f)
            {
                HUD.InstructFullOrEmpty("Fuel tank full");
            }
            else if (fuel == 0f)
            {
                HUD.InstructFullOrEmpty("Fuel tank empty");
            }
            else
            {
                HUD.InstructManualRefuelOrSiphon();
            }
            Game.DisableControlAction(0, GameControl.Attack, true);
            Game.DisableControlAction(0, GameControl.Attack2, true);
            Game.DisableControlAction(0, GameControl.Aim, true);
            Game.DisableControlAction(0, GameControl.AccurateAim, true);
            if (N.IsDisabledControlPressed(0, 24) && !N.IsDisabledControlPressed(0, 25))
            {
                if (fuel < max)
                {
                    Globals.JerryCanAnimation.Magick(playerPed);
                    Managed.FuelAmountPumped += 0.023f;
                    vehicle.SetFuelLevel(fuel + 0.023f);
                }
            }
            else if (N.IsDisabledControlPressed(0, 25) && !N.IsDisabledControlPressed(0, 24))
            {
                if (fuel > 0f)
                {
                    if (!NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, playerPed, Globals.DictSiphoning, Globals.AnimSiphoning, 3)) // IS_ENTITY_PLAYING_ANIM
                    {
                        playerPed.Tasks.PlayAnimation(Globals.DictSiphoning, Globals.AnimSiphoning, -1, 2f, 8f, 0f, AnimationFlags.Loop);
                        Managed.FuelAmountSiphoned += 0.00125f;
                        vehicle.SetFuelLevel(fuel - 0.00125f);
                    }
                    else
                    {
                        Managed.FuelAmountSiphoned += 0.00125f;
                        vehicle.SetFuelLevel(fuel - 0.00125f);
                    }
                }
                else
                {
                    vehicle.SetFuelLevel(0f);
                    NativeFunction.CallByHash<bool>(0x28004F88151E03E0, playerPed, Globals.AnimSiphoning, Globals.DictSiphoning); // STOP_ENTITY_ANIM
                }
            }
            if (NativeFunction.CallByHash<bool>(0x305C8DCD79DA8B0F, 0, 69) && fuel >= max) // IS_DISABLED_CONTROL_JUST_RELEASED
            {
                vehicle.SetFuelLevel(max);
                Globals.JerryCanAnimation.RewindAndStop(playerPed);
            }
            if (NativeFunction.CallByHash<bool>(0x305C8DCD79DA8B0F, 9, 24)) // IS_DISABLED_CONTROL_JUST_RELEASED
            {
                Globals.JerryCanAnimation.RewindAndStop(playerPed);
            }
            if (NativeFunction.CallByHash<bool>(0x305C8DCD79DA8B0F, 9, 68) && fuel <= 0f) // IS_DISABLED_CONTROL_JUST_RELEASED
            {
                vehicle.SetFuelLevel(0f);
                NativeFunction.CallByHash<bool>(0x28004F88151E03E0, playerPed, Globals.AnimSiphoning, Globals.DictSiphoning); // STOP_ENTITY_ANIM
            }
            if (NativeFunction.CallByHash<bool>(0x305C8DCD79DA8B0F, 9, 25)) // IS_DISABLED_CONTROL_JUST_RELEASED
            {
                Game.DisableControlAction(0, (GameControl)24, true);
                Game.DisableControlAction(0, (GameControl)257, true);
                NativeFunction.CallByHash<bool>(0x28004F88151E03E0, playerPed, Globals.AnimSiphoning, Globals.DictSiphoning); // STOP_ENTITY_ANIM
            }
            CustomUI.RenderInstructions();
            if (!Globals.HudActive) 
            {
                NativeFunction.CallByHash<int>(0x67C540AA08E4A6F5, -1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", 1);
            }
            Globals.HudActive = true;
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
