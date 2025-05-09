namespace SimpleCTRL.Engine.InternalSystems
{
    internal class KeybindManager : CommonPlugin
    {
        #region Configuration & State
        // --- Engine & Emergency Restrictions ---
        private static bool _restrictEmergency = false;
        private static bool _keepDoorsOpen = true;
        private static bool _doorsNotify = false;

        // --- Seatbelt Tracking ---
        private static bool _isSeatbeltFastened;

        // --- Control Hold Tracking ---
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
            // Decrease the timeout counter if it's greater than zero
            if (timeout > 0)
                timeout--;

            // Check if the control condition is met and the timeout has elapsed
            if (controlCondition.Invoke() && timeout <= 0)
            {
                // If the control is not already held, mark the start time
                if (!isHeld)
                {
                    isHeld = true;
                    heldTime = (int)Game.GameTime;  // Store the time when the control is first held
                }
                else
                {
                    // Calculate the elapsed time since the control was first held
                    elapsedTime = (int)(Game.GameTime - heldTime);

                    // If the required time has passed, trigger the first action
                    if (elapsedTime >= requiredTime)
                    {
                        firstAction.Invoke();
                    }
                }
            }
            else
            {
                // If the control was held but released too early, trigger the alternative action (if provided)
                if (isHeld && elapsedTime <= requiredTime && alternativeAction != null)
                {
                    alternativeAction.Invoke();
                }

                // Reset the holding state
                isHeld = false;
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
