using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.Helper;
using System.Collections.Generic;

namespace WebPWrapper.WPF.UnmanagedLibrary
{
    /// <summary>
    /// Provides managed function wrapper for unmanaged libwebp library
    /// </summary>
    internal class Libwebp : ILibwebp, IDisposable
    {
        public const int WEBP_DECODER_ABI_VERSION = 0x0208;
        private static readonly ConcurrentDictionary<string, Libwebp> cache = new ConcurrentDictionary<string, Libwebp>(StringComparer.OrdinalIgnoreCase);
        
        private int partners;
        private SafeLibraryHandle libraryHandle;
        private ConcurrentDictionary<string, Delegate> _methods;
        private string _libpath;

        /// <summary>
        /// Full path to the imported library
        /// </summary>
        public string LibraryPath => this._libpath;

        internal Libwebp(string myLibPath)
        {
            if (string.IsNullOrWhiteSpace(myLibPath))
                throw new ArgumentNullException("myLibPath");
            // if (!File.Exists(myLibPath)) throw new FileNotFoundException("Library not found", myLibPath);

            this.LoadLib(Path.GetFullPath(myLibPath));
            Interlocked.Exchange(ref this.partners, 0);
        }

        internal static Libwebp Init(WebP who, string library_path)
        {
            if (string.IsNullOrWhiteSpace(library_path))
                throw new ArgumentNullException("library_path");

            Libwebp myLib;
            library_path = Path.GetFullPath(library_path);
            if (!cache.TryGetValue(library_path, out myLib))
            {
                myLib = new Libwebp(library_path);
                cache.TryAdd(library_path, myLib);
            }

            Interlocked.Increment(ref myLib.partners);

            return myLib;
        }

        internal static void DeInit(WebP who)
        {
            Libwebp myLib = who.library;
            string library_path = who.library._libpath;
            if (Interlocked.Decrement(ref myLib.partners) == 0)
            {
                if (cache.TryRemove(library_path, out myLib))
                {
                    // Unload the library when there is no WebP instance use the library anymore.
                    myLib.Dispose();
                    myLib = null;
                }
            }
        }

