using Common;
using Rage;

namespace SimpleCTRL.Handlers
{
    internal static class DoorHandler
    {
        #region Entity Control Handlers
        internal static void ControlDoor(Vehicle car, int door, bool open)
        {
            if (EntityExtensions.Exists(car))
            {
                if (open)
                {
                    car.Doors[(int)(VehicleDoorIndex)door].Open(true);
                }
                else
                {
                    car.Doors[(int)(VehicleDoorIndex)door].Close(true);
                }
            }
        }
        #endregion

        #region Hood/Trunk Handlers
        internal static void HandleTrunk()
        {
            Vehicle veh = GetClosestVehicle();
            if (!EntityExtensions.Exists(veh))
            {
                return;
            }

            ControlDoor(veh, (int)VehicleDoorIndex.Trunk, !veh.Doors[(int)VehicleDoorIndex.Trunk].IsOpen);
        }

        internal static void HandleHood()
        {
            Vehicle veh = GetClosestVehicle();
            if (!EntityExtensions.Exists(veh))
            {
                return;
            }

            ControlDoor(veh, (int)VehicleDoorIndex.Hood, !veh.Doors[(int)VehicleDoorIndex.Hood].IsOpen);
        
        }
        #endregion

        #region Utilities
        private static Vehicle GetClosestVehicle()
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                return Game.LocalPlayer.Character.CurrentVehicle;
            }
            // Later do a raycast to get closest vehicle [NOTE]
            return null;
        }
        #endregion
    }
}
