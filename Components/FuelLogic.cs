using Common;
using Rage;
using SimpleCTRL.Extensions;
using System;

namespace SimpleCTRL.Components
{
    internal static class FuelLogic
    {
		internal static float ConsumeCarFuel(Vehicle v, float kilometresTravelled)
		{
			if (EntityExtensions.Exists(v))
			{
#pragma warning disable CS0219 // Variable is assigned but its value is never used
				float fuelUsed = 0f;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
				if (v.Model.IsBicycle || v.Model.IsHelicopter || v.Model.IsPlane)
				{
					return 0f;
				}
				if (v.Model.IsCar || v.Model.IsBike || v.Model.IsQuadBike)
				{
					return ConsumeFuelRoadVehicle(v, kilometresTravelled);
				}
				return 0f;
			}
			return 0f;
		}

		internal static float ConsumeFuelAircraft(Vehicle v, TimeSpan timeElapsed)
		{
			if (EntityExtensions.Exists(v))
			{
				float fuelUsed = 0f;

				if (Math.Abs(timeElapsed.TotalHours) > 1.0)
				{
					return 0f;
				}

				AircraftFuelSpecs afs = v.GetAircraftFuelSpecs();
				if (afs == null)
				{
					afs = new AircraftFuelSpecs();
					afs.Models.Add(v.DisplayName());
				}

				float baseLitresPerHour = afs.LitresPerHour;
				float fuelConsumptionMultiplier = 4f;
				fuelUsed = baseLitresPerHour * fuelConsumptionMultiplier * (float)timeElapsed.TotalHours;
				fuelUsed = FloatExtensions.SafeFloat(fuelUsed, 0f);

				return Math.Abs(fuelUsed);
			}
			return 0f;
		}

		private static float ConsumeFuelRoadVehicle(Vehicle v, float kilometresTravelled)
		{
			VehicleFuelSpecs vfs = VehicleProperties.VehicleSpecs[(VehicleClass)18];
			if (VehicleProperties.VehicleSpecs.ContainsKey(v.Class))
			{
				vfs = VehicleProperties.VehicleSpecs[v.Class];
			}
			if (v.Model.IsQuadBike)
			{
				VehicleFuelSpecs vehicleFuelSpecs = default(VehicleFuelSpecs);
				vehicleFuelSpecs.Displacement = 1f;
				vehicleFuelSpecs.LPer100KM = 4f;
				vfs = vehicleFuelSpecs;
			}
			float fuelUsed = 0f;
			float vehSpeed = Math.Abs(v.Speed);
			if (vehSpeed > 3f)
			{
				float baseLitresPer100Km = vfs.LPer100KM;
				int gear = v.CurrentGear;
				if (v.Acceleration() < 0f)
				{
					gear = 1;
				}
				float econLossFactor = v.CurrentRPM() / (float)v.CurrentGear;
				econLossFactor = FloatExtensions.SafeFloat(econLossFactor, 1f);
				float accelerationLossFactor = Math.Abs(v.Acceleration() * 1.5f);
				accelerationLossFactor = FloatExtensions.SafeFloat(accelerationLossFactor, 0f);
				float normalizedFuelEconomy = baseLitresPer100Km + baseLitresPer100Km * econLossFactor + accelerationLossFactor;
				normalizedFuelEconomy = FloatExtensions.SafeFloat(normalizedFuelEconomy, baseLitresPer100Km);
				fuelUsed = normalizedFuelEconomy * kilometresTravelled / 100f;
			}
			else
			{
				float idleFuelFlowFactor = 2.7777778E-06f;
				float engineDisplacement = vfs.Displacement;
				fuelUsed = idleFuelFlowFactor * engineDisplacement;
				if (v.WheelSpeed() > 3f || v.CurrentRPM() > 0.3f)
				{
					float rpmfactor = 1f + Math.Abs(v.CurrentRPM() * 5f);
					fuelUsed *= rpmfactor;
				}
			}
			fuelUsed = FloatExtensions.SafeFloat(fuelUsed, 0f);
			return Math.Abs(fuelUsed);
		}
	}
}
