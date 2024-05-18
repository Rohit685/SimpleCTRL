using Rage;
using RAGENativeUI;
using SimpleCTRL.Handlers;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

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

    internal static class ConversionAndFormattingHelper
    {
        #region String to Integer Conversion
        public static int ToInt32(this string text, [CallerMemberName] string callingMethod = null)
        {
            int i = 0;
            try
            {
                i = Convert.ToInt32(text);
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.Message);
            }
            return i;
        }
        #endregion

        #region Key Binding Formatting
        public static string FormatKeyBinding(ControllerButtons key) => $"{key.GetInstructionalId()}";

        public static string FormatKeyBinding(Keys key) => $"{key.GetInstructionalId()}";
        #endregion
    }

    internal static class VehicleHelper
    {
        public static RepairShop GetNearestRepairShop()
        {
            foreach (RepairShop repairShop in ConfigHandler.RepairShops)
            {
                if (Game.LocalPlayer.Character.DistanceTo(repairShop.Position) < repairShop.UseRange)
                {
                    return repairShop;
                }
            }
            return null;
        }

        public static int GetBrakePressure(int pressure)
        {
            if (pressure > 230)
            {
                return 10;
            }
            else if (pressure > 205)
            {
                return 8;
            }
            else if (pressure > 180)
            {
                return 6;
            }
            else if (pressure > 155)
            {
                return 4;
            }
            return 2;
        }
    }
}
