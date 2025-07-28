using System.IO;
using System.Xml.Serialization;

namespace SimpleCTRL.Engine.Data
{
    [XmlRoot("VehicleDamageConfig")]
    public class VehicleDamageConfig
    {
        public Deformation Deformation { get; set; }
        public DamageFactors DamageFactors { get; set; }
        public HealthThresholds HealthThresholds { get; set; }
        public LimpModeSettings LimpModeSettings { get; set; }

        [XmlArray("ClassDamageMultiplier")]
        [XmlArrayItem("Multiplier")]
        public List<float> ClassDamageMultiplier { get; set; }

        public static VehicleDamageConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found at: {path}");

            using var stream = File.OpenRead(path);
            var serializer = new XmlSerializer(typeof(VehicleDamageConfig));
            return (VehicleDamageConfig)serializer.Deserialize(stream);
        }
    }

    public class Deformation
    {
        public float DeformationMultiplier { get; set; }
        public float DeformationExponent { get; set; }
        public float CollisionDamageExponent { get; set; }
    }

    public class DamageFactors
    {
        public float DamageFactorEngine { get; set; }
        public float DamageFactorBody { get; set; }
        public float DamageFactorPetrolTank { get; set; }
        public float EngineDamageExponent { get; set; }
        public float WeaponsDamageMultiplier { get; set; }
    }

    public class HealthThresholds
    {
        public float DegradingHealthSpeedFactor { get; set; }
        public float CascadingFailureSpeedFactor { get; set; }
        public float DegradingFailureThreshold { get; set; }
        public float CascadingFailureThreshold { get; set; }
        public float EngineSafeGuard { get; set; }
    }

    public class LimpModeSettings
    {
        public bool TorqueMultiplierEnable { get; set; }
        public bool LimpMode { get; set; }
        public float LimpModeMultiplier { get; set; }
    }
}
