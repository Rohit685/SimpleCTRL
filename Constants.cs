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
			("RAGENativeUI.dll", "1.9.2")
		};

		public static readonly string[] VehicleFuelTankBones = new string[5] { "petrolcap", "petroltank", "petroltank_r", "petroltank_l", "wheel_lr" };
	}
}