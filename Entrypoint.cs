using Common;
using Rage;
using Rage.Attributes;
using SimpleCTRL.API;
using SimpleCTRL.Handlers;
using SimpleCTRL.Threads;
using SimpleCTRL.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

[assembly: Plugin("SimpleCTRL", Author = "Venoxity Development", PrefersSingleInstance = true, ShouldTickInPauseMenu = true, SupportUrl = "https://discord.gg/jCEdAF8AQz")]
namespace SimpleCTRL
{
    internal class Entrypoint 
    {
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "_Fuel_Level", DecoratorType.Float },
            { "brakeHeat", DecoratorType.Int },
        };

        public static void Main()
        {
            if (CheckDependencies())
            {
                Logging.Info("starting...", "SimpleCTRL");
                ConfigHandler.Initialize();
                Decorators.Initialize();
                Decorators.Register(decorators);
                PlayerController.Start();
                SpecialModesManager.Start();
                FobHandler.Start();
                UIHandler.Start();
                Current.LastWorldTime = DateTime.UtcNow;
                Functions.CreateDepartmentPumps();
                Functions.CreateBlips();
            }
            else
            {
                Game.DisplayNotification("new_editor", "warningtriangle", "SimpleCTRL", "~r~Initialization Failure", "~y~SimpleCTRL could not start.  You are missing required libraries.");
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            Logging.Info("stopping SimpleCTRL", "SimpleCTRL");
            Functions.RemoveBlips();
        }

        private static bool CheckDependencies()
        {
            foreach (var dependency in Constants.Dependencies)
            {
                if (!IsAssemblyAvailable(dependency.Name, dependency.Version))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsAssemblyAvailable(string assemblyName, string version)
        {
            try
            {
                AssemblyName assemblyName2 = AssemblyName.GetAssemblyName(AppDomain.CurrentDomain.BaseDirectory + "/" + assemblyName);
                if (assemblyName2.Version >= new Version(version))
                {
                    Game.LogTrivial($"SimpleCTRL dependency {assemblyName} is available ({assemblyName2.Version}).");
                    return true;
                }
                Game.LogTrivial($"SimpleCTRL dependency {assemblyName} does not meet minimum requirements ({assemblyName2.Version} < {version}).");
                return false;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is BadImageFormatException)
            {
                Game.LogTrivial("SimpleCTRL dependency " + assemblyName + " is not available.");
                return false;
            }
        }
    }
}