using Rage;
using System;
using System.Runtime.InteropServices;

namespace SimpleCTRL.Handlers
{
    static class Dll
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
    }

    public class ExternalPluginHandler
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool FnGetBool();

        // We use this for strings, conversion on call
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FnGetIntPtr();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FnSetInt(int arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FnVoid();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U4)]
        private delegate uint FnGetUint();

        public ExternalPluginHandler()
        {
            IntPtr dhLib = Dll.GetModuleHandle(@"DashHook.dll");
            if (dhLib == IntPtr.Zero)
            {
                Game.LogTrivial("Couldn't get module handle?");
            } 
            else
            {
                Game.LogTrivial("Load DashHook.dll success");
            }
        }
    }
}
