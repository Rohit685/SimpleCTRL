using Common;
using Common.Models;
using Common.Native;
using Rage;
using Rage.Attributes;
using Rage.Native;
using SimpleCTRL.API;
using SimpleCTRL.Handlers;
using SimpleCTRL.UI;
using SimpleCTRL.Utils;
using System;

namespace SimpleCTRL.Threads
{
    internal class PlayerController : CommonPlugin
    {
        #region Fields
        // Leave Engine Running
        protected const bool _restrictEmergency = false;    // Only allow this feature for emergency vehicles
        protected const bool _keepDoorsOpen = true;         // Keep the door open when getting out
        protected static bool _doorsNotify = false;                // Show notification first time they get any vehicle after joining
        protected static bool _seatBeltOn = Game.LocalPlayer.Character.CanFlyThroughWindshields;
            
        // Vehicle Control
        private static bool _isShuffleDisabled = true;
        protected static VehicleIndicatorLightsStatus intendedStatus = (VehicleIndicatorLightsStatus)0;
        protected static VehicleIndicatorLightsStatus status = (VehicleIndicatorLightsStatus)0;
        protected static float initialHeading = 0f;
        protected static uint turnOffAt = 0u;
        public static bool isDisabled = false;
        protected bool _areWindowsDown = false;
        protected static float _steeringAngle;
        protected static Vehicle _steeringVeh = null;

        // GPS System
        private static bool first = true;
        private static bool waypoint = false;

        // Instances
        private static ControlHandler leaveEngineRunning = new ControlHandler();
        private static ControlHandler turnEngineOn = new ControlHandler();

        public static Vehicle vehicle;
        #endregion

        #region Commands
        [ConsoleCommand]
        private static void Command_Hood() => HandleHood();

        [ConsoleCommand]
        private static void Command_Trunk() => HandleTrunk();

        [ConsoleCommand]
        private static void Command_Shuffle() => ShuffleSeats();
        #endregion

