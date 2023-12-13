using Rage;
using System;

namespace SimpleCTRL.Handlers
{
    public class ControlHandler
    {
        private bool isHeld;
        private int heldTime;
        private int elapsedTime;
        private int timeout;

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
            if (timeout > 0) timeout--;

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
    }
}
