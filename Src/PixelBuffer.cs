using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace WebPWrapper
{
    class PixelBuffer : IDisposable
    {
        private byte[] _buffer;
        private bool? _hasAlpha;
        private bool _working;
        private int length, _stride, _pixelWidth, _pixelHeight;
        private PixelFormat _pixelFormat;
        private GCHandle _handle;

        public PixelBuffer(BitmapSource bitmap, System.Windows.Media.PixelFormat preferedFormat) : this(bitmap, preferedFormat, 0.0d) { }

        /// <summary>
        /// Copy the BackBuffer of <see cref="BitmapSource"/> and convert the <see cref="System.Windows.Media.PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap source</param>
        /// <param name="preferedFormat">Just as it explain</param>
        /// <param name="preferedFormat">Just as it explain</param>
        public PixelBuffer(BitmapSource bitmap, System.Windows.Media.PixelFormat preferedFormat, double alphaThreshold)
        {
            if (bitmap == null || preferedFormat == null)
                throw new ArgumentNullException();
            if (bitmap.Format == null)
                throw new InvalidOperationException("Unknown image format");

            if (bitmap.Format == preferedFormat)
                this.Init(bitmap);
            else
            {
                this.Init(new FormatConvertedBitmap(bitmap, preferedFormat, bitmap.Palette, alphaThreshold));
            }
        }

        public int PixelWidth => this._pixelWidth;
        public int PixelHeight => this._pixelHeight;
        public int BackBufferStride => this._stride;
        

        /// <summary>
        /// Copy BackBuffer from a <see cref="BitmapSource"/> instance.
        /// </summary>
        /// <param name="bitmap">The bitmap source</param>
        public PixelBuffer(BitmapSource bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException();
            if (bitmap.Format == null)
                throw new InvalidOperationException("Unknown image format");

            this.Init(bitmap);
        }

        private void Init(BitmapSource bitmap)
        {
            this._hasAlpha = null;
            this._disposed = false;
            this._working = false;

            this._pixelFormat = bitmap.Format;
            this._pixelWidth = bitmap.PixelWidth;
            this._pixelHeight = bitmap.PixelHeight;

            // 8-bit = 1-byte. So we devide the total bits per pixel with 8 to get the total bytes per pixel, then multiply with the image size in pixel.
            this._stride = this._pixelWidth * (this._pixelFormat.BitsPerPixel / 8);
            this.length = this._pixelHeight * this._stride;
            this._buffer = new byte[length];
            bitmap.CopyPixels(this._buffer, this._stride, 0);
            this._handle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);
        }

        public byte[] GetBuffer()
        {
            if (this._disposed)
                throw new ObjectDisposedException("PixelBuffer");
            return this._buffer;
        }

        public IntPtr GetPointer()
        {
            if (this._disposed)
                throw new ObjectDisposedException("PixelBuffer");
            return this._handle.AddrOfPinnedObject();
        }

        public bool HasAlpha
        {
            get
            {
                if (this._hasAlpha.HasValue)
                    return this._hasAlpha.Value;
                else
                {
                    this._hasAlpha = this.DoesItHaveAndUseAlpha();
                    return this._hasAlpha.Value;
                }
            }
        }

        private bool DoesItHaveAndUseAlpha()
        {
            if (this._working)
                throw new InvalidOperationException("I'm working on searching for alpha");
            this._working = true;
            if (this.IsItPossibleToContainsAlpha())
                return this.DoesItReallyUseAlpha();
            else
                return false;
        }

        public bool IsItPossibleToContainsAlpha()
        {
            return IsItPossibleToContainsAlpha(this._pixelFormat);
        }

        /// <summary>
        /// It should works for common case, although it's not good at all
        /// </summary>
        /// <returns></returns>
        public static bool IsItPossibleToContainsAlpha(PixelFormat format)
        {
            // (image.PixelFormat & (PixelFormat.Indexed | PixelFormat.Alpha | PixelFormat.PAlpha)) == PixelFormat.Undefined
            if (format == PixelFormats.Bgr32)
                return true;
            else if (format == PixelFormats.Bgra32)
                return true;
            else if (format == PixelFormats.Indexed8)
                return true;
            else if (format == PixelFormats.Pbgra32)
                return true;
            else if (format == PixelFormats.Rgba64)
                return true;
            else
                return false;
        }

        /// <summary>
        /// It's slow, I know. And only work for 32bpp image.
        /// </summary>
        private bool DoesItReallyUseAlpha()
        {
            int off = 3,
                gap = this._stride - this._pixelWidth * 4;
            for (var y = 0; y < this._pixelHeight; y++, off += gap)
                for (var x = 0; x < this._pixelWidth; x++, off += 4)
                {
                    if (this._buffer[off] != 255)
                        return true;
                }
            return false;
        }

        public ref PixelFormat PixelFormat => ref this._pixelFormat;

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            if (this._handle.IsAllocated)
                this._handle.Free();

            this._buffer = null;
        }
    }
}
