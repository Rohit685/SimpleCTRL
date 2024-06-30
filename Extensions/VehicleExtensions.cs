using Common;
using Common.Elements;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Components;
using SimpleCTRL.Core.Models.UI;
using SimpleCTRL.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCTRL.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Vehicle"/> class.
    /// </summary>
    internal static class VehicleExtensions
    {
        private static Vector3 grabbedNozzleCoords;
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private static List<string> props = new List<string>
        {
            "prop_gas_pump_1d",
            "prop_gas_pump_1a",
            "prop_gas_pump_1b",
            "prop_gas_pump_1c", 
            "prop_vintage_pump",
            "prop_gas_pump_old2",
            "prop_gas_pump_old3",
        };

        #region Pump Methods

        internal static Rage.Object GetClosestPump(Vector3 coords)
        {
            foreach (var prop in props)
            {
                Rage.Object pump = NativeFunction.Natives.GET_CLOSEST_OBJECT_OF_TYPE<Rage.Object>(coords.X, coords.Y, coords.Z, 3.0f, Game.GetHashKey(prop), true, true, true);

                if (pump != null)
                {
                    return pump;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the vehicle is near any fuel pump.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns><c>true</c> if the vehicle is near any fuel pump; otherwise, <c>false</c>.</returns>
        internal static bool IsVehicleNearAnyPump(this Vehicle vehicle)
        {
            Vector3 fuelTankPos = GetVehicleTankPos(vehicle);
            if (Managed.GasStation != null)
            {
                return Managed.GasStation.Pumps.Any((GasPump x) => Vector3.DistanceSquared(x.Position, fuelTankPos) <= 20f);
            }
            return false;
        }

        /// <summary>
        /// Retrieves the position of the vehicle's fuel tank.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The position of the vehicle's fuel tank.</returns>
        private static Vector3 GetVehicleTankPos(Vehicle vehicle)
        {
            string[] vehicleFuelTankBones = UtilityConstants.VehicleFuelTankBones;
            int foundBoneIndex = -1;
            foreach (string boneName in vehicleFuelTankBones)
            {
                try
                {
                    int boneIndex = vehicle.GetBoneIndex(boneName);
                    if (boneIndex != -1 && vehicle.HasBone(boneIndex))
                    {
                        foundBoneIndex = boneIndex;
                        break;
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            return vehicle.GetBonePosition(foundBoneIndex);
        }

        private static Entity vehicleInFront()
        {
            Vector3 offset = N.GetOffsetFromEntityInWorldCoords(Game.LocalPlayer.Character, 0.0f, 2.0f, 0.0f);
            Vector3 pedCoords = Game.LocalPlayer.Character.Position;

            int rayHandle = NativeFunction.CallByHash<int>(0x377906D8A31E5586, pedCoords.X, pedCoords.Y, pedCoords.Z - 1.3f, offset.X, offset.Y, offset.Z, 10, Game.LocalPlayer.Character, 0);

            int hit = 0;
            Vector3 endCoords = Vector3.Zero;
            Vector3 surfaceNormal = Vector3.Zero;
            int entityHandle = 0;

            NativeFunction.Natives.GET_SHAPE_TEST_RESULT<int>(rayHandle, out hit, out endCoords, out surfaceNormal, out entityHandle);

            if (NativeFunction.CallByHash<bool>(0x6AC7003FA6E5575E, entityHandle)) // IS_ENTITY_A_VEHICLE
            {
                return World.GetEntityByHandle<Vehicle>((uint)entityHandle);
            }

            return null;
        }
        #endregion

        #region Tanker Methods
        /// <summary>
        /// Checks if the vehicle is near any fuel tanker or fuel pump.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns><c>true</c> if the vehicle is near any fuel tanker or fuel pump; otherwise, <c>false</c>.</returns>
        internal static bool IsVehicleNearAnyTankerOrFuelPump(this Vehicle vehicle)
        {
            if (ConfigHandler.AircraftUseAirportPumps)
            {
                AirportFuelPump airport = AirportFuelPump.GetClosestInRange(vehicle.Position, 100f);
                if (airport != null && airport.Position != Vector3.Zero)
                {
                    return true;
                }
            }
            if (ConfigHandler.AircraftUseFuelTankers)
            {
                Vehicle tanker = FindNearbyTanker(vehicle);
                if (tanker != null && EntityExtensions.Exists(tanker))
                {
                    return vehicle.Position.DistanceTo(tanker.Position) <= 100f;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Finds the nearby fuel tanker.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The nearby fuel tanker, if found; otherwise, <c>null</c>.</returns>
        private static Vehicle FindNearbyTanker(Vehicle v)
        {
            Vehicle tanker = null;

            if (ConfigHandler.AircraftUseFuelTankers)
            {
                var nearbyEntities = World.GetEntities(v.Position, 100f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle);

                if (nearbyEntities.Any())
                {
                    tanker = nearbyEntities
                        .OfType<Vehicle>()
                        .FirstOrDefault(entity => entity != null &&
                           entity.Exists() &&
                           ConfigHandler.AircraftFuelTankers
                               .Any(x => x.Equals(entity.Model.Hash)));
                }
            }

            return tanker;
        }
        #endregion

        #region Driver Methods
        /// <summary>
        /// Checks if the player is driving the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns><c>true</c> if the player is driving the vehicle; otherwise, <c>false</c>.</returns>
        public static bool IsPlayerDriving(this Vehicle vehicle)
        {
            bool driving = false;
            if (Game.LocalPlayer.Character != null && EntityExtensions.Exists(Game.LocalPlayer.Character))
            {
                driving = !Game.LocalPlayer.Character.IsOnFoot && vehicle.Driver != null && EntityExtensions.Exists(vehicle.Driver) && vehicle.Driver.Handle == Game.LocalPlayer.Character.Handle;
            }
            return driving;
        }
        #endregion

        #region Vehicle Type Methods
        /// <summary>
        /// Checks if the vehicle is a road vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns><c>true</c> if the vehicle is a road vehicle; otherwise, <c>false</c>.</returns>
        internal static bool IsRoadVehicle(this Vehicle vehicle)
        {
            if (!vehicle.Model.IsBicycle)
            {
                if (!vehicle.Model.IsCar && !vehicle.Model.IsBike)
                {
                    return vehicle.Model.IsQuadBike;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the vehicle is an aircraft.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns><c>true</c> if the vehicle is an aircraft; otherwise, <c>false</c>.</returns>
        internal static bool IsAircraft(this Vehicle vehicle) => vehicle != null && (vehicle.Model.IsHelicopter || vehicle.Model.IsPlane);
        #endregion

        #region Fuel Methods
        /// <summary>
        /// Randomizes the fuel level of the vehicle.
        /// </summary>
        /// <param name="veh">The vehicle.</param>
        /// <param name="fuelCapacity">The fuel capacity of the vehicle.</param>
        /// <returns>The randomized fuel level.</returns>
        public static float RandomizeFuelLevel(this Vehicle veh, float fuelCapacity)
        {
            float randomizedFuelLevel;
            if (new Random().Next(0, 4) != 0)
            {
                float min = fuelCapacity / 4f;
                float max = fuelCapacity / 2f;
                randomizedFuelLevel = (float)(new Random().NextDouble() * (double)(max - min) + (double)min);
            }
            else
            {
                randomizedFuelLevel = fuelCapacity;
            }
            return randomizedFuelLevel;
        }

        /// <summary>
        /// Initializes the fuel level of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public static void InitFuel(this Vehicle vehicle)
        {
            Managed.VehicleFuelLevelInitialized = true;
            Managed.VehicleFuelCapacity = MaxFuelLevel(vehicle);
            if (!N.DecorExistOn(vehicle, "_Fuel_Level"))
            {
                if (IsAircraft(vehicle))
                {
                    N.DecorSetFloat(vehicle, "_Fuel_Level", Managed.VehicleFuelCapacity);
                }
                else
                {
                    N.DecorSetFloat(vehicle, "_Fuel_Level", RandomizeFuelLevel(vehicle, Managed.VehicleFuelCapacity));
                }
            }
            vehicle.FuelLevel = N.DecorGetFloat(vehicle, "_Fuel_Level");
        }

        /// <summary>
        /// Sets the fuel level of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="fuelLevel">The fuel level to set.</param>
        public static void SetFuelLevel(this Vehicle vehicle, float fuelLevel)
        {
            float max = MaxFuelLevel(vehicle);
            if (fuelLevel > max)
            {
                fuelLevel = max;
            }
            vehicle.FuelLevel = fuelLevel;
            N.DecorSetFloat(vehicle, "_Fuel_Level", fuelLevel);
        }

        /// <summary>
        /// Gets the fuel level of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The fuel level of the vehicle.</returns>
        public static float GetFuelLevel(this Vehicle vehicle)
        {
            if (N.DecorExistOn(vehicle, "_Fuel_Level"))
            {
                return N.DecorGetFloat(vehicle, "_Fuel_Level");
            }
            return 65f;
        }

        /// <summary>
        /// Gets the maximum fuel level of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The maximum fuel level of the vehicle.</returns>
        public static float MaxFuelLevel(this Vehicle vehicle)
        {
            float maxFuel = vehicle.HandlingData.PetrolTankVolume;
            if (maxFuel == 0f)
            {
                return 65f;
            }
            return maxFuel;
        }
        #endregion

        #region Aircraft Methods
        /// <summary>
        /// Gets the aircraft fuel specifications for the given vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The aircraft fuel specifications.</returns>
        public static AircraftFuelSpecs GetAircraftFuelSpecs(this Vehicle vehicle)
        {
            AircraftFuelSpecs afs = null;
            if (IsAircraft(vehicle))
            {
                afs = VehicleProperties.AircraftSpecs.FirstOrDefault((AircraftFuelSpecs x) => x.Models.Any((string y) => y.ToUpper() == vehicle.DisplayName().ToUpper()));
                if (afs == null)
                {
                    afs = new AircraftFuelSpecs();
                    afs.Models.Add(vehicle.DisplayName());
                }
            }
            return afs;
        }

        /// <summary>
        /// Controls the aircraft engine.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public static void ControlAircraftEngine(this Vehicle vehicle)
        {
            if (Game.IsControlJustPressed(0, GameControl.VehicleFlyUnderCarriage))
            {
                ToggleEngine(vehicle);
            }
        }

        /// <summary>
        /// Toggles the engine of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public static void ToggleEngine(this Vehicle vehicle)
        {
            SetEngine(vehicle, !vehicle.IsEngineOn);
        }

        /// <summary>
        /// Sets the engine state of the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="engineOn">Whether to turn the engine on or off.</param>
        public static void SetEngine(this Vehicle vehicle, bool engineOn)
        {
            vehicle.IsDriveable = engineOn;
            N.SetVehicleEngineOn(vehicle, engineOn, false, true);
            Managed.AircraftEngineOn = engineOn;
        }
        #endregion

        #region Boat Methods
        /// <summary>
        /// Checks if the vehicle is a boat.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns><c>true</c> if the vehicle is a boat; otherwise, <c>false</c>.</returns>
        public static bool IsBoat(this Vehicle vehicle)
        {
            if (!vehicle.Model.IsBoat && !(vehicle.DisplayName() == "SUBMERS"))
            {
                return vehicle.DisplayName() == "SUBMERS2";
            }
            return true;
        }
        #endregion

        #region Electric Vehicle Methods
        /// <summary>
        /// Checks if the vehicle is an electric vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns><c>true</c> if the vehicle is an electric vehicle; otherwise, <c>false</c>.</returns>
        public static bool IsElectric(this Vehicle vehicle)
        {
            List<string> electricVehicles = new List<string>
            {
                "airtug", "caddy", "caddy2", "caddy3", "cyclone", "dilettante", "imorgon", "iwagen", "khamelion", "neon", 
                "omnisegt", "raiden", "surge", "tezeract", "voltic", "voltic2"
            };
        
            return electricVehicles.Contains(vehicle.DisplayName().ToLower());
        }
        #endregion

        #region Refueling Methods
        /// <summary>
        /// Consumes fuel for an aircraft.
        /// </summary>
        /// <param name="vehicle">The aircraft.</param>
        public static void ConsumeAircraftFuel(this Vehicle vehicle)
        {
            float fuel = GetFuelLevel(vehicle);
            if (fuel > 0f && vehicle.IsEngineOn)
            {
                TimeSpan timeElapsed = DateTime.UtcNow - Managed.LastWorldTime;
                float fuelUsed = FuelLogic.ConsumeFuelAircraft(vehicle, timeElapsed);
                fuel -= fuelUsed;
                fuel = ((fuel < 0f) ? 0f : fuel);
            }
            fuel = ProcessRefuelingAircraft(vehicle, fuel);
            SetFuelLevel(vehicle, fuel);
        }

        /// <summary>
        /// Processes refueling for an aircraft.
        /// </summary>
        /// <param name="vehicle">The aircraft.</param>
        /// <param name="fuel">The current fuel level.</param>
        /// <returns>The updated fuel level.</returns>
        private static float ProcessRefuelingAircraft(this Vehicle vehicle, float fuel)
        {
            if (vehicle.IsInAir)
            {
                return fuel;
            }
            if (vehicle.Speed < 2f)
            {
                ControlAircraftEngine(vehicle);
            }
            if (IsVehicleNearAnyTankerOrFuelPump(vehicle))
            {
                HUD.InstructRefuel();
                // Game.IsControlPressed(0, GameControl.Context)
                if (ControlHandler.IsControlDownWithModifier(SimpleControls.REFUEL))
                {
                    float pumpRate = MaxFuelLevel(vehicle) * 0.001f;

                    if (fuel + pumpRate <= MaxFuelLevel(vehicle))
                    {
                        fuel += pumpRate;
                        Managed.FuelAmountPumped += pumpRate;
                    }
                }
                // Game.IsControlJustReleased(0, GameControl.Context)
                if (!ControlHandler.IsControlDownWithModifier(SimpleControls.REFUEL) && Managed.FuelAmountPumped > 0f)
                {
                    if (ConfigHandler.RefuelNotification == true)
                    {
                        float gallonsPumped = Common.API.Math.ConvertLitresToGallons(Managed.FuelAmountPumped);
                        string fuelMsg = $"Pumped {Math.Round(Managed.FuelAmountPumped, 1)} L // {Math.Round(gallonsPumped, 1)} gallons";
                        Game.DisplayNotification("~o~[FUEL] ~w~" + fuelMsg);
                    }
                    Managed.FuelAmountPumped = 0f;
                }
                if (Game.LocalPlayer.Character.CurrentVehicle != null)
                {
                    HUD.RenderInstructions();
                    if (!Globals.HudActive) //  Due to fix sound check only being called once per load plugin
                    {
                        NativeFunction.CallByHash<int>(0x67C540AA08E4A6F5, -1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", 1);
                    }
                    Globals.HudActive = true;
                }
            }
            return fuel;
        }

        /// <summary>
        /// Consumes fuel for a road vehicle.
        /// </summary>
        /// <param name="vehicle">The road vehicle.</param>
        internal static void ConsumeRoadVehicleFuel(this Vehicle vehicle)
        {
            float fuel = GetFuelLevel(vehicle);
            if (fuel > 0f && vehicle.IsEngineOn)
            {
                if (!Managed.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                {
                    Managed.TripInfos[vehicle.Handle.ToInt32()] = new TripInfo();
                }
                if (Managed.TripInfos[vehicle.Handle.ToInt32()].LastPosition == Vector3.Zero)
                {
                    Managed.TripInfos[vehicle.Handle.ToInt32()].LastPosition = vehicle.Position;
                }
                float distance = Vector3.Distance(Managed.TripInfos[vehicle.Handle.ToInt32()].LastPosition, vehicle.Position);
                float kmTravelled = Math.Abs(distance / 1000f) * 5.2f;
                float fuelUsed = FuelLogic.ConsumeCarFuel(vehicle, kmTravelled);
                Managed.TripInfos[vehicle.Handle.ToInt32()].DistanceTraveledKM += kmTravelled;
                Managed.TripInfos[vehicle.Handle.ToInt32()].LastPosition = vehicle.Position;
                Managed.TripInfos[vehicle.Handle.ToInt32()].FuelConsumed += fuelUsed;
                fuel -= fuelUsed;
                if (fuel < 0.2f && IsElectric(vehicle) && vehicle.IsDriveable)
                {
                    Game.DisplayNotification("Your battery has run out of juice!");
                    vehicle.IsDriveable = false;
                }
                fuel = ((fuel < 0f) ? 0f : fuel);
            }
            fuel = ProcessRefuelingCar(vehicle, fuel);
            SetFuelLevel(vehicle, fuel);
        }

        private static void HandleRefuelCompletion(Vehicle vehicle)
        {
            if (!IsElectric(vehicle))
            {
                if (ConfigHandler.RefuelNotification == true)
                {
                    float gallonsPumped = Common.API.Math.ConvertLitresToGallons(Managed.FuelAmountPumped);
                    string fuelMsg = $"Pumped {Math.Round(Managed.FuelAmountPumped, 1)} L // {Math.Round(gallonsPumped, 1)} gallons";
                    Game.DisplayNotification("~o~[FUEL] ~w~" + fuelMsg);
                }
                if (Managed.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                {
                    TripInfo t = Managed.TripInfos[vehicle.Handle.ToInt32()];
                    if (t.DistanceTraveledKM > 0f)
                    {
                        if (ConfigHandler.RefuelNotification == true)
                        {
                            string fuelEcon = $"Average: {Math.Round(t.FuelEconomyInLPer100Km, 1)} L/100 km // {Math.Round(t.FuelEconomyInMPG, 1)} MPG";
                            Game.DisplayNotification("~o~[FUEL] ~w~" + fuelEcon);
                        }
                    }
                }
                if (Managed.TripInfos.ContainsKey(vehicle.Handle.ToInt32()))
                {
                    Managed.TripInfos[vehicle.Handle.ToInt32()].Reset(((Entity)vehicle).Position);
                }
                Managed.FuelAmountPumped = 0f;
            }
            Game.LocalPlayer.Character.Tasks.ClearSecondary();
        }

        private static void PutNozzleInVehicle(Vehicle vehicle, int ptankBone)
        {
            if (Globals.fuelNozzle != null)
            {
                NativeFunction.CallByHash<int>(0x6B9BBD38AB0796DF, Globals.fuelNozzle, vehicle, ptankBone, -0.18f, 0.0f, 0.75f, -125.0f, -90.0f, -90.0f, true, true, false, false, 1, true); // ATTACH_ENTITY_TO_ENTITY
            }
        }

        private static void AttachNozzle()
        {
            Common.API.Functions.LoadAnim("anim@am_hold_up@male");
            NativeFunction.CallByHash<int>(0xEA47FE3719165B94, Game.LocalPlayer.Character, "anim@am_hold_up@male", "shoplift_high", 2.0f, 8.0f, -1, 50, 0, 0, 0, 0);
            SoundHandler.PlayAudio(SoundHandler.Audio.PickUpNozzle);
            GameFiber.Sleep(300);
            NativeFunction.CallByHash<int>(0x97FF36A1D40EA00A, Game.LocalPlayer.Character, "anim@am_hold_up@male", "shoplift_high", 1.0f);
            Vector3 position = new Vector3(1.0f, 1.0f, 1.0f);
            Globals.fuelNozzle = new Rage.Object(Globals.fuelNozzleModel.Hash, position, 1.0f);
            int lefthand = NativeFunction.CallByHash<int>(0x3F428D08BE5AAE31, Game.LocalPlayer.Character, 18905);
            NativeFunction.CallByHash<int>(0x6B9BBD38AB0796DF, Globals.fuelNozzle, Game.LocalPlayer.Character, lefthand, 0.13f, 0.04f, 0.01f, -42.0f, -115.0f, -63.42f, false, true, false, true, false, true);
            // Load Rope Textures
            NativeFunction.CallByHash<int>(0x9B9039DBF2D258C1); // ROPE_LOAD_TEXTURES
            while (!NativeFunction.CallByHash<bool>(0xF2D0E6A75CC05597)) // ROPE_ARE_TEXTURES_LOADED
            {
                GameFiber.Yield();
                NativeFunction.CallByHash<int>(0x9B9039DBF2D258C1); // ROPE_LOAD_TEXTURES
            }
            Rage.Object closestPump = GetClosestPump(Game.LocalPlayer.Character.Position);
            if (closestPump != null)
            {
                grabbedNozzleCoords = NativeFunction.CallByHash<Vector3>(0x3FEF770D40960D5A, closestPump);
                Globals.Rope = NativeFunction.CallByHash<int>(0xE832D760399EB220, grabbedNozzleCoords.X, grabbedNozzleCoords.Y, grabbedNozzleCoords.Z, 0.0f, 0.0f, 0.0f, 3.0f, 5, 8.0f, 0.0f, 1.0f, false, false, false, 1.0f, true); // ADD_ROPE
                NativeFunction.CallByHash<int>(0x710311ADF0E20730, Globals.Rope); // ACTIVATE_PHYSICS
                GameFiber.Sleep(100);
                Vector3 nozzlePos = NativeFunction.CallByHash<Vector3>(0x3FEF770D40960D5A, Globals.fuelNozzle); // GET_ENTITY_COORDS
                nozzlePos = N.GetOffsetFromEntityInWorldCoords(Globals.fuelNozzle, 0.0f, -0.033f, -0.195f);
                NativeFunction.CallByHash<int>(0x3D95EC8B6D940AC3, Globals.Rope, closestPump, Globals.fuelNozzle, grabbedNozzleCoords.X, grabbedNozzleCoords.Y, grabbedNozzleCoords.Z + 2.1f, nozzlePos.X, nozzlePos.Y, nozzlePos.Z, 20.0f, false, false); // ATTACH_ENTITIES_TO_ROPE
            }
            Managed.nozzleAttached = true;
        }

        private static void DetachNozzle()
        {
            if (Managed.nozzleAttached)
            {
                SoundHandler.PlayAudio(SoundHandler.Audio.PutBackNozzle);
                GameFiber.Sleep(300);
                Globals.fuelNozzle?.Delete();
                Managed.nozzleAttached = false;
                NativeFunction.CallByHash<int>(0x6CE36C35C1AC8163); // ROPE_UNLOAD_TEXTURES
                unsafe
                {
                    fixed (int* pRopeId = &Globals.Rope)
                    {
                        if (NativeFunction.Natives.DOES_ROPE_EXIST<bool>((IntPtr)pRopeId))
                        {
                            NativeFunction.Natives.DELETE_ROPE<int>((IntPtr)pRopeId);
                        }
                    }
                }
            }
        }

        private static void PlayRefuelingAnimation()
        {
            if (!NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 3))
            {
                NativeFunction.CallByHash<int>(0xEA47FE3719165B94, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 2f, 8f, -1, 49, 0f);
            }
        }

        private static void StopRefuelingAnimation()
        {
            if (NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, Game.LocalPlayer.Character, Globals.DictRefueling, Globals.AnimRefueling, 3))
            {
                Game.LocalPlayer.Character.Tasks.ClearSecondary();
            }
        }

        /// <summary>
        /// Processes the refueling for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="fuel">The current fuel level.</param>
        /// <returns>The updated fuel level.</returns>
        private static float ProcessRefuelingCar(this Vehicle vehicle, float fuel)
        {
            if (Managed.GasStation != null && IsVehicleNearAnyPump(vehicle))
            {
                GasPump closestPump = null;
                float minDistance = float.MaxValue;

                Vector3 fuelTankPos = GetVehicleTankPos(vehicle);

                foreach (GasPump pump in Managed.GasStation.Pumps)
                {
                    float distance = Vector3.Distance(fuelTankPos, pump.Position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPump = pump;
                    }

                    GameFiber.Yield();
                }

                if (closestPump != null)
                {
                    Entity veh = vehicleInFront();

                    if (Managed.nozzleAttached)
                    {
                        Text.Draw3D(closestPump.Position, "Return Nozzle [E]", 0.08f);

                        if (veh)
                        {
                            Text.Draw3D(fuelTankPos, "Attach Nozzle [E]", 0.08f);
                        }
                    }
                    else 
                    {
                        Text.Draw3D(closestPump.Position, "Grab Nozzle [E]", 0.08f);
                    }

                    GameFiber.StartNew(delegate
                    {
                        if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                        {
                            if (Game.IsKeyDown(System.Windows.Forms.Keys.E))
                            {
                                if (Managed.nozzleAttached)
                                {
                                    if (veh == null)
                                    {
                                        DetachNozzle();
                                    } 
                                    else
                                    {
                                        string[] vehicleFuelTankBones = UtilityConstants.VehicleFuelTankBones;
                                        int foundBoneIndex = -1;
                                        foreach (string boneName in vehicleFuelTankBones)
                                        {
                                            try
                                            {
                                                int boneIndex = vehicle.GetBoneIndex(boneName);
                                                if (boneIndex != -1 && vehicle.HasBone(boneIndex))
                                                {
                                                    foundBoneIndex = boneIndex;
                                                    break;
                                                }
                                            }
                                            catch (ArgumentException)
                                            {
                                            }
                                        }

                                        PutNozzleInVehicle(Game.LocalPlayer.Character.LastVehicle, foundBoneIndex);
                                    }
                                }
                                else
                                {
                                    AttachNozzle();
                                }
                            }
                        }

                        while (Managed.nozzleAttached)
                        {
                            Vector3 currentCoords = Game.LocalPlayer.Character.Position;
                            float dist = (grabbedNozzleCoords - currentCoords).Length();
                            if (dist > 7.5f)
                            {
                                Globals.fuelNozzle?.Delete();
                                Managed.nozzleAttached = false;
                                NativeFunction.CallByHash<int>(0x6CE36C35C1AC8163); // ROPE_UNLOAD_TEXTURES
                                unsafe
                                {
                                    fixed (int* pRopeId = &Globals.Rope)
                                    {
                                        if (NativeFunction.Natives.DOES_ROPE_EXIST<bool>((IntPtr)pRopeId))
                                        {
                                            NativeFunction.Natives.DELETE_ROPE<int>((IntPtr)pRopeId);
                                        }
                                    }
                                }
                            }

                            GameFiber.Yield();
                        }
                    });
                }
            }

            return fuel;
        }

        #endregion
        
        public static bool IsVehicleReversing(Vehicle vehicle)
        {
            Vector3 relativeSpeed = NativeFunction.CallByHash<Vector3>(0x9A8D700A51CB7B0D, vehicle, true); // GET_ENTITY_SPEED_VECTOR
            return relativeSpeed.Y < 0f && vehicle.CurrentGear < 1;
        }

        public static void LockTransmission(Vehicle playerVeh, bool toggle)
        {
            NativeFunction.CallByHash<int>(0x684785568EF26A22, playerVeh, toggle); // SET_VEHICLE_HANDBRAKE
        }

        public static void CreateVehicleBlip(Vehicle playerVeh)
        {
            int parkedVehicleBlip = NativeFunction.CallByHash<int>(0xBC8DBDCA2436F7E8, playerVeh); // GET_BLIP_FROM_ENTITY
            if (!NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST
            {
                parkedVehicleBlip = NativeFunction.CallByHash<int>(0x5CDE92C702A8FCE7, playerVeh); // ADD_BLIP_FOR_ENTITY

                if (NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST
                {
                    NativeFunction.CallByHash<int>(0xDF735600A4696DAF, parkedVehicleBlip, 326); // SET_BLIP_SPRITE
                    NativeFunction.CallByHash<int>(0xD38744167B2FA257, parkedVehicleBlip, 0.7f); // SET_BLIP_SCALE
                    NativeFunction.CallByHash<int>(0xF9113A30DE5C6670, "STRING"); // BEGIN_TEXT_COMMAND_SET_BLIP_NAME
                    NativeFunction.CallByHash<int>(0x6C188BE134E074AA, "Personal Vehicle"); // ADD_​TEXT_​COMPONENT_​SUBSTRING_​PLAYER_​NAME
                    NativeFunction.CallByHash<int>(0xBC38B49BCB83BC9B, parkedVehicleBlip); // END_TEXT_COMMAND_SET_BLIP_NAME
                    NativeFunction.CallByHash<int>(0x6F6F290102C02AB4, parkedVehicleBlip, true); // SET_BLIP_AS_FRIENDLY
                }
            }
        }

        public static void DeleteVehicleBlip(Vehicle playerVeh)
        {
            int parkedVehicleBlip = NativeFunction.CallByHash<int>(0xBC8DBDCA2436F7E8, playerVeh); // GET_BLIP_FROM_ENTITY
            if (NativeFunction.CallByHash<bool>(0xA6DB27D19ECBB7DA, parkedVehicleBlip)) // DOES_BLIP_EXIST 
            {
                unsafe
                {
                    NativeFunction.CallByHash<int>(0x86A652570E5F25DD, &parkedVehicleBlip); // REMOVE_BLIP
                }
            }
        }

        #region Indicator Modes
        public static void HandleNormalMode(ref VehicleIndicatorLightsStatus intendedStatus, ref VehicleIndicatorLightsStatus status)
        {
            try
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (EntityExtensions.Exists((IHandleable)(object)currentVehicle))
                {
                    if ((int)intendedStatus == 1 && currentVehicle.IsEngineOn)
                    {
                        if ((int)status == 1)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)1;
                        }
                        currentVehicle.IndicatorLightsStatus = status;
                    }
                    else if ((int)intendedStatus == 2 && currentVehicle.IsEngineOn)
                    {
                        if ((int)status == 2)
                        {
                            status = (VehicleIndicatorLightsStatus)0;
                        }
                        else
                        {
                            status = (VehicleIndicatorLightsStatus)2;
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
                }
                intendedStatus = (VehicleIndicatorLightsStatus)0;
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }

        public static void HandleTurnOffAtTurnMode(ref VehicleIndicatorLightsStatus intendedStatus, ref uint turnOffAt, ref VehicleIndicatorLightsStatus status, ref float initialHeading)
        {
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

        // WIP Function

        public static void HandleAutomaticTurnMode(float initialHeading)
        {
            try
            {
                Vehicle currentVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (EntityExtensions.Exists((IHandleable)(object)currentVehicle))
                {
                    initialHeading = ((Entity)currentVehicle).Heading;

                    Game.LogTrivial("initialHeading: " + initialHeading);

                    float headingChange = Math.Abs(((Entity)currentVehicle).Heading - initialHeading);

                    Game.LogTrivial("headingChange: " + headingChange);

                    float turnThreshold = 10f;

                    if (headingChange > turnThreshold)
                    {
                        if (currentVehicle.SteeringAngle < 0)
                        {
                            Game.LogTrivial("Turn detected: Left turn");
                        }
                        else if (currentVehicle.SteeringAngle > 0)
                        {
                            Game.LogTrivial("Turn detected: Right turn");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred: {ex.Message}");
            }
        }
        #endregion
    }
}
