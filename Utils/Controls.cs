using Rage;
using SimpleCTRL.Handlers;
using SimpleCTRL.Threads;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleCTRL.Utils
{
    internal class Controls
    {
        private static Dictionary<SimpleControls, Keys> listKeys = new Dictionary<SimpleControls, Keys>();

        public static bool IsControlDownWithModifier(SimpleControls controls)
        {
            switch (controls)
            {
                case SimpleControls.LIGHT_INDL:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDownRightNow(ConfigHandler.LeftBlinkerKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.LeftBlinkerControllerButton);
                case SimpleControls.LIGHT_INDR:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDownRightNow(ConfigHandler.RightBlinkerKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.RightBlinkerControllerButton);
                case SimpleControls.LIGHT_HAZRD:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDownRightNow(ConfigHandler.HazardKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.HazardControllerButton);
                case SimpleControls.ENG_TOGGLE:
                    return (ConfigHandler.EngineModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.EngineModifierKey))
                        && Game.IsKeyDownRightNow(ConfigHandler.EngineKey) && !PlayerController.isDisabled
                        || (ConfigHandler.EngineModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.EngineModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.EngineControllerButton) && !PlayerController.isDisabled;
                default:
                    return false;
            }
        }

        internal enum SimpleControls
        {
            LIGHT_INDL,
            LIGHT_INDR,
            LIGHT_HAZRD,
            ENG_TOGGLE,
        }
    }
}
