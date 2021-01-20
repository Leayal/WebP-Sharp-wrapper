using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>
    /// Webp factory for convenience in creating encoder/decoder/buffers.
    /// </summary>
    public sealed class WebpFactory : IDisposable
    {
        private ILibwebp library;
        private bool disposed;

        #region | Constructors |
        /// <summary>
        /// Initialize a new instance of <see cref="WebpFactory"/>.
        /// </summary>
        /// <param name="libraryPath">The path to the native library.</param>
        public WebpFactory(string libraryPath)
        {
            this.disposed = false;
            this.library = Libwebp.Init(libraryPath);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="WebpFactory"/> from loaded native library.
        /// </summary>
        /// <param name="library">The native library.</param>
        /// <exception cref="ArgumentException">Throws when the given native library is not from <seealso cref="Libwebp.Init(string)"/>.</exception>
        public WebpFactory(ILibwebp library)
        {
            this.disposed = false;
            if (library is Libwebp lib)
            {
                lib.IncreaseReferenceCount();
                this.library = library;
            }
            else
            {
                throw new ArgumentException(nameof(library));
            }
        }
        #endregion

        #region | Public Properties |
        /// <summary>Gets a boolean whether the loaded native library supports decoding.</summary>
        public bool CanDecode => this.library.CanDecode;

        /// <summary>Gets a boolean whether the loaded native library supports encoding.</summary>
        public bool CanEncode => this.library.CanEncode;
        #endregion

        #region | Public Decompress Methods |
        /// <summary>Creates a new incremental decoder with default settings (Output with MODE_RGB)</summary>
        public WebpImageDecoder CreateDecoder()
        {
            this.ThrowIfDisposed();
            return new WebpImageDecoder(this.library);
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
        public WebpImageDecoder CreateDecoder(ref WebPDecBuffer output_buffer)
        {
            this.ThrowIfDisposed();
            return new WebpImageDecoder(this.library, ref output_buffer);
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="input_buffer">The input buffer of the webp image data. Can be NULL</param>
        /// <param name="input_buffer_size">The size of the input buffer.</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>In case '<paramref name="input_buffer"/>' is NULL, '<paramref name="input_buffer_size"/>' is ignored</remarks>
        public WebpImageDecoder CreateDecoder(IntPtr input_buffer, UIntPtr input_buffer_size, ref WebPDecoderConfig options)
        {
            this.ThrowIfDisposed();
            return new WebpImageDecoder(this.library, input_buffer, input_buffer_size, ref options);
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This constructor is the shortcut for <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, ref WebPDecoderConfig)"/> with 'input_buffer' is NULL</remarks>
        public WebpImageDecoder CreateDecoder(ref WebPDecoderConfig options) => this.CreateDecoder(IntPtr.Zero, UIntPtr.Zero, ref options);

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="input_buffer">The input buffer of the webp image data. Can be NULL</param>
        /// <param name="input_buffer_size">The size of the input buffer.</param>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This is an alternative to <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, ref WebPDecoderConfig)"/>. In case '<paramref name="input_buffer"/>' is NULL, '<paramref name="input_buffer_size"/>' is ignored</remarks>
        public WebpImageDecoder CreateDecoder(IntPtr input_buffer, UIntPtr input_buffer_size, DecoderOptions options)
        {
            this.ThrowIfDisposed();
            return new WebpImageDecoder(this.library, input_buffer, input_buffer_size, options);
        }

        /// <summary>Creates a new incremental decoder with the given options and input buffer</summary>
        /// <param name="options">The decoder options.</param>
        /// <remarks>This constructor is the shortcut for <seealso cref="WebpImageDecoder(ILibwebp, IntPtr, UIntPtr, DecoderOptions)"/> with 'input_buffer' is NULL</remarks>
        public WebpImageDecoder CreateDecoder(DecoderOptions options) => this.CreateDecoder(IntPtr.Zero, UIntPtr.Zero, options);

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
        public WebpImageDecoder CreateDecoderForRGBX(Colorspace colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            this.ThrowIfDisposed();
            return WebpImageDecoder.CreateDecoderForRGBX(this.library, colorspace, output_buffer, output_buffer_size, output_stride);
        }

        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the RGB/A samples specified by '<paramref name="colorspace"/>' into a preallocated internal buffer.
        /// </summary>
        /// <remarks>
        /// Equivalent to <seealso cref="CreateDecoderForRGBX(Colorspace, IntPtr, UIntPtr, int)"/>, with 'output_buffer' is NULL.
        /// Use <seealso cref="WebpImageDecoder.GetDecodedImage(ref int, out int, out int, out int, out IntPtr)"/> or <seealso cref="WebpImageDecoder.GetDecodedImage(ref int, out int, out int, out int, out ReadOnlySpan{byte})"/>
        /// to obtain the decoded data.
        /// </remarks>
        /// <exception cref="InvalidProgramException">Unknown error occured.</exception>
        /// <exception cref="NotSupportedException"><paramref name="colorspace"/> is not RGB(A) colorspace</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorspace"/> is not a valid value</exception>
        /// <returns><see cref="WebpImageDecoder"/></returns>
        public WebpImageDecoder CreateDecoderForRGBX(Colorspace colorspace)
        {
            this.ThrowIfDisposed();
            return WebpImageDecoder.CreateDecoderForRGBX(this.library, colorspace);
        }

        /// <summary>Initialize a new empty <see cref="WebPDecBuffer"/> structure.</summary>
        /// <exception cref="BadImageFormatException">Version mismatch</exception>
        /// <remarks>
        /// Must be called before any other use in case the decoder doesn't initialize one internally.
        /// </remarks>
        public WebPDecBuffer CreateDecodeBuffer()
        {
            this.ThrowIfDisposed();
            var output_buffer = new WebPDecBuffer();

            // Unnecessary but welp, check for version anyway.
            if (!this.library.WebPInitDecBuffer(ref output_buffer))
            {
                throw new BadImageFormatException("Version mismatch.");
            }

            return output_buffer;
        }

        /// <summary>Initialize a new empty <see cref="WebPDecoderConfig"/> structure.</summary>
        /// <exception cref="BadImageFormatException">Version mismatch</exception>
        /// <remarks>
        /// Must be called before any other use in case the decoder doesn't initialize one internally.
        /// </remarks>
        public WebPDecoderConfig CreateDecoderConfig()
        {
            this.ThrowIfDisposed();
            var output_buffer = new WebPDecoderConfig();

            // Unnecessary but welp, check for version anyway.
            if (this.library.WebPInitDecoderConfig(ref output_buffer) == 0)
            {
                throw new BadImageFormatException("Version mismatch.");
            }

            return output_buffer;
        }

        /// <summary>Decode webp data into pixel buffer.</summary>
        /// <param name="input_buffer">The pointer to the memory contains the webp data.</param>
        /// <param name="input_buffer_size">The size of the memory contains the webp data.</param>
        /// <param name="output_colorspace">The colorspace (or pixel format) of the output pixel buffer.</param>
        /// <param name="output_buffer">The memory to write the output pixel data.</param>
        /// <param name="output_buffer_size">The size of the memory to write the output pixel data.</param>
        /// <param name="options">The options which will be used to configure the decoder.</param>
        /// <remarks>The memory must be pinned until the decoding operation finishes.</remarks>
        /// <exception cref="WebpDecodeException">Error occured during decoding operation in the native library. Check the <seealso cref="WebpDecodeException.ErrorCode"/> to see the error.</exception>
        public void DecodeRGB(IntPtr input_buffer, UIntPtr input_buffer_size, IntPtr output_buffer, UIntPtr output_buffer_size, Colorspace output_colorspace, DecoderOptions options)
        {
            int bpp;
            switch (output_colorspace)
            {
                case Colorspace.MODE_ARGB:
                case Colorspace.MODE_Argb:
                case Colorspace.MODE_BGRA:
                case Colorspace.MODE_bgrA:
                case Colorspace.MODE_RGBA:
                case Colorspace.MODE_rgbA:
                    bpp = 4;
                    break;
                case Colorspace.MODE_RGB:
                case Colorspace.MODE_BGR:
                    bpp = 3;
                    break;
                case Colorspace.MODE_LAST:
                    throw new ArgumentException(nameof(output_colorspace));
                default:
                    throw new NotSupportedException();
            }

            var decodeConf = new WebPDecoderConfig();
            VP8StatusCode errorCode;
            // this.library.WebPInitDecoderConfig(ref decodeConf) != 0
            errorCode = this.library.WebPGetFeatures(input_buffer, input_buffer_size, ref decodeConf.input);
            if (errorCode != VP8StatusCode.VP8_STATUS_OK)
            {
                throw new WebpDecodeException(errorCode);
            }
            try
            {
                int width, height;

                var opts = options ?? DecoderOptions.Default;
                opts.ApplyOptions(ref decodeConf);
                decodeConf.output.colorspace = output_colorspace;
                decodeConf.output.is_external_memory = 1;

                if (decodeConf.options.use_scaling != 0)
                {
                    width = decodeConf.options.scaled_width;
                    height = decodeConf.options.scaled_height;
                }
                else if (decodeConf.options.use_cropping != 0)
                {
                    width = decodeConf.options.crop_width;
                    height = decodeConf.options.crop_height;
                }
                else
                {
                    width = decodeConf.input.width;
                    height = decodeConf.input.height;
                }

                decodeConf.output.width = width;
                decodeConf.output.height = height;

                decodeConf.output.u.RGBA.rgba = output_buffer;
                decodeConf.output.u.RGBA.size = output_buffer_size;
                if (bpp == 4)
                {
                    decodeConf.output.u.RGBA.stride = width * 4;
                }
                else
                {
                    decodeConf.output.u.RGBA.stride = (width * bpp) + (width % 4);
                }

                errorCode = this.library.WebPDecode(input_buffer, input_buffer_size, ref decodeConf);
                if (errorCode != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new WebpDecodeException(errorCode);
                }
            }
            finally
            {
                this.library.WebPFreeDecBuffer(ref decodeConf.output);
            }
        }

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="output_buffer">The <seealso cref="WebPDecBuffer"/> to free the associated memory.</param>
        /// <remarks>External memory will not be touched.</remarks>
        public void Free(ref WebPDecBuffer output_buffer)
        {
            this.ThrowIfDisposed();
            this.library.WebPFreeDecBuffer(ref output_buffer);
        }

        /// <summary>Free any memory associated with the buffer. Must always be called last.</summary>
        /// <param name="decoderConfig">The <seealso cref="WebPDecoderConfig"/> to free the associated memory.</param>
        /// <remarks>External memory will not be touched.</remarks>
        public void Free(ref WebPDecoderConfig decoderConfig)
        {
            this.ThrowIfDisposed();
            this.library.WebPFreeDecBuffer(ref decoderConfig.output);
        }
        #endregion

        #region | Public Compress Methods |
        /// <summary>Encode RGB/BGR pixel data into a webp picture.</summary>
        /// <param name="data">The memory pointer to of the pixel buffer.</param>
        /// <param name="pixelWidth">The width (in pixel) of the buffer.</param>
        /// <param name="pixelHeight">The height (in pixel) of the buffer.</param>
        /// <param name="stride">The stride (in bytes) of the pixel buffer.</param>
        /// <param name="isBGR">Determine whether the input data is BGR or RGB.</param>
        /// <param name="outputStream">The output interface which will be used to written the encoded webp picture.</param>
        /// <param name="options">The options which will be used to configure the encoder.</param>
        /// <remarks>If the data contains BGRA (or ARGB for Big Endian machines), the buffer will be used directly instead of being copied and converted into BGRA (or ARGB for Big Endian machines).</remarks>
        /// <exception cref="WebpEncodeException">Error occured during decoding operation in the native library. Check the <seealso cref="WebpEncodeException.ErrorCode"/> to see the error.</exception>
        public void EncodeRGB(IntPtr data, int pixelWidth, int pixelHeight, int stride, bool isBGR, IOutputStream outputStream, EncoderOptions options)
        {
            this.ThrowIfDisposed();

            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var webppic = new WebPPicture();
            if (this.library.WebPPictureInitInternal(ref webppic) != 1)
            {
                throw new WebpEncodeException(WebPEncodingError.VP8_ENC_ERROR_INVALID_CONFIGURATION);
            }

            var webpconfig = new WebPConfig();
            if (!options.ApplyOptions(this.library, ref webpconfig))
            {
                throw new WebpEncodeException(WebPEncodingError.VP8_ENC_ERROR_INVALID_CONFIGURATION);
            }

            webppic.width = pixelWidth;
            webppic.height = pixelHeight;
            webppic.use_argb = 1;

            var bbp = stride / pixelWidth;
            switch (bbp)
            {
                case 4:
                    if (BitConverter.IsLittleEndian)
                    {
                        if (isBGR)
                        {
                            webppic.argb = data;
                            webppic.argb_stride = pixelWidth;
                        }
                        else
                        {
                            this.library.WebPPictureImportRGBA(ref webppic, data, stride);
                        }
                    }
                    else
                    {
                        if (isBGR)
                        {
                            this.library.WebPPictureImportBGRA(ref webppic, data, stride);
                        }
                        else
                        {
                            webppic.argb = data;
                            webppic.argb_stride = pixelWidth;
                        }
                    }
                    break;
                case 3:
                    if (isBGR)
                    {
                        this.library.WebPPictureImportBGR(ref webppic, data, stride);
                    }
                    else
                    {
                        this.library.WebPPictureImportRGB(ref webppic, data, stride);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            WebpDelegate.WebPWriterFunction writer = new WebpDelegate.WebPWriterFunction((IntPtr encoded_data, UIntPtr encoded_data_size, ref WebPPicture picture) =>
            {
                try
                {
                    ReadOnlySpan<byte> buffer;
                    unsafe
                    {
                        buffer = new ReadOnlySpan<byte>(encoded_data.ToPointer(), Convert.ToInt32(encoded_data_size.ToUInt32()));
                    }
                    outputStream.Write(buffer);
                    return WEBP_WRITER_RESPONSE.CONTINUE;
                }
                catch
                {
                    return WEBP_WRITER_RESPONSE.ABORT;
                }
            });
            try
            {
                webppic.writer = writer;

                if (this.library.WebPEncode(ref webpconfig, ref webppic) == 0)
                {
                    throw new WebpEncodeException(webppic.error_code);
                }

                GC.KeepAlive(writer);
            }
            finally
            {
                // Flush the stream if it supports. It doesn't matter anyway.
                try
                {
                    outputStream.Flush();
                }
                catch { }
            }
        }
        #endregion

        #region | Public Generic Methods |
        /// <summary>Validate the WebP image header and retrieve the image height and width</summary>
        /// <param name="data">The WebP image data</param>
        /// <param name="imageWidth">The image width from header. (The range is limited currently from 1 to 16383)</param>
        /// <param name="imageHeight">The image height from header. (The range is limited currently from 1 to 16383)</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool TryGetImageSize(ReadOnlySpan<byte> data, out int imageWidth, out int imageHeight)
        {
            bool result;
            unsafe
            {
                fixed (byte* b = data)
                {
                    result = (this.library.WebPGetInfo(new IntPtr(b), new UIntPtr(Convert.ToUInt32(data.Length)), out imageWidth, out imageHeight) == 1);
                }
            }
            return result;
        }

        /// <summary>Validate the WebP image header and retrieve the image height and width</summary>
        /// <param name="data">The WebP image data</param>
        /// <param name="imageWidth">The image width from header. (The range is limited currently from 1 to 16383)</param>
        /// <param name="imageHeight">The image height from header. (The range is limited currently from 1 to 16383)</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool TryGetImageSize(ReadOnlyMemory<byte> data, out int imageWidth, out int imageHeight)
        {
            bool result;
            using (var pinned = data.Pin())
            {
                unsafe
                {
                    result = (this.library.WebPGetInfo(new IntPtr(pinned.Pointer), new UIntPtr(Convert.ToUInt32(data.Length)), out imageWidth, out imageHeight) == 1);
                }
            }
            return result;
        }

        /// <summary>Validate the WebP image header</summary>
        /// <param name="data">The WebP image data</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool ValidateImageHeader(ReadOnlySpan<byte> data)
        {
            bool result;
            unsafe
            {
                fixed (byte* b = data)
                {
                    result = (this.library.WebPGetInfo(new IntPtr(b), new UIntPtr(Convert.ToUInt32(data.Length))) == 1);
                }
            }
            return result;
        }

        /// <summary>Validate the WebP image header</summary>
        /// <param name="data">The WebP image data</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool ValidateImageHeader(ReadOnlyMemory<byte> data)
        {
            bool result;
            using (var pinned = data.Pin())
            {
                unsafe
                {
                    result = (this.library.WebPGetInfo(new IntPtr(pinned.Pointer), new UIntPtr(Convert.ToUInt32(data.Length))) == 1);
                }
            }
            return result;
        }

        /// <summary>Try get the webp bitstream's feature from data buffer.</summary>
        /// <param name="data">The buffer contains webp image data.</param>
        /// <param name="feature">The structure to store the feature values.</param>
        /// <returns><seealso cref="VP8StatusCode.VP8_STATUS_OK"/> on success. Otherwise the error code.</returns>
        public VP8StatusCode TryGetImageHeaderInfo(ReadOnlyMemory<byte> data, ref WebPBitstreamFeatures feature)
        {
            VP8StatusCode result;
            using (var pinned = data.Pin())
            {
                unsafe
                {
                    result = this.library.WebPGetFeatures(new IntPtr(pinned.Pointer), new UIntPtr(Convert.ToUInt32(data.Length)), ref feature);
                }
            }
            return result;
        }

        /// <summary>Try get the webp bitstream's feature from data buffer.</summary>
        /// <param name="data">The buffer contains webp image data.</param>
        /// <param name="feature">The structure to store the feature values.</param>
        /// <returns><seealso cref="VP8StatusCode.VP8_STATUS_OK"/> on success. Otherwise the error code.</returns>
        public VP8StatusCode TryGetImageHeaderInfo(ReadOnlySpan<byte> data, ref WebPBitstreamFeatures feature)
        {
            VP8StatusCode result;
            unsafe
            {
                fixed (byte* b = data)
                {
                    result = this.library.WebPGetFeatures(new IntPtr(b), new UIntPtr(Convert.ToUInt32(data.Length)), ref feature);
                }
            }
            return result;
        }
        #endregion

        #region | Private Generic Methods|
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        internal ILibwebp GetUnmanagedInterface() => this.library;
        #endregion

        #region | Destructor |
        /// <summary>Decrease the reference count toward the loaded native library.</summary>
        /// <remarks>
        /// When reference count reaches 0, the native library will be unloaded.
        /// Calling this method does not guarantee the native library to be unloaded immediately.
        /// This method will not dispose any objects (Eg: <seealso cref="WebpImageDecoder"/>) created from this <see cref="WebpFactory"/> instance, either.
        /// </remarks>
        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;

            Libwebp.Deinit(this.library);
            this.library = null;
        }
        #endregion
    }
}
