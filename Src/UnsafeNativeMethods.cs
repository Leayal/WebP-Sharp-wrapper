using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper.WPF.Helper;

namespace WebPWrapper.WPF
{
    [SuppressUnmanagedCodeSecurityAttribute]
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

        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        public static extern SafeLibraryHandle LoadLibrary(string fileName);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, String procname);
    }
}
