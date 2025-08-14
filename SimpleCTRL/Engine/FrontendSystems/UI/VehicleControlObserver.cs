using System.Linq;

namespace SimpleCTRL.Engine.FrontendSystems.UI
{
    #region Enums
    public enum VehicleFeature
    {
        LeftIndicator,
        Hazards,
        RightIndicator,
        Hood,
        LowBeamHeadlights,
        InteriorLight,
        Trunk
    }
    #endregion

    public class VehicleControlObserver : CommonPlugin, IObserver
    {
        #region Fields

        private readonly Dictionary<string, VehicleFeature> buttonFeatureMap = new()
        {
            { "left_indicator", VehicleFeature.LeftIndicator },
            { "hazards", VehicleFeature.Hazards },
            { "right_indicator", VehicleFeature.RightIndicator },
            { "hood", VehicleFeature.Hood },
            { "headlight_low", VehicleFeature.LowBeamHeadlights },
            { "interior_light", VehicleFeature.InteriorLight },
            { "trunk", VehicleFeature.Trunk }
        };

        private readonly Dictionary<VehicleFeature, bool> featureStates =
            Enum.GetValues(typeof(VehicleFeature))
                .Cast<VehicleFeature>()
                .ToDictionary(f => f, _ => false);

        #endregion

        #region IObserver Implementation

        public void OnUpdated(IObservable obj)
        {
            if (obj is not SelectableButton button) return;

            if (buttonFeatureMap.TryGetValue(button.Id, out var feature))
            {
                ToggleFeature(feature);
            }
            else
            {
                Game.LogTrivial($"Unknown button: {button.Id}");
            }
        }

        #endregion

        #region Feature Toggle Logic

        private void ToggleFeature(VehicleFeature feature)
        {
            if (ClientCurrentVehicle == null) return;

            bool isActive = featureStates[feature];

            if (!isActive && feature is VehicleFeature.LeftIndicator or VehicleFeature.RightIndicator or VehicleFeature.Hazards)
            {
                ResetIndicators();
            }

            if (!isActive)
                ActivateFeature(feature);
            else
                DeactivateFeature(feature);

            Game.LogTrivial($"Feature '{feature}' is now {(featureStates[feature] ? "ON" : "OFF")}");
        }

        private void ResetIndicators()
        {
            foreach (var f in featureStates.Keys
                         .Where(f => f is VehicleFeature.LeftIndicator or VehicleFeature.RightIndicator or VehicleFeature.Hazards)
                         .ToList())
            {
                featureStates[f] = false;
            }

            ClientCurrentVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
        }

        #endregion

        #region Activation / Deactivation

        private void ActivateFeature(VehicleFeature feature)
        {
            featureStates[feature] = true;

            switch (feature)
            {
                case VehicleFeature.LeftIndicator:
                    ClientCurrentVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.LeftOnly;
                    break;

                case VehicleFeature.RightIndicator:
                    ClientCurrentVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.RightOnly;
                    break;

                case VehicleFeature.Hazards:
                    ClientCurrentVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
                    break;

                case VehicleFeature.Hood:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_OPEN", ClientCurrentVehicle, (int)VehicleDoorIndex.Hood, false, false);
                    break;

                case VehicleFeature.LowBeamHeadlights:
                    // TODO: Implement low beam toggle
                    break;

                case VehicleFeature.InteriorLight:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_INTERIORLIGHT", ClientCurrentVehicle, true);
                    break;

                case VehicleFeature.Trunk:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_OPEN", ClientCurrentVehicle, (int)VehicleDoorIndex.Trunk, false, false);
                    break;
            }
        }

        private void DeactivateFeature(VehicleFeature feature)
        {
            switch (feature)
            {
                case VehicleFeature.LeftIndicator:
                case VehicleFeature.RightIndicator:
                case VehicleFeature.Hazards:
                    ClientCurrentVehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
                    break;

                case VehicleFeature.Hood:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_SHUT", ClientCurrentVehicle, (int)VehicleDoorIndex.Hood, false);
                    break;

                case VehicleFeature.LowBeamHeadlights:
                    // TODO: Implement low beam toggle
                    break;

                case VehicleFeature.InteriorLight:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_INTERIORLIGHT", ClientCurrentVehicle, false);
                    break;

                case VehicleFeature.Trunk:
                    NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_SHUT", ClientCurrentVehicle, (int)VehicleDoorIndex.Trunk, false);
                    break;
            }

            featureStates[feature] = false;
        }

        #endregion
    }
}