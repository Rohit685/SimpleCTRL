using System.Collections.Generic;

public class AircraftFuelSpecs
{
    public string Class { get; set; }

    public float CapacityLitres { get; set; } = 450f;


    public float LitresPerHour { get; set; } = 75f;


    public List<string> Models { get; set; } = new List<string>();
}
