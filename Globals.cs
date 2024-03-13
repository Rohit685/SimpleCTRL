using System.Collections.Generic;
using Rage;

namespace SimpleCTRL
{
    public static class Globals
    {
        public static List<GasStation> GasStations = new List<GasStation>();

        public static List<GasPump> DepartmentPumps = new List<GasPump>();

        public static List<Blip> Blips = new List<Blip>();

        public static List<AirportFuelPump> AirportFuelPumps = new List<AirportFuelPump>();

        public static List<VehicleClass> disallowedClasses = new List<VehicleClass> { VehicleClass.Boat, VehicleClass.Cycle, VehicleClass.Helicopter, VehicleClass.Industrial, VehicleClass.Plane, VehicleClass.Rail, VehicleClass.Military, VehicleClass.Motorcycle };

        public static bool HudActive = false;

        public static bool RefuelingAllowed = true;

        public static bool WasDriver = false;

        public static bool isParked = false;

        public static readonly string DictRefueling = "timetable@gardener@filling_can";

        public static readonly string AnimRefueling = "gar_ig_5_filling_can";

        public static readonly string DictSiphoning = "rcmextreme3";

        public static readonly string AnimSiphoning = "idle";

        public static uint DepartmentPumpObjectHash = (uint)new Model("prop_gas_pump_old2").Hash;
    }
}
