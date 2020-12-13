using System;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>The image decoder that is used to progressively decode a Webp image.</summary>
    public unsafe sealed class WebpImageDecoder : IDisposable
    {
        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the RGB/A samples specified by '<paramref name="colorspace"/>' into a preallocated internal buffer.
        /// </summary>
        /// <remarks>
        /// Equivalent to <seealso cref="CreateDecoderForRGBX(in ILibwebp, WEBP_CSP_MODE, IntPtr, UIntPtr, in int)"/>, with 'output_buffer' is NULL.
        /// Use <seealso cref="GetDecodedImage(out int, out int, out int, out int, out IntPtr)"/> or <seealso cref="GetDecodedImage(out int, out int, out int, out int, out ReadOnlySpan{byte})"/>
        /// to obtain the decoded data.
        /// </remarks>
        /// <exception cref="InvalidProgramException">Unknown error occured.</exception>
        /// <exception cref="NotSupportedException"><paramref name="colorspace"/> is not RGB(A) colorspace</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorspace"/> is not a valid value</exception>
        /// <returns><see cref="WebpImageDecoder"/></returns>
        public static WebpImageDecoder CreateDecoderForRGBX(in ILibwebp library, WEBP_CSP_MODE colorspace)
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
        public static WebpImageDecoder CreateDecoderForRGBX(in ILibwebp library, WEBP_CSP_MODE colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, in int output_stride)
        {
            WebpImageDecoder result;
            if (colorspace >= WEBP_CSP_MODE.MODE_LAST)
            {
                throw new ArgumentOutOfRangeException(nameof(colorspace));
            }
            if (colorspace == WEBP_CSP_MODE.MODE_YUVA || colorspace == WEBP_CSP_MODE.MODE_YUV)
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
        private bool hasHeader;
        private WebPBitstreamFeatures imageHeader;

        private WebpImageDecoder(ILibwebp library, bool dummy)
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
            this.hasHeader = false;
            this.imageHeader = new WebPBitstreamFeatures();
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

        /// <summary>Creates a new incremental decoder with default settings (Output with MODE_RGB)</summary>
        public WebpImageDecoder(in ILibwebp library) : this(library, false)
        {
            var ptr = library.WebPINewDecoder();
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            this.decoder = ptr;
        }

        /// <summary>
        /// Creates a new incremental decoder with the supplied buffer parameter
        /// </summary>
        /// <param name="output_buffer">The data of <see cref="WebPDecBuffer"/> to create decoder</param>
        /// <remarks>
        /// The supplied 'output_buffer' content MUST NOT be changed between calls to
        /// WebPIAppend() or WebPIUpdate() unless 'output_buffer.is_external_memory' is
        /// not set to 0. In such a case, it is allowed to modify the pointers, size and
        /// stride of output_buffer.u.RGBA or output_buffer.u.YUVA, provided they remain
        /// within valid bounds.
        /// All other fields of WebPDecBuffer MUST remain constant between calls.
        /// </remarks>
        public WebpImageDecoder(in ILibwebp library, ref WebPDecBuffer output_buffer) : this(library, false)
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
        public VP8StatusCode AppendEncodedData(in ReadOnlySpan<byte> data)
        {
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
        public VP8StatusCode AppendEncodedData(in ReadOnlyMemory<byte> data)
        {
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

        /// <summary>
        /// Returns the RGB/A image decoded so far.
        /// </summary>
        /// <param name="last_y">The index of last decoded row in raster scan order</param>
        /// <param name="width">The width of the decoded image</param>
        /// <param name="height">The height of the decoded image</param>
        /// <param name="stride">The stride of the decoded image</param>
        /// <param name="buffer">The buffer of the decoded image data</param>
        /// <returns>
        /// Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if successful.
        /// Otherwise <seealso cref="VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA"/> in case the decode doesn't have enough data to decode.
        /// </returns>
        public VP8StatusCode GetDecodedImage(out int last_y, out int width, out int height, out int stride, out ReadOnlySpan<byte> buffer)
        {
            int _last_y = 0, _width = 0, _height = 0, _stride = 0;
            var ptr = this.iwebp.WebPIDecGetRGB(this.decoder, ref _last_y, ref _width, ref _height, ref _stride);
            if (ptr == IntPtr.Zero)
            {
                last_y = 0;
                width = 0;
                height = 0;
                stride = 0;
                buffer = ReadOnlySpan<byte>.Empty;
                return VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
            }
            else
            {
                last_y = _last_y;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="last_y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="backBufferPointer"></param>
        /// <returns></returns>
        public VP8StatusCode GetDecodedImage(out int last_y, out int width, out int height, out int stride, out IntPtr backBufferPointer)
        {
            int _last_y = 0, _width = 0, _height = 0, _stride = 0;
            var ptr = this.iwebp.WebPIDecGetRGB(this.decoder, ref _last_y, ref _width, ref _height, ref _stride);
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
                last_y = _last_y;
                width = _width;
                height = _height;
                stride = _stride;
                backBufferPointer = ptr;
                WebPIDecoder a;
                return VP8StatusCode.VP8_STATUS_OK;
            }
        }

        /// <summary>Release all resources allocated by this <see cref="WebpImageDecoder"/> instance.</summary>
        /// <remarks>Safe to call even when the passed <see cref="WebPDecBuffer.is_external_memory"/> is non-zero. In this case, the external memory will not be freed.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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