        private static void OnTick()
        {
            Ped player = Game.LocalPlayer.Character;

            #region Vehicle Control
            Vehicle playerVeh = player.CurrentVehicle;

            if (!EntityExtensions.Exists(playerVeh))
            {
                return;
            }

            // if (_isShuffleDisabled && playerVeh != null && playerVeh.GetPedOnSeat((int)VehicleSeat.Passenger) == player && NativeFunction.CallByHash<bool>(0xB0760331C7AA4155, player, 165))
            if (_isShuffleDisabled && playerVeh != null && playerVeh.GetPedOnSeat((int)VehicleSeat.Passenger) == player && N.GetIsTaskActive(player, 165))
            {
                if (!playerVeh.IsSeatFree((int)VehicleSeat.Driver) && !playerVeh.Driver.IsPlayer)
                {
                    return;
                }
                else
                {
                    // NativeFunction.CallByHash<int>(0x1913FE4CBF41C463, player, 184, true);
                    N.SetPedConfigFlag(player, 184, true);
                    player.Tasks.ClearImmediately();
                    // NativeFunction.CallByHash<int>(0xF75B0D629E1C063D, player, playerVeh, (int)VehicleSeat.Passenger);
                    N.SetPedIntoVehicle(player, playerVeh, (int)VehicleSeat.Passenger);
                }
            }
            else if (!_isShuffleDisabled && playerVeh != null && playerVeh.IsSeatFree((int)VehicleSeat.Driver))
            {
                // NativeFunction.CallByHash<int>(0xC1E8A365BF3B29F2, player, 184, true); 
                N.SetPedConfigFlag(player, 184, true);
                // NativeFunction.CallByHash<int>(0xF75B0D629E1C063D, player, playerVeh, (int)VehicleSeat.Driver); 
                N.SetPedIntoVehicle(player, playerVeh, (int)VehicleSeat.Driver);
                _isShuffleDisabled = true;
            }

            if (ConfigHandler.VehicleIndicators == true)
            {
                if (Controls.IsControlDownWithModifier(Controls.SimpleControls.LIGHT_INDR))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)1;
                }
                if (Controls.IsControlDownWithModifier(Controls.SimpleControls.LIGHT_INDL))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)2;
                }
                if (Controls.IsControlDownWithModifier(Controls.SimpleControls.LIGHT_HAZRD))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)3;
                }

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

            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                Func<bool> controlCondition = () => Controls.IsControlDownWithModifier(Controls.SimpleControls.ENG_TOGGLE);

                turnEngineOn.CheckControlHoldDuration(controlCondition, 1000, () =>
                {
                    if (Game.LocalPlayer.Character.CurrentVehicle.Speed < 5f)
                    {
                        N.SetVehicleEngineOn(Game.LocalPlayer.Character.CurrentVehicle, false, false, true);
                    }
                    isDisabled = true;
                });

                if (Game.IsControlPressed(0, GameControl.VehicleAccelerate) && isDisabled)
                {
                    N.SetVehicleEngineOn(Game.LocalPlayer.Character.CurrentVehicle, true, false, false);
                    isDisabled = false;
                }
            }

            if (ConfigHandler.TireRentainment == true)
            {
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
        }

        private static void FuelTick()
        {
            Ped player = Game.LocalPlayer.Character;

            #region Fuel System
            Vehicle playerVeh = player.CurrentVehicle;

            if (EntityExtensions.Exists(ClientCurrentVehicle))
            {
                Globals.WasDriver = ClientCurrentVehicle.Driver == ClientPed;
            }
            if (Current.MyVehicle != ClientCurrentVehicle)
            {
                Current.VehicleFuelLevelInitialized = false;
            }
            Current.MyVehicle = ClientLastVehicle ?? null;
            int refuelingAllowed;
            if (Current.MyVehicle != null)
            {
                vehicle = Current.MyVehicle;
                if (vehicle != null && (int)vehicle.Class != 13 && !vehicle.IsBoat() && !vehicle.DisplayName().Contains("BLIMP"))
                {
                    if (ClientPed.IsOnFoot)
                    {
                        Vector3 position = ClientPed.Position;
                        if (position.DistanceToSquared(vehicle.Position) <= 15f)
                        {
                            refuelingAllowed = (Globals.WasDriver ? 1 : 0);
                            goto IL_0162;
                        }
                    }
                    refuelingAllowed = 0;
                    goto IL_0162;
                }
            }
            goto IL_0328;
            IL_0162:
            Globals.RefuelingAllowed = (byte)refuelingAllowed != 0;
            if (!Current.VehicleFuelLevelInitialized)
            {
                vehicle.InitFuel();
            }
            if (playerVeh.IsAircraft())
            {
                if (!Current.AircraftEngineOn && vehicle.IsEngineOn)
                {
                    N.SetVehicleEngineOn(vehicle, false, false, true);
                    Current.AircraftEngineOn = false;
                }
                else if (Current.AircraftEngineOn && vehicle.IsEngineOn)
                {
                    float percentFuel = vehicle.FuelLevel / vehicle.MaxFuelLevel() * 100f;
                    if (vehicle.IsInAir && percentFuel < ConfigHandler.AircraftLowFuelWarning && (DateTime.Now - Current.LastAircraftLowFuelWarning).TotalSeconds > 5.0) // adjust timer 
                    {
                        SoundHandler.PlayAudio(SoundHandler.Audio.LowFuel);
                        Current.LastAircraftLowFuelWarning = DateTime.Now;
                    }
                }
                vehicle.ConsumeAircraftFuel();
                if (!vehicle.IsInAir && vehicle.Speed < 2f && !vehicle.IsEngineOn && (DateTime.Now - Current.LastAircraftEngineHintDisplayed).TotalSeconds > 30.0)
                {
                    Game.DisplayHelp("Press ~INPUT_VEH_FLY_UNDERCARRIAGE~ to start the engine.");
                    Current.LastAircraftEngineHintDisplayed = DateTime.Now;
                }
                Current.LastAircraftAltitude = vehicle.HeightAboveGround;
            }
            else
            {
                if (vehicle.Model.IsCar && vehicle.PetrolTankHealth() < 700f && vehicle.GetFuelLevel() > 0f)
                {
                    if (vehicle.PetrolTankHealth() < 250f && !vehicle.IsElectric())
                    {
                        vehicle.SetFuelLevel(vehicle.GetFuelLevel() - 0.005f);
                    }
                    vehicle.SetFuelLevel(vehicle.GetFuelLevel() - 0.003f);
                }
                if (vehicle.IsPlayerDriving() || Globals.RefuelingAllowed)
                {
                    vehicle.ConsumeRoadVehicleFuel();
                }
            }
            // if (!NativeFunction.CallByHash<bool>(0x157F93B036700462) && (Globals.RefuelingAllowed || vehicle.IsPlayerDriving()))
            if (!N.IsRadarHidden() && (Globals.RefuelingAllowed || vehicle.IsPlayerDriving()))
            {
                if (!N.IsHudHidden() || (player.CurrentVehicle != null && player.CurrentVehicle.IsAircraft()))
                {
                    CustomUI.RenderBar(vehicle.FuelLevel, Current.VehicleFuelCapacity, vehicle.IsElectric());
                }
                GasStation gas = Functions.GetGasStationInRange(player.Position, 250f);
                if (gas != null)
                {
                    if (gas != Current.GasStation)
                    {
                        Current.GasStation = gas;
                    }
                }
                else if (Current.GasStation != null)
                {
                    Current.GasStation = null;
                }
            }
            goto IL_0328;
            IL_0328:
            if (Game.LocalPlayer.Character.IsOnFoot)
            {
                Current.AircraftEngineOn = false;
                ClientPed.ManualRefuel();
                Current.VehicleFuelLevelInitialized = false;
                foreach (int x in Current.TripInfos.Keys)
                {
                    if (Current.TripInfos[x] != null)
                    {
                        try
                        {
                            Vehicle v = World.GetEntityByHandle<Vehicle>(new PoolHandle((uint)x));
                            if (v.Exists() && v.IsEngineOn)
                            {
                                v.ConsumeRoadVehicleFuel();
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            }
            Current.LastWorldTime = DateTime.UtcNow;
            #endregion
        }

        private static void GPSTick()
        {
            GameFiber.StartNew(delegate
            {
                Ped player = Game.LocalPlayer.Character;

                #region GPS System
                if (!player.IsInAnyVehicle(false))
                {
                    first = true;
                }
                if (!player.IsInAnyVehicle(false) || player.CurrentVehicle.IsHelicopter || player.CurrentVehicle.IsPlane)
                {
                    return;
                }
                if (!NativeFunction.CallByHash<bool>(0x1DD1F58F493F1DA5) && waypoint)
                {
                    waypoint = false;
                    first = true;
                    SoundHandler.PlayAudio(SoundHandler.Audio.Arrived);
                }
                if (!NativeFunction.CallByHash<bool>(0x1DD1F58F493F1DA5))
                {
                    return;
                }
                if (first)
                {
                    waypoint = true;
                    first = false;
                    string[] audioFiles = { "TONE.WAV", "CALCULATINROUTE.WAV", "HIGHLIGHTEDROUTE.WAV" };
                    int[] delays = { 1378, 1980, 2497 };
                    SoundHandler.PlayAudioSequence(audioFiles, delays);
                }
                #endregion
            }, "Player Controller - GPS System");
        }

        public static void Start()
        {
            Logging.Info("starting...", "PlayerController");
            GameFiber.StartNew(delegate { Run(); });
            GameFiber.StartNew(delegate { Run2(); });
        }

        public static void Run()
        {
            // NativeFunction.CallByHash<int>(0xD3BD40951412FEF6, Globals.DictRefueling); 
            N.RequestAnimDict(Globals.DictRefueling);

            while (true)
            {
                GameFiber.Yield();

                OnTick();
                if (ConfigHandler.FuelSystem == true)
                {
                    FuelTick();
                }
                if (ConfigHandler.GlobalPositioningSystem == true)
                {
                    GPSTick();
                }
            }
        }

        private static void Run2()
        {
            #region Leave Engine Running
            while (true)
            {
                GameFiber.Yield();

                Vehicle currentVehicle = ClientPed.LastVehicle;
                bool isInVehicle = ClientPed.IsInAnyVehicle(false);

                if (_restrictEmergency && currentVehicle.Class != VehicleClass.Emergency)
                {
                    return;
                }

                if (ConfigHandler.LeaveEngineOnNotification && !_doorsNotify && isInVehicle &&
                    currentVehicle.Driver == ClientPed && currentVehicle.Class != VehicleClass.Helicopter &&
                    currentVehicle.Class != VehicleClass.Plane)
                {
                    Game.DisplayNotification("Hold ~b~F ~w~when exiting to leave the engine running.");
                    _doorsNotify = true;
                }

                if (isInVehicle && ClientPed.IsAlive && currentVehicle.Class != VehicleClass.Helicopter &&
    currentVehicle.Class != VehicleClass.Plane)
                {
                    Func<bool> controlCondition = () => !_seatBeltOn && N.IsDisabledControlPressed(0, (int)GameControl.VehicleExit);

                    Game.DisableControlAction(0, GameControl.VehicleExit, true);

                    leaveEngineRunning.CheckControlHoldDuration(controlCondition, 200, () => 
                    {
                        currentVehicle.IsEngineOn = true;
                        ClientPed.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    }, () =>
                    {
                        ClientPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                    });
                }
            }
            #endregion
        }

        #region Vehicle Control Handler
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

        internal void Windows()
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
                    // NativeFunction.CallByHash<int>(0xF75B0D629E1C063D, playerPed, playerVeh, (int)VehicleSeat.Passenger);
                    N.SetPedIntoVehicle(playerPed, playerVeh, (int)VehicleSeat.Passenger);
                }
                else
                {
                    _isShuffleDisabled = false;
                }
            }
        }
        #endregion

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