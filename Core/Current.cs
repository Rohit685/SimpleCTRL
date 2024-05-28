using System;
using System.Collections.Generic;
using Rage;

namespace SimpleCTRL
{
    public class Current
    {
        public static GasStation GasStation = null;

        public static bool VehicleFuelLevelInitialized = false;

        public static float VehicleFuelCapacity = 65f;

        public static float FuelAmountPumped = 0f;

        public static float FuelAmountSiphoned = 0f;

        public static DateTime LastAircraftEngineHintDisplayed = DateTime.MinValue;

        public static DateTime LastAircraftLowFuelWarning = DateTime.MinValue;

        public static DateTime LastVehicleIndicator = DateTime.MinValue;

        public static Vehicle MyVehicle;

        public static Dictionary<int, TripInfo> TripInfos = new Dictionary<int, TripInfo>();

        public static DateTime LastWorldTime;

        public static bool AircraftEngineOn = false;

        public static float LastAircraftAltitude = 0f;
    }
}
