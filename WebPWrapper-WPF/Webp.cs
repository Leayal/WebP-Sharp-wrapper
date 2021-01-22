using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper;
using WebPWrapper.LowLevel;

namespace WebPWrapper.WPF
{
    /// <summary>Simple webp wrapper for WPF.</summary>
    public class Webp : IDisposable
    {
        private readonly WebpFactory webp;
        private bool disposed;
        private readonly bool _shouldDisposeFactory;

        /// <summary>Initialize a new <see cref="Webp"/> instance with the given library path.</summary>
        /// <param name="libraryPath">The file path to the native webp library.</param>
        public Webp(string libraryPath) : this(new WebpFactory(libraryPath), true) { }

        /// <summary>Initialize a new <see cref="Webp"/> instance with the given <seealso cref="WebpFactory"/>.</summary>
        /// <param name="factory">The <seealso cref="WebpFactory"/> to wrap.</param>
        /// <param name="disposeFactory">Determines whether the given <seealso cref="WebpFactory"/> will be disposed when this <seealso cref="Webp"/> instance is disposed.</param>
        public Webp(WebpFactory factory, bool disposeFactory)
        {
            this.disposed = false;
            this._shouldDisposeFactory = disposeFactory;
            this.webp = factory;
        }

        /// <summary>Gets the <seealso cref="WebpFactory"/> which is associated with this <seealso cref="Webp"/> instance.</summary>
        public WebpFactory Factory => this.webp;

