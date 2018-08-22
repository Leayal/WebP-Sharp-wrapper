using System;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Provides generic properties of an WebP Image.
    /// </summary>
    public sealed class WebPImage : IDisposable
    {
        private readonly WebPContentStream _content;
        private bool headerRead;
        private WebPHeader header;

        internal WebPImage(WebPContentStream contentStream)
        {
            this.headerRead = false;
            contentStream.Position = 0;
            this._content = contentStream;
        }

        private void InitHeader()
        {
            if (this.headerRead) return;
            this.headerRead = true;

            if (this._disposed)
                throw new ObjectDisposedException("WebPImage");

            var pointer = this._content.GetPointer();

            WebPDecoderConfig config = new WebPDecoderConfig();
            if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
            {
                throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
            }

            var result = UnsafeNativeMethods.WebPGetFeatures(pointer, (int)this._content.Length, ref config.input);
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
        /// Get the stream which contains the WebP file
        /// </summary>
        public WebPContentStream Content
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
