using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using WebPWrapper.Helper;

namespace WebPWrapper
{
    internal sealed partial class UnsafeNativeMethods
    {
        public const int MAX_PATH_WINDOWS = 1024; // Ideal at 260. But....

        [DllImport("kernel32.dll", EntryPoint = "GetModuleFileName", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true), PreserveSig]
        private static extern uint GetWindowsModuleFileName([In]SafeLibraryHandle hModule, [Out]StringBuilder lpFilename, [In][MarshalAs(UnmanagedType.U4)]int nSize);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, BestFitMapping = false, SetLastError = true)]
        private static extern SafeWindowsLibraryHandle LoadWindowsLibrary(string fileName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
            DllImport("kernel32.dll", EntryPoint = "FreeLibrary", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeWindowsLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi, ThrowOnUnmappableChar = true)]
        [SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "<Pending>")]
        private static extern IntPtr GetWindowsProcAddress(SafeLibraryHandle hModule, string procname);
    }
}
