using System.Collections.Generic;
using System.Linq;
using Rage;
using SimpleCTRL;

public class GasStation
{
    public Vector3 Position { get; set; } = Vector3.Zero;

    public string Description { get; set; }

    public List<GasPump> Pumps { get; set; } = new List<GasPump>();

    #region Functions
    public static GasStation GetClosestInRange(Vector3 pos, float rangeSquared)
    {
        return Globals.GasStations
            .Where(gs => Vector3.DistanceSquared(gs.Position, pos) < rangeSquared)
            .OrderBy(gs => Vector3.DistanceSquared(gs.Position, pos))
            .FirstOrDefault();
    }
    #endregion
}
