using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;

namespace WebPWrapper.Internal
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    abstract class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeLibraryHandle() : base(true) { }
    }

    sealed class SafeWindowsLibraryHandle : SafeLibraryHandle
    {
        private SafeWindowsLibraryHandle() : base() { }

        protected override bool ReleaseHandle() => UnsafeNativeMethods.FreeWindowsLibrary(handle);
    }

    sealed class SafeLinuxLibraryHandle : SafeLibraryHandle
    {
        private SafeLinuxLibraryHandle() : base() { }

        protected override bool ReleaseHandle() => UnsafeNativeMethods.Linux_dlclose(handle);
    }

    sealed class SafeOSXLibraryHandle : SafeLibraryHandle
    {
        private SafeOSXLibraryHandle() : base() { }

        protected override bool ReleaseHandle() => UnsafeNativeMethods.OSX_dlclose(handle);
    }
}
