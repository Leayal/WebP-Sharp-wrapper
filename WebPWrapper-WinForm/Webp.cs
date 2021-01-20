using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using WebPWrapper;
using WebPWrapper.LowLevel;

namespace WebPWrapper.WinForms
{
    /// <summary>Simple webp wrapper for Windows Forms.</summary>
    public class Webp : IDisposable
    {
        private static readonly byte[] WEBP_CONTAINER_HEADER_1 = System.Text.Encoding.ASCII.GetBytes("RIFF"),
                                       WEBP_CONTAINER_HEADER_2 = System.Text.Encoding.ASCII.GetBytes("WEBP");

        private readonly WebpFactory webp;
        private bool disposed;

        /// <summary>Initialize a new <see cref="Webp"/> instance with the given library path.</summary>
        /// <param name="libraryPath">The file path to the native webp library.</param>
        public Webp(string libraryPath) : this(new WebpFactory(libraryPath)) { }

        /// <summary>Initialize a new <see cref="Webp"/> instance with the given <seealso cref="WebpFactory"/>.</summary>
        public Webp(WebpFactory factory)
        {
            this.disposed = false;
            this.webp = factory;
        }

        /// <summary>Decodes Webp data stream to <seealso cref="Bitmap"/>.</summary>
        /// <param name="dataStream">The data stream which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="Bitmap"/> which contains the image data.</returns>
        /// <remarks>Incomplete API. Therefore, in case when decoder is progressive, only <seealso cref="WindowsDecoderOptions.PixelFormat"/> will be used, all other options will be ignored.</remarks>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the stream contains invalid data for Webp Image.</exception>
        public Bitmap Decode(Stream dataStream, WindowsDecoderOptions options)
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
                            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(bytes, 0, 4),
                                               header = new ReadOnlySpan<byte>(WEBP_CONTAINER_HEADER_1);
                            if (span.SequenceEqual(header))
                            {
                                span = new ReadOnlySpan<byte>(bytes, 8, 4);
                                header = new ReadOnlySpan<byte>(WEBP_CONTAINER_HEADER_2);
                                if (span.SequenceEqual(header))
                                {
                                    length = BitConverter.ToInt32(bytes, 4) + 8;
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
                    var bitstreaminfo = new WebPBitstreamFeatures();
                    if (this.webp.TryGetImageHeaderInfo(memBuffer.Slice(0, streamRead), ref bitstreaminfo) == VP8StatusCode.VP8_STATUS_OK)
                    {
                        var width = bitstreaminfo.width;
                        var height = bitstreaminfo.height;
                        var decidedPxFmt = Helper.DecideOutputPixelFormat(options.PixelFormat, bitstreaminfo.has_alpha != 0);
                        var decodedBuffer = this.webp.CreateDecodeBuffer();
                        decodedBuffer.colorspace = Helper.GetWebpPixelFormat(decidedPxFmt);
                        decodedBuffer.is_external_memory = 1;
                        decodedBuffer.width = width;
                        decodedBuffer.height = height;

                        var pixelFmt = Helper.GetPixelFormat(decidedPxFmt);
                        var result = new Bitmap(width, height, pixelFmt);
                        var lockedData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, pixelFmt);
                        try
                        { 
                            decodedBuffer.u.RGBA.rgba = lockedData.Scan0;
                            decodedBuffer.u.RGBA.size = new UIntPtr((uint)(lockedData.Stride * lockedData.Height));
                            decodedBuffer.u.RGBA.stride = lockedData.Stride;
                            using (var decoder = this.webp.CreateDecoder(ref decodedBuffer))
                            {
                                int last_scanline_index = 0;
                                VP8StatusCode status = VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
                                while (streamRead != 0)
                                {
                                    status = decoder.AppendEncodedData(memBuffer.Slice(0, streamRead));
                                    if (decoder.GetDecodedImage(ref last_scanline_index, out var decodedwidth, out var decodedheight, out var stride, out IntPtr pointer) == VP8StatusCode.VP8_STATUS_OK)
                                    {

                                    }
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
                                    result.Dispose();
                                    throw new WebpDecodeException(status);
                                }
                            }
                        }
                        catch (Exception ex) when (!(ex is WebpDecodeException))
                        {
                            result.Dispose();
                            throw;
                        }
                        finally
                        {
                            result.UnlockBits(lockedData);
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
                                    var pixelFmt = Helper.GetPixelFormat(decidedPxFmt);
                                    var result = new Bitmap(width, height, pixelFmt);
                                    var lockedData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFmt);
                                    try
                                    {
                                        var backBufferSize = stride * height;
                                        unsafe
                                        {
                                            Buffer.MemoryCopy(pointer.ToPointer(), lockedData.Scan0.ToPointer(), backBufferSize, backBufferSize);
                                        }
                                        return result;
                                    }
                                    catch
                                    {
                                        result.Dispose();
                                        throw;
                                    }
                                    finally
                                    {
                                        result.UnlockBits(lockedData);
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

        /// <summary>Decodes Webp data buffer to <seealso cref="Bitmap"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="Bitmap"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public Bitmap Decode(ReadOnlySpan<byte> data, WindowsDecoderOptions options)
        {
            unsafe
            {
                fixed (byte* b = data)
                {
                    return this.Decode(new IntPtr(b), data.Length, options);
                }
            }
        }

        /// <summary>Decodes Webp data buffer to <seealso cref="Bitmap"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="Bitmap"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public Bitmap Decode(ReadOnlyMemory<byte> data, WindowsDecoderOptions options)
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

        /// <summary>Decodes Webp data buffer to <seealso cref="Bitmap"/>.</summary>
        /// <param name="data">The data buffer which contains WebP image.</param>
        /// <param name="data_size">The size of the data buffer which contains WebP image.</param>
        /// <param name="options">The decoder options for webp decoder.</param>
        /// <returns><seealso cref="Bitmap"/> which contains the image data.</returns>
        /// <exception cref="WebpDecodeException">Thrown when the decoder has wrong options or the buffer contains invalid data for Webp Image.</exception>
        public Bitmap Decode(IntPtr data, int data_size, WindowsDecoderOptions options)
        {
            ReadOnlySpan<byte> buffer;
            unsafe
            {
                buffer = new ReadOnlySpan<byte>(data.ToPointer(), data_size);
            }
            var bitstreaminfo = new WebPBitstreamFeatures();
            if (this.webp.TryGetImageHeaderInfo(buffer, ref bitstreaminfo) == VP8StatusCode.VP8_STATUS_OK)
            {
                var width = bitstreaminfo.width;
                var height = bitstreaminfo.height;
                var decidedPxFmt = Helper.DecideOutputPixelFormat(options.PixelFormat, bitstreaminfo.has_alpha != 0);
                var pixelFormat = Helper.GetPixelFormat(decidedPxFmt);
                var bm = new Bitmap(width, height, pixelFormat);
                var lockedData = bm.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
                try
                {
                    uint inputSize = Convert.ToUInt32(data_size),
                        outputSize = Convert.ToUInt32(lockedData.Stride * height);
                    this.webp.DecodeRGB(data, new UIntPtr(inputSize), lockedData.Scan0, new UIntPtr(outputSize), Helper.GetWebpPixelFormat(decidedPxFmt), options);
                }
                catch
                {
                    bm.Dispose();
                    throw;
                }
                finally
                {
                    bm.UnlockBits(lockedData);
                }
                return bm;
            }
            else
            {
                throw new WebpDecodeException(VP8StatusCode.VP8_STATUS_BITSTREAM_ERROR);
            }
        }

        /// <summary>Encodes <seealso cref="Image"/> to Webp image and write the result into <seealso cref="MemoryStream"/>.</summary>
        /// <param name="image">The image which will be used to encode to WebP image.</param>
        /// <param name="options">The encoder options for webp encoder.</param>
        /// <returns><seealso cref="MemoryStream"/> which contains the encoded webp image.</returns>
        /// <exception cref="WebpEncodeException">Thrown when the encoder has wrong options.</exception>
        public MemoryStream Encode(Image image, EncoderOptions options)
        {
            var memStream = new MemoryStream();
            this.Encode(image, memStream, options);
            return memStream;
        }

        /// <summary>Encodes <seealso cref="Image"/> to Webp image and write the result into the given stream.</summary>
        /// <param name="image">The image which will be used to encode to WebP image.</param>
        /// <param name="outputStream">The output stream to write the encoded webp data to.</param>
        /// <param name="options">The encoder options for webp encoder.</param>
        /// <exception cref="WebpEncodeException">Thrown when the encoder has wrong options.</exception>
        public void Encode(Image image, Stream outputStream, EncoderOptions options)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(Webp));
            }
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("The output stream must be writable.", nameof(outputStream));
            }

