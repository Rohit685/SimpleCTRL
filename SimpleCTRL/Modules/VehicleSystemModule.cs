namespace SimpleCTRL.Modules
{
    public class VehicleSystemModule : CommonPlugin
    {
        #region Fields
        private static readonly List<VehicleClass> ignoredClasses = new List<VehicleClass>
        {
           (VehicleClass)14, (VehicleClass)15, (VehicleClass)16, (VehicleClass)13,
           (VehicleClass)19, (VehicleClass)21, (VehicleClass)11
        };

        private static bool hotNotify;
        private static bool isBrakingForward;
        private static bool isBrakingReverse;
        private static bool notified;
        private static bool _isShuffleDisabled = true;
        private static float _steeringAngle;
        private static Vehicle _steeringVeh = null;
        #endregion

        #region Initialization and Main Loop
        public static void Start()
        {
            Logging.Info("Starting VehicleSystemModule...", "VehicleSystemModule");
            GameFiber.StartNew(Run, "SimpleCTRL - Vehicle System Module");
            Logging.Info("VehicleSystemModule has started.", "VehicleSystemModule");
        }

        public static void Run()
        {
            while (true)
            {
                MainLoop();
                GameFiber.Yield();
            }
        }

        public static void MainLoop()
        {
            if (ClientPed.IsInAnyVehicle(false))
            {
                HandleVehicleInUse();
            }
            PreventSeatShuffling();
            MaintainWheelPosition();
            // ManageBrakeOverheating();
        }
        #endregion

        #region Vehicle Management
        private static void HandleVehicleInUse()
        {
            AutomaticBrakeLights();
            PreventRolloverRecovery();
            PreventAutomaticReversing();
        }

        private static void PreventSeatShuffling()
        {
            if (Settings.AllowShuffle) return;

            Vehicle playerVeh = ClientCurrentVehicle;
            if (!EntityExtensions.Exists(playerVeh)) return;

            if (_isShuffleDisabled && playerVeh.GetPedOnSeat((int)VehicleSeat.Passenger) == ClientPed && N.GetIsTaskActive(ClientPed, 165))
            {
                HandleShuffleDisable(playerVeh);
            }
            else if (!_isShuffleDisabled && playerVeh.IsSeatFree((int)VehicleSeat.Driver))
            {
                N.SetPedConfigFlag(ClientPed, 184, true);
                N.SetPedIntoVehicle(ClientPed, playerVeh, (int)VehicleSeat.Driver);
                _isShuffleDisabled = true;
            }
        }

        private static void HandleShuffleDisable(Vehicle playerVeh)
        {
            if (playerVeh.IsSeatFree((int)VehicleSeat.Driver) || playerVeh.Driver.IsPlayer)
            {
                N.SetPedConfigFlag(ClientPed, 184, true);
                ClientPed.Tasks.ClearImmediately();
                N.SetPedIntoVehicle(ClientPed, playerVeh, (int)VehicleSeat.Passenger);
            }
        }
        #endregion

        #region Brake and Wheel Management
        private static void AutomaticBrakeLights()
        {
            if (Settings.EnableBrakeLights && ClientCurrentVehicle.Exists() && ClientCurrentVehicle.Speed < 0.1)
            {
                N.SetVehicleBrakeLights(ClientLastVehicle, true);
            }
        }

        private static void MaintainWheelPosition()
        {
            if (Settings.EnableTireRentainment)
            {
                if (ShouldMaintainSteeringPosition())
                {
                    _steeringVeh = ClientCurrentVehicle;
                    if (ClientCurrentVehicle.SteeringAngle > 20)
                    {
                        _steeringAngle = 40;
                    }
                    else if (ClientCurrentVehicle.SteeringAngle < -20)
                    {
                        _steeringAngle = -40;
                    }
                    else if (ClientCurrentVehicle.SteeringAngle > 5 || ClientCurrentVehicle.SteeringAngle < -5)
                    {
                        _steeringAngle = ClientCurrentVehicle.SteeringAngle;
                    }
                }

                if (EntityExtensions.Exists(_steeringVeh) && (ClientPed.IsOnFoot || N.IsVehicleStopped(_steeringVeh)))
                {
                    _steeringVeh.SteeringAngle = _steeringAngle;
                }
            }
        }

        private static bool ShouldMaintainSteeringPosition()
        {
            return EntityExtensions.Exists(ClientPed) && EntityExtensions.Exists(ClientCurrentVehicle) &&
                   ClientCurrentVehicle.Driver == ClientPed && ClientCurrentVehicle.IsAlive &&
                   !ClientCurrentVehicle.Model.IsBicycle && (ClientCurrentVehicle.Model.IsCar ||
                   ClientCurrentVehicle.Model.IsBike || ClientCurrentVehicle.Model.IsQuadBike);
        }

        private static void ManageBrakeOverheating()
        {
            if (!Settings.EnableBrakeOverheating || !EntityExtensions.Exists(ClientCurrentVehicle) ||
                ignoredClasses.Contains(ClientCurrentVehicle.Class) || ClientCurrentVehicle.Driver != ClientPed)
                return;

            int hotBrakes = N.DecorExistOn(ClientCurrentVehicle, "brakeHeat") ?
                            N.DecorGetInt(ClientCurrentVehicle, "brakeHeat") : 0;

            if (hotBrakes < 5)
            {
                notified = hotNotify = false;
            }

            HandleBrakeOverheating(hotBrakes);
        }

        private static void HandleBrakeOverheating(int hotBrakes)
        {
            if (hotBrakes > 10000)
            {
                Game.DisplayHelp("~r~Your brakes are disabled due to being too hot.");
                Game.DisableControlAction(0, GameControl.VehicleBrake, true);
            }
            else if (hotBrakes > 5000)
            {
                DisplayBrakeOverheatingNotification("~y~Your brakes are ~r~REALLY ~y~getting hot!", ref hotBrakes);
            }
            else if (hotBrakes > 3500)
            {
                Game.DisableControlAction(0, GameControl.VehicleBrake, true);
            }
            else if (hotBrakes > 2500)
            {
                DisplayBrakeOverheatingNotification("~y~Your brakes are getting hot!", ref hotBrakes);
            }

            hotBrakes = UpdateBrakeHeat(hotBrakes);
            N.DecorSetInt(ClientCurrentVehicle, "brakeHeat", hotBrakes);
        }

        private static void DisplayBrakeOverheatingNotification(string message, ref int hotBrakes)
        {
            if (Settings.BrakeOverheatingNotification && !hotNotify)
            {
                Game.DisplayNotification(message);
                notified = true;
                hotNotify = true;
            }
            Game.DisableControlAction(0, GameControl.VehicleBrake, true);
        }

        private static int UpdateBrakeHeat(int hotBrakes)
        {
            hotBrakes += GetBrakePressure(N.GetControlValue(0, 72));
            if (Game.IsControlPressed(0, GameControl.VehicleAccelerate)) hotBrakes += 5;
            return Math.Max(0, hotBrakes);
        }
        #endregion

        #region Rollover and Automatic Reversing
        private static void PreventRolloverRecovery()
        {
            if (Settings.PreventVehicleRolloverRecovery)
            {
                float roll = N.GetEntityRoll(ClientCurrentVehicle);
                if ((roll > 75f || roll < -75f) && ClientCurrentVehicle.Speed < 2f)
                {
                    Game.DisableControlAction(2, GameControl.VehicleMoveLeftRight, true);
                    Game.DisableControlAction(2, GameControl.VehicleMoveUpDown, true);
                }
            }
        }

        private static void PreventAutomaticReversing()
        {
            if (!Settings.PreventAutomaticReversing || ClientCurrentVehicle.Class == VehicleClass.Boat) return;

            if (N.GetEntitySpeedVector(ClientCurrentVehicle, true).Y >= 1f && N.GetControlValue(2, 72) > 127)
            {
                isBrakingForward = true;
            }

            if (N.GetEntitySpeedVector(ClientCurrentVehicle, true).Y <= -1f && N.GetControlValue(2, 71) > 127)
            {
                isBrakingReverse = true;
            }

            if (ClientCurrentVehicle.Speed < 1f)
            {
                HandleAutomaticReversing();
            }
        }

        private static void HandleAutomaticReversing()
        {
            if (isBrakingForward)
            {
                Game.DisableControlAction(2, GameControl.VehicleBrake, true);
                N.SetVehicleForwardSpeed(ClientCurrentVehicle, ClientCurrentVehicle.Speed * 0.98f);
                N.SetVehicleBrakeLights(ClientCurrentVehicle, true);
            }

            if (isBrakingReverse)
            {
                Game.DisableControlAction(2, GameControl.VehicleAccelerate, true);
                N.SetVehicleForwardSpeed(ClientCurrentVehicle, ClientCurrentVehicle.Speed * 0.98f);
                N.SetVehicleBrakeLights(ClientCurrentVehicle, true);
            }

            ResetBrakingState();
        }

        private static void ResetBrakingState()
        {
            if (isBrakingForward && N.GetDisabledControlNormal(2, 72) == 0)
            {
                isBrakingForward = false;
            }

            if (isBrakingReverse && N.GetDisabledControlNormal(2, 71) == 0)
            {
                isBrakingReverse = false;
            }
        }
        #endregion

        #region Utilities
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
    }
    #endregion
}
