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
        Trunk,
        Door,
        Window,
        Seat
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
            { "front_hood", VehicleFeature.Hood },
            { "headlight_low", VehicleFeature.LowBeamHeadlights },
            { "interior_light", VehicleFeature.InteriorLight },
            { "rear_hood", VehicleFeature.Trunk },
            { "door_1", VehicleFeature.Door },
            { "door_2", VehicleFeature.Door },
            { "door_3", VehicleFeature.Door },
            { "door_4", VehicleFeature.Door },
            { "window_1", VehicleFeature.Window },
            { "window_2", VehicleFeature.Window },
            { "window_3", VehicleFeature.Window },
            { "window_4", VehicleFeature.Window },
            { "seat_1", VehicleFeature.Seat },
            { "seat_2", VehicleFeature.Seat },
            { "seat_3", VehicleFeature.Seat },
            { "seat_4", VehicleFeature.Seat },
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
                ToggleFeature(button.Id, feature);
            }
            else
            {
                Game.LogTrivial($"Unknown button: {button.Id}");
            }
        }

        #endregion

        #region Feature Toggle Logic

        private void ToggleFeature(string buttonId, VehicleFeature feature)
        {
            if (ClientCurrentVehicle == null) return;

            bool isActive = featureStates[feature];

            if (!isActive && feature is VehicleFeature.LeftIndicator or VehicleFeature.RightIndicator or VehicleFeature.Hazards)
                ResetIndicators();

            if (!isActive)
                ActivateFeature(buttonId, feature);
            else
                DeactivateFeature(buttonId, feature);

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

        private void ActivateFeature(string buttonId, VehicleFeature feature)
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

                case VehicleFeature.Door:
                    ToggleDoor(buttonId, true);
                    break;

                case VehicleFeature.Window:
                    ToggleWindow(buttonId, true);
                    break;

                case VehicleFeature.Seat:
                    ToggleSeat(buttonId);
                    break;
            }
        }

        private void DeactivateFeature(string buttonId, VehicleFeature feature)
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

                case VehicleFeature.Door:
                    ToggleDoor(buttonId, false);
                    break;

                case VehicleFeature.Window:
                    ToggleWindow(buttonId, false);
                    break;
            }

            featureStates[feature] = false;
        }

        #endregion

        private int GetDoorIndex(string buttonId)
        {
            return buttonId switch
            {
                "door_1" => 0,
                "door_2" => 1,
                "door_3" => 2,
                "door_4" => 3,
                _ => -1 // invalid door
            };
        }

        private int GetWindowIndex(string buttonId)
        {
            return buttonId switch
            {
                "window_1" => 0,
                "window_2" => 1,
                "window_3" => 2,
                "window_4" => 3,
                _ => -1 // invalid window
            };
        }

        private int GetSeatIndex(string buttonId, Vehicle vehicle)
        {
            int index = buttonId switch
            {
                "seat_1" => -1,
                "seat_2" => 0,
                "seat_3" => 1,
                "seat_4" => 2,
                _ => -2
            };

            if (index == -2) 
            {
                for (int i = 0; i <= 2; i++) 
                {
                    if (!vehicle.IsSeatFree(i)) continue;
                    index = i;
                    break;
                }
            }

            return index;
        }

        private void ToggleDoor(string buttonId, bool open)
        {
            int index = GetDoorIndex(buttonId);
            if (index == -1) return;

            NativeFunction.CallByName<uint>(
                open ? "SET_VEHICLE_DOOR_OPEN" : "SET_VEHICLE_DOOR_SHUT",
                ClientCurrentVehicle,
                index,
                false,
                false
            );
        }

        private void ToggleWindow(string buttonId, bool down)
        {
            int index = GetWindowIndex(buttonId);
            if (index == -1) return;

            NativeFunction.CallByName<uint>(
                down ? "ROLL_DOWN_WINDOW" : "ROLL_UP_WINDOW",
                ClientCurrentVehicle,
                index
            );
        }

        private void ToggleSeat(string buttonId)
        {
            int index = GetSeatIndex(buttonId, ClientCurrentVehicle);
            if (index == -2) return;

            NativeFunction.CallByName<uint>("SET_PED_INTO_VEHICLE", ClientPed, ClientCurrentVehicle, index);
        }
    }
}