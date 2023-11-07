using Rage.Attributes;
using Rage;
using SimpleCTRL.Threads;
using SimpleCTRL.Handlers;
using static SimpleCTRL.Threads.SpecialModesManager;
using SimpleCTRL.UI;

[assembly: Plugin("SimpleCTRL", Author = "Venoxity Development")]
namespace SimpleCTRL
{
    internal class Entrypoint
    {
        public static void Main()
        {
            ConfigHandler.Initialize();

            GameFiber.StartNew(delegate { PlayerController.Process(); }, "SimpleCTRL - Player Controller");

            GameFiber.StartNew(delegate { ProcessPlayer(); }, "SimpleCTRL - Special Modes Player Manager");

            CustomUI.Process();
        }

        private static void OnUnload(bool isTerminating)
        {
            foreach (Blip b in mechanicBlips)
            {
                b.Delete();
            }
            Game.LogTrivial("Clearing all repair blips");
            mechanicBlips.Clear();
        }
    }
}