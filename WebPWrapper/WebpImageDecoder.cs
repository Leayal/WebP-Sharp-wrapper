using System;
using System.Buffers;
using System.Runtime.InteropServices;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>The image decoder that is used to progressively decode a Webp image.</summary>
    public sealed class WebpImageDecoder : IDisposable
    {
        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the RGB/A samples specified by '<paramref name="colorspace"/>' into a preallocated internal buffer.
        /// </summary>
        /// <remarks>
        /// Equivalent to <seealso cref="CreateDecoderForRGBX(ILibwebp, Colorspace, IntPtr, UIntPtr, int)"/>, with 'output_buffer' is NULL.
        /// Use <seealso cref="GetDecodedImage(ref int, out int, out int, out int, out IntPtr)"/> or <seealso cref="GetDecodedImage(ref int, out int, out int, out int, out ReadOnlySpan{byte})"/>
        /// to obtain the decoded data.
        /// </remarks>
        /// <exception cref="InvalidProgramException">Unknown error occured.</exception>
        /// <exception cref="NotSupportedException"><paramref name="colorspace"/> is not RGB(A) colorspace</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorspace"/> is not a valid value</exception>
        /// <returns><see cref="WebpImageDecoder"/></returns>
        public static WebpImageDecoder CreateDecoderForRGBX(ILibwebp library, Colorspace colorspace)
            => CreateDecoderForRGBX(library, colorspace, IntPtr.Zero, UIntPtr.Zero, 0);

        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the RGB/A samples specified by '<paramref name="colorspace"/>' into a preallocated
        /// buffer '<paramref name="output_buffer"/>'. The size of this buffer is at least
        /// '<paramref name="output_buffer_size"/>' and the stride (distance in bytes between two scanlines)
        /// is specified by '<paramref name="output_stride"/>'
        /// </summary>
        /// <remarks>
        /// Additionally, <paramref name="output_buffer"/> can be passed NULL in which case the output
        /// buffer will be allocated automatically when the decoding starts. The
        /// '<paramref name="colorspace"/>' is taken into account for allocating this buffer. All other
        /// parameters are ignored.
        /// </remarks>
        /// <exception cref="InvalidProgramException">Unknown error occured.</exception>
        /// <exception cref="NotSupportedException"><paramref name="colorspace"/> is not RGB(A) colorspace</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorspace"/> is not a valid value</exception>
        /// <returns><see cref="WebpImageDecoder"/></returns>
        public static WebpImageDecoder CreateDecoderForRGBX(ILibwebp library, Colorspace colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            WebpImageDecoder result;
            if (colorspace >= Colorspace.MODE_LAST)
            {
                throw new ArgumentOutOfRangeException(nameof(colorspace));
            }
            if (colorspace == Colorspace.MODE_YUVA || colorspace == Colorspace.MODE_YUV)
            {
                throw new NotSupportedException();
            }
            else
            {
                var ptr = library.WebPINewRGB(colorspace, output_buffer, output_buffer_size, output_stride);
                if (ptr == IntPtr.Zero)
                {
                    throw new Exception();
                }
                else
                {
                    result = new WebpImageDecoder(library, false);
                    result.decoder = ptr;
                }
            }
            if (result == null)
            {
                throw new InvalidProgramException();
            }
            return result;
        }

        private IntPtr decoder;
        private Libwebp webp;
        private ILibwebp iwebp;
        private bool disposed;

        private WebpImageDecoder(ILibwebp library, in bool dummy)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }
            this.disposed = false;
            if (!library.CanDecode)
            {
                throw new BadImageFormatException("The library does not support decoding.");
            }
            this.iwebp = library;
            if (library is Libwebp lib)
            {
                this.webp = lib;
                this.webp.IncreaseReferenceCount();
            }
            else
            {
                this.webp = null;
            }
        }

        /// <summary>Creates a new incremental decoder with default options (Output with MODE_RGB)</summary>
        public WebpImageDecoder(ILibwebp library) : this(library, false)
        {
            var ptr = library.WebPINewDecoder();
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            this.decoder = ptr;
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="library">The webp interface of the native library</param>
        /// <param name="input_buffer">The input buffer of the webp image data. Can be NULL</param>
        /// <param name="input_buffer_size">The size of the input buffer.</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>In case '<paramref name="input_buffer"/>' is NULL, '<paramref name="input_buffer_size"/>' is ignored</remarks>
        public WebpImageDecoder(ILibwebp library, IntPtr input_buffer, UIntPtr input_buffer_size, ref WebPDecoderConfig options) : this(library, false)
        {
            var ptr = library.WebPIDecode(input_buffer, input_buffer_size, ref options);
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            this.decoder = ptr;
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="library">The webp interface of the native library</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This constructor is the shortcut for <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, ref WebPDecoderConfig)"/> with 'input_buffer' is NULL</remarks>
        public WebpImageDecoder(ILibwebp library, ref WebPDecoderConfig options) : this(library, IntPtr.Zero, UIntPtr.Zero, ref options) { }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="library">The webp interface of the native library</param>
        /// <param name="input_buffer">The input buffer of the webp image data. Can be NULL</param>
        /// <param name="input_buffer_size">The size of the input buffer.</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This is an alternative to <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, ref WebPDecoderConfig)"/>. In case '<paramref name="input_buffer"/>' is NULL, '<paramref name="input_buffer_size"/>' is ignored</remarks>
        public WebpImageDecoder(ILibwebp library, IntPtr input_buffer, UIntPtr input_buffer_size, DecoderOptions options) : this(library, false)
        {
            var config = new WebPDecoderConfig();
            options.ApplyOptions(ref config);
            var ptr = library.WebPIDecode(input_buffer, input_buffer_size, ref config);
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            this.decoder = ptr;
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="library">The webp interface of the native library</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This constructor is the shortcut for <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, DecoderOptions)"/> with 'input_buffer' is NULL</remarks>
        public WebpImageDecoder(ILibwebp library, DecoderOptions options) : this(library, IntPtr.Zero, UIntPtr.Zero, options) { }

        /// <summary>
        /// Creates a new incremental decoder with the supplied buffer parameter
        /// </summary>
        /// <param name="library">The libwebp interface obtained from <seealso cref="Libwebp.Init(string)"/></param>
        /// <param name="output_buffer">The data of <see cref="WebPDecBuffer"/> to create decoder</param>
        /// <remarks>
        /// The supplied 'output_buffer' content MUST NOT be changed between calls to
        /// WebPIAppend() or WebPIUpdate() unless 'output_buffer.is_external_memory' is
        /// not set to 0. In such a case, it is allowed to modify the pointers, size and
        /// stride of output_buffer.u.RGBA or output_buffer.u.YUVA, provided they remain
        /// within valid bounds.
        /// All other fields of WebPDecBuffer MUST remain constant between calls.
        /// </remarks>
        public WebpImageDecoder(ILibwebp library, ref WebPDecBuffer output_buffer) : this(library, false)
        {
            var ptr = library.WebPINewDecoder(ref output_buffer);
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException(nameof(output_buffer));
            }
            this.decoder = ptr;
        }

        /// <summary>Copies and decodes the data once the next data is available</summary>
        /// <param name="data">The input data to be copied</param>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode AppendEncodedData(ReadOnlySpan<byte> data)
        {
            this.ThrowIfDisposed();

            var result = VP8StatusCode.VP8_STATUS_INVALID_PARAM;
            unsafe
            {
                fixed (byte* b = data)
                {
                    result = this.iwebp.WebPIAppend(this.decoder, new IntPtr(b), new UIntPtr(Convert.ToUInt32(data.Length)));
                }
            }

            return result;
        }

        /// <summary>Copies and decodes the data once the next data is available</summary>
        /// <param name="data">The input data to be copied</param>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode AppendEncodedData(ReadOnlyMemory<byte> data)
        {
            this.ThrowIfDisposed();

            var result = VP8StatusCode.VP8_STATUS_INVALID_PARAM;
            using (var pinned = data.Pin())
            {
                IntPtr pointer;
                unsafe
                {
                    pointer = new IntPtr(pinned.Pointer);
                }
                result = this.iwebp.WebPIAppend(this.decoder, pointer, new UIntPtr(Convert.ToUInt32(data.Length)));
            }
            return result;
        }

        /// <summary>Map memory and decodes the data once the next data is available</summary>
        /// <param name="data">The input data to be re-mapped</param>
        /// <param name="data_size">The size of the input data to be re-mapped</param>
        /// <remarks>The mapped memory must be pinned (not moved by GC) until the image data is decoded.</remarks>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode UpdateEncodedData(IntPtr data, int data_size)
        {
            this.ThrowIfDisposed();

            var result = this.iwebp.WebPIUpdate(this.decoder, data, new UIntPtr(Convert.ToUInt32(data_size)));
            return result;
        }

        /// <summary>Map memory and decodes the data once the next data is available</summary>
        /// <param name="data">The input data to be re-mapped</param>
        /// <param name="data_size">The size of the input data to be re-mapped</param>
        /// <remarks>The mapped memory must be pinned (not moved by GC) until the image data is decoded.</remarks>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode UpdateEncodedData(MemoryHandle data, int data_size)
        {
            IntPtr pointer;
            unsafe
            {
                pointer = new IntPtr(data.Pointer);
            }
            return this.UpdateEncodedData(pointer, data_size);
        }

        /// <summary>Gets the RGB/A of the whole decoded image.</summary>
        /// <param name="last_y">The index of last decoded row in raster scan order</param>
        /// <param name="width">The width of the decoded image</param>
        /// <param name="height">The height of the decoded image</param>
        /// <param name="stride">The stride of the decoded image</param>
        /// <param name="buffer">The buffer of the decoded image data</param>
        /// <returns>
        /// Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if successful.
        /// Otherwise <seealso cref="VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA"/> in case the decode doesn't have enough data to decode.
        /// </returns>
        public VP8StatusCode GetDecodedImage(ref int last_y, out int width, out int height, out int stride, out ReadOnlySpan<byte> buffer)
        {
            this.ThrowIfDisposed();

            int _width = 0, _height = 0, _stride = 0;
            var ptr = this.iwebp.WebPIDecGetRGB(this.decoder, ref last_y, ref _width, ref _height, ref _stride);
            if (ptr == IntPtr.Zero)
            {
                width = 0;
                height = 0;
                stride = 0;
                buffer = ReadOnlySpan<byte>.Empty;
                return VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
            }
            else
            {
                width = _width;
                height = _height;
                stride = _stride;
                unsafe
                {
                    buffer = new ReadOnlySpan<byte>(ptr.ToPointer(), stride * height);
                }
                return VP8StatusCode.VP8_STATUS_OK;
            }
        }

        /// <summary>Gets the RGB/A of the whole decoded image.</summary>
        /// <param name="last_y">The index of last decoded row in raster scan order</param>
        /// <param name="width">The width of the decoded image</param>
        /// <param name="height">The height of the decoded image</param>
        /// <param name="stride">The stride of the decoded image</param>
        /// <param name="backBufferPointer">The pointer to the backbuffer's memory.</param>
        /// <returns>
        /// Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if successful.
        /// Otherwise <seealso cref="VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA"/> in case the decode doesn't have enough data to decode.
        /// </returns>
        public VP8StatusCode GetDecodedImage(ref int last_y, out int width, out int height, out int stride, out IntPtr backBufferPointer)
        {
            this.ThrowIfDisposed();

            int _width = 0, _height = 0, _stride = 0;
            var ptr = this.iwebp.WebPIDecGetRGB(this.decoder, ref last_y, ref _width, ref _height, ref _stride);
            if (ptr == IntPtr.Zero)
            {
                last_y = 0;
                width = 0;
                height = 0;
                stride = 0;
                backBufferPointer = IntPtr.Zero;
                return VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
            }
            else
            {
                width = _width;
                height = _height;
                stride = _stride;
                backBufferPointer = ptr;
                return VP8StatusCode.VP8_STATUS_OK;
            }
        }

        /// <summary>Gets the displayable data decoded so far.</summary>
        /// <param name="left">The rectangle's left the displayable area.</param>
        /// <param name="top">The rectangle's top the displayable area.</param>
        /// <param name="width">The rectangle's width the displayable area.</param>
        /// <param name="height">The rectangle's height the displayable area.</param>
        /// <param name="decodedData">The structure contains the displayable data.</param>
        /// <returns>
        /// Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if successful.
        /// Otherwise <seealso cref="VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA"/> in case the decode doesn't have enough data to decode or the decoder is in invalid state for this operation.
        /// </returns>
        public VP8StatusCode GetDecodedImage(out int left, out int top, out int width, out int height, out WebPDecodedDataBuffer decodedData)
        {
            this.ThrowIfDisposed();

            int _left = 0, _width = 0, _height = 0, _top = 0;
            var ptr = this.webp.WebPIDecodedArea(this.decoder, ref _left, ref _top, ref _width, ref _height);
            if (ptr == IntPtr.Zero)
            {
                left = 0;
                width = 0;
                height = 0;
                top = 0;
                decodedData = default;
                return VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
            }
            else
            {
                left = _left;
                width = _width;
                height = _height;
                top = _top;
                // decodedData = (WebPDecBuffer)Marshal.PtrToStructure(ptr, typeof(WebPDecBuffer));
                decodedData = Marshal.PtrToStructure<WebPDecodedDataBuffer>(ptr);
                return VP8StatusCode.VP8_STATUS_OK;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>Release all resources allocated by this <see cref="WebpImageDecoder"/> instance.</summary>
        /// <remarks>Safe to call even when the passed <see cref="WebPDecBuffer.is_external_memory"/> is non-zero. In this case, the external memory will not be freed.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Destructor. Used by GC</summary>
        ~WebpImageDecoder()
        {
            this.Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed) return;
            this.disposed = true;

            this.iwebp.WebPIDelete(this.decoder);
            this.iwebp = null;

            if (this.webp != null)
            {
                this.webp.DecreaseReferenceCount();
                this.webp = null;
            }
        }
    }
}
