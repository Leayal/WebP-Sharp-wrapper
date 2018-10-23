using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

namespace WebPWrapper.WPF
{
    class PixelBuffer : IDisposable
    {
        private static readonly Lazy<PropertyInfo> PixelFormat_HasAlphaProperty = new Lazy<PropertyInfo>(() => typeof(PixelFormat).GetProperty("HasAlpha", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), LazyThreadSafetyMode.ExecutionAndPublication);
        private byte[] _buffer;
        private bool? _hasAlpha;
        private long _working;
        private int length, _stride, _pixelWidth, _pixelHeight;
        private PixelFormat _pixelFormat;
        private GCHandle _handle;
        private bool _directAccessMode;
        private WriteableBitmap directAccessTarget;
        private ReadOnlyCollection<byte> publicBuffer;

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

        public PixelBuffer(BitmapSource bitmap, PixelFormat preferedFormat) : this(bitmap, preferedFormat, 0.0d) { }

        /// <summary>
        /// Copy the BackBuffer of <see cref="BitmapSource"/> and convert to the <see cref="System.Windows.Media.PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap source</param>
        /// <param name="preferedFormat">Just as it explain</param>
        /// <param name="alphaThreshold">Just as it explain. Default value is 0.0</param>
        public PixelBuffer(BitmapSource bitmap, PixelFormat preferedFormat, double alphaThreshold) : this(bitmap, preferedFormat, false, alphaThreshold) { }

        /// <summary>
        /// Initialize a new <see cref="PixelBuffer"/> instance with given <see cref="WriteableBitmap"/> and convert to the <see cref="System.Windows.Media.PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap source</param>
        /// <param name="preferedFormat">Just as it explain. If pixel format conversion is needed, switch to PixelCopy regardless argument <paramref name="allowDirectAccess"/>'s value.</param>
        /// <param name="allowDirectAccess">Determine if PixelBuffer should be allowed to call <see cref="WriteableBitmap.TryLock(System.Windows.Duration)"/> and use the <see cref="WriteableBitmap.BackBuffer"/>. If the bitmap can't be locked, fallback to PixelCopy.</param>
        /// <param name="alphaThreshold">Just as it explain. Default value is 0.0</param>
        public PixelBuffer(BitmapSource bitmap, PixelFormat preferedFormat, bool allowDirectAccess, double alphaThreshold)
        {
            if (bitmap == null || preferedFormat == null)
                throw new ArgumentNullException();
            if (bitmap.Format == null)
                throw new InvalidOperationException("Unknown image format");

            if (bitmap.Format == preferedFormat)
            {
                if (allowDirectAccess && bitmap is WriteableBitmap writeable)
                {
                    this.InitDirect(writeable);
                }
                else
                {
                    this.Init(bitmap);
                }
            }
            else
            {
                FormatConvertedBitmap a = new FormatConvertedBitmap(bitmap, preferedFormat, bitmap.Palette, alphaThreshold);
                if (a.CanFreeze)
                    a.Freeze();
                this.Init(a);
            }
        }

        public int PixelWidth => this._pixelWidth;
        public int PixelHeight => this._pixelHeight;
        public int BackBufferStride => this._stride;

        private void InitDirect(WriteableBitmap bitmap)
        {
            if (bitmap.TryLock(System.Windows.Duration.Forever))
            {
                this._directAccessMode = true;
                this.directAccessTarget = bitmap;

                this._hasAlpha = null;
                this._disposed = false;
                this._working = 0;

                this._pixelFormat = bitmap.Format;
                this._pixelWidth = bitmap.PixelWidth;
                this._pixelHeight = bitmap.PixelHeight;

                this._stride = bitmap.BackBufferStride;
                this.length = bitmap.BackBufferStride * this._pixelHeight;
            }
            else
            {
                this.Init(bitmap);
            }
        }

        private void Init(BitmapSource bitmap)
        {
            this._hasAlpha = null;
            this._disposed = false;
            this._working = 0;

            this._pixelFormat = bitmap.Format;
            this._pixelWidth = bitmap.PixelWidth;
            this._pixelHeight = bitmap.PixelHeight;

            // 8-bit = 1-byte. So we devide the total bits per pixel with 8 to get the total bytes per pixel, then multiply with the image size in pixel.
            this._stride = this._pixelWidth * (this._pixelFormat.BitsPerPixel / 8);
            this.length = this._pixelHeight * this._stride;
            this._buffer = new byte[length];
            bitmap.CopyPixels(this._buffer, this._stride, 0);
            this._handle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);

            this.publicBuffer = new ReadOnlyCollection<byte>(this._buffer);
        }

        /// <summary>
        /// Directly access the back buffer. Unsupported if <see cref="PixelBuffer"/> is directly accessing to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<byte> AccessBuffer()
        {
            if (this._disposed)
                throw new ObjectDisposedException("PixelBuffer");
            if (this._directAccessMode)
                throw new InvalidOperationException();

            return this.publicBuffer;
        }

        /// <summary>
        /// Get a copy of back buffer. If you don't want to copy, use <see cref="AccessBuffer"/>.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            if (this._disposed)
                throw new ObjectDisposedException("PixelBuffer");
            if (this._directAccessMode)
                throw new InvalidOperationException();

            byte[] copiedBuffer = new byte[this.length];

            if (this._directAccessMode)
            {
                this.directAccessTarget.CopyPixels(copiedBuffer, this._stride, 0);
            }
            else
            {
                Array.Copy(this._buffer, copiedBuffer, copiedBuffer.Length);
            }

            return copiedBuffer;
        }

        public IntPtr GetPointer()
        {
            if (this._disposed)
                throw new ObjectDisposedException("PixelBuffer");
            if (this._directAccessMode)
            {
                return this.directAccessTarget.BackBuffer;
            }
            else
            {
                return this._handle.AddrOfPinnedObject();
            }
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
            if (Interlocked.Exchange(ref this._working, 1) == 0)
            {
                if (this.IsItPossibleToContainsAlpha() && this.DoesItReallyUseAlpha())
                {
                    this._hasAlpha = true;
                    Interlocked.Increment(ref this._working);
                    return true;
                }
                else
                {
                    this._hasAlpha = false;
                    Interlocked.Increment(ref this._working);
                    return false;
                }
            }
            else
            {
                if (Interlocked.Read(ref this._working) == 2)
                {
                    return this._hasAlpha.Value;
                }
                else
                {
                    throw new InvalidOperationException("I'm working on searching for alpha");
                }
            }
        }

        public bool IsItPossibleToContainsAlpha()
        {
            return IsItPossibleToContainsAlpha(this._pixelFormat);
        }

        /// <summary>
        /// It should works for common case, although it's not good at all
        /// </summary>
        /// <returns></returns>
        internal static bool IsItPossibleToContainsAlpha(PixelFormat format)
        {
            var propertyinfo = PixelFormat_HasAlphaProperty.Value;
            if (propertyinfo == null)
            {
                // (image.PixelFormat & (PixelFormat.Indexed | PixelFormat.Alpha | PixelFormat.PAlpha)) == PixelFormat.Undefined
                if (format == PixelFormats.Bgra32)
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
            else
            {
                // Use Reflector to get internal "HasAlpha" property.
                return (bool)propertyinfo.GetValue(format, null);
            }
        }

        /// <summary>
        /// It's slow, I know. And only work for 32bpp image.
        /// </summary>
        private bool DoesItReallyUseAlpha()
        {
            int off = 3,
                gap = this._stride - this._pixelWidth * 4;
            if (this._directAccessMode)
            {
                unsafe
                {
                    byte* b = (byte*)this.directAccessTarget.BackBuffer.ToPointer();
                    for (var y = 0; y < this._pixelHeight; y++, off += gap)
                        for (var x = 0; x < this._pixelWidth; x++, off += 4)
                        {
                            if (b[off] != 255)
                                return true;
                        }
                    b = null;
                }
            }
            else
            {
                for (var y = 0; y < this._pixelHeight; y++, off += gap)
                    for (var x = 0; x < this._pixelWidth; x++, off += 4)
                    {
                        if (this._buffer[off] != 255)
                            return true;
                    }
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

            if (this._directAccessMode && (this.directAccessTarget != null))
                this.directAccessTarget.Unlock();

            this.directAccessTarget = null;
            this._buffer = null;
        }
    }
}
