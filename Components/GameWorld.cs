using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Extensions;
using SimpleCTRL.Handlers;

namespace SimpleCTRL.Components
{
    internal static class GameWorld
    {
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

				Extensions.VehicleExtensions.DeleteVehicleBlip(playerVeh);
			}
			#endregion
		}

		internal static void CreateDepartmentPumps()
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
	}
}
