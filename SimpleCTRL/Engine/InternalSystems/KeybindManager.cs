namespace SimpleCTRL.Engine.InternalSystems
{
    internal class KeybindManager : CommonPlugin
    {
        #region Configuration & State
        private static bool _restrictEmergency = false;
        private static bool _keepDoorsOpen = true;
        private static bool _doorsNotify = false;
        private static bool _isSeatbeltFastened;
        private static bool isHeld;
        private static int heldTime;
        private static int elapsedTime;
        private static int timeout;
        #endregion

        #region Startup & Execution
        public static void Start()
        {
            Logging.Info("Initializing input and keybind monitoring threads...", "KeybindManager");
            GameFiber.StartNew(Run, "SimpleCTRL - Keybind Monitor");
            Logging.Info("User input processing thread successfully initialized.", "KeybindManager");
            Logging.Info("Keybind monitoring thread started.", "KeybindManager");
        }

        public static void Run()
        {
            while (true)
            {
                MainLoop();
                GameFiber.Yield();
            }
        }
        #endregion

        #region Keybind Processing
        private static void MainLoop()
        {
            // --- Engine Run On Exit Feature --- 
            // This section checks if the "Engine Run On Exit" feature is enabled and handles the related logic.

            if (Settings.EnableEngineRunOnExit)
            {
                UpdateSeatbeltStatus();

                Vehicle currentVehicle = ClientPed.LastVehicle;
                bool isInVehicle = ClientPed.IsInAnyVehicle(false);

                if (_restrictEmergency && currentVehicle.Class != VehicleClass.Emergency)
                    return;

                if (ShouldNotifyEngineRunOnExit(currentVehicle, isInVehicle))
                {
                    Game.DisplayNotification("Hold ~b~F ~w~when exiting to leave the engine running.");
                    _doorsNotify = true;
                }

                if (isInVehicle && ClientPed.IsAlive && 
                    currentVehicle.Class != VehicleClass.Helicopter && 
                    currentVehicle.Class != VehicleClass.Plane)
                {
                    Func<bool> controlCondition = () =>
                        N.IsDisabledControlPressed(0, (int)GameControl.VehicleExit) &&
                        !_isSeatbeltFastened;

                    Game.DisableControlAction(0, GameControl.VehicleExit, true);

                    CheckControlHoldDuration(controlCondition, 200, () =>
                    {
                        currentVehicle.IsEngineOn = true;
                        ClientPed.Tasks.LeaveVehicle(_keepDoorsOpen ? LeaveVehicleFlags.LeaveDoorOpen : LeaveVehicleFlags.None);
                    }, () =>
                    {
                        ClientPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                    });
                }    
            }
        }
        #endregion

        #region Utility Methods
        private static void CheckControlHoldDuration(Func<bool> controlCondition, int requiredTime, Action firstAction, Action alternativeAction = null)
        {
            if (timeout > 0)
                timeout--;

            bool isConditionMet = controlCondition.Invoke();

            if (isConditionMet && timeout <= 0)
            {
                if (!isHeld)
                {
                    isHeld = true;
                    heldTime = (int)Game.GameTime;
                    Logging.Debug("Control hold started", "KeybindManager");
                }
                else
                {
                    elapsedTime = (int)(Game.GameTime - heldTime);
                    Logging.Debug($"Control hold ongoing: elapsedTime = {elapsedTime}ms", "KeybindManager");

                    if (elapsedTime >= requiredTime)
                    {
                        Logging.Debug("Required hold time met, invoking first action", "KeybindManager");
                        firstAction.Invoke();
                    }
                }
            }
            else
            {
                if (isHeld && elapsedTime <= requiredTime && alternativeAction != null)
                {
                    Logging.Debug("Control released early, invoking alternative action", "KeybindManager");
                    alternativeAction.Invoke();
                }

                if (isHeld)
                    Logging.Debug("Control hold reset", "KeybindManager");

                isHeld = false;
                elapsedTime = 0;
            }
        }

        private static void UpdateSeatbeltStatus()
        {
            if (ClientPed.IsInAnyVehicle(false) &&
                N.DecorExistOn(ClientPed.CurrentVehicle, "seatbeltFastened"))
            {
                _isSeatbeltFastened = N.DecorGetBool(ClientPed.CurrentVehicle, "seatbeltFastened");
            }
        }

        private static bool ShouldNotifyEngineRunOnExit(Vehicle vehicle, bool isInVehicle)
        {
            return Settings.EngineRunOnExitNotification &&
                   !_doorsNotify &&
                   isInVehicle &&
                   vehicle.Driver == ClientPed &&
                   vehicle.Class != VehicleClass.Helicopter &&
                   vehicle.Class != VehicleClass.Plane;
        }
        #endregion
    }
}
