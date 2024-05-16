using Common;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Handlers;
using System;
using System.Linq;

namespace SimpleCTRL.Utils
{
	public static class VehicleUtility
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
				fuelUsed = SafeFloat(fuelUsed, 0f);

				return Math.Abs(fuelUsed);
			}
			return 0f;
		}

		private static float SafeFloat(float f, float _default)
		{
			if (float.IsInfinity(f) || float.IsNaN(f))
			{
				f = _default;
			}
			return f;
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
				econLossFactor = SafeFloat(econLossFactor, 1f);
				float accelerationLossFactor = Math.Abs(v.Acceleration() * 1.5f);
				accelerationLossFactor = SafeFloat(accelerationLossFactor, 0f);
				float normalizedFuelEconomy = baseLitresPer100Km + baseLitresPer100Km * econLossFactor + accelerationLossFactor;
				normalizedFuelEconomy = SafeFloat(normalizedFuelEconomy, baseLitresPer100Km);
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
			fuelUsed = SafeFloat(fuelUsed, 0f);
			return Math.Abs(fuelUsed);
		}

		public static void CreateBlips()
        {
            #region Gas Stations
			if (ConfigHandler.FuelSystem == true)
            {
				foreach (GasStation x in Globals.GasStations)
				{
					Blip blip = new Blip(x.Position);
					blip.Sprite = (BlipSprite)361;
					blip.Color = System.Drawing.Color.White;
					blip.Scale = 1f;
					NativeFunction.CallByHash<int>(0xBE8BE4FE60E27B72, blip, true); // SET_BLIP_AS_SHORT_RANGE
					blip.Name = "Gas Station";
					Globals.Blips.Add(blip);
				}
			}
			#endregion

			#region Repair Shops
			foreach (RepairShop repairShop in ConfigHandler.RepairShops)
            {
				if (repairShop.ShowBlip == true)
                {
					Blip blip = new Blip(repairShop.Position);
					blip.Sprite = (BlipSprite)446;
					blip.Scale = 1f;
					NativeFunction.CallByHash<int>(0xBE8BE4FE60E27B72, blip, true); // SET_BLIP_AS_SHORT_RANGE
					blip.Name = "Repair Shop";
					RepairShop.mechanicBlips.Add(blip);
				}
			}
			#endregion
		}

		public static void RemoveBlips()
        {
			#region Gas Stations
			if (Globals.Blips.Count > 0)
			{
				foreach (Blip blip in Globals.Blips)
				{
					if (blip.Exists())
					{
						blip.Delete();
					}
				}
				Globals.Blips.Clear();
			}
			#endregion

			#region Repair Shops
			foreach (Blip b in RepairShop.mechanicBlips)
			{
				b.Delete();
			}
			RepairShop.mechanicBlips.Clear();
            #endregion

            #region Parked Vehicle
            if (Globals.isParked)
            {
				Vehicle playerVeh = Game.LocalPlayer.Character.CurrentVehicle;

				if (!EntityExtensions.Exists(playerVeh))
				{
					return;
				}

				Extensions.DeleteVehicleBlip(playerVeh);
			}
            #endregion
		}

		//public static void CreateJerryCanPickUps()
  //      {
		//	if (ConfigHandler.FuelSystem == true)
  //          {
		//		Model model = new Model("prop_jerrycan_01a");
		//		N.RequestModel(model);

		//		if (N.HasModelLoaded(model))
		//		{
		//			var MainFiber = new GameFiber(delegate
		//			{
		//				while (true)
		//				{
		//					foreach (GasStation x in Globals.GasStations)
		//					{
		//						Vector3 p = x.Position;
		//						NativeFunction.CallByHash<int>(0xFBA08C503DD5FA58, 3332236287, p.X, p.Y, p.Z - 0.5f, 8, true, model); // CREATE_PICKUP
		//						Game.LogTrivial("creating");
		//					}
		//					GameFiber.Yield();
		//				}
		//			});
		//			MainFiber.Start();
		//		}
		//	}
		//}

		public static void CreateDepartmentPumps()
        {
			if (ConfigHandler.FuelSystem == true)
			{
				while (true)
                {
					Model model = new Model("prop_gas_pump_old2");
					N.RequestModel(model);

					if (N.HasModelLoaded(model))
					{
						foreach (GasPump pump in Globals.DepartmentPumps)
						{
							if (pump != null && Game.LocalPlayer.Character != null)
							{
								unsafe
								{
									Vector3 position = Game.LocalPlayer.Character.Position;
									if (position.DistanceToSquared(pump.Position) <= 50000f && !N.DoesObjectOfTypeExistAtCoords(pump.Position.X, pump.Position.Y, pump.Position.Z, 2f, Globals.DepartmentPumpObjectHash))
									{
										position = pump.Position;
										Logging.Info("No pump found at " + ((object)(Vector3)(position)).ToString() + ". Adding!", "Functions");
										N.RequestCollisionAtCoord(pump.Position.X, pump.Position.Y, 1000f);
										float resultArg = N.GetGroundZFor3DCoord(pump.Position.X, pump.Position.Y, 1000f, false);
										pump.Position.Z = resultArg;
										Rage.Object obj = new Rage.Object(model.Hash, pump.Position);
										Globals.DepartmentPumpObjects.Add(obj);
										NativeFunction.CallByHash<int>(0x8524A8B0171D5E07, obj, 0.0f, 0.0f, Common.API.Math.DirectionToRotation(Common.API.Math.HeadingToDirection(pump.Rotation), 0f).Z, 1);
										if (EntityExtensions.Exists(obj)) 
										{
											obj.IsInvincible = true;
											obj.IsPositionFrozen = true;
											obj.IsExplosionProof = true;
											obj.IsFireProof = true;
											obj.IsCollisionProof = true;
										}
									}
								}
							}
							GameFiber.Wait(250);
						}
						GameFiber.Wait(250);
					}
					GameFiber.Yield();
				}
			}
		}

		internal static GasStation GetGasStationInRange(Vector3 pos, float rangeSquared)
        {
			return (from x in Globals.GasStations
					where Vector3.DistanceSquared(x.Position, pos) < rangeSquared
					orderby Vector3.DistanceSquared(x.Position, pos)
					select x).FirstOrDefault();
		}

		internal static AirportFuelPump GetAirportFuelPumpInRange(Vector3 pos, float rangeSquared)
        {
			return (from x in Globals.AirportFuelPumps
					where Vector3.DistanceSquared(x.Position, pos) < rangeSquared
					orderby Vector3.DistanceSquared(x.Position, pos)
					select x).FirstOrDefault();
		}
	}
}