            bool shouldDispose = false;
            Bitmap bm = image as Bitmap;
            if (bm == null)
            {
                shouldDispose = true;
                if (image.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    bm = new Bitmap(image);
                }
                else
                {
                    // using (var tmpBm = new Bitmap(image)) bm = tmpBm.Clone(new Rectangle(0, 0, tmpBm.Width, tmpBm.Height), PixelFormat.Format32bppArgb);
                    bm = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                    using (var graphic = Graphics.FromImage(bm))
                    {
                        graphic.DrawImageUnscaled(image, 0, 0);
                        graphic.Flush();
                    }
                }
            }
            else
            {
                if (bm.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    var oldBm = bm;
                    shouldDispose = true;
                    bm = oldBm.Clone(new Rectangle(0, 0, bm.Width, bm.Height), PixelFormat.Format32bppArgb);
                }
            }

            try
            {
                var lockedData = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var wrappedStream = new OutputStream(outputStream);
                    this.webp.EncodeRGB(lockedData.Scan0, lockedData.Width, lockedData.Height, lockedData.Stride, true, wrappedStream, options);
                }
                finally
                {
                    bm.UnlockBits(lockedData);
                }
            }
            finally
            {
                if (shouldDispose)
                {
                    bm.Dispose();
                }
            }
        }

        /// <summary>Attempts to unload the underlying native webp library.</summary>
        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;

            this.webp.Dispose();
        }
    }
}
