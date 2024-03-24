using Rage;
using System;
using System.Runtime.InteropServices;

namespace SimpleCTRL.Handlers
{
    static class Dll
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

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
            IntPtr mtLib = Dll.GetModuleHandle(@"DashHook.dll");
            if (mtLib == IntPtr.Zero)
            {
                mtLib = Dll.LoadLibrary(@"DashHook.dll");
                if (mtLib == IntPtr.Zero)
                {
                    Game.LogTrivial("Library missing");
                }
                else
                {
                   Game.LogTrivial("Load DashHook.dll success");
                }
            }
            else
            {
                Game.LogTrivial("Grab handle DashHook.dll success");
            }
        }
    }
}
