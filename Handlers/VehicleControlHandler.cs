using Common;
using Common.Native;
using Rage;

namespace SimpleCTRL.Handlers
{
    internal static class VehicleControlHandler
    {
        public static bool _isShuffleDisabled = true;
        private static bool _areWindowsDown = false;

        public static void OpenDoor(string door)
        {
            int doorIndex = -1;
            switch (door)
            {
                case "lfront":
                    doorIndex = 0;
                    break;
                case "rfront":
                    doorIndex = 1;
                    break;
                case "lrear":
                    doorIndex = 2;
                    break;
                case "rrear":
                    doorIndex = 3;
                    break;
                case "hood":
                    doorIndex = 4;
                    break;
                case "trunk":
                    doorIndex = 5;
                    break;
            }
            if (doorIndex >= 0)
            {
                Vehicle veh = Game.LocalPlayer.Character.LastVehicle;
                VehicleDoorIndex index = (VehicleDoorIndex)doorIndex;

                if (veh.Doors[(int)index].IsOpen)
                {
                    veh.Doors[(int)index].Close(true);
                }
                else
                {
                    veh.Doors[(int)index].Open(true);
                }
            }
        }

        internal static void Windows()
        {
            _areWindowsDown = !_areWindowsDown;

            if (_areWindowsDown)
            {
                Game.LocalPlayer.Character.CurrentVehicle.Windows[(int)VehicleWindowIndex.FrontLeftWindow].RollUp();
                Game.LocalPlayer.Character.CurrentVehicle.Windows[(int)VehicleWindowIndex.FrontRightWindow].RollUp();
                Game.LocalPlayer.Character.CurrentVehicle.Windows[(int)VehicleWindowIndex.BackLeftWindow].RollUp();
                Game.LocalPlayer.Character.CurrentVehicle.Windows[(int)VehicleWindowIndex.BackRightWindow].RollUp();
            }
            else
            {
                // NativeFunction.CallByHash<int>(0x85796B0549DDE156, Game.LocalPlayer.Character.CurrentVehicle);
                N.RollDownWindows(Game.LocalPlayer.Character.CurrentVehicle);
            }
        }

        internal static void ShuffleSeats()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vehicle playerVeh = playerPed.CurrentVehicle;

            if (playerVeh != null)
            {
                if (playerVeh.Driver == playerPed)
                {
                    N.SetPedIntoVehicle(playerPed, playerVeh, (int)VehicleSeat.Passenger);
                }
                else
                {
                    _isShuffleDisabled = false;
                }
            }
        }
    }
}