        private void LoadLib(string path)
        {
            if (this.libraryHandle != null)
                return;

#if DEBUG
            // For debugging only
            var header = PEReader.GetExecutableInfo(path);
            if (RuntimeValue.is64bit == header.Is32bitAssembly())
            {
                if (RuntimeValue.is64bit)
                {
                    throw new InvalidOperationException("Cannot load a 32-bit library to current process (which is a 64-bit process)");
                }
                else
                {
                    throw new InvalidOperationException("Cannot load a 64-bit library to current process (which is a 32-bit process)");
                }
            }
#endif
            // Load the library
            var handle = UnsafeNativeMethods.LoadLibrary(path);
            if (handle.IsInvalid)
            {
                int hr = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hr);
            }
            else
            {
                // Check if the library is really libwebp
                this._methods = new ConcurrentDictionary<string, Delegate>();

                // WebPConfigInitInternal
                this._methods.TryAdd("WebPConfigInitInternal", AssertFunctionLoadFailure<NativeDelegates.WebPConfigInitInternal>(ref handle, "WebPConfigInitInternal"));

                // WebPConfigInitInternal
                this._methods.TryAdd("WebPGetFeaturesInternal", AssertFunctionLoadFailure<NativeDelegates.WebPGetFeaturesInternal>(ref handle, "WebPGetFeaturesInternal"));

                // WebPConfigLosslessPreset
                this._methods.TryAdd("WebPConfigLosslessPreset", AssertFunctionLoadFailure<NativeDelegates.WebPConfigLosslessPreset>(ref handle, "WebPConfigLosslessPreset"));

                // WebPValidateConfig
                this._methods.TryAdd("WebPValidateConfig", AssertFunctionLoadFailure<NativeDelegates.WebPValidateConfig>(ref handle, "WebPValidateConfig"));

                // WebPPictureInitInternal
                this._methods.TryAdd("WebPPictureInitInternal", AssertFunctionLoadFailure<NativeDelegates.WebPPictureInitInternal>(ref handle, "WebPPictureInitInternal"));

                // WebPPictureImportBGR
                this._methods.TryAdd("WebPPictureImportBGR", AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>(ref handle, "WebPPictureImportBGR"));

                // WebPPictureImportBGRA
                this._methods.TryAdd("WebPPictureImportBGRA", AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>(ref handle, "WebPPictureImportBGRA"));

                // WebPPictureImportRGB
                this._methods.TryAdd("WebPPictureImportRGB", AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>(ref handle, "WebPPictureImportRGB"));

                // WebPPictureImportRGBA
                this._methods.TryAdd("WebPPictureImportRGBA", AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>(ref handle, "WebPPictureImportRGBA"));

                // WebPPictureImportBGRX
                this._methods.TryAdd("WebPPictureImportBGRX", AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>(ref handle, "WebPPictureImportBGRX"));

                // WebPEncode
                this._methods.TryAdd("WebPEncode", AssertFunctionLoadFailure<NativeDelegates.WebPEncode>(ref handle, "WebPEncode"));

                // WebPPictureFree
                this._methods.TryAdd("WebPPictureFree", AssertFunctionLoadFailure<NativeDelegates.WebPPictureFree>(ref handle, "WebPPictureFree"));

                // WebPGetInfo
                this._methods.TryAdd("WebPGetInfo", AssertFunctionLoadFailure<NativeDelegates.WebPGetInfo>(ref handle, "WebPGetInfo"));

                // WebPDecodeBGRInto
                this._methods.TryAdd("WebPDecodeBGRInto", AssertFunctionLoadFailure<NativeDelegates.WebPDecodeBGRInto>(ref handle, "WebPDecodeBGRInto"));

                // WebPInitDecoderConfigInternal
                this._methods.TryAdd("WebPInitDecoderConfigInternal", AssertFunctionLoadFailure<NativeDelegates.WebPInitDecoderConfigInternal>(ref handle, "WebPInitDecoderConfigInternal"));

                // WebPDecode
                this._methods.TryAdd("WebPDecode", AssertFunctionLoadFailure<NativeDelegates.WebPDecode>(ref handle, "WebPDecode"));

                // WebPFreeDecBuffer
                this._methods.TryAdd("WebPFreeDecBuffer", AssertFunctionLoadFailure<NativeDelegates.WebPFreeDecBuffer>(ref handle, "WebPFreeDecBuffer"));

                // WebPEncodeBGR
                this._methods.TryAdd("WebPEncodeBGR", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>(ref handle, "WebPEncodeBGR"));

                // WebPEncodeRGB
                this._methods.TryAdd("WebPEncodeRGB", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>(ref handle, "WebPEncodeRGB"));

                // WebPEncodeBGRA
                this._methods.TryAdd("WebPEncodeBGRA", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>(ref handle, "WebPEncodeBGRA"));

                // WebPEncodeRGBA
                this._methods.TryAdd("WebPEncodeRGBA", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>(ref handle, "WebPEncodeRGBA"));

                // WebPEncodeLosslessBGR
                this._methods.TryAdd("WebPEncodeLosslessBGR", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>(ref handle, "WebPEncodeLosslessBGR"));

                // WebPEncodeLosslessBGRA
                this._methods.TryAdd("WebPEncodeLosslessBGRA", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>(ref handle, "WebPEncodeLosslessBGRA"));

                // WebPEncodeLosslessRGB
                this._methods.TryAdd("WebPEncodeLosslessRGB", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>(ref handle, "WebPEncodeLosslessRGB"));

                // WebPEncodeLosslessRGBA
                this._methods.TryAdd("WebPEncodeLosslessRGBA", AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>(ref handle, "WebPEncodeLosslessRGBA"));

                // WebPFree
                this._methods.TryAdd("WebPFree", AssertFunctionLoadFailure<NativeDelegates.WebPFree>(ref handle, "WebPFree"));

                // WebPGetDecoderVersion
                this._methods.TryAdd("WebPGetDecoderVersion", AssertFunctionLoadFailure<NativeDelegates.WebPGetDecoderVersion>(ref handle, "WebPGetDecoderVersion"));

                // WebPPictureDistortion
                this._methods.TryAdd("WebPPictureDistortion", AssertFunctionLoadFailure<NativeDelegates.WebPPictureDistortion>(ref handle, "WebPPictureDistortion"));

                this.libraryHandle = handle;
                this._libpath = path;
            }
        }

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality">Quality of compresion</param>
        /// <returns>0 if error</returns>
        public int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPConfigInitInternal)this._methods["WebPConfigInitInternal"];
            return @delegate.Invoke(ref config, preset, quality, WEBP_DECODER_ABI_VERSION);
        }

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        public VP8StatusCode WebPGetFeatures(IntPtr rawWebP, uint data_size, ref WebPBitstreamFeatures features)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPGetFeaturesInternal)this._methods["WebPGetFeaturesInternal"];
            return (VP8StatusCode)@delegate.Invoke(rawWebP, new UIntPtr(data_size), ref features, WEBP_DECODER_ABI_VERSION);
        }

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        public VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features)
        {
            this.AssertLibraryCallFailure();
            return this.WebPGetFeatures(rawWebP, Convert.ToUInt32(data_size), ref features);
        }

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        public int WebPConfigLosslessPreset(ref WebPConfig config, int level)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPConfigLosslessPreset)this._methods["WebPConfigLosslessPreset"];
            return @delegate.Invoke(ref config, level);
        }

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <returns>1 if config are OK</returns>
        public int WebPValidateConfig(ref WebPConfig config)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPValidateConfig)this._methods["WebPValidateConfig"];
            return @delegate.Invoke(ref config);
        }

        /// <summary>Init the struct WebPPicture ckecking the dll version</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <returns>1 if not error</returns>
        public int WebPPictureInitInternal(ref WebPPicture wpic)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureInitInternal)this._methods["WebPPictureInitInternal"];
            return @delegate.Invoke(ref wpic, WEBP_DECODER_ABI_VERSION);
        }

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to BGR data</param>
        /// <param name="stride">stride of BGR data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureImportAuto)this._methods["WebPPictureImportBGR"];
            return @delegate.Invoke(ref wpic, bgr, stride);
        }

        /// <summary>
        /// Colorspace conversion function to import BGRA samples.
        /// </summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgba">Point to BGRA data</param>
        /// <param name="stride">stride of BGRA data</param>
        /// <returns></returns>
        public int WebPPictureImportBGRA(ref WebPPicture wpic, IntPtr rgba, int stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureImportAuto)this._methods["WebPPictureImportBGRA"];
            return @delegate.Invoke(ref wpic, rgba, stride);
        }

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgb">Point to RGB data</param>
        /// <param name="stride">stride of RGB data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportRGB(ref WebPPicture wpic, IntPtr rgb, int stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureImportAuto)this._methods["WebPPictureImportRGB"];
            return @delegate.Invoke(ref wpic, rgb, stride);
        }

        /// <summary>Colorspace conversion function to import RGBA samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to RGBA data</param>
        /// <param name="stride">stride of RGBA data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportRGBA(ref WebPPicture wpic, IntPtr rgba, int stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureImportAuto)this._methods["WebPPictureImportRGBA"];
            return @delegate.Invoke(ref wpic, rgba, stride);
        }

        /// <summary>Colorspace conversion function to import BGRX samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgrx">Point to BGRX data</param>
        /// <param name="stride">stride of BGRX data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgrx, int stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureImportAuto)this._methods["WebPPictureImportBGRX"];
            return @delegate.Invoke(ref wpic, bgrx, stride);
        }

        /// <summary>Compress to webp format</summary>
        /// <param name="config">The config struct for compresion parameters</param>
        /// <param name="picture">'picture' hold the source samples in both YUV(A) or ARGB input</param>
        /// <returns>Returns 0 in case of error, 1 otherwise. In case of error, picture->error_code is updated accordingly.</returns>
        public int WebPEncode(ref WebPConfig config, ref WebPPicture picture)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncode)this._methods["WebPEncode"];
            return @delegate.Invoke(ref config, ref picture);
        }

        /// <summary>Release the memory allocated by WebPPictureAlloc() or WebPPictureImport*()
        /// Note that this function does _not_ free the memory used by the 'picture' object itself.
        /// Besides memory (which is reclaimed) all other fields of 'picture' are preserved.</summary>
        /// <param name="picture">Picture struct</param>
        public void WebPPictureFree(ref WebPPicture picture)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureFree)this._methods["WebPPictureFree"];
            @delegate.Invoke(ref picture);
        }

        /// <summary>Validate the WebP image header and retrieve the image height and width. Pointers *width and *height can be passed NULL if deemed irrelevant</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        public int WebPGetInfo(IntPtr data, int data_size, out int width, out int height)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPGetInfo)this._methods["WebPGetInfo"];
            return @delegate.Invoke(data, (UIntPtr)data_size, out width, out height);
        }

        /// <summary>Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeBGRInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPDecodeBGRInto)this._methods["WebPDecodeBGRInto"];
            return @delegate.Invoke(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride);
        }

        /// <summary>Initialize the configuration as empty. This function must always be called first, unless WebPGetFeatures() is to be called.</summary>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>False in case of mismatched version.</returns>
        public int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPInitDecoderConfigInternal)this._methods["WebPInitDecoderConfigInternal"];
            return @delegate.Invoke(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION);
        }

        /// <summary>Decodes the full data at once, taking 'config' into account.</summary>
        /// <param name="data">WebP raw data to decode</param>
        /// <param name="data_size">Size of WebP data </param>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>VP8_STATUS_OK if the decoding was successful</returns>
        public VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPDecode)this._methods["WebPDecode"];
            return @delegate.Invoke(data, (UIntPtr)data_size, ref webPDecoderConfig);
        }

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">WebPDecBuffer</param>
        public void WebPFreeDecBuffer(ref WebPDecBuffer buffer)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPFreeDecBuffer)this._methods["WebPFreeDecBuffer"];
            @delegate.Invoke(ref buffer);
        }

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeAuto)this._methods["WebPEncodeBGR"];
            return @delegate.Invoke(bgr, width, height, stride, quality_factor, out output);
        }

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public int WebPEncodeRGB(IntPtr rgb, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeAuto)this._methods["WebPEncodeRGB"];
            return @delegate.Invoke(rgb, width, height, stride, quality_factor, out output);
        }

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public int WebPEncodeBGRA(IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeAuto)this._methods["WebPEncodeBGRA"];
            return @delegate.Invoke(bgra, width, height, stride, quality_factor, out output);
        }

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public int WebPEncodeRGBA(IntPtr rgba, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeAuto)this._methods["WebPEncodeRGBA"];
            return @delegate.Invoke(rgba, width, height, stride, quality_factor, out output);
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeLosslessAuto)this._methods["WebPEncodeLosslessBGR"];
            return @delegate.Invoke(bgr, width, height, stride, out output);
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessBGRA(IntPtr bgra, int width, int height, int stride, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeLosslessAuto)this._methods["WebPEncodeLosslessBGRA"];
            return @delegate.Invoke(bgra, width, height, stride, out output);
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessRGB(IntPtr rgb, int width, int height, int stride, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeLosslessAuto)this._methods["WebPEncodeLosslessRGB"];
            return @delegate.Invoke(rgb, width, height, stride, out output);
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public int WebPEncodeLosslessRGBA(IntPtr rgba, int width, int height, int stride, out IntPtr output)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPEncodeLosslessAuto)this._methods["WebPEncodeLosslessRGBA"];
            return @delegate.Invoke(rgba, width, height, stride, out output);
        }

        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="p">Pointer to memory</param>
        public void WebPFree(IntPtr p)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPFree)this._methods["WebPFree"];
            @delegate.Invoke(p);
        }

        /// <summary>Get the webp version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        public object WebPGetDecoderVersion2()
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPGetDecoderVersion)this._methods["WebPGetDecoderVersion"];
            return (object)@delegate.Invoke();
        }

        /// <summary>Get the webp version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        public int WebPGetDecoderVersion()
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPGetDecoderVersion)this._methods["WebPGetDecoderVersion"];
            return (int)@delegate.Invoke();
        }

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures.</summary>
        /// <param name="srcPicture">Picture to measure</param>
        /// <param name="refPicture">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="pResult">dB in the Y/U/V/Alpha/All order</param>
        /// <returns>False in case of error (src and ref don't have same dimension, ...)</returns>
        public int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult)
        {
            this.AssertLibraryCallFailure();
            var @delegate = (NativeDelegates.WebPPictureDistortion)this._methods["WebPPictureDistortion"];
            return @delegate.Invoke(ref srcPicture, ref refPicture, metric_type, pResult);
        }

        /// <summary>
        /// Invoke a function of the library. (Warning: Low Performance because of <see cref="Delegate.DynamicInvoke(object[])"/>)
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="args">Arguments for the function</param>
        /// <returns></returns>
        public object Invoke(string functionName, params object[] args)
        {
            this.AssertLibraryCallFailure();
            if (this._methods.TryGetValue(functionName, out var delega))
            {
                return delega.DynamicInvoke(args);
            }
            else
            {
                IntPtr p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                // Failure is a common case, especially for adaptive code.
                if (p == IntPtr.Zero)
                    throw new EntryPointNotFoundException($"Function '{functionName}' not found");

                Delegate function = Marshal.GetDelegateForFunctionPointer(p, typeof(Delegate));
                this._methods.TryAdd(functionName, function);
                return function.DynamicInvoke(args);
            }
        }

        /// <summary>
        /// Attempt to search the function with the given name and type.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate which will define the overload</typeparam>
        /// <param name="functionName">The name to search for the function</param>
        /// <param name="function">The first result of the search if it's found.</param>
        /// <returns>A boolean determine whether the function is found or not</returns>
        /// <exception cref="ObjectDisposedException">The library has been unloaded</exception>
        /// <exception cref="InvalidOperationException">The library wasn not loaded successfully</exception>
        public bool TryGetFunction<TDelegate>(string functionName, out TDelegate function) where TDelegate : Delegate
        {
            this.AssertLibraryCallFailure();
            if (this._methods.TryGetValue(functionName, out var @delegate))
            {
                if (@delegate is TDelegate result)
                {
                    function = result;
                    return true;
                }
                else
                {
                    function = default;
                    return false;
                }
            }
            else
            {
                var e = FindFunctionPointers<TDelegate>(ref this.libraryHandle, functionName);
                IntPtr p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                // Failure is a common case, especially for adaptive code.
                if (p == IntPtr.Zero)
                {
                    function = default;
                    return false;
                }

                TDelegate foundFunction = (TDelegate)Marshal.GetDelegateForFunctionPointer(p, typeof(TDelegate));
                this._methods.TryAdd(functionName, foundFunction);
                function = foundFunction;
                return true;
            }
        }

        /// <summary>
        /// Return a boolean whether the given function name is existed or not.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>Return a boolean whether the given function name is existed or not.</returns>
        public bool IsMethodExists(string functionName)
        {
            this.AssertLibraryCallFailure();
            if (this._methods.TryGetValue(functionName, out var throwAway))
            {
                return true;
            }
            else
            {
                IntPtr p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                // Failure is a common case, especially for adaptive code.
                if (p == IntPtr.Zero)
                    return false;

                this._methods.TryAdd(functionName, Marshal.GetDelegateForFunctionPointer(p, typeof(Delegate)));
                return true;
            }
        }

        private static TDelegate AssertFunctionLoadFailure<TDelegate>(ref SafeLibraryHandle handle, string functionName, bool throwOnNotFound = true) where TDelegate : Delegate
        {
            TDelegate func_delegate = FindFunctionPointers<TDelegate>(ref handle, functionName);
            if (throwOnNotFound && (func_delegate == null))
            {
                handle.Close();
                handle.Dispose();
                throw new FileLoadException($"Function '{(typeof(TDelegate)).ToString()}' not found in the library. Wrong library or wrong version?");
            }
            return func_delegate;
        }

        private void AssertLibraryCallFailure()
        {
            if (this.libraryHandle == null || this.libraryHandle.IsInvalid)
                throw new InvalidOperationException("The library was not loaded successfully.");
            if (this.libraryHandle.IsClosed)
                throw new ObjectDisposedException("libwebp", "The library has been unloaded. Cannot reload the library.");
        }

        /// <summary>
        /// Dynamically lookup a function in the dll via kernel32!GetProcAddress.
        /// </summary>
        /// <param name="functionName">raw name of the function in the export table.</param>
        /// <returns>null if function is not found. Else a delegate to the unmanaged function.
        /// </returns>
        /// <remarks>GetProcAddress results are valid as long as the dll is not yet unloaded. This
        /// is very very dangerous to use since you need to ensure that the dll is not unloaded
        /// until after you're done with any objects implemented by the dll. For example, if you
        /// get a delegate that then gets an IUnknown implemented by this dll,
        /// you can not dispose this library until that IUnknown is collected. Else, you may free
        /// the library and then the CLR may call release on that IUnknown and it will crash.</remarks>
        private static TDelegate FindFunctionPointers<TDelegate>(ref SafeLibraryHandle handle,  string functionName) where TDelegate : Delegate
        {
            IntPtr p = UnsafeNativeMethods.GetProcAddress(handle, functionName);
            // Failure is a common case, especially for adaptive code.
            if (p == IntPtr.Zero)
                return null;

            return (TDelegate)Marshal.GetDelegateForFunctionPointer(p, typeof(TDelegate));
        }

        public void Dispose()
        {
            if (this.libraryHandle == null || this.libraryHandle.IsClosed || this.libraryHandle.IsInvalid)
                return;

            if (this._methods != null)
                this._methods.Clear();

            this._methods = null;

            this.libraryHandle.Close();
            this.libraryHandle.Dispose();
        }
    }
}
