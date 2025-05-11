namespace SimpleCTRL.Modules
{
    public class VehicleDamageModule : CommonPlugin
    {
        #region Constants

        private const float MaxVehicleHealth = 1000f;
        private const float EngineSafeGuard = 100f;
        private const float CascadingFailureThreshold = 310f;

        #endregion

        #region Vehicle Configuration and Damage Settings

        // Vehicle state
        private static bool pedInSameVehicleLast;
        private static Vehicle _lastVehicle, _repairedVehicle;

        // Deformation and collision settings
        private static int DeformationMultiplier = -1;
        private static float DeformationExponent = 1f;
        private static float CollisionDamageExponent = 1f;

        // Damage factors
        private static float DamageFactorEngine = 5.1f;
        private static float DamageFactorBody = 5.1f;
        private static float DamageFactorPetrolTank = 61f;
        private static float EngineDamageExponent = 1f;
        private static float WeaponsDamageMultiplier = 0.124f;

        // Health and failure thresholds
        private static float DegradingHealthSpeedFactor = 3.0f;
        private static float CascadingFailureSpeedFactor = 1.5f;
        private static float DegradingFailureThreshold = 677f;

        // Torque and limp mode settings
        private static bool TorqueMultiplierEnable = true;
        private static bool LimpMode = true;
        private static float LimpModeMultiplier = 0.15f;

        // Class-specific damage multipliers
        private static List<float> ClassDamageMultiplier = new List<float>
        {
            1.0f, 1.0f, 1.0f, 0.95f, 1.0f, 0.95f, 0.95f, 0.95f, 0.27f, 0.7f, 0.25f, 0.35f, 0.85f, 1.0f, 0.4f, 0.7f, 0.7f, 0.75f, 0.05f, 0.67f, 0.43f, 1.0f
        };

        #endregion

        #region Vehicle Health and Damage Multiplier Settings

        // Damage multipliers
        private static float _fCollisionDamageMult, _fDeformationDamageMult, _fEngineDamageMult = 0f;
        private static float _fBrakeForce = 1f;

        // Engine Health
        private static float healthEngineLast, healthEngineCurrent, healthEngineNew = MaxVehicleHealth;
        private static float healthEngineDelta, healthEngineDeltaScaled = 0f;

        // Body Health
        private static float healthBodyLast, healthBodyCurrent, healthBodyNew = MaxVehicleHealth;
        private static float healthBodyDelta, healthBodyDeltaScaled = 0f;

        // Petrol Tank Health
        private static float healthPetrolTankLast, healthPetrolTankCurrent, healthPetrolTankNew = MaxVehicleHealth;
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
            if (!TorqueMultiplierEnable && !LimpMode) return;

            if (!pedInSameVehicleLast) return;

            float factor = 1f;

            if (TorqueMultiplierEnable && healthEngineNew < 900)
            {
                factor = (healthEngineNew + 200f) / 1100;
            }

            if (LimpMode && healthEngineNew < (EngineSafeGuard + 5))
            {
                factor = LimpModeMultiplier;
                N.SetVehicleMaxSpeed(ClientPed.CurrentVehicle, 20f);
            }

            ClientPed.CurrentVehicle.EngineTorqueMultiplier(factor);
        }

        private static void MainLoop()
        {
            if (!ClientPed.IsInAnyVehicle(false))
            {
                if (pedInSameVehicleLast)
                {
                    RestoreVehicleSettings();
                }

                pedInSameVehicleLast = false;
                return;
            }

            if (ClientPed.IsInAnyVehicle(false))
            {
                CalculateVehicleHealth();

                UpdateVehicleState();

                if (ClientPed.CurrentVehicle != _lastVehicle)
                {
                    pedInSameVehicleLast = false;
                }

                if (pedInSameVehicleLast)
                {
                    HandleDamageInVehicle();
                }
                else
                {
                    InitializeNewVehicle();
                }

                StoreCurrentValues();
            }
        }
        #endregion

        #region Vehicle Handling
        private static void RestoreVehicleSettings()
        {
            _lastVehicle = ClientPed.LastVehicle;

            if (EntityExtensions.Exists(_lastVehicle))
            {
                if (DeformationMultiplier != -1)
                    _lastVehicle.HandlingData.DeformationDamageMultiplier = _fDeformationDamageMult;

                _lastVehicle.HandlingData.BrakeForce = _fBrakeForce;

                if (WeaponsDamageMultiplier != 1)
                    _lastVehicle.HandlingData.WeaponDamageMultiplier = WeaponsDamageMultiplier;

                _lastVehicle.HandlingData.CollisionDamageMultiplier = _fCollisionDamageMult;
                _lastVehicle.HandlingData.EngineDamageMultiplier = _fEngineDamageMult;
            }
        }

        private static void UpdateVehicleState()
        {
            if (healthEngineCurrent > EngineSafeGuard + 1 && ClientPed.CurrentVehicle.FuelLevel > 1f)
            {
                ClientPed.CurrentVehicle.IsDriveable = true;
            }

            if (healthEngineCurrent <= EngineSafeGuard && (!LimpMode || ClientPed.CurrentVehicle.OilLevel() < 3f) && !N.IsVehicleTyreBurst(ClientPed.CurrentVehicle, 1, true))
            {
                ClientPed.CurrentVehicle.IsDriveable = false;
                N.SetVehicleTyreBurst(ClientPed.CurrentVehicle, 1, true, 1000f);
            }
        }

        private static void HandleDamageInVehicle()
        {
            if (healthEngineCurrent != MaxVehicleHealth || healthBodyCurrent != MaxVehicleHealth || healthPetrolTankCurrent != MaxVehicleHealth)
            {
                // Combine the delta values (Get the largest of the three)
                float healthEngineCombinedDelta = Math.Max(healthEngineDeltaScaled, Math.Max(healthBodyDeltaScaled, healthPetrolTankDeltaScaled));

                // Prevent catastrophic damage from going below a reasonable threshold
                healthEngineCombinedDelta = Math.Min(healthEngineCombinedDelta, healthEngineCurrent - (CascadingFailureThreshold / 5));

                // ======= Calculate new value =======
                healthEngineNew = healthEngineLast - healthEngineCombinedDelta;

                // ======= Sanity Check and further manipulations =======
                ApplyHealthDecay();
            }
            else
            {
                // Vehicle is fixed?
                _repairedVehicle = null;
                N.SetVehicleMaxSpeed(ClientPed.CurrentVehicle, 500.01f);
            }
        }

        private static void ApplyHealthDecay()
        {
            if (healthEngineNew > (DegradingFailureThreshold + 5) && (ClientPed.CurrentVehicle.Class == VehicleClass.Emergency ? healthEngineNew < 850f : healthEngineNew < 950f) && ClientPed.CurrentVehicle.IsEngineOn && ClientPed.CurrentVehicle.Speed > 2f)
            {
                healthEngineNew -= (0.02f * DegradingHealthSpeedFactor);
            }

            if (healthEngineNew < CascadingFailureThreshold && ClientPed.CurrentVehicle.IsEngineOn && ClientPed.CurrentVehicle.Speed > 2f)
            {
                healthEngineNew -= (0.05f * CascadingFailureSpeedFactor);
            }

            if (healthEngineNew < EngineSafeGuard)
            {
                healthEngineNew = EngineSafeGuard;
            }

            if (healthBodyNew < 0f)
            {
                healthBodyNew = 0f;
            }
        }

        private static void InitializeNewVehicle()
        {
            // Set vehicle handling meta
            _fDeformationDamageMult = ClientPed.CurrentVehicle.HandlingData.DeformationDamageMultiplier;
            _fBrakeForce = ClientPed.CurrentVehicle.HandlingData.BrakeForce;

            if (DeformationMultiplier != -1)
            {
                ClientPed.CurrentVehicle.HandlingData.DeformationDamageMultiplier = (float)Math.Pow(_fDeformationDamageMult, DeformationExponent) * DeformationMultiplier;
            }

            if (WeaponsDamageMultiplier != -1)
            {
                ClientPed.CurrentVehicle.HandlingData.WeaponDamageMultiplier = WeaponsDamageMultiplier / DamageFactorBody;
            }

            _fCollisionDamageMult = ClientPed.CurrentVehicle.HandlingData.CollisionDamageMultiplier;
            ClientPed.CurrentVehicle.HandlingData.CollisionDamageMultiplier = (float)Math.Pow(_fCollisionDamageMult, CollisionDamageExponent);

            _fEngineDamageMult = ClientPed.CurrentVehicle.HandlingData.EngineDamageMultiplier;
            ClientPed.CurrentVehicle.HandlingData.EngineDamageMultiplier = (float)Math.Pow(_fEngineDamageMult, EngineDamageExponent);

            // If body damage is catastrophic, reset health
            if (healthBodyCurrent < CascadingFailureThreshold)
            {
                healthBodyNew = CascadingFailureThreshold;
            }

            pedInSameVehicleLast = true;
        }

        private static void StoreCurrentValues()
        {
            if (healthEngineNew != healthEngineCurrent)
            {
                ClientPed.CurrentVehicle.EngineHealth = healthEngineNew;
            }
            if (healthBodyNew != healthBodyCurrent)
            {
                N.SetVehicleBodyHealth(ClientPed.CurrentVehicle, healthBodyNew);
            }
            if (healthPetrolTankNew != healthPetrolTankCurrent)
            {
                ClientPed.CurrentVehicle.FuelTankHealth = healthPetrolTankNew;
            }

            healthEngineLast = healthEngineNew;
            healthBodyLast = healthBodyNew;
            healthPetrolTankLast = healthPetrolTankNew;
            _lastVehicle = ClientPed.CurrentVehicle;
        }
        #endregion

        #region Health Calculation
        // Calculate health changes for engine, body, and petrol tank
        private static void CalculateVehicleHealth()
        {
            float classMultiplier = GetClassDamageMultiplier();

            // Engine Health Calculation
            healthEngineCurrent = ClientPed.CurrentVehicle.EngineHealth;
            healthEngineLast = (healthEngineCurrent == MaxVehicleHealth) ? MaxVehicleHealth : healthEngineLast;
            healthEngineNew = healthEngineCurrent;
            healthEngineDelta = healthEngineLast - healthEngineCurrent;
            healthEngineDeltaScaled = healthEngineDelta * DamageFactorEngine * classMultiplier;

            // Body Health Calculation
            healthBodyCurrent = N.GetVehicleBodyHealth(ClientPed.CurrentVehicle);
            healthBodyLast = (healthBodyCurrent == MaxVehicleHealth) ? MaxVehicleHealth : healthBodyLast;
            healthBodyNew = healthBodyCurrent;
            healthBodyDelta = healthBodyLast - healthBodyCurrent;
            healthBodyDeltaScaled = healthBodyDelta * DamageFactorBody * classMultiplier;

            // Petrol Tank Health Calculation
            healthPetrolTankCurrent = ClientPed.CurrentVehicle.FuelTankHealth;
            healthPetrolTankLast = (healthPetrolTankCurrent == MaxVehicleHealth) ? MaxVehicleHealth : healthPetrolTankLast;
            healthPetrolTankNew = healthPetrolTankCurrent;
            healthPetrolTankDelta = healthPetrolTankLast - healthPetrolTankCurrent;
            healthPetrolTankDeltaScaled = healthPetrolTankDelta * DamageFactorPetrolTank * classMultiplier;
        }

        private static float GetClassDamageMultiplier()
        {
            try
            {
                return ClassDamageMultiplier[(int)ClientPed.CurrentVehicle.Class];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logging.Error($"ArgumentOutOfRangeException in vehicle class multiplier: {ex.Message}", "VehicleDamageModule");
                return 1.0f; // Default to 1.0 if there's an error
            }
        }
        #endregion
    }
}
