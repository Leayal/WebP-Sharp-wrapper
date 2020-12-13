using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using WebPWrapper.Helper;

namespace WebPWrapper
{
    internal sealed partial class UnsafeNativeMethods
    {
        internal unsafe static void Memcpy(byte[] dest, int destIndex, byte* src, int srcIndex, int len)
        {
            if ((srcIndex < 0) && (destIndex < 0) && (len < 0))
                throw new InvalidOperationException("Index and length must be non-negative!");
            if (dest.Length - destIndex < len)
                throw new InvalidOperationException("not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len == 0)
                return;
            fixed (byte* pDest = dest)
            {
                MemoryCopy(pDest + destIndex, src + srcIndex, len);
            }
        }

        private static unsafe void MemoryCopy(void* dest, void* src, int count)
        {
            int block;

            block = count >> 3;

            long* pDest = (long*)dest;
            long* pSrc = (long*)src;

            for (int i = 0; i < block; i++)
            {
                *pDest = *pSrc; pDest++; pSrc++;
            }
            dest = pDest;
            src = pSrc;
            count = count - (block << 3);

            if (count > 0)
            {
                byte* pDestB = (byte*)dest;
                byte* pSrcB = (byte*)src;
                for (int i = 0; i < count; i++)
                {
                    *pDestB = *pSrcB; pDestB++; pSrcB++;
                }
            }
        }

        #region "Unix-based"

        internal const int MAX_PATH_UNIX = 1024;

        /*
        class link_map
        {
            public IntPtr l_addr;  // Difference between the address in the ELF file and the address in memory
            public string l_name;  // Absolute pathname where object was found
            public IntPtr l_ld;    // Dynamic section of the shared object
            public link_map l_next, l_prev; // Chain of loaded objects

            // Plus additional fields private to the implementation
        }
        */

        /// <summary>The MODE argument to 'dlopen'. Read the remarks if you're combining this flag.</summary>
        /// <remarks>Can only combined between one 'mode' and one 'flag'. 'mode' is LAZY|NOW|BINDING_MASK|NOLOAD|DEEPBIND and 'flag' is GLOBAL|LOCAL|NODELETE</remarks>
        [Flags]
        enum RTLD : int
        {
            /* The MODE argument to 'dlopen' contains one of the following: */
            /// <summary>Lazy function call binding.</summary>
            LAZY = 0x00001,
            /// <summary>Immediate function call binding.</summary>
            NOW = 0x00002,
            /// <summary>Mask of binding time value.</summary>
            BINDING_MASK = 0x3,
            /// <summary>Do not load the object.</summary>
            NOLOAD = 0x00004,
            /// <summary>Use deep binding.</summary>
            DEEPBIND = 0x00008,

            /* And can also be combined with one of the following: */
            /// <summary>If the following bit is set in the MODE argument to `dlopen', the symbols of the loaded object and its dependencies are made visible as if the object were linked directly into the program.</summary>
            GLOBAL = 0x00100,
            /// <summary>Unix98 demands the following flag which is the inverse to RTLD_GLOBAL. The implementation does this by default and so we can define the value to zero.</summary>
            LOCAL = 0,
            /// <summary>Do not delete object when closed.</summary>
            NODELETE = 0x01000
        }

        /// <summary>These are the possible values for the REQUEST argument to `dlinfo'.</summary>
        enum RTLD_DI : int
        {
            /// <summary>Treat ARG as `lmid_t *'; store namespace ID for HANDLE there.</summary>
            LMID = 1,
            /// <summary>Treat ARG as `struct link_map **'. Store the `struct link_map *' for HANDLE there.</summary>
            LINKMAP = 2,
            /// <summary>Unsupported, defined by Solaris.</summary>
            CONFIGADDR = 3,
            /// <summary>
            /// Treat ARG as `Dl_serinfo *' (see below), and fill in to describe the
            /// directories that will be searched for dependencies of this object.
            /// RTLD_DI_SERINFOSIZE fills in just the `dls_cnt' and `dls_size'
            /// entries to indicate the size of the buffer that must be passed to
            /// RTLD_DI_SERINFO to fill in the full information.
            /// </summary>
            SERINFO = 4,
            SERINFOSIZE = 5,
            /// <summary>Treat ARG as `char *', and store there the directory name used to expand $ORIGIN in this shared object's dependency file names.</summary>
            ORIGIN = 6,
            /// <summary>Unsupported, defined by Solaris.</summary>
            PROFILENAME = 7,
            /// <summary>Unsupported, defined by Solaris.</summary>
            PROFILEOUT = 8,
            /// <summary>Treat ARG as `size_t *', and store there the TLS module ID of this object's PT_TLS segment, as used in TLS relocations; store zero if this object does not define a PT_TLS segment.</summary>
            TLS_MODID = 9,
            /// <summary>
            /// Treat ARG as `void **', and store there a pointer to the calling
            /// thread's TLS block corresponding to this object's PT_TLS segment.
            /// Store a null pointer if this object does not define a PT_TLS
            /// segment, or if the calling thread has not allocated a block for it.
            /// </summary>
            TLS_DATA = 10,
            RTLD_DI_MAX = 10
        }

        // const int RTLD_NOW = 2;

        [StructLayout(LayoutKind.Sequential)]
        struct UNIX_DL_INFO
        {
            public string dli_fname;
            public IntPtr dli_fbase;
            public string dli_sname;
            public IntPtr dli_saddr;
        }

        #endregion

        internal static SafeLibraryHandle LoadLibrary(string filename)
        {
            if (RuntimeValue.isWindows)
            {
                return LoadWindowsLibrary(filename);
            }
            else if (RuntimeValue.isLinux)
            {
                return Linux_dlopen(filename, RTLD.LAZY | RTLD.GLOBAL);
            }
            else if (RuntimeValue.isOSX)
            {
                return OSX_dlopen(filename, RTLD.LAZY | RTLD.GLOBAL);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        internal static IntPtr FindFunction(SafeLibraryHandle dllHandle, string name)
        {
            if (RuntimeValue.isWindows)
            {
                return GetWindowsProcAddress(dllHandle, name);
            }
            else if (RuntimeValue.isLinux)
            {
                // clear previous errors if any
                Linux_dlerror();
                var res = Linux_dlsym(dllHandle, name);
                var errPtr = Linux_dlerror();
                if (errPtr != IntPtr.Zero)
                {
                    throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
                }
                return res;
            }
            else if (RuntimeValue.isOSX)
            {
                OSX_dlerror();
                var res = OSX_dlsym(dllHandle, name);
                var errPtr = OSX_dlerror();
                if (errPtr != IntPtr.Zero)
                {
                    throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
                }
                return res;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        internal static string GetModuleFileName(SafeLibraryHandle dllHandle)
        {
            if (RuntimeValue.isWindows)
            {
                var sb = new StringBuilder(MAX_PATH_WINDOWS);
                if (GetWindowsModuleFileName(dllHandle, sb, sb.Capacity) != 0)
                {
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else if (RuntimeValue.isLinux)
            {
                var sb = new StringBuilder(MAX_PATH_UNIX);
                if (Linux_dlinfo(dllHandle, RTLD_DI.ORIGIN, sb) == 0)
                {
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else if (RuntimeValue.isOSX)
            {
                var sb = new StringBuilder(MAX_PATH_UNIX);
                if (OSX_dlinfo(dllHandle, RTLD_DI.ORIGIN, sb) == 0)
                {
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
