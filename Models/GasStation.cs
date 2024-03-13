using System.Collections.Generic;
using Rage;

public class GasStation
{
    public Vector3 Position = Vector3.Zero;

    public string Description;

    public List<GasPump> Pumps = new List<GasPump>();
}
