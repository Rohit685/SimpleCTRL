using Common.Native;
using Rage;
using System;
using System.Collections.Generic;

namespace SimpleCTRL
{
    /// <summary>
    /// The type of decorator to create.
    /// </summary>
    public enum DecoratorType
    {
        /// <summary>
        /// <see cref="System.Single"/>.
        /// </summary>
        Float = 1,
        /// <summary>
        /// <see cref="System.Boolean"/>.
        /// </summary>
        Bool = 2,
        /// <summary>
        /// <see cref="System.Int32"/>.
        /// </summary>
        Int = 3,
        /// <summary>
        /// <see cref="System.TimeSpan"/>.
        /// </summary>
        Time = 5
    }

    /// <summary>
    /// Class used to manage the decorators.
    /// </summary>
    public unsafe class Decorators
    {
        #region Fields
        private static bool ready = false;
        private static byte* pointer;
        #endregion

        #region Functions
        private static void EnsureReady()
        {
            if (!ready)
            {
                throw new InvalidOperationException("Decorator system is not initialized.");
            }
        }
        /// <summary>
        /// Initializes the decorator system.
        /// </summary>
        public static void Initialize()
        {
            IntPtr addr = Game.FindPattern("40 53 48 83 EC 20 80 3D ?? ?? ?? ?? 00 8B DA 75 29");
            if (addr == IntPtr.Zero)
            {
                throw new DataMisalignedException("Memory pattern was not found.");
            }

            pointer = (byte*)(addr + *(int*)(addr + 8) + 13);
            ready = true;
        }
        /// <summary>
        /// Registers a new decorator.
        /// </summary>
        /// <param name="decorator">The name of the decorator to register.</param>
        /// <param name="type">The type of decorator.</param>
        public static void Register(string decorator, DecoratorType type)
        {
            EnsureReady();

            *pointer = 0;
            N.DecorRegister(decorator, (int)type);
            *pointer = 1;
        }
        /// <summary>
        /// Registers a set of decorators.
        /// </summary>
        /// <param name="decorators"></param>
        public static void Register(Dictionary<string, DecoratorType> decorators)
        {
            EnsureReady();

            *pointer = 0;
            foreach (KeyValuePair<string, DecoratorType> decorator in decorators)
            {
                N.DecorRegister(decorator.Key, (int)decorator.Value);
            }
            *pointer = 1;
        }
        #endregion
    }
}