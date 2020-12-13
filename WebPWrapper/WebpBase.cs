/////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Wrapper for WebP format in C#. (GPL) Jose M. Piñeiro
///////////////////////////////////////////////////////////////////////////////////////////////////////////// 
/// Decompress Functions:
/// BitmapSource Load(string pathFileName) - Load a WebP file in bitmap.
/// BitmapSource Decode(byte[] rawWebP) - Decode WebP data (rawWebP) to bitmap.
/// BitmapSource Decode(byte[] rawWebP, WebPDecoderOptions options) - Decode WebP data (rawWebP) to bitmap using 'options'.
/// BitmapSource GetThumbnailFast(byte[] rawWebP, int width, int height) - Get a thumbnail from WebP data (rawWebP) with dimensions 'width x height'. Fast mode.
/// BitmapSource GetThumbnailQuality(byte[] rawWebP, int width, int height) - Fast get a thumbnail from WebP data (rawWebP) with dimensions 'width x height'. Quality mode.
/// 
/// Compress Functions:
/// Save(BitmapSource bmp, string pathFileName, int quality = 75) - Save bitmap with quality lost to WebP file. Opcionally select 'quality'.
/// WebPImage EncodeLossy(BitmapSource bmp, int quality = 75) - Encode bitmap with quality lost to WebP byte array. Opcionally select 'quality'.
/// WebPImage EncodeLossy(BitmapSource bmp, int quality, int speed, bool info = false) - Encode bitmap with quality lost to WebP byte array. Select 'quality' and 'speed'. 
/// WebPImage EncodeLossless(BitmapSource bmp) - Encode bitmap without quality lost to WebP byte array. 
/// WebPImage EncodeLossless(BitmapSource bmp, int speed, bool info = false) - Encode bitmap without quality lost to WebP byte array. Select 'speed'. 
/// WebPImage EncodeNearLossless(BitmapSource bmp, int quality, int speed = 9, bool info = false) - Encode bitmap with nearlossless to WebP byte array. Select 'quality' and 'speed'. 
/// 
/// Another functions:
/// Version GetVersion() - Get the library version
/// GetInfo(byte[] rawWebP, out int width, out int height, out bool has_alpha, out bool has_animation, out string format) - Get information of WEBP data
/// float[] PictureDistortion(Bitmap source, Bitmap reference, int metric_type) - Get PSNR, SSIM or LSIM distortion metric between two pictures
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>
    /// Provides high-level access to library. This is a full-managed-code wrapper for native libwebp. Can load either decode-only, encode-only or all-in-one libwebp library.
    /// </summary>
    public class WebpBase : IDisposable
    {
        private readonly ILibwebp library;

        #region | Constructors |
        /// <summary>
        /// Initialize a new <seealso cref="WebpBase"/> instance.
        /// </summary>
        /// <param name="library_path">The path to native library file (E.g: libwebp.dll)</param>
        public WebpBase(string library_path)
        {
            this.library = Libwebp.Init(library_path);
        }
        #endregion

        #region | Abstract Decompress Functions |
        /// <summary>Parse WebP header from a buffer.</summary>
        /// <param name="buffer">The buffer to parse the WebP header.</param>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is an empty buffer.</exception>
        /// <exception cref="InvalidOperationException">The underlying webp native library is not valid or usable.</exception>
        /// <exception cref="InvalidDataException">The buffer doesn't contain a webp image.</exception>
        /// <returns></returns>
        public WebpImageDecoder CreateDecoder(in ReadOnlySpan<byte> buffer)
        {
            this.ThrowIfDisposed();
            
            if (buffer.IsEmpty)
            {
                throw new ArgumentException("Cannot decode header from an empty buffer", nameof(buffer));
            }
            var imageDecoder = new WebpImageDecoder(this.library);
            return imageDecoder;
        }

        protected void DecodePixels(WebpImageDecoder decoder, in ReadOnlySpan<byte> buffer)
            => this.InternalDecodePixels(decoder, buffer, null);

        protected void DecodePixels(WebpImageDecoder decoder, in ReadOnlySpan<byte> buffer, DecoderOptions decoderOptions)
        {
            if (decoderOptions == null)
            {
                throw new ArgumentNullException(nameof(decoderOptions), "Decoder option cannot be null.");
            }
            this.InternalDecodePixels(decoder, buffer, decoderOptions);
        }

        private void InternalDecodePixels(WebpImageDecoder decoder, in ReadOnlySpan<byte> buffer, DecoderOptions decoderOptions)
        {
            this.ThrowIfDisposed();

            ref var config = ref decoder.Config;
            if (decoderOptions == null)
            {
                DecoderOptions.Default.ApplyOptions(ref config);
            }
            else
            {
                decoderOptions.ApplyOptions(ref config);
            }
            int width, height;

            ref var options = ref config.options;

            if (options.use_scaling != 0)
            {
                width = config.options.scaled_width;
                height = config.options.scaled_height;
            }
            else if (options.use_cropping != 0)
            {
                width = options.crop_width;
                height = options.crop_height;
            }
            else
            {
                width = config.input.width;
                height = config.input.height;
            }

            if (width == 0 || height == 0)
            {
                throw new ArgumentOutOfRangeException("Final resolution seems to be empty.");
            }

            config.output.height = height;
            config.output.width = width;

            unsafe
            {
                fixed (byte* bufferPointer = buffer)
                {
                    var memoryPointer = new IntPtr(bufferPointer);
                    var result = this.library.WebPDecode(memoryPointer, buffer.Length, ref config);
                    switch (result)
                    {
                        case VP8StatusCode.VP8_STATUS_OK:
                            break;
                        case VP8StatusCode.VP8_STATUS_OUT_OF_MEMORY:
                            throw new InsufficientMemoryException();
                        case VP8StatusCode.VP8_STATUS_UNSUPPORTED_FEATURE:
                            throw new NotSupportedException();
                        case VP8StatusCode.VP8_STATUS_INVALID_PARAM:
                            throw new ArgumentException();
                        case VP8StatusCode.VP8_STATUS_BITSTREAM_ERROR:
                            throw new InvalidDataException();
                        default:
                            throw new InvalidOperationException($"Failed to decode pixels. Error code: {result}");
                    }
                }
            }
        }
        #endregion

        #region | Abstract Compress Functions |
        /// <summary>Encode bitmap to Lossy WebP bypass default <see cref="EncoderOptions"/> (Simple encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLossy(BitmapSource bmp, int quality = 75)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            IntPtr unmanagedData = IntPtr.Zero;
            
            try
            {
                int size = this.WebPEncodeLossySimple(bmp, quality, out unmanagedData);
                if (size == 0)
                    throw new Exception("Can´t encode WebP");

                return new WebPImage(this.library, new SimpleWebPContentStream(unmanagedData, size));
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly", ex); }
            finally
            {
                if (unmanagedData != IntPtr.Zero)
                {
                    this.library.WebPFree(unmanagedData);
                }
            }
        }

        /// <summary>Encode bitmap to Near-Lossless WebP bypass default <see cref="EncoderOptions"/> (Simple encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        /// <remarks>Just a fake method one, not offically from libwebp</remarks>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeNearLossless(BitmapSource bmp, int quality = 75)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            try
            {
                return this.Encode(bmp, new EncoderOptions(CompressionType.NearLossless, WebPPreset.Default, quality));
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeNearLossless", ex); }
        }

        /// <summary>Encode bitmap to Lossless WebP bypass default <see cref="EncoderOptions"/> (Simple encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLossless(BitmapSource bmp)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            IntPtr unmanagedData = IntPtr.Zero;

            try
            {
                int size = this.WebPEncodeLosslessSimple(bmp, out unmanagedData);
                if (size == 0)
                    throw new Exception("Can´t encode WebP");

                return new WebPImage(this.library, new SimpleWebPContentStream(unmanagedData, size));
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless", ex); }
            finally
            {
                if (unmanagedData != IntPtr.Zero)
                {
                    this.library.WebPFree(unmanagedData);
                }
            }
        }

        /// <summary>Encode bitmap to WebP with given option and return the encoded image</summary>
        /// <param name="bmp">Bitmap to be encode to the WebP image</param>
        /// <returns></returns>
        public WebPImage Encode(BitmapSource bmp) => this.Encode(bmp, null);

        /// <summary>Encode bitmap to WebP with given option and return the encoded image. (Advanced API)</summary>
        /// <param name="bmp">Bitmap to be encode to the WebP image</param>
        /// <param name="options"></param>
        public WebPImage Encode(BitmapSource bmp, EncoderOptions options)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;
            bool isContiguousMemory = ((options.MemoryUsage & MemoryAllowance.ForcedContiguousMemory) == MemoryAllowance.ForcedContiguousMemory);
            
            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                if (options == null)
                    options = this.defaultEncodeOption;

                options.InitConfig(this.library, ref config);

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating the bitmap, width and height
                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Set up a byte-writing method (write-to-memory, in this case)
                WebPMemoryCopyBuffer webPMemoryBuffer = new WebPMemoryCopyBuffer(this.ManagedChunkPool, isContiguousMemory);
                Delegate somedeed = new NativeDelegates.WebPDataWriterCallback(webPMemoryBuffer.MyWriter);
                
                // Is this even needed, "somedeed" above is still reference to the delegate. And WebPEncode is a synchronous operation.
                // Better safe than sorry?
                var preventDelegateFromBeingCollectedButMovingIsOkay = GCHandle.Alloc(somedeed, GCHandleType.Normal);

                wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);

                try
                {
                    //compress the input samples, synchronous operation
                    if (this.library.WebPEncode(ref config, ref wpic) != 1)
                        throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (preventDelegateFromBeingCollectedButMovingIsOkay.IsAllocated)
                        preventDelegateFromBeingCollectedButMovingIsOkay.Free();
                    webPMemoryBuffer.ToReadOnly();
                    somedeed = null;
                }
                return new WebPImage(this.library, webPMemoryBuffer);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.Encode", ex); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();

                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Encode bitmap to WebP with given option and write to a file</summary>
        /// <param name="bmp">Bitmap to be encode to the WebP image</param>
        /// <param name="pathFileName">The file to write</param>
        public void EncodeToFile(BitmapSource bmp, string pathFileName) => this.EncodeToFile(bmp, pathFileName, null);

        /// <summary>Encode bitmap to WebP with given option and write to a file. (Advanced API)</summary>
        /// <param name="bmp">Bitmap to be encode to the WebP image</param>
        /// <param name="pathFileName">The file to write</param>
        /// <param name="options"></param>
        public void EncodeToFile(BitmapSource bmp, string pathFileName, EncoderOptions options)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;
            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                if (options == null)
                    options = this.defaultEncodeOption;

                options.InitConfig(this.library, ref config);

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating the bitmap, width and height
                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                using (WebPFileWriter webPMemoryBuffer = new WebPFileWriter(pathFileName))
                {
                    Delegate somedeed = new NativeDelegates.WebPDataWriterCallback(webPMemoryBuffer.MyFileWriter);

                    // Is this even needed, "somedeed" above is still reference to the delegate. And WebPEncode is a synchronous operation.
                    // Better safe than sorry?
                    var preventDelegateFromBeingCollectedButMovingIsOkay = GCHandle.Alloc(somedeed, GCHandleType.Normal);

                    wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);

                    try
                    {
                        //compress the input samples, synchronous operation
                        if (this.library.WebPEncode(ref config, ref wpic) != 1)
                            throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (preventDelegateFromBeingCollectedButMovingIsOkay.IsAllocated)
                            preventDelegateFromBeingCollectedButMovingIsOkay.Free();
                        somedeed = null;
                    }

                }
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeToFile", ex); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();
                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }
        #endregion

        #region | Private Functions |
        private void ThrowIfDisposed()
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 1) != 0)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
        #endregion

        #region | Public Static Functions |
        private static readonly byte[] HeaderSignature1 = System.Text.Encoding.ASCII.GetBytes("RIFF");
        private static readonly byte[] HeaderSignature2 = System.Text.Encoding.ASCII.GetBytes("WEBP");

        /// <summary>
        /// Verify the file header to see if the file really contains a WebP image.
        /// </summary>
        /// <param name="filepath">Path to the file to check</param>
        /// <returns>A boolean determine whether if the file is a WebP image or not</returns>
        public static bool IsWebP(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 8))
                return IsWebP(fs, false);
        }

        /// <summary>
        /// Verify the content (from current stream's position) if the stream really contains a WebP image.
        /// </summary>
        /// <param name="stream">The stream to verify</param>
        /// <returns>A boolean determine whether if the stream contains WebP data or not</returns>
        public static bool IsWebP(Stream stream) => IsWebP(stream, true);

        /// <summary>
        /// Verify the content (from current stream's position) if the stream really contains a WebP image.
        /// </summary>
        /// <param name="stream">The stream to verify</param>
        /// <param name="preservePosition">Determine if the position should be preserve after checking. <see cref="Stream.CanSeek"/> must be true.</param>
        /// <returns></returns>
        public static bool IsWebP(Stream stream, bool preservePosition)
        {
            if (preservePosition && !stream.CanSeek)
                throw new NotSupportedException("The stream cannot be seeked.");

            // Read the 12 bytes:
            // 4: ASCII "RIFF"
            // 4: Image size in byte
            // 4: ASCII "RIFF"
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(12);
            try
            {
                int read = stream.Read(buffer, 0, 12);
                if (read == 0)
                    return false;
                if (preservePosition)
                    stream.Seek(read * -1, SeekOrigin.Current);
                if (read == 12)
                {
                    byte[] textbuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(4);
                    try
                    {
                        var firstSig = new ReadOnlySpan<byte>(buffer, 0, 4);
                        var secondSig = new ReadOnlySpan<byte>(buffer, 8, 4);
                        if (firstSig.SequenceEqual(HeaderSignature1) && secondSig.SequenceEqual(HeaderSignature2))
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        System.Buffers.ArrayPool<byte>.Shared.Return(textbuffer);
                    }
                }
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            }
            return false;
        }
        #endregion

        #region | Another Public Functions |
        /// <summary>Get the libwebp version</summary>
        /// <returns>Version of library</returns>
        public Version GetVersion()
        {
            this.ThrowIfDisposed();

            // Do both of these function return the same version packet?
            int v;
            if (this.library.IsFunctionExists("WebPGetEncoderVersion"))
            {
                v = this.library.WebPGetEncoderVersion();
            }
            else if (this.library.IsFunctionExists("WebPGetDecoderVersion"))
            {
                v = this.library.WebPGetDecoderVersion();
            }
            else
            {
                throw new EntryPointNotFoundException("Cannot get version of the library. Function 'WebPGetEncoderVersion' and 'WebPGetDecoderVersion' are not found. Wrong library or wrong version?");
            }

            // Yes, it seems that it's "revision", not "build". Although the 3rd arg of the constructor require "build"
            int revision = v % 256,
                minor = (v >> 8) % 256,
                major = (v >> 16) % 256;
            return new Version(major, minor, revision, 0);
        }

        /// <summary>Gets a value indicating whether the current loaded library supports WebP encoding.</summary>
        public bool CanEncode => this.library.CanEncode;

        /// <summary>Gets a value indicating whether the current loaded library supports WebP decoding.</summary>
        public bool CanDecode => this.library.CanDecode;

        /// <summary>Gets the full path to the imported library.</summary>
        public string FullFilename => this.library.LibraryPath;

        /// <summary>Return low-level access to unmanaged code. USE IT AT YOUR OWN RISK.</summary>
        /// <return>An interface of low-level native library, it should provide most (if not all) of neccessary functions of the native library.</return>
        protected ILibwebp Library
        {
            get
            {
                this.ThrowIfDisposed();

                return this.library;
            }
        }
        #endregion

        #region | Destruction |
        /// <summary>Free memory</summary>
        private int _disposed = 0;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref this._disposed, 1) == 0)
            {
                Libwebp.Deinit(this.library);
            }
        }

        ~WebpBase()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
