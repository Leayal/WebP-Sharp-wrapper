using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper.WPF.Helper;

namespace WebPWrapper.WPF
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

        public static unsafe void MemoryCopy(void* dest, void* src, int count)
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private unsafe static extern int ReadFile(SafeFileHandle handle, IntPtr bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);

        internal unsafe static int ReadFileUnsafe(FileStream fs, IntPtr pointer, int offset, int count, out int hr)
        {
            if (count == 0)
            {
                hr = 0;
                return 0;
            }
            int operationSuccess = ReadFile(fs.SafeFileHandle, IntPtr.Add(pointer, offset), count, out var byteread, IntPtr.Zero);
            if (operationSuccess == 0)
            {
                hr = Marshal.GetLastWin32Error();
                // 109 means EOF
                if (hr == 109 || hr == 233)
                {
                    return -1;
                }
                if (hr == 6)
                {
                    fs.Dispose();
                }
                return -1;
            }
            hr = 0;
            return byteread;
        }

        [DllImport("kernel32.dll", SetLastError = true), PreserveSig]
        internal static extern uint GetModuleFileName([In]SafeLibraryHandle hModule, [Out]StringBuilder lpFilename, [In][MarshalAs(UnmanagedType.U4)]int nSize);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary(string fileName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, String procname);
    }
}
