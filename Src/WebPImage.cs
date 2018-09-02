using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Provides generic properties of an WebP Image.
    /// </summary>
    public sealed class WebPImage :  IDisposable
    {
        private readonly Stream _content;
        private bool headerRead;
        private WebPHeader header;

        internal WebPImage(Stream contentStream)
        {
            this.headerRead = false;
            this._content = contentStream;
        }

        private void InitHeader()
        {
            if (this.headerRead) return;
            this.headerRead = true;

            if (this._disposed)
                throw new ObjectDisposedException("WebPImage");


            byte[] buffer = new byte[30];
            long currentpos = this.Content.Position;

            if (currentpos != 0)
                this.Content.Position = 0;
            this.Content.Read(buffer, 0, buffer.Length);
            this.Content.Position = currentpos;

            WebPDecoderConfig config = new WebPDecoderConfig();
            if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
            {
                throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
            }

            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            VP8StatusCode result;
            try
            {
                result = UnsafeNativeMethods.WebPGetFeatures(gch.AddrOfPinnedObject(), buffer.Length, ref config.input);
            }
            finally
            {
                if (gch.IsAllocated)
                    gch.Free();
            }
            if (result != VP8StatusCode.VP8_STATUS_OK)
                throw new Exception("Failed WebPGetFeatures with error " + result);

            this.header = new WebPHeader(ref config.input);
        }

        /// <summary>
        /// Get the image info
        /// </summary>
        public WebPHeader Info
        {
            get
            {
                this.InitHeader();
                return this.header;
            }
        }

        /// <summary>
        /// Get the stream which contains the WebP image data
        /// </summary>
        public Stream Content
        {
            get
            {
                if (this._disposed)
                    throw new ObjectDisposedException("WebPImage");
                return this._content;
            }
        }

        private bool _disposed;
        /// <summary>
        /// This will call Dispose() on <see cref="WebPContentStream"/> of this instance. Just the same as using <see cref="Content"/>.Dispose().
        /// </summary>
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;
            this._content.Dispose();
        }
    }
}