        /// <summary>Decodes Webp data stream to <seealso cref="BitmapSource"/>.</summary>
        /// <param name="dataStream">The data stream which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="BitmapSource"/> which contains the image data.</returns>
        /// <remarks>Incomplete API. Therefore, in case when decoder is progressive, only <seealso cref="WindowsDecoderOptions.PixelFormat"/> will be used, all other options will be ignored.</remarks>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the stream contains invalid data for Webp Image.</exception>
        public BitmapSource Decode(Stream dataStream, WindowsDecoderOptions options)
        {
            if (dataStream == null)
            {
                throw new ArgumentNullException(nameof(dataStream));
            }
            if (!dataStream.CanRead)
            {
                throw new ArgumentException("The stream must be readable.", nameof(dataStream));
            }

            if (dataStream is MemoryStream memStream)
            {
                if (memStream.TryGetBuffer(out var segment))
                {
                    var mem = new ReadOnlyMemory<byte>(segment.Array, segment.Offset, segment.Count);
                    return this.Decode(mem, options);
                }
            }

            bool canEvenSeek = false;
            try
            {
                canEvenSeek = dataStream.CanSeek;
            }
            catch (NotSupportedException) { }
            catch (NotImplementedException) { }

            if (canEvenSeek)
            {
                int length;
                long currentPos = -1;
                try
                {
                    currentPos = dataStream.Position;
                    // Try get length if it supports.
                    length = (int)dataStream.Length;
                }
                catch (NotSupportedException) { length = -1; }
                // Length is longer than int.MaxValue
                catch { length = -2; }
                if (length != -2)
                {
                    // Try to get the webp's data length starting from the current position of the stream (if possible)
                    var bytes = ArrayPool<byte>.Shared.Rent(4096);
                    try
                    {
                        if (dataStream.Read(bytes, 0, 12) == 12)
                        {
                            if (WebpFactory.TryGetFileSizeFromImage(new ReadOnlyMemory<byte>(bytes), out length))
                            {
                                length += 8; // full image file's length.
                                if (length > bytes.Length)
                                {
                                    ArrayPool<byte>.Shared.Return(bytes);
                                    bytes = ArrayPool<byte>.Shared.Rent(length);
                                }
                                dataStream.Position = currentPos;
                                if (dataStream.Read(bytes, 0, length) == length)
                                {
                                    var mem = new ReadOnlyMemory<byte>(bytes, 0, length);
                                    return this.Decode(mem, options);
                                }
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        if (currentPos != -1)
                        {
                            dataStream.Position = currentPos;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(bytes);
                    }
                }
            }

            byte[] currentBuffer = ArrayPool<byte>.Shared.Rent(4096);
            var memBuffer = new ReadOnlyMemory<byte>(currentBuffer);
            try
            {
                int streamRead = dataStream.Read(currentBuffer, 0, currentBuffer.Length);
                if (streamRead > 0)
                {
                    var features = new WebPBitstreamFeatures();
                    if (this.webp.TryGetImageHeaderInfo(new ReadOnlyMemory<byte>(currentBuffer), ref features) == VP8StatusCode.VP8_STATUS_OK)
                    {
                        int width = features.width, height = features.height;
                        var decodedBuffer = this.webp.CreateDecodeBuffer();
                        var decidedPxFmt = Helper.DecideOutputPixelFormat(options.PixelFormat, features.has_alpha != 0);
                        decodedBuffer.colorspace = Helper.GetWebpPixelFormat(decidedPxFmt);
                        decodedBuffer.is_external_memory = 1;
                        decodedBuffer.width = width;
                        decodedBuffer.height = height;

                        var pixelFmt = Helper.GetPixelFormat(decidedPxFmt);
                        var result = new WriteableBitmap(width, height, 96, 96, pixelFmt, null);
                        result.Lock();
                        try
                        {
                            decodedBuffer.u.RGBA.rgba = result.BackBuffer;
                            decodedBuffer.u.RGBA.size = new UIntPtr((uint)(result.BackBufferStride * result.PixelHeight));
                            decodedBuffer.u.RGBA.stride = result.BackBufferStride;
                            using (var decoder = this.webp.CreateDecoder(ref decodedBuffer))
                            {
                                VP8StatusCode status = VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
                                while (streamRead != 0)
                                {
                                    status = decoder.AppendEncodedData(memBuffer.Slice(0, streamRead));
                                    // if (decoder.GetDecodedImage(out last_scanline_index, out var width, out var height, out var stride, out IntPtr pointer) == VP8StatusCode.VP8_STATUS_OK) { }
                                    if (status != VP8StatusCode.VP8_STATUS_SUSPENDED)
                                    {
                                        break;
                                    }
                                    streamRead = dataStream.Read(currentBuffer, 0, currentBuffer.Length);
                                }

                                if (status == VP8StatusCode.VP8_STATUS_OK)
                                {
                                    return result;
                                }
                                else
                                {
                                    throw new WebpDecodeException(status);
                                }
                            }
                        }
                        finally
                        {
                            result.Unlock();
                            this.webp.Free(ref decodedBuffer);
                        }
                    }
                    else
                    {
                        var decidedPxFmt = Helper.DecideOutputPixelFormat(options.PixelFormat, null);
                        using (var decoder = this.webp.CreateDecoderForRGBX(Helper.GetWebpPixelFormat(decidedPxFmt)))
                        {
                            int last_scanline_index = 0;
                            VP8StatusCode status = VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
                            while (streamRead != 0)
                            {
                                status = decoder.AppendEncodedData(memBuffer.Slice(0, streamRead));
                                // if (decoder.GetDecodedImage(out last_scanline_index, out var width, out var height, out var stride, out IntPtr pointer) == VP8StatusCode.VP8_STATUS_OK) { }
                                if (status != VP8StatusCode.VP8_STATUS_SUSPENDED)
                                {
                                    break;
                                }
                                streamRead = dataStream.Read(currentBuffer, 0, currentBuffer.Length);
                            }
                            if (status == VP8StatusCode.VP8_STATUS_OK)
                            {
                                status = decoder.GetDecodedImage(ref last_scanline_index, out var width, out var height, out var stride, out IntPtr pointer);
                                if (status == VP8StatusCode.VP8_STATUS_OK)
                                {
                                    var result = new WriteableBitmap(width, height, 96, 96, Helper.GetPixelFormat(decidedPxFmt), null);
                                    result.Lock();
                                    try
                                    {
                                        var backBufferSize = stride * height;
                                        unsafe
                                        {
                                            Buffer.MemoryCopy(pointer.ToPointer(), result.BackBuffer.ToPointer(), backBufferSize, backBufferSize);
                                        }
                                        return result;
                                    }
                                    finally
                                    {
                                        result.Unlock();
                                    }
                                }
                                else
                                {
                                    throw new WebpDecodeException(status);
                                }
                            }
                            else
                            {
                                throw new WebpDecodeException(status);
                            }
                        }
                    }
                }
                else
                {
                    throw new WebpDecodeException(VP8StatusCode.VP8_STATUS_BITSTREAM_ERROR);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(currentBuffer);
            }
        }

        /// <summary>Decodes Webp data buffer to <seealso cref="BitmapSource"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="BitmapSource"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public BitmapSource Decode(ReadOnlySpan<byte> data, WindowsDecoderOptions options)
        {
            unsafe
            {
                fixed (byte* b = data)
                {
                    return this.Decode(new IntPtr(b), data.Length, options);
                }
            }
        }

        /// <summary>Decodes Webp data buffer to <seealso cref="BitmapSource"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="BitmapSource"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public BitmapSource Decode(ReadOnlyMemory<byte> data, WindowsDecoderOptions options)
        {
            using (var pinned = data.Pin())
            {
                IntPtr pointer;
                unsafe
                {
                    pointer = new IntPtr(pinned.Pointer);
                }
                return this.Decode(pointer, data.Length, options);
            }
        }

        /// <summary>Decodes Webp data buffer to <seealso cref="BitmapSource"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="data_size">The size of the data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="BitmapSource"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public BitmapSource Decode(IntPtr data, int data_size, WindowsDecoderOptions options)
        {
            ReadOnlySpan<byte> buffer;
            unsafe
            {
                buffer = new ReadOnlySpan<byte>(data.ToPointer(), data_size);
            }
            var features = new WebPBitstreamFeatures();
            if (this.webp.TryGetImageHeaderInfo(buffer, ref features) == VP8StatusCode.VP8_STATUS_OK)
            {
                var height = features.height;
                var decidedPxFmt = Helper.DecideOutputPixelFormat(options.PixelFormat, features.has_alpha != 0);
                var wbm = new WriteableBitmap(features.width, height, 96, 96, Helper.GetPixelFormat(decidedPxFmt), null);
                wbm.Lock();
                try
                {
                    uint inputSize = Convert.ToUInt32(data_size),
                        outputSize = Convert.ToUInt32(wbm.BackBufferStride * height);
                    this.webp.DecodeRGB(data, new UIntPtr(inputSize), wbm.BackBuffer, new UIntPtr(outputSize), Helper.GetWebpPixelFormat(decidedPxFmt), options);
                }
                finally
                {
                    wbm.Unlock();
                }
                return wbm;
            }
            else
            {
                throw new WebpDecodeException(VP8StatusCode.VP8_STATUS_BITSTREAM_ERROR);
            }
        }

        /// <summary>Encodes <seealso cref="BitmapSource"/> to Webp image and write the result into <seealso cref="MemoryStream"/>.</summary>
        /// <param name="image">The image which will be used to encode to WebP image.</param>
        /// <param name="options">The encoder options for webp encoder.</param>
        /// <returns><seealso cref="MemoryStream"/> which contains the encoded webp image.</returns>
        /// <exception cref="WebpEncodeException">Thrown when the encoder has wrong options.</exception>
        public MemoryStream Encode(BitmapSource image, EncoderOptions options)
        {
            var memStream = new MemoryStream();
            this.Encode(image, memStream, options);
            return memStream;
        }

        /// <summary>Encodes <seealso cref="BitmapSource"/> to Webp image and write the result into the given stream.</summary>
        /// <param name="image">The image which will be used to encode to WebP image.</param>
        /// <param name="outputStream">The output stream to write the encoded webp data to.</param>
        /// <param name="options">The encoder options for webp encoder.</param>
        /// <exception cref="WebpEncodeException">Thrown when the encoder has wrong options.</exception>
        public void Encode(BitmapSource image, Stream outputStream, EncoderOptions options)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(Webp));
            }
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (image.IsDownloading)
            {
                throw new ArgumentException("The image is still being downloaded.", nameof(image));
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("The output stream must be writable.", nameof(outputStream));
            }
            BitmapSource src;
            if (image.Format == PixelFormats.Bgra32)
            {
                src = image;
                if (image is WriteableBitmap wbm)
                {
                    wbm.Lock();
                    try
                    {
                        var wrappedStream = new OutputStream(outputStream);
                        this.webp.EncodeRGB(wbm.BackBuffer, wbm.PixelWidth, wbm.PixelHeight, wbm.BackBufferStride, true, wrappedStream, options);
                    }
                    finally
                    {
                        wbm.Unlock();
                    }
                    return;
                }
            }
            else
            {
                src = new FormatConvertedBitmap(image, PixelFormats.Bgra32, image.Palette, 0d);
            }
            // BGRA32 is 32 bits-per-pixel => 4 bytes-per-pixel (1 byte = 8 bits) => Stride = Width (in pixels) * 4
            var stride = src.PixelWidth * 4;
            var bufferSize = stride * src.PixelHeight;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                if (src.CanFreeze)
                {
                    src.Freeze();
                }
                src.CopyPixels(buffer, stride, 0);
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                if (handle.IsAllocated)
                {
                    try
                    {
                        var wrappedStream = new OutputStream(outputStream);
                        this.webp.EncodeRGB(handle.AddrOfPinnedObject(), src.PixelWidth, src.PixelHeight, stride, true, wrappedStream, options);
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>Attempts to unload the underlying native webp library.</summary>
        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;

            if (this._shouldDisposeFactory)
            {
                this.webp.Dispose();
            }
        }
    }
}
