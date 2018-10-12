using System;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace WebPWrapper.WPF.Helper
{
    internal static class RuntimeValue
    {
        internal const int DefaultBufferSize = 4096;

        internal static readonly bool is64bit = Environment.Is64BitProcess;

        internal static string StringDependsArchitecture(string x86, string x64)
        {
            if (is64bit)
                return x64;
            else
                return x86;
        }
    }
}
