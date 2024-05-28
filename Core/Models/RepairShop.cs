using Rage;
using System.Collections.Generic;

public class RepairShop
{
    public string Location { get; set; }
    public Vector3 Position { get; set; }
    public bool ShowBlip { get; set; }
    public float UseRange { get; set; }
    public static List<Blip> mechanicBlips = new List<Blip>();

    public RepairShop(float x, float y, float z, float t, bool blip, string description)
    {
        Location = description;
        Position = new Vector3(x, y, z);
        ShowBlip = blip;
        UseRange = t;
    }
}