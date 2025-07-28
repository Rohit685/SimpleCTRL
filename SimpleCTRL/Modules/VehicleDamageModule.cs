namespace SimpleCTRL.Modules
{
    public class VehicleDamageModule : CommonPlugin
    {
        #region Vehicle Damage State Fields

        private static bool pedInSameVehicleLast;
        private static Vehicle _currentVehicle, _lastVehicle, _repairedVehicle;

        #endregion

        #region Vehicle Health and Damage Multiplier Settings

        // Damage multipliers
        private static float _fCollisionDamageMult, _fDeformationDamageMult, _fEngineDamageMult = 0f;
        private static float _fBrakeForce = 1f;

        // Engine Health
        private static float healthEngineLast, healthEngineCurrent, healthEngineNew = 1000f;
        private static float healthEngineDelta, healthEngineDeltaScaled = 0f;

        // Body Health
        private static float healthBodyLast, healthBodyCurrent, healthBodyNew = 1000f;
        private static float healthBodyDelta, healthBodyDeltaScaled = 0f;

        // Petrol Tank Health
        private static float healthPetrolTankLast, healthPetrolTankCurrent, healthPetrolTankNew = 1000f;
        private static float healthPetrolTankDelta, healthPetrolTankDeltaScaled = 0f;

        #endregion

        #region Initialization
        public static void Start()
        {
            Logging.Info("Starting VehicleDamageModule...", "VehicleDamageModule");
            GameFiber.StartNew(Run, "SimpleCTRL - Vehicle Damage Module");
            Logging.Info("VehicleDamageModule has started.", "VehicleDamageModule");
        }

        public static void Run()
        {
            while (true)
            {
                FlipTick();
                MainLoop();
                GameFiber.Yield();
            }
        }
        #endregion

        #region Main Logic
        private static void FlipTick()
        {
            if (!Settings.TorqueMultiplierEnable && !Settings.LimpMode)
            {
                return;
            }

            if (Settings.TorqueMultiplierEnable || Settings.LimpMode)
            {
                if (!pedInSameVehicleLast)
                {
                    return;
                }

                float factor = 1f;
                if (Settings.TorqueMultiplierEnable && healthEngineNew < 900)
                {
                    factor = (healthEngineNew + 200f) / 1100;
                }

                if (Settings.LimpMode && healthEngineNew < (Settings.EngineSafeGuard + 5))
                {
                    factor = Settings.LimpModeMultiplier;
                    N.SetVehicleMaxSpeed(_currentVehicle, 20f);
                }

                _currentVehicle.EngineTorqueMultiplier(factor);
            }
        }

        private static void MainLoop()
        {
            if (!ClientPed.IsInAnyVehicle(false))
            {
                if (pedInSameVehicleLast)
                {
                    _lastVehicle = ClientPed.LastVehicle;

                    if (EntityExtensions.Exists(_lastVehicle))
                    {
                        if (Settings.DeformationMultiplier != -1)
                        {
                            _lastVehicle.HandlingData.DeformationDamageMultiplier = _fDeformationDamageMult; // Restore deformation multiplier
                        }

                        _lastVehicle.HandlingData.BrakeForce = _fBrakeForce; // Restore Brake Force multiplier

                        if (Settings.WeaponsDamageMultiplier != 1)
                        {
                            _lastVehicle.HandlingData.WeaponDamageMultiplier = Settings.WeaponsDamageMultiplier; // Since we are out of the vehicle, we should no longer compensate for bodyDamageFactor
                        }
                        _lastVehicle.HandlingData.CollisionDamageMultiplier = _fCollisionDamageMult; // Restore the original CollisionDamageMultiplier
                        _lastVehicle.HandlingData.EngineDamageMultiplier = _fEngineDamageMult; // Restore the original EngineDamageMultiplier
                    }
                }

                pedInSameVehicleLast = false;
                return;
            } 

            if (ClientPed.IsInAnyVehicle(false))
            {
                _currentVehicle = ClientPed.CurrentVehicle;

                float classMultiplier;

                try
                {
                    classMultiplier = Settings.ClassDamageMultiplier[(int)_currentVehicle.Class];
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    classMultiplier = 1.0f;
                    Logging.Error($"ArgumentOutOfRangeException in vehicle class multiplier: {ex.Message}", "VehicleDamageModule");
                }

                healthEngineCurrent = _currentVehicle.EngineHealth;
                if (healthEngineCurrent == 1000f) healthEngineLast = 1000f;
                healthEngineNew = healthEngineCurrent;
                healthEngineDelta = healthEngineLast - healthEngineCurrent;
                healthEngineDeltaScaled = healthEngineDelta * Settings.DamageFactorEngine * classMultiplier;

                if (healthEngineDelta > 5)
                {
                    Logging.Debug($"Engine damage sustained. New value {healthEngineNew}", "VehicleDamageModule");
                }

                healthBodyCurrent = N.GetVehicleBodyHealth(_currentVehicle);
                if (healthBodyCurrent == 1000f) healthBodyLast = 1000f;
                healthBodyNew = healthBodyCurrent;
                healthBodyDelta = healthBodyLast - healthBodyCurrent;
                healthBodyDeltaScaled = healthBodyDelta * Settings.DamageFactorBody * classMultiplier;

                if (healthBodyDelta > 5)
                {
                    Logging.Debug($"Body damage sustained. New value {healthBodyNew}", "VehicleDamageModule");
                }

                healthPetrolTankCurrent = _currentVehicle.FuelTankHealth;
                if (healthPetrolTankCurrent == 1000f) healthPetrolTankLast = 1000f;
                healthPetrolTankNew = healthPetrolTankCurrent;
                healthPetrolTankDelta = healthPetrolTankLast - healthPetrolTankCurrent;
                healthPetrolTankDeltaScaled = healthPetrolTankDelta * Settings.DamageFactorPetrolTank * classMultiplier;

                if (healthPetrolTankDelta > 5)
                {
                    Logging.Debug($"Tank damage sustained. New value {healthPetrolTankNew}", "VehicleDamageModule");
                }

                if (healthEngineCurrent > Settings.EngineSafeGuard + 1 && _currentVehicle.FuelLevel > 1f)
                {
                    _currentVehicle.IsDriveable = true;
                }

                if (healthEngineCurrent <= Settings.EngineSafeGuard && (!Settings.LimpMode || _currentVehicle.OilLevel() < 3f) && !N.IsVehicleTyreBurst(_currentVehicle, 1, true))
                {
                    Logging.Debug("Engine health fell below threshold. Making vehicle undriveable (limp mode off)", "VehicleDamageModule");
                    _currentVehicle.IsDriveable = false;
                    N.SetVehicleTyreBurst(_currentVehicle, 1, true, 1000f);
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

                        // If complete damage, but not catastrophic(ie.explosion territory) pull back a bit, to give a couple seconds of engine runtime before dying
                        if (healthEngineCombinedDelta > healthEngineCurrent)
                        {
                            healthEngineCombinedDelta = healthEngineCurrent - (Settings.CascadingFailureThreshold / 5);
                        }

                        // ======= Calculate new value =======
                        healthEngineNew = healthEngineLast - healthEngineCombinedDelta;

                        // ======= Sanity Check on new values and further manipulations
                        //  If somewhat damaged, slowly degrade until slightly before cascading failure sets in, then stop

                        if (healthEngineNew > (Settings.DegradingFailureThreshold + 5) && (_currentVehicle.Class == VehicleClass.Emergency ? healthEngineNew < 850f : healthEngineNew < 950f) && _currentVehicle.IsEngineOn && _currentVehicle.Speed > 2f)
                        {
                            healthEngineNew -= (0.02f * Settings.DegradingHealthSpeedFactor);
                        }

                        // If Damage is near catastrophic, cascade the failure
                        if (healthEngineNew < Settings.CascadingFailureThreshold && _currentVehicle.IsEngineOn && _currentVehicle.Speed > 2f)
                        {
                            healthEngineNew -= (0.05f * Settings.CascadingFailureSpeedFactor);
                        }

                        // Prevent Engine going to or below zero. Ensures you can reenter a damaged car.
                        if (healthEngineNew < Settings.EngineSafeGuard)
                        {
                            healthEngineNew = Settings.EngineSafeGuard;
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
                    Logging.Debug("New vehicle detected. Ignoring damage this tick", "VehicleDamageModule");
                    // Just got into a vehicle. Damage cannot be multipled this round

                    // Set vehicle handling meta
                    _fDeformationDamageMult = _currentVehicle.HandlingData.DeformationDamageMultiplier;
                    _fBrakeForce = _currentVehicle.HandlingData.BrakeForce;
                    if (Settings.DeformationMultiplier != -1)
                    {
                        _currentVehicle.HandlingData.DeformationDamageMultiplier = (float)Math.Pow(_fDeformationDamageMult, Settings.DeformationExponent) * Settings.DeformationMultiplier; // Multiply by our factor
                    }

                    if (Settings.WeaponsDamageMultiplier != -1)
                    {
                        _currentVehicle.HandlingData.WeaponDamageMultiplier = Settings.WeaponsDamageMultiplier / Settings.DamageFactorBody; // Set weaponsDamageMultiplier and compensate for damageFactorBody
                    }

                    _fCollisionDamageMult = _currentVehicle.HandlingData.CollisionDamageMultiplier;
                    // Modify it by pulling all numbers to 1f
                    _currentVehicle.HandlingData.CollisionDamageMultiplier = (float)Math.Pow(_fCollisionDamageMult, Settings.CollisionDamageExponent);

                    _fEngineDamageMult = _currentVehicle.HandlingData.EngineDamageMultiplier;
                    _currentVehicle.HandlingData.EngineDamageMultiplier = (float)Math.Pow(_fEngineDamageMult, Settings.EngineDamageExponent);

                    // If body damage catastrophic, reset somewhat so we can get new damage to multiply
                    if (healthBodyCurrent < Settings.CascadingFailureThreshold)
                    {
                        healthBodyNew = Settings.CascadingFailureThreshold;
                    }

                    pedInSameVehicleLast = true;
                }

                // Set the actual values
                if (healthEngineNew != healthEngineCurrent)
                {
                    _currentVehicle.EngineHealth = healthEngineNew;
                }
                if (healthBodyNew != healthBodyCurrent)
                {
                    N.SetVehicleBodyHealth(_currentVehicle, healthBodyNew);
                }
                if (healthPetrolTankNew != healthPetrolTankCurrent)
                {
                    _currentVehicle.FuelTankHealth = healthPetrolTankNew;
                }

                // Store current values, so we can calculate delta next time
                healthEngineLast = healthEngineNew;
                healthBodyLast = healthBodyNew;
                healthPetrolTankLast = healthPetrolTankNew;
                _lastVehicle = _currentVehicle;
            }
        }
        #endregion
    }
}
