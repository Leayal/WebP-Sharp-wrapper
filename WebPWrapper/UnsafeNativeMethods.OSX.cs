using System;
using System.Runtime.InteropServices;
using System.Text;
using WebPWrapper.Internal;

namespace WebPWrapper
{
    internal sealed partial class UnsafeNativeMethods
    {
        [DllImport("libdl.dylib", EntryPoint = "dlopen", CharSet = CharSet.Unicode)]
        private static extern SafeOSXLibraryHandle OSX_dlopen(string fileName, RTLD flags);

        [DllImport("libdl.dylib", EntryPoint = "dlsym", CharSet = CharSet.Unicode)]
        private static extern IntPtr OSX_dlsym(SafeLibraryHandle handle, string symbol);

        [DllImport("libdl.dylib", EntryPoint = "dlclose")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OSX_dlclose(IntPtr handle);

        [DllImport("libdl.dylib", EntryPoint = "dlerror")]
        private static extern IntPtr OSX_dlerror();

        [DllImport("libdl.dylib", EntryPoint = "dladdr")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OSX_dladdr(SafeLibraryHandle handle, ref UNIX_DL_INFO dl_info);

        [DllImport("libdl.dylib", EntryPoint = "dlinfo")]
        private static unsafe extern int OSX_dlinfo(SafeLibraryHandle handle, RTLD_DI request, void* arg);

        [DllImport("libdl.dylib", EntryPoint = "dlinfo", CharSet = CharSet.Unicode)]
        private static extern int OSX_dlinfo(SafeLibraryHandle handle, RTLD_DI request, StringBuilder charBuffer);
    }
}
