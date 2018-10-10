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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper.WPF.Buffer;
using WebPWrapper.WPF.Helper;
using WebPWrapper.WPF.LowLevel;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Provides high-level access to library. This is a full-managed-code wrapper for native libwebp. Can load either decode-only, encode-only or all-in-one libwebp library.
    /// </summary>
    public sealed class WebP : IDisposable
    {
        internal ChunkPool ManagedChunkPool;
        internal Libwebp library;
        private EncoderOptions encodeOption;
        private DecoderOptions decodeOption;

        #region Constructors
        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <remarks>
        /// For backward-compatible only
        /// </remarks>
        public WebP() : this(RuntimeValue.DefaultBufferSize) { }

        /// <summary>
        /// Create a new WebP instance with given buffer pool size.
        /// </summary>
        /// <remarks>
        /// For backward-compatible only
        /// </remarks>
        public WebP(int bufferBlockSize) : this(RuntimeValue.StringDependsArchitecture("libwebp_x86.dll", "libwebp_x64.dll"), bufferBlockSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path) : this(library_path, RuntimeValue.DefaultBufferSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, int bufferBlockSize) : this(library_path, new EncoderOptions(), new DecoderOptions(), bufferBlockSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, EncoderOptions defaultEncodeOptions) : this(library_path, defaultEncodeOptions, RuntimeValue.DefaultBufferSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, DecoderOptions defaultDecodeOptions) : this(library_path, defaultDecodeOptions, RuntimeValue.DefaultBufferSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, EncoderOptions defaultEncodeOptions, int bufferBlockSize) : this(library_path, defaultEncodeOptions, new DecoderOptions(), bufferBlockSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, DecoderOptions defaultDecodeOptions, int bufferBlockSize) : this(library_path, new EncoderOptions(), defaultDecodeOptions, bufferBlockSize) { }

        /// <summary>
        /// Create a new WebP instance with default buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        public WebP(string library_path, EncoderOptions defaultEncodeOptions, DecoderOptions defaultDecodeOptions) : this(library_path, defaultEncodeOptions, defaultDecodeOptions, RuntimeValue.DefaultBufferSize) { }

        /// <summary>
        /// Create a new WebP instance with given buffer pool size.
        /// </summary>
        /// <param name="library_path">The path to libwebp dll file</param>
        /// <param name="bufferBlockSize">The size (in bytes) of each buffer block in the cache pool (for re-using buffer). Set to 0 to disable buffer pool. Size smaller than 1024 will be adjusted to 1024.</param>
        public WebP(string library_path, EncoderOptions defaultEncodeOptions, DecoderOptions defaultDecodeOptions, int bufferBlockSize)
        {
            if (bufferBlockSize < 0)
                throw new ArgumentException("The size must be non-negative value.");
            else if (bufferBlockSize == 0)
                this.ManagedChunkPool = null;
            else if (bufferBlockSize < 1024)
                this.ManagedChunkPool = new ChunkPool(1024);
            else
                this.ManagedChunkPool = new ChunkPool(bufferBlockSize);

            this.decodeOption = defaultDecodeOptions;
            this.encodeOption = defaultEncodeOptions;

            this.library = Libwebp.Init(this, library_path);
        }
        #endregion

        #region | Public Decompress Functions |
        /// <summary>Read a WebP file</summary>
        /// <param name="pathFileName">WebP file to load</param>
        /// <returns>Bitmap with the WebP image</returns>
        public BitmapSource DecodeFile(string pathFileName) => DecodeFile(pathFileName, this.decodeOption);
        /// <summary>Read a WebP file (Advanced API)</summary>
        /// <param name="pathFileName">WebP file to load</param>
        /// <param name="options">Decoder options</param>
        /// <returns>Bitmap with the WebP image</returns>
        public BitmapSource DecodeFile(string pathFileName, DecoderOptions options)
        {
            try
            {
                using (FileStream fs = File.OpenRead(pathFileName))
                {
                    if (fs.Length == 0)
                        throw new FileFormatException("There is nothing to decode");
                    if (fs.Length > int.MaxValue)
                        throw new StackOverflowException();

                    int size = (int)fs.Length;
                    byte[] bytes = new byte[size];
                    if (fs.Read(bytes, 0, size) > 0)
                    {
                        return Decode(bytes, options);
                    }
                    else
                        throw new Exception("File empty.");
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.DecodeFile"); }
        }

        /// <summary>Decode a WebP image from memory</summary>
        /// <param name="webpData">An array of <see cref="Byte"/> which contains webp image data</param>
        /// <returns></returns>
        public WriteableBitmap Decode(byte[] webpData) => this.Decode(webpData, this.decodeOption);

        /// <summary>Decode a WebP image from memory</summary>
        /// <param name="webpData">An array of <see cref="Byte"/> which contains webp image data</param>
        /// <param name="decodeOptions">The decode options. This will override the default decode options.</param>
        /// <returns></returns>
        public WriteableBitmap Decode(byte[] webpData, DecoderOptions decodeOptions) => this.Decode(webpData, 0, webpData.Length, decodeOptions);

        /// <summary>Decode a WebP image from memory</summary>
        /// <param name="webpData">An array of <see cref="Byte"/> which contains webp image data</param>
        /// <param name="offset">Start offset of the array</param>
        /// <param name="count">Length from offset</param>
        /// <param name="decodeOptions">The decode options. This will override the default decode options.</param>
        /// <returns></returns>
        public WriteableBitmap Decode(byte[] webpData, int offset, int count, DecoderOptions decodeOptions)
        {
            if (offset < 0)
                throw new ArgumentException("Offset cannot be a negative value", "offset");

            if ((offset + count) > webpData.Length)
                throw new ArgumentException("Count exceeded the buffer length", "count");

            WriteableBitmap bitmap;

            unsafe
            {
                fixed (byte* b = webpData)
                {
                    bitmap = Decode(new IntPtr(b + offset), count, decodeOptions);
                }
            }
            return bitmap;
        }

        /// <summary>Decode a WebP image from memory</summary>
        /// <param name="memoryPointer">Memory pointer to the start of the memory</param>
        /// <param name="lengthToRead">Length of the memory</param>
        /// <returns></returns>
        public WriteableBitmap Decode(IntPtr memoryPointer, int lengthToRead) => this.Decode(memoryPointer, lengthToRead, this.decodeOption);

        /// <summary>Decode a WebP image from memory</summary>
        /// <param name="memoryPointer">Memory pointer to the start of the memory</param>
        /// <param name="lengthToRead">Length of the memory</param>
        /// <param name="decodeOptions">The decode options. This will override the default decode options.</param>
        /// <returns></returns>
        public WriteableBitmap Decode(IntPtr memoryPointer, int lengthToRead, DecoderOptions decodeOptions)
        {
            VP8StatusCode result;
            int width = 0;
            int height = 0;

            try
            {

                WebPDecoderConfig config = new WebPDecoderConfig();
                if (this.library.WebPInitDecoderConfig(ref config) == 0)
                {
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
                }

                result = this.library.WebPGetFeatures(memoryPointer, lengthToRead, ref config.input);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                    throw new Exception("Failed WebPGetFeatures with error " + result);

                width = config.input.width;
                height = config.input.height;

                WebPDecoderOptions options = decodeOptions.GetStruct();

                if (options.use_scaling == 0)
                {
                    //Test cropping values
                    if (options.use_cropping == 1)
                    {
                        if (options.crop_left + options.crop_width > config.input.width || options.crop_top + options.crop_height > config.input.height)
                            throw new Exception("Crop options exceded WebP image dimensions");
                        width = options.crop_width;
                        height = options.crop_height;
                    }
                }
                else
                {
                    width = options.scaled_width;
                    height = options.scaled_height;
                }

                config.options.bypass_filtering = options.bypass_filtering;
                config.options.no_fancy_upsampling = options.no_fancy_upsampling;
                config.options.use_cropping = options.use_cropping;
                config.options.crop_left = options.crop_left;
                config.options.crop_top = options.crop_top;
                config.options.crop_width = options.crop_width;
                config.options.crop_height = options.crop_height;
                config.options.use_scaling = options.use_scaling;
                config.options.scaled_width = options.scaled_width;
                config.options.scaled_height = options.scaled_height;
                config.options.use_threads = options.use_threads;
                config.options.dithering_strength = options.dithering_strength;
                config.options.flip = options.flip;
                config.options.alpha_dithering_strength = options.alpha_dithering_strength;

                // Specify the output format
                WriteableBitmap bitmap;

                if (config.input.has_alpha == 0)
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_BGRA;
                    bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
                    bitmap.Lock();
                }
                else
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_bgrA;
                    bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
                    bitmap.Lock();
                }

                config.output.u.RGBA.rgba = bitmap.BackBuffer;
                config.output.u.RGBA.stride = bitmap.BackBufferStride;
                config.output.u.RGBA.size = (UIntPtr)(height * bitmap.BackBufferStride);
                config.output.height = height;
                config.output.width = width;
                config.output.is_external_memory = 1;

                // Decode
                result = this.library.WebPDecode(memoryPointer, lengthToRead, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    bitmap.Unlock();
                    throw new Exception("Failed WebPDecode with error " + result);
                }
                else
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                    bitmap.Unlock();
                }

                this.library.WebPFreeDecBuffer(ref config.output);

                return bitmap;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.Decode", ex); }
            finally
            {
            }
        }

        /// <summary>Get Thumbnail from webP in mode faster/low quality</summary>
        /// <param name="webpData">An array of <see cref="Byte"/> which contains webp image data</param>
        /// <param name="width">Wanted width of thumbnail</param>
        /// <param name="height">Wanted height of thumbnail</param>
        /// <returns>Bitmap with the WebP thumbnail</returns>
        public BitmapSource GetThumbnailFast(byte[] webpData, int width, int height)
        {
            BitmapSource result;
            unsafe
            {
                fixed (byte* b = webpData)
                {
                    result = GetThumbnailFast(new IntPtr(b), webpData.Length, width, height);
                }
            }
            return result;
        }

        /// <summary>Get Thumbnail from webP in mode faster/low quality</summary>
        /// <param name="memoryPointer">Memory pointer to the buffer to uncompress</param>
        /// <param name="length">The size of the memory buffer</param>
        /// <param name="width">Wanted width of thumbnail</param>
        /// <param name="height">Wanted height of thumbnail</param>
        /// <returns>CachedBitmap, because of Thumbnail. Or should I use WritableBitmap?</returns>
        public BitmapSource GetThumbnailFast(IntPtr memoryPointer, int length, int width, int height)
        {
            IntPtr outputPointer = IntPtr.Zero;
            int _stride = 0,
                outputsize = 0;

            try
            {
                WebPDecoderConfig config = new WebPDecoderConfig();
                if (this.library.WebPInitDecoderConfig(ref config) == 0)
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");

                // Set up decode options
                config.options.bypass_filtering = 1;
                config.options.no_fancy_upsampling = 1;
                config.options.use_threads = 1;
                config.options.use_scaling = 1;
                config.options.scaled_width = width;
                config.options.scaled_height = height;


                // Create a BitmapData and Lock all pixels to be written
                PixelFormat format = PixelFormats.Pbgra32;

                _stride = width * (format.BitsPerPixel / 8);
                outputsize = height * _stride;
                outputPointer = Marshal.AllocHGlobal(outputsize);

                // Specify the output format
                config.output.colorspace = WEBP_CSP_MODE.MODE_bgrA;
                config.output.u.RGBA.rgba = outputPointer;
                config.output.u.RGBA.stride = _stride;
                config.output.u.RGBA.size = (UIntPtr)outputsize;
                config.output.height = width;
                config.output.width = height;
                config.output.is_external_memory = 1;

                // Decode
                VP8StatusCode result = this.library.WebPDecode(memoryPointer, length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                    throw new Exception("Failed WebPDecode with error " + result);

                BitmapSource bmp = CachedBitmap.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, outputPointer, outputsize, _stride);

                this.library.WebPFreeDecBuffer(ref config.output);

                return bmp;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.Thumbnail"); }
            finally
            {
                if (outputPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(outputPointer);
            }
        }

        /// <summary>Thumbnail from webP in mode slow/high quality</summary>
        /// <param name="webpData">An array of <see cref="Byte"/> which contains webp image data</param>
        /// <param name="width">Wanted width of thumbnail</param>
        /// <param name="height">Wanted height of thumbnail</param>
        /// <returns>Bitmap with the WebP thumbnail</returns>
        public BitmapSource GetThumbnailQuality(byte[] webpData, int width, int height)
        {
            BitmapSource result;
            unsafe
            {
                fixed (byte* b = webpData)
                {
                    result = GetThumbnailQuality(new IntPtr(b), webpData.Length, width, height);
                }
            }
            return result;
        }

        /// <summary>Get Thumbnail from webP in mode faster/low quality</summary>
        /// <param name="memoryPointer">Memory pointer to the buffer to uncompress</param>
        /// <param name="length">The size of the memory buffer</param>
        /// <param name="width">Wanted width of thumbnail</param>
        /// <param name="height">Wanted height of thumbnail</param>
        /// <returns>CachedBitmap, because of Thumbnail. Or should I use WritableBitmap?</returns>
        public BitmapSource GetThumbnailQuality(IntPtr memoryPointer, int length, int width, int height)
        {
            IntPtr outputPointer = IntPtr.Zero;
            int _stride = 0,
                outputsize = 0;

            try
            {
                WebPDecoderConfig config = new WebPDecoderConfig();
                if (this.library.WebPInitDecoderConfig(ref config) == 0)
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");

                // Set up decode options
                config.options.bypass_filtering = 0;
                config.options.no_fancy_upsampling = 0;
                config.options.use_threads = 1;
                config.options.use_scaling = 1;
                config.options.scaled_width = width;
                config.options.scaled_height = height;

                // Create a BitmapData and Lock all pixels to be written
                PixelFormat format = PixelFormats.Pbgra32;

                _stride = width * (format.BitsPerPixel / 8);
                outputsize = height * _stride;
                outputPointer = Marshal.AllocHGlobal(outputsize);

                // Specify the output format
                config.output.colorspace = WEBP_CSP_MODE.MODE_bgrA;
                config.output.u.RGBA.rgba = outputPointer;
                config.output.u.RGBA.stride = _stride;
                config.output.u.RGBA.size = (UIntPtr)(outputsize);
                config.output.height = width;
                config.output.width = height;
                config.output.is_external_memory = 1;

                // Decode
                VP8StatusCode result = this.library.WebPDecode(memoryPointer, length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                    throw new Exception("Failed WebPDecode with error " + result);

                BitmapSource bmp = CachedBitmap.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, outputPointer, outputsize, _stride);

                this.library.WebPFreeDecBuffer(ref config.output);

                return bmp;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.Thumbnail"); }
            finally
            {
                if (outputPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(outputPointer);
            }
        }
        #endregion

        #region | Public Compress Functions |
        /// <summary>Save bitmap to file in WebP lossy format</summary>
        /// <param name="bmp">Bitmap with the WebP image</param>
        /// <param name="pathFileName">The file to write</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        public void EncodeLossyToFile(BitmapSource bmp, string pathFileName, int quality = 75, int speed = 6, WebPPreset preset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;

            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, preset, quality) == 0)
                    throw new Exception("Can't config preset");

                // Add additional tuning:
                config.method = speed;
                if (config.method > 6)
                    config.method = 6;
                config.alpha_compression = 1;
                config.alpha_filtering = 1;
                config.alpha_quality = 100;
                config.quality = quality;
                config.autofilter = 1;
                config.pass = speed + 1;
                config.segments = 4;
                config.partitions = 3;
                config.thread_level = 1;
                if (this.library.WebPGetDecoderVersion() > 1082)     //Old version don´t suport preprocessing 4
                    config.preprocessing = 4;
                else
                    config.preprocessing = 3;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can't init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Use buffering enqueue (I don't know why but it won't work without enqueue) to write the byte[] chunks to disk.
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
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly (File)"); }
            finally
            {
                //Free memory
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();
                pixelBuffer = null;
                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        public void EncodeNearLosslessToFile(BitmapSource bmp, string pathFileName, int quality = 100, int speed = 9, WebPPreset preset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;
            try
            {
                //test dll version
                if (this.library.WebPGetDecoderVersion() <= 1082)
                    throw new Exception("This dll version not suport EncodeNearLossless");

                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, preset, quality) == 0)
                    throw new Exception("Can´t config preset");
                if (this.library.WebPConfigLosslessPreset(ref config, speed) == 0)
                    throw new Exception("Can´t config lossless preset");
                config.thread_level = 1;
                config.pass = speed + 1;
                config.near_lossless = quality;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating the bitmap, width and height
                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Use buffering enqueue (I don't know why but it won't work without enqueue) to write the byte[] chunks to disk.
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
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeNearLossless"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();
                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        public void EncodeLosslessToFile(BitmapSource bmp, string pathFileName, int quality = 75, int speed = 6, WebPPreset preset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;

            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, preset, quality) == 0)
                    throw new Exception("Can´t config preset");

                //Old version of dll not suport info and WebPConfigLosslessPreset
                if (this.library.WebPGetDecoderVersion() > 1082)
                {
                    if (this.library.WebPConfigLosslessPreset(ref config, speed) == 0)
                        throw new Exception("Can´t config lossless preset");
                }

                config.thread_level = 1;
                config.pass = speed + 1;
                //config.image_hint = WebPImageHint.WEBP_HINT_PICTURE;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating a the bitmap, width and height

                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Use buffering enqueue (I don't know why but it won't work without enqueue) to write the byte[] chunks to disk.
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
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();
                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Lossy encoding bitmap to WebP (Simple encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLossySimple(BitmapSource bmp, int quality = 75)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            IntPtr unmanagedData = IntPtr.Zero;

            try
            {
                int size = this.WebPEncodeSimple(bmp, quality, out unmanagedData);
                if (size == 0)
                    throw new Exception("Can´t encode WebP");

                return new WebPImage(this.library, new SimpleWebPContentStream(unmanagedData, size));
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly"); }
            finally
            {
                if (unmanagedData != IntPtr.Zero)
                {
                    this.library.WebPFree(unmanagedData);
                }
            }
        }

        /// <summary>Lossy encoding bitmap to WebP (Advanced encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        /// <param name="speed">Between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLossy(BitmapSource bmp, int quality, int speed, WebPPreset preset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;

            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, preset, quality) == 0)
                    throw new Exception("Can´t config preset");

                // Add additional tuning:
                config.method = speed;
                if (config.method > 6)
                    config.method = 6;
                config.alpha_compression = 1;
                config.alpha_filtering = 1;
                config.alpha_quality = 100;
                config.quality = quality;
                config.autofilter = 1;
                config.pass = speed + 1;
                config.segments = 4;
                config.partitions = 3;
                config.thread_level = 1;
                if (this.library.WebPGetDecoderVersion() > 1082)     //Old version don´t suport preprocessing 4
                    config.preprocessing = 4;
                else
                    config.preprocessing = 3;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating the bitmap, width and height

                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Set up a byte-writing method (write-to-memory, in this case)
                WebPMemoryCopyBuffer webPMemoryBuffer = new WebPMemoryCopyBuffer(this.ManagedChunkPool, false);

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
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly (Advanced)"); }
            finally
            {
                //Free memory
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();

                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Lossless encoding bitmap to WebP (Simple encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLosslessSimple(BitmapSource bmp)
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
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless (Simple)"); }
            finally
            {
                if (unmanagedData != IntPtr.Zero)
                    this.library.WebPFree(unmanagedData);
            }
        }

        /// <summary>Lossless encoding image in bitmap (Advanced encoding API)</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="speed">Between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>Compressed data</returns>
        public WebPImage EncodeLossless(BitmapSource bmp, int quality, int speed, WebPPreset webPPreset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;

            try
            {
                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, webPPreset, quality) == 0)
                    throw new Exception("Can´t config preset");

                //Old version of dll not suport info and WebPConfigLosslessPreset
                if (this.library.WebPGetDecoderVersion() > 1082)
                {
                    if (this.library.WebPConfigLosslessPreset(ref config, speed) == 0)
                        throw new Exception("Can´t config lossless preset");
                }

                config.thread_level = 1;
                config.pass = speed + 1;
                //config.image_hint = WebPImageHint.WEBP_HINT_PICTURE;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating a the bitmap, width and height

                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Set up a byte-writing method (write-to-memory, in this case)
                WebPMemoryCopyBuffer webPMemoryBuffer = new WebPMemoryCopyBuffer(this.ManagedChunkPool, false);
                Delegate somedeed = new NativeDelegates.WebPDataWriterCallback(webPMemoryBuffer.MyWriter);
                wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);
                webPMemoryBuffer.ToReadOnly();

                //compress the input samples
                if (this.library.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());

                return new WebPImage(this.library, webPMemoryBuffer);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();

                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Near lossless encoding image in bitmap</summary>
        /// <param name="bmp">Bitmap with the image</param>
        /// <param name="quality">Between 0 (lower quality, lowest file size) and 100 (highest quality, higher file size)</param>
        /// <param name="speed">Between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>Compress data</returns>
        public WebPImage EncodeNearLossless(BitmapSource bmp, int quality, int speed = 9, WebPPreset preset = WebPPreset.Default)
        {
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0)
                throw new ArgumentException("Bitmap contains no data.", "bmp");

            WebPPicture wpic = new WebPPicture();
            PixelBuffer pixelBuffer = null;

            try
            {
                //test dll version
                if (this.library.WebPGetDecoderVersion() <= 1082)
                    throw new Exception("This dll version not suport EncodeNearLossless");

                //Inicialize config struct
                WebPConfig config = new WebPConfig();

                //Set compresion parameters
                if (this.library.WebPConfigInit(ref config, preset, quality) == 0)
                    throw new Exception("Can´t config preset");
                if (this.library.WebPConfigLosslessPreset(ref config, speed) == 0)
                    throw new Exception("Can´t config lossless preset");

                config.thread_level = 1;
                config.pass = speed + 1;
                config.near_lossless = quality;

                //Validate the config
                if (this.library.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                // Setup the input data, allocating the bitmap, width and height
                if (this.library.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");

                //Put the bitmap componets in wpic
                pixelBuffer = WebPPictureImportAuto(bmp, ref wpic);

                // Set up a byte-writing method (write-to-memory, in this case)
                WebPMemoryCopyBuffer webPMemoryBuffer = new WebPMemoryCopyBuffer(this.ManagedChunkPool, false);
                Delegate somedeed = new NativeDelegates.WebPDataWriterCallback(webPMemoryBuffer.MyWriter);
                wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);
                webPMemoryBuffer.ToReadOnly();
                //compress the input samples
                if (this.library.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());

                return new WebPImage(this.library, webPMemoryBuffer);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeNearLossless"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();

                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Encode bitmap to WebP with given option and return the encoded image. (Advanced API)</summary>
        /// <param name="bmp">Bitmap with the WebP image</param>
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
                options.ApplyConfigStruct(this.library, ref config);

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
                wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);
                //compress the input samples
                if (this.library.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());
                webPMemoryBuffer.ToReadOnly();
                return new WebPImage(this.library, webPMemoryBuffer);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.Encode"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();

                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }

        /// <summary>Encode bitmap to WebP with given option and write to a file. (Advanced API)</summary>
        /// <param name="bmp">Bitmap with the WebP image</param>
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
                options.ApplyConfigStruct(this.library, ref config);

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
                    wpic.writer = Marshal.GetFunctionPointerForDelegate(somedeed);

                    //compress the input samples
                    if (this.library.WebPEncode(ref config, ref wpic) != 1)
                        throw new Exception("Encoding error: " + ((WebPEncodingError)wpic.error_code).ToString());
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.EncodeNearLossless"); }
            finally
            {
                if (pixelBuffer != null)
                    pixelBuffer.Dispose();
                if (wpic.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpic);
            }
        }
        #endregion

        #region Private Functions
        private PixelBuffer WebPPictureImportAuto(BitmapSource bmp, ref WebPPicture wpic)
        {
            PixelBuffer pixelBuffer;

            wpic.width = bmp.PixelWidth;
            wpic.height = bmp.PixelHeight;
            wpic.use_argb = 1;

            /*
            Compare GUID of pixel formats.
            According to .NET's source code: https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/System/Windows/Media/PixelFormat.cs,465
            `==` and PixelFormat.Equals(PixelFormat) do the same. Use `==` should be easier to look.
            */
            if (bmp.Format == PixelFormats.Bgr24)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (this.library.WebPPictureImportBGR(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (this.library.WebPPictureImportBGRX(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (this.library.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32);
                if (this.library.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (this.library.WebPPictureImportRGB(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportRGB");
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32);
                if (this.library.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    pixelBuffer = new PixelBuffer(bmp);
                    if (this.library.WebPPictureImportBGR(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                        throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    pixelBuffer = new PixelBuffer(bmp);
                    if (this.library.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                        throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return pixelBuffer;
        }

        private int WebPPictureImportAuto(PixelFormat pixelFormat, ref WebPPicture wpic, IntPtr buffer, int stride)
        {
            if (pixelFormat == PixelFormats.Bgr24)
                return this.library.WebPPictureImportBGR(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Bgr32)
                return this.library.WebPPictureImportBGRX(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Bgra32)
                return this.library.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Pbgra32)
                return this.library.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Rgb24)
                return this.library.WebPPictureImportRGB(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Indexed8)
                return this.library.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else
            {
                if (pixelFormat.BitsPerPixel == 24)
                    return this.library.WebPPictureImportBGR(ref wpic, buffer, stride);
                else if (pixelFormat.BitsPerPixel == 32)
                    return this.library.WebPPictureImportBGRA(ref wpic, buffer, stride);
                else
                    throw new NotSupportedException("Image format not supported.");
            }
        }

        private int WebPEncodeSimple(BitmapSource bmp, float quality, out IntPtr output)
        {
            int size = 0;
            if (bmp.Format == PixelFormats.Bgr24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                    size = this.library.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = this.library.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeRGB(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = this.library.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                        size = this.library.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                        size = this.library.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return size;
        }

        private int WebPEncodeLosslessSimple(BitmapSource bmp, out IntPtr output)
        {
            int size = 0;
            if (bmp.Format == PixelFormats.Bgr24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                    size = this.library.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = this.library.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = this.library.WebPEncodeLosslessRGB(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = this.library.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                        size = this.library.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                        size = this.library.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return size;
        }
        #endregion

        #region | Another Public Functions |
        /// <summary>Get the libwebp version</summary>
        /// <returns>Version of library</returns>
        public Version GetVersion()
        {
            try
            {
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
                var revision = v % 256;
                var minor = (v >> 8) % 256;
                var major = (v >> 16) % 256;
                return new Version((int)major, (int)minor, (int)revision, 0);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.GetVersion"); }
        }

        /// <summary>
        /// Return low-level access to unmanaged code. USE IT AT YOUR OWN RISK.
        /// </summary>
        /// <returns></returns>
        public ILibwebp GetDirectAccessToLibrary()
        {
            return this.library;
        }

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
        public static bool IsWebP(Stream stream)
        {
            return IsWebP(stream, true);
        }

        private static bool IsWebP(Stream stream, bool returnToOldPos)
        {
            if (returnToOldPos && !stream.CanSeek)
                throw new NotSupportedException("Sorry");

            byte[] buffer = new byte[12];
            // Read the 12 bytes:
            // 4: ASCII "RIFF"
            // 4: Image size in byte
            // 4: ASCII "RIFF"
            try
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (returnToOldPos)
                    stream.Seek(-12, SeekOrigin.Current);
                if (read == 12)
                {
                    if (System.Text.Encoding.ASCII.GetString(buffer, 0, 4) == "RIFF")
                        if (System.Text.Encoding.ASCII.GetString(buffer, 8, 4) == "WEBP")
                            return true;
                }
            }
            finally
            {
                buffer = null;
            }
            return false;
        }

        /// <summary>Get info of WEBP data</summary>
        /// <param name="rawWebP">The data of WebP</param>
        public WebPHeader GetInfo(byte[] rawWebP)
        {
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            try
            {
                return GetInfo(pinnedWebP.AddrOfPinnedObject(), rawWebP.Length);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.GetInfo"); }
            finally
            {
                //Free memory
                if (pinnedWebP.IsAllocated)
                    pinnedWebP.Free();
            }
        }

        /// <summary>Get info of WEBP data</summary>
        /// <param name="memoryPointer">Memonry pointer where the data start</param>
        /// <param name="length">The length of the memory</param>
        /// <returns></returns>
        public WebPHeader GetInfo(IntPtr memoryPointer, int length)
        {
            VP8StatusCode result;

            try
            {
                WebPBitstreamFeatures features = new WebPBitstreamFeatures();
                result = this.library.WebPGetFeatures(memoryPointer, length, ref features);

                if (result != 0)
                    throw new Exception(result.ToString());

                return new WebPHeader(ref features);
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.GetInfo"); }
            finally
            {
            }
        }

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures. Warning: this function is rather CPU-intensive.</summary>
        /// <param name="source">Picture to measure</param>
        /// <param name="reference">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <returns>dB in the Y/U/V/Alpha/All order</returns>
        public float[] GetPictureDistortion(BitmapSource source, BitmapSource reference, int metric_type)
        {
            WebPPicture wpicSource = new WebPPicture();
            WebPPicture wpicReference = new WebPPicture();
            float[] result = new float[5];
            GCHandle pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr memoryPointer = IntPtr.Zero;

            try
            {
                if (source == null)
                    throw new Exception("Source picture is void");
                if (reference == null)
                    throw new Exception("Reference picture is void");
                if (metric_type > 2)
                    throw new Exception("Bad metric_type. Use 0 = PSNR, 1 = SSIM, 2 = LSIM");
                if (source.Width != reference.Width || source.Height != reference.Height)
                    throw new Exception("Source and Reference pictures have diferent dimensions");

                int src_width = source.PixelWidth,
                    src_height = source.PixelHeight,
                    src_stride = src_width * (source.Format.BitsPerPixel / 8),
                    src_size = src_height * src_stride,
                    ref_width = reference.PixelWidth,
                    ref_height = reference.PixelHeight,
                    ref_stride = ref_width * (reference.Format.BitsPerPixel / 8),
                    ref_size = ref_height * ref_stride;

                memoryPointer = Marshal.AllocHGlobal(Math.Max(src_size, ref_size));
                source.CopyPixels(Int32Rect.Empty, memoryPointer, 1024, src_stride);

                // Setup the source picture data, allocating the bitmap, width and height
                wpicSource = new WebPPicture();
                if (this.library.WebPPictureInitInternal(ref wpicSource) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpicSource.width = src_width;
                wpicSource.height = src_height;
                wpicSource.use_argb = 1;
                if (WebPPictureImportAuto(source.Format, ref wpicSource, memoryPointer, src_stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                reference.CopyPixels(Int32Rect.Empty, memoryPointer, 1024, ref_stride);

                // Setup the reference picture data, allocating the bitmap, width and height
                wpicReference = new WebPPicture();
                if (this.library.WebPPictureInitInternal(ref wpicReference) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpicReference.width = ref_width;
                wpicReference.height = ref_height;
                wpicReference.use_argb = 1;
                if (WebPPictureImportAuto(reference.Format, ref wpicReference, memoryPointer, ref_stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                //Measure
                IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();
                if (this.library.WebPPictureDistortion(ref wpicSource, ref wpicReference, metric_type, ptrResult) != 1)
                    throw new Exception("Can´t measure.");
                return result;
            }
            catch (Exception ex) { throw new Exception(ex.Message + "\r\nIn WebP.GetPictureDistortion"); }
            finally
            {
                //Free memory
                if (wpicSource.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpicSource);
                if (wpicReference.argb != IntPtr.Zero)
                    this.library.WebPPictureFree(ref wpicReference);
                if (memoryPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(memoryPointer);
                //Free memory
                if (pinnedResult.IsAllocated)
                    pinnedResult.Free();
            }
        }
        #endregion

        #region | Destruction |
        /// <summary>Free memory</summary>
        private int _disposed = 0;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) == 0)
            {
                Libwebp.DeInit(this);

                this.ManagedChunkPool = null;
                this.library = null;

                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
