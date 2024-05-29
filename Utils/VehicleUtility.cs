using Common;
using Common.Native;
using Rage;
using Rage.Native;
using SimpleCTRL.Extensions;
using SimpleCTRL.Handlers;
using System;
using System.Linq;

namespace SimpleCTRL.Utils
{
	public static class VehicleUtility
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
	}
}
