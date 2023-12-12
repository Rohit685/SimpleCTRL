using System.Collections.Generic;

namespace SimpleCTRL
{
	public static class Constants
	{
		public static readonly List<(string Name, string Version)> Dependencies = new List<(string, string)>
		{
			("Common.dll", "1.0.0.0"),
			("NAudio.dll", "1.10.0"),
			("Newtonsoft.Json.dll", "13.0.0.0"),
			("RAGENativeUI.dll", "1.9.3.0")
		};

		public const string FuelLevelPropertyName = "_Fuel_Level";

		public const float DefaultMaxFuelCapacity = 65f;

		public const float ShowMarkerInRangeSquared = 250f;

		public const string ManualRefuelAnimDict = "weapon@w_sp_jerrycan";

		public static readonly string[] VehicleFuelTankBones = new string[5] { "petrolcap", "petroltank", "petroltank_r", "petroltank_l", "wheel_lr" };

		public const float RefuelRateStation = 0.045f;

		public const float RefuelRateJerryCan = 0.023f;

		public const float SiphonRateJerryCan = 0.00125f;

		public const float FuelDrainRate = 0.003f;

		public const float MATH_MPH_TO_KPH = 1.609344f;

		public const float MATH_USGAL_TO_LITRES = 3.7854118f;
	}
}