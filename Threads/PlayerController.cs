using Common;
using Common.Native;
using InputManager;
using Rage;
using Rage.Attributes;
using Rage.Native;
using SimpleCTRL.Extensions;
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
        protected static bool _keepDoorsOpen = ConfigHandler.LeaveDoorOpenWhenEngineOn;         // Keep the door open when getting out
        protected static bool _doorsNotify = false;                // Show notification first time they get any vehicle after joining
            
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
        private static int shuffleButtonPresses;
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
            Vehicle lastParkedVehicle = player.CurrentVehicle;

            if (!EntityExtensions.Exists(playerVeh))
            {
                return;
            }

            if (ConfigHandler.ParkingMode == true)
            {
                GameFiber.StartNew(delegate
                {
                    #region Parking System
                    if (ControlHandler.IsControlDownWithModifier(SimpleControls.PARK))
                    {
                        if (playerVeh.GetPedOnSeat((int)VehicleSeat.Driver) == player && player.IsAlive && playerVeh.IsEngineOn && playerVeh.IsCar && playerVeh.Speed == 0)
                        {
                            if (!Globals.isParked)
                            {
                                NativeFunction.CallByHash<int>(0xAD738C3085FE7E11, playerVeh); // SET_ENTITY_AS_MISSION_ENTITY
                                SoundHandler.PlayAudio(SoundHandler.Audio.ShiftParkPull);
                                GameFiber.Wait(1000);
                                Globals.isParked = true;

                                if (ConfigHandler.VehicleParkSirenKill && playerVeh.Class == VehicleClass.Emergency)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Keyboard.KeyDown(ConfigHandler.ELSKey);
                                        GameFiber.Wait(1);
                                        Keyboard.KeyUp(ConfigHandler.ELSKey);
                                        GameFiber.Wait(1);
                                    }
                                }

                                Extensions.VehicleExtensions.LockTransmission(playerVeh, true);
                                Extensions.VehicleExtensions.CreateVehicleBlip(playerVeh);
                            }
                            else
                            {
                                unsafe
                                {
                                    int handle = (int)vehicle.Handle.Value;
                                    NativeFunction.Natives.SET_VEHICLE_AS_NO_LONGER_NEEDED(ref handle);
                                    SoundHandler.PlayAudio(SoundHandler.Audio.ShiftParkRelease);
                                    GameFiber.Wait(1000);
                                    Globals.isParked = false;
                                }

                                Extensions.VehicleExtensions.LockTransmission(playerVeh, false);
                                Extensions.VehicleExtensions.DeleteVehicleBlip(playerVeh);
                            }
                        }
                    }

                    // Tempoary fix gonna do a really advanced system later on where it stores parked vehicles, etc
                    if (Globals.isParked && Game.LocalPlayer.Character.LastVehicle != lastParkedVehicle)
                    {
                        Globals.isParked = false;

                        Extensions.VehicleExtensions.LockTransmission(playerVeh, false);
                        Extensions.VehicleExtensions.DeleteVehicleBlip(playerVeh);
                    }
                    #endregion
                }, "Player Controller - Parking System");
            }

            // Check if shuffling is allowed
            if (ConfigHandler.AllowShuffle == false)
            {
                // Detect if shuffle key is double tapped
                if (Game.IsKeyDown(ConfigHandler.ShuffleKey))
                {
                    shuffleButtonPresses++;
                    if (shuffleButtonPresses == 2)
                    {
                        ShuffleSeats();
                        shuffleButtonPresses = 0;
                    }
                }

                // Check conditions for disabling shuffle
                if (_isShuffleDisabled && playerVeh != null && playerVeh.GetPedOnSeat((int)VehicleSeat.Passenger) == player && N.GetIsTaskActive(player, 165))
                {
                    if (!playerVeh.IsSeatFree((int)VehicleSeat.Driver) && !playerVeh.Driver.IsPlayer)
                    {
                        return;
                    }
                    else
                    {
                        N.SetPedConfigFlag(player, 184, true);
                        player.Tasks.ClearImmediately();
                        N.SetPedIntoVehicle(player, playerVeh, (int)VehicleSeat.Passenger);
                    }
                }
                else if (!_isShuffleDisabled && playerVeh != null && playerVeh.IsSeatFree((int)VehicleSeat.Driver))
                {
                    N.SetPedConfigFlag(player, 184, true);
                    N.SetPedIntoVehicle(player, playerVeh, (int)VehicleSeat.Driver);
                    _isShuffleDisabled = true;
                }
            }

            if (ConfigHandler.VehicleIndicators == true)
            {
                if (ControlHandler.IsControlDownWithModifier(SimpleControls.LIGHT_INDR))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)1;
                }
                if (ControlHandler.IsControlDownWithModifier(SimpleControls.LIGHT_INDL))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)2;
                }
                if (ControlHandler.IsControlDownWithModifier(SimpleControls.LIGHT_HAZRD))
                {
                    intendedStatus = (VehicleIndicatorLightsStatus)3;
                }

                if (ConfigHandler.VehicleIndicatorSounds)
                {
                    switch (status)
                    {
                        case VehicleIndicatorLightsStatus.RightOnly:
                            if (vehicle.IsEngineOn && (DateTime.Now - Managed.LastVehicleIndicator).TotalSeconds > 1.1) // not sure if need adjustment
                            {
                                SoundHandler.PlayAudio(SoundHandler.Audio.Indicator);
                                Managed.LastVehicleIndicator = DateTime.Now;
                            }
                            break;
                        case VehicleIndicatorLightsStatus.LeftOnly:
                            if (vehicle.IsEngineOn && (DateTime.Now - Managed.LastVehicleIndicator).TotalSeconds > 1.1) // not sure if need adjustment
                            {
                                SoundHandler.PlayAudio(SoundHandler.Audio.Indicator);
                                Managed.LastVehicleIndicator = DateTime.Now;
                            }
                            break;
                    }
                }

                switch (ConfigHandler.VehicleIndicatorMode)
                {
                    case "Normal":
                        Extensions.VehicleExtensions.HandleNormalMode(ref intendedStatus, ref status);
                        break;
                    case "TurnOffAtTurn":
                        Extensions.VehicleExtensions.HandleTurnOffAtTurnMode(ref intendedStatus, ref turnOffAt, ref status, ref initialHeading);
                        break;
                    default:
                        Game.LogTrivial("not valid ini option");
                        break;
                        //case "AutomaticTurn":
                        //    Extensions.HandleAutomaticTurnMode(initialHeading);
                        //    break;
                }
            }

            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                Func<bool> controlCondition = () => ControlHandler.IsControlDownWithModifier(SimpleControls.ENG_TOGGLE);

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
            if (Managed.MyVehicle != ClientCurrentVehicle)
            {
                Managed.VehicleFuelLevelInitialized = false;
            }
            Managed.MyVehicle = ClientLastVehicle ?? null;
            int refuelingAllowed;
            if (Managed.MyVehicle != null)
            {
                vehicle = Managed.MyVehicle;
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
            if (!Managed.VehicleFuelLevelInitialized)
            {
                vehicle.InitFuel();
            }
            if (playerVeh.IsAircraft())
            {
                if (!Managed.AircraftEngineOn && vehicle.IsEngineOn)
                {
                    N.SetVehicleEngineOn(vehicle, false, false, true);
                    Managed.AircraftEngineOn = false;
                }
                else if (Managed.AircraftEngineOn && vehicle.IsEngineOn)
                {
                    float percentFuel = vehicle.FuelLevel / vehicle.MaxFuelLevel() * 100f;
                    if (vehicle.IsInAir && percentFuel < ConfigHandler.AircraftLowFuelWarning && (DateTime.Now - Managed.LastAircraftLowFuelWarning).TotalSeconds > 5.0) // adjust timer 
                    {
                        SoundHandler.PlayAudio(SoundHandler.Audio.LowFuel);
                        Managed.LastAircraftLowFuelWarning = DateTime.Now;
                    }
                }
                vehicle.ConsumeAircraftFuel();
                if (!vehicle.IsInAir && vehicle.Speed < 2f && !vehicle.IsEngineOn && (DateTime.Now - Managed.LastAircraftEngineHintDisplayed).TotalSeconds > 30.0)
                {
                    Game.DisplayHelp("Press ~INPUT_VEH_FLY_UNDERCARRIAGE~ to start the engine.");
                    Managed.LastAircraftEngineHintDisplayed = DateTime.Now;
                }
                Managed.LastAircraftAltitude = vehicle.HeightAboveGround;
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
            // if (!N.IsRadarHidden() && (Globals.RefuelingAllowed || vehicle.IsPlayerDriving()))
            if (Globals.RefuelingAllowed || vehicle.IsPlayerDriving())
            {
                if (!N.IsHudHidden() || (player.CurrentVehicle != null && player.CurrentVehicle.IsAircraft()))
                {
                    CustomUI.RenderBar(vehicle.FuelLevel, Managed.VehicleFuelCapacity, vehicle.IsElectric());
                }
                GasStation gas = GasStation.GetClosestInRange(player.Position, 250f);
                if (gas != null)
                {
                    if (gas != Managed.GasStation)
                    {
                        Managed.GasStation = gas;
                    }
                }
                else if (Managed.GasStation != null)
                {
                    Managed.GasStation = null;
                }
            }
            goto IL_0328;
            IL_0328:
            if (Game.LocalPlayer.Character.IsOnFoot)
            {
                Managed.AircraftEngineOn = false;
                ClientPed.ManualRefuel();
                Managed.VehicleFuelLevelInitialized = false;
                foreach (int x in Managed.TripInfos.Keys)
                {
                    if (Managed.TripInfos[x] != null)
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
            Managed.LastWorldTime = DateTime.UtcNow;
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
                bool _seatBeltOn = ClientPed.CanFlyThroughWindshields;

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

                if (isInVehicle && ClientPed.IsAlive && currentVehicle.Class != VehicleClass.Helicopter && currentVehicle.Class != VehicleClass.Plane)
                {
                    Func<bool> controlCondition = () =>
                        N.IsDisabledControlPressed(0, (int)GameControl.VehicleExit) &&
                        _seatBeltOn; 

                    Game.DisableControlAction(0, GameControl.VehicleExit, true);

                    leaveEngineRunning.CheckControlHoldDuration(controlCondition, 200, () => 
                    {
                        currentVehicle.IsEngineOn = true;
                        if (_keepDoorsOpen)
                        {
                            ClientPed.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                        } 
                        else
                        {
                            ClientPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                        }
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