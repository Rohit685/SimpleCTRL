using Common;
using Rage;
using System;
using System.Reflection;

namespace SimpleCTRL.Utils
{
    internal static class UIHelper
    {
        public static bool IsUIAbleToDisplay { get; set; } = false;

        private static void Process()
        {
            while (true)
            {
                GameFiber.Yield();
                if (!Game.IsPaused && !Game.IsLoading && !Game.IsScreenFadedOut)
                    IsUIAbleToDisplay = true;
                else
                    IsUIAbleToDisplay = false;
            }
        }

        static UIHelper()
        {
            GameFiber.StartNew(Process, "SimpleCTRL - UI Helper");
        }
    }
}
