using Common.API;
using Rage;

public class TripInfo
{
    public float DistanceTraveledKM { get; set; }

    public float FuelConsumed { get; set; }

    public Vector3 LastPosition { get; set; } = Vector3.Zero;


    public float FuelEconomyInLPer100Km => MathUtils.GetFuelEconomyInLPer100Km(FuelConsumed, DistanceTraveledKM);

    public float FuelEconomyInMPG => MathUtils.ConvertLPer100KmToMPG(FuelEconomyInLPer100Km);

    public float DistanceTraveledMiles => MathUtils.ConvertKilometersToMiles(DistanceTraveledKM);

    public void Reset(Vector3 v)
    {
        DistanceTraveledKM = 0f;
        FuelConsumed = 0f;
        LastPosition = v;
    }
}
