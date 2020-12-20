using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WebPWrapper.LowLevel
{
    /// <summary>Webp function wrapper</summary>
    public readonly struct WebpDelegate : IDisposable
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
        public delegate WEBP_WRITER_RESPONSE WebPWriterFunction([In] IntPtr data, [In] UIntPtr data_size, ref WebPPicture wpic);

        /// <summary>Progress hook, called from time to time to report progress</summary>
        /// <param name="percent"></param>
        /// <param name="picture"></param>
        /// <returns>
        /// <see cref="WEBP_WRITER_RESPONSE.ABORT"/> to request an abort of the encoding process, or <see cref="WEBP_WRITER_RESPONSE.CONTINUE"/> otherwise if
        /// everything is OK.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WEBP_WRITER_RESPONSE WebPProgressHook([In] int percent, ref WebPPicture picture);

        /// <summary>Wrap the function and pinned it in the memory</summary>
        /// <param name="func">The function to be pinned.</param>
        /// <returns>A <see cref="WebpDelegate"/> wrapper to manage the pinned object.</returns>
        /// <remarks>You should call <seealso cref="Dispose"/> immediately once you don't need it anymore.</remarks>
        /// <exception cref="ApplicationException">The object failed to be pinned</exception>
        public static WebpDelegate Create(Delegate func)
        {
            return new WebpDelegate(func);
        }

        private readonly GCHandle pinnedhandle;
        private readonly Delegate wrappedDelegate;

        private WebpDelegate(Delegate @delegate)
        {
            this.pinnedhandle = GCHandle.Alloc(@delegate, GCHandleType.Normal);
            if (!this.pinnedhandle.IsAllocated)
            {
                throw new ApplicationException();
            }
            this.wrappedDelegate = @delegate;
        }

        /// <summary>Gets the wrapped delegate in this instance.</summary>
        /// <returns>The wrapped function</returns>
        /// <remarks>Not really useful.</remarks>
        public Delegate Get() => this.wrappedDelegate;

        /// <summary>Unpin the object in the memory.</summary>
        public void Dispose()
        {
            if (this.pinnedhandle.IsAllocated)
            {
                this.pinnedhandle.Free();
            }
        }
    }
}
