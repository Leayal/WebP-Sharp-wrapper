using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WebPWrapper.LowLevel
{
    /// <summary>Contains delegate definitions</summary>
    public static class Delegates
    {
        /// <summary>The writer type for output compress data</summary>
        /// <param name="data">Data returned</param>
        /// <param name="data_size">Size of data returned</param>
        /// <param name="wpic">Picture struct</param>
        /// <returns>
        /// <see cref="WEBP_WRITER_RESPONSE.ABORT"/> to request an abort of the encoding process, or <see cref="WEBP_WRITER_RESPONSE.CONTINUE"/> otherwise if
        /// everything is OK.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WEBP_WRITER_RESPONSE WebPWriterFunction(IntPtr data, UIntPtr data_size, ref WebPPicture wpic);

        /// <summary>Progress hook, called from time to time to report progress</summary>
        /// <param name="percent"></param>
        /// <param name="picture"></param>
        /// <returns>
        /// <see cref="WEBP_WRITER_RESPONSE.ABORT"/> to request an abort of the encoding process, or <see cref="WEBP_WRITER_RESPONSE.CONTINUE"/> otherwise if
        /// everything is OK.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WEBP_WRITER_RESPONSE WebPProgressHook(int percent, ref WebPPicture picture);
    }
}
