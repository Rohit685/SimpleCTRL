using Rage;
using SimpleCTRL.Threads;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleCTRL.Handlers
{
    /// <summary>
    /// Provides methods for managing controls and triggering actions based on their state.
    /// </summary>
    public class ControlHandler
    {
        #region Fields
        private bool isHeld;
        private int heldTime;
        private int elapsedTime;
        private int timeout;
        private static Dictionary<SimpleControls, Keys> listKeys = new Dictionary<SimpleControls, Keys>();
        #endregion

        #region Methods
        /// <summary>
        /// Checks the duration a control is held and triggers actions accordingly.
        /// </summary>
        /// <param name="controlCondition">A function representing the condition for holding the control.</param>
        /// <param name="requiredTime">The minimum time the control must be held to trigger the first action.</param>
        /// <param name="firstAction">The action to be executed when the control is held for the required time.</param>
        /// <param name="alternativeAction">The action to be executed when the control is released before the required time.</param>
        public void CheckControlHoldDuration(Func<bool> controlCondition, int requiredTime, Action firstAction, Action alternativeAction = null)
        {
            // Decrease the timeout counter if it's greater than zero.
            if (timeout > 0)
                timeout--;

            // Check if the control condition is met and the timeout has elapsed.
            if (controlCondition.Invoke() && timeout <= 0)
            {
                // If the control is not already held, mark the start time.
                if (!isHeld)
                {
                    isHeld = true;
                    heldTime = (int)Game.GameTime;
                }
                else
                {
                    // Calculate the elapsed time since the control was first held.
                    elapsedTime = (int)(Game.GameTime - heldTime);

                    // If the required time has passed, trigger the first action.
                    if (elapsedTime >= requiredTime)
                    {
                        firstAction.Invoke();
                    }
                }
            }
            else
            {
                // If the control was held but released too early, trigger the alternative action (if provided).
                if (isHeld && elapsedTime <= requiredTime && alternativeAction != null)
                {
                    alternativeAction.Invoke();
                }

                isHeld = false;
            }
        }

        /// <summary>
        /// Determines if the specified control is pressed with a modifier key.
        /// </summary>
        /// <param name="controls">The control to check.</param>
        /// <returns>True if the control is pressed with a modifier, otherwise false.</returns>
        public static bool IsControlDownWithModifier(SimpleControls controls)
        {
            switch (controls)
            {
                case SimpleControls.LIGHT_INDL:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDown(ConfigHandler.LeftBlinkerKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.LeftBlinkerControllerButton);
                case SimpleControls.LIGHT_INDR:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDown(ConfigHandler.RightBlinkerKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.RightBlinkerControllerButton);
                case SimpleControls.LIGHT_HAZRD:
                    return (ConfigHandler.BlinkerModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.BlinkerModifierKey))
                        && Game.IsKeyDown(ConfigHandler.HazardKey)
                        || (ConfigHandler.BlinkerModifierControllerButton == ControllerButtons.None || Game.IsControllerButtonDownRightNow(ConfigHandler.BlinkerModifierControllerButton))
                        && Game.IsControllerButtonDownRightNow(ConfigHandler.HazardControllerButton);
                case SimpleControls.ENG_TOGGLE:
                    return Game.IsKeyDownRightNow(ConfigHandler.EngineToggleKey) && !PlayerController.isDisabled
                        || Game.IsControllerButtonDownRightNow(ConfigHandler.EngineControllerButton) && !PlayerController.isDisabled;
                case SimpleControls.REFUEL:
                    return Game.IsKeyDownRightNow(ConfigHandler.RefuelKey)
                        || Game.IsControllerButtonDownRightNow(ConfigHandler.RefuelControllerButton);
                case SimpleControls.PARK:
                    return (ConfigHandler.ParkModifierKey == Keys.None || Game.IsKeyDownRightNow(ConfigHandler.ParkModifierKey)) && Game.IsKeyDown(ConfigHandler.ParkKey);
                default:
                    return false;
            }
        }
        #endregion
    }

    /// <summary>
    /// Enumerates different types of simple controls.
    /// </summary>
    public enum SimpleControls
    {
        LIGHT_INDL,
        LIGHT_INDR,
        LIGHT_HAZRD,
        ENG_TOGGLE,
        REFUEL,
        PARK
    }
}
