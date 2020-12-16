using System;
using System.Collections.Generic;
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
        public WebpImageDecoder CreateDecoderForRGBX(in WEBP_CSP_MODE colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, in int output_stride)
        {
            this.ThrowIfDisposed();
            return WebpImageDecoder.CreateDecoderForRGBX(this.library, colorspace, output_buffer, output_buffer_size, output_stride);
        }

        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the RGB/A samples specified by '<paramref name="colorspace"/>' into a preallocated internal buffer.
        /// </summary>
        /// <remarks>
        /// Equivalent to <seealso cref="CreateDecoderForRGBX(in WEBP_CSP_MODE, IntPtr, UIntPtr, in int)"/>, with 'output_buffer' is NULL.
        /// Use <seealso cref="WebpImageDecoder.GetDecodedImage(out int, out int, out int, out int, out IntPtr)"/> or <seealso cref="WebpImageDecoder.GetDecodedImage(out int, out int, out int, out int, out ReadOnlySpan{byte})"/>
        /// to obtain the decoded data.
        /// </remarks>
        /// <exception cref="InvalidProgramException">Unknown error occured.</exception>
        /// <exception cref="NotSupportedException"><paramref name="colorspace"/> is not RGB(A) colorspace</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorspace"/> is not a valid value</exception>
        /// <returns><see cref="WebpImageDecoder"/></returns>
        public WebpImageDecoder CreateDecoderForRGBX(in WEBP_CSP_MODE colorspace)
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

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="output_buffer">The <seealso cref="WebPDecBuffer"/> to free the associated memory.</param>
        /// <remarks>External memory will not be touched.</remarks>
        public void Free(ref WebPDecBuffer output_buffer)
        {
            this.ThrowIfDisposed();
            this.library.WebPFreeDecBuffer(ref output_buffer);
        }
        #endregion

        #region | Public Generic Methods |
        /// <summary>Validate the WebP image header and retrieve the image height and width</summary>
        /// <param name="data">The WebP image data</param>
        /// <param name="imageWidth">The image width from header. (The range is limited currently from 1 to 16383)</param>
        /// <param name="imageHeight">The image height from header. (The range is limited currently from 1 to 16383)</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool TryGetImageInfo(in ReadOnlySpan<byte> data, out int imageWidth, out int imageHeight)
        {
            int result = 0;
            unsafe
            {
                fixed (byte* b = data)
                {
                    result = this.library.WebPGetInfo(new IntPtr(b), data.Length, out imageWidth, out imageHeight);
                }
            }
            return (result == 1);
        }

        /// <summary>Validate the WebP image header and retrieve the image height and width</summary>
        /// <param name="data">The WebP image data</param>
        /// <param name="imageWidth">The image width from header. (The range is limited currently from 1 to 16383)</param>
        /// <param name="imageHeight">The image height from header. (The range is limited currently from 1 to 16383)</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool TryGetImageInfo(in ReadOnlyMemory<byte> data, out int imageWidth, out int imageHeight)
        {
            int result = 0;
            using (var pinned = data.Pin())
            {
                unsafe
                {
                    result = this.library.WebPGetInfo(new IntPtr(pinned.Pointer), data.Length, out imageWidth, out imageHeight);
                }
            }
            return (result == 1);
        }

        /// <summary>Validate the WebP image header</summary>
        /// <param name="data">The WebP image data</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool ValidateImageHeader(in ReadOnlySpan<byte> data) => this.TryGetImageInfo(data, out _, out _);

        /// <summary>Validate the WebP image header</summary>
        /// <param name="data">The WebP image data</param>
        /// <returns>True if success. Otherwise false, usually failure because of format error?</returns>
        public bool ValidateImageHeader(in ReadOnlyMemory<byte> data) => this.TryGetImageInfo(data, out _, out _);
        #endregion

        #region | Private Generic Methods|
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
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
