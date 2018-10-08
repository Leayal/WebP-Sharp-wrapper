using System;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.UnmanagedLibrary;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Provides generic properties of an WebP Image.
    /// </summary>
    public sealed class WebPImage :  IDisposable
    {
        private Stream _content;
        private WebPHeader header;

        internal WebPImage(Libwebp lib, Stream contentStream)
        {
            this._content = contentStream;

            this.InitHeader(lib);
        }

        private void InitHeader(Libwebp library)
        {
            byte[] buffer = new byte[32];
            long currentpos = this.Content.Position;

            if (currentpos != 0)
                this.Content.Position = 0;
            this.Content.Read(buffer, 0, buffer.Length);
            this.Content.Position = currentpos;

            WebPDecoderConfig config = new WebPDecoderConfig();
            if (library.WebPInitDecoderConfig(ref config) == 0)
            {
                throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
            }

            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            VP8StatusCode result = VP8StatusCode.VP8_STATUS_SUSPENDED;
            if (gch.IsAllocated)
            {
                try
                {
                    result = library.WebPGetFeatures(gch.AddrOfPinnedObject(), buffer.Length, ref config.input);
                }
                finally
                {
                    gch.Free();
                }
            }
            if (result != VP8StatusCode.VP8_STATUS_OK)
                throw new Exception("Failed WebPGetFeatures with error " + result);

            this.header = new WebPHeader(ref config.input);
        }

        /// <summary>
        /// Get the image info
        /// </summary>
        public WebPHeader Info => this.header;

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

            this.header = null;
            this._content = null;
        }
    }
}
