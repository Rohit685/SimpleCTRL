using Rage;
using SimpleCTRL;
using System.Linq;

public class AirportFuelPump
{
    public string Description;

    public Vector3 Position = Vector3.Zero;

    #region Functions
    public static AirportFuelPump GetClosestInRange(Vector3 pos, float rangeSquared)
	{
		return (from x in Globals.AirportFuelPumps
				where Vector3.DistanceSquared(x.Position, pos) < rangeSquared
				orderby Vector3.DistanceSquared(x.Position, pos)
				select x).FirstOrDefault();
	}
    #endregion
}
