using System;
using System.Runtime.InteropServices;
using System.Text;
using WebPWrapper.Helper;

namespace WebPWrapper
{
    internal sealed partial class UnsafeNativeMethods
    {
        [DllImport("libdl.so", EntryPoint = "dlopen", CharSet = CharSet.Unicode)]
        private static extern SafeLinuxLibraryHandle Linux_dlopen(string fileName, RTLD flags);

        [DllImport("libdl.so", EntryPoint = "dlsym", CharSet = CharSet.Unicode)]
        private static extern IntPtr Linux_dlsym(SafeLibraryHandle handle, string symbol);

        [DllImport("libdl.so", EntryPoint = "dlclose")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Linux_dlclose(IntPtr handle);

        [DllImport("libdl.so", EntryPoint = "dlerror")]
        private static extern IntPtr Linux_dlerror();

        [DllImport("libdl.so", EntryPoint = "dladdr")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Linux_dladdr(SafeLibraryHandle handle, ref UNIX_DL_INFO dl_info);

        [DllImport("libdl.so", EntryPoint = "dlinfo")]
        private static unsafe extern int Linux_dlinfo(SafeLibraryHandle handle, RTLD_DI request, void* arg);

        [DllImport("libdl.so", EntryPoint = "dlinfo", CharSet = CharSet.Unicode)]
        private static extern int Linux_dlinfo(SafeLibraryHandle handle, RTLD_DI request, StringBuilder charBuffer);

        // extern int dlinfo (void *__restrict __handle, int __request, void* __restrict __arg)

        // int dladdr(const void * addr, Dl_info * dlip);
    }
}
