using System.Collections.Generic;
using Rage;

namespace SimpleCTRL
{
    public static class VehicleProperties
    {

        public static Dictionary<VehicleClass, VehicleFuelSpecs> VehicleSpecs = new Dictionary<VehicleClass, VehicleFuelSpecs>
        {
        {
            (VehicleClass)0,
            new VehicleFuelSpecs
            {
                Displacement = 1.5f,
                LPer100KM = 7f
            }
        },
        {
            (VehicleClass)1,
            new VehicleFuelSpecs
            {
                Displacement = 3f,
                LPer100KM = 9f
            }
        },
        {
            (VehicleClass)2,
            new VehicleFuelSpecs
            {
                Displacement = 3.5f,
                LPer100KM = 11f
            }
        },
        {
            (VehicleClass)3,
            new VehicleFuelSpecs
            {
                Displacement = 3.5f,
                LPer100KM = 10f
            }
        },
        {
            (VehicleClass)4,
            new VehicleFuelSpecs
            {
                Displacement = 5f,
                LPer100KM = 13f
            }
        },
        {
            (VehicleClass)5,
            new VehicleFuelSpecs
            {
                Displacement = 3.5f,
                LPer100KM = 17f
            }
        },
        {
            (VehicleClass)6,
            new VehicleFuelSpecs
            {
                Displacement = 3.9f,
                LPer100KM = 13f
            }
        },
        {
            (VehicleClass)7,
            new VehicleFuelSpecs
            {
                Displacement = 6.5f,
                LPer100KM = 19.6f
            }
        },
        {
            (VehicleClass)8,
            new VehicleFuelSpecs
            {
                Displacement = 0.6f,
                LPer100KM = 3.9f
            }
        },
        {
            (VehicleClass)9,
            new VehicleFuelSpecs
            {
                Displacement = 1.5f,
                LPer100KM = 13f
            }
        },
        {
            (VehicleClass)10,
            new VehicleFuelSpecs
            {
                Displacement = 13f,
                LPer100KM = 36.2f
            }
        },
        {
            (VehicleClass)11,
            new VehicleFuelSpecs
            {
                Displacement = 6f,
                LPer100KM = 19.6f
            }
        },
        {
            (VehicleClass)12,
            new VehicleFuelSpecs
            {
                Displacement = 5.4f,
                LPer100KM = 15.7f
            }
        },
        {
            (VehicleClass)13,
            new VehicleFuelSpecs
            {
                Displacement = 0f,
                LPer100KM = 0f
            }
        },
        {
            (VehicleClass)14,
            new VehicleFuelSpecs
            {
                Displacement = 0f,
                LPer100KM = 0f
            }
        },
        {
            (VehicleClass)15,
            new VehicleFuelSpecs
            {
                Displacement = 0f,
                LPer100KM = 0f
            }
        },
        {
            (VehicleClass)16,
            new VehicleFuelSpecs
            {
                Displacement = 0f,
                LPer100KM = 0f
            }
        },
        {
            (VehicleClass)17,
            new VehicleFuelSpecs
            {
                Displacement = 5.4f,
                LPer100KM = 15.68f
            }
        },
        {
            (VehicleClass)18,
            new VehicleFuelSpecs
            {
                Displacement = 3.5f,
                LPer100KM = 10f
            }
        },
        {
            (VehicleClass)19,
            new VehicleFuelSpecs
            {
                Displacement = 6.5f,
                LPer100KM = 23.5f
            }
        },
        {
            (VehicleClass)20,
            new VehicleFuelSpecs
            {
                Displacement = 6f,
                LPer100KM = 19.6f
            }
        },
        {
            (VehicleClass)21,
            new VehicleFuelSpecs
            {
                Displacement = 0f,
                LPer100KM = 0f
            }
        }
        };

        public static List<AircraftFuelSpecs> AircraftSpecs = new List<AircraftFuelSpecs>();
    }
}
