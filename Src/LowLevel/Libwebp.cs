using System;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.Helper;

namespace WebPWrapper.WPF.LowLevel
{
    /// <summary>
    /// Provides managed function wrapper for unmanaged libwebp library
    /// </summary>
    internal class Libwebp : ILibwebp, IDisposable
    {
        private static readonly ConcurrentDictionary<string, Libwebp> cache = new ConcurrentDictionary<string, Libwebp>(StringComparer.OrdinalIgnoreCase);
        
        private int partners;
        private SafeLibraryHandle libraryHandle;
        private ConcurrentDictionary<Type, Delegate> _methods;
        private ConcurrentDictionary<string, IntPtr> _functionPointer;
        private string _libpath;
        private bool _canEncode, _canDecode;

        /// <summary>
        /// Full path to the imported library
        /// </summary>
        public string LibraryPath => this._libpath;

        /// <summary>
        /// Gets a value indicating whether the current loaded library supports WebP encoding.
        /// </summary>
        public bool CanEncode => this._canEncode;

        /// <summary>
        /// Gets a value indicating whether the current loaded library supports WebP decoding.
        /// </summary>
        public bool CanDecode => this._canDecode;

        /// <summary>Huh??</summary>
        /// <param name="myLibPath">Library path to load</param>
        /// <param name="preload">True to load the library without load all needed functions.</param>
        /// <remarks>
        /// Preloading will only load the library, then find the function pointers on demand. This will allow to load seamlessly all sub-build of the libwebp: decode-only library, encode-only library and all-in-one library.
        /// Without preload, all functions will be found at library load. This means only all-in-one libwebp library is qualified, any other sub-build may give error because of missing functions.
        /// </remarks>
        internal Libwebp(string myLibPath, bool preload)
        {
            if (string.IsNullOrWhiteSpace(myLibPath))
                throw new ArgumentNullException("myLibPath");
            // if (!File.Exists(myLibPath)) throw new FileNotFoundException("Library not found", myLibPath);

            this._canEncode = false;
            this._canDecode = false;

            this.LoadLib(myLibPath, preload);
            Interlocked.Exchange(ref this.partners, 0);
        }

        internal static Libwebp Init(WebP who, string library_path)
        {
            if (string.IsNullOrWhiteSpace(library_path))
                throw new ArgumentNullException("library_path");

            Libwebp myLib;
            if (!cache.TryGetValue(library_path, out myLib))
            {
                // Let's leave the full-load library for another time.
                // Make use of LoadLibrary to search for the library
                var loadedLib = new Libwebp(library_path, true);

                // Check if the library is already existed in cache
                var addedOrNot = cache.GetOrAdd(loadedLib.LibraryPath, loadedLib);

                if (addedOrNot == loadedLib)
                {
                    // It's added successfully. Which means loadedLib is (in a valid way) the first-arrive in cache.
                    // Let's use it.
                    myLib = loadedLib;
                }
                else
                {
                    // It's failed. Which means another library is already added before this.
                    // Let's dispose the current one (unload library) and use the already existed one.
                    loadedLib.Dispose();
                    myLib = addedOrNot;
                }
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

        private void LoadLib(string path, bool preload)
        {
            if (this.libraryHandle != null)
                return;

            /*
            // For debugging only
            PEReader header;
            System.Reflection.AssemblyName dotnetAssemblyName;
            try
            {
                dotnetAssemblyName = System.Reflection.AssemblyName.GetAssemblyName(path);

                throw new BadImageFormatException($"'{path}' is a .NET library. This is probably not libwebp library. Wrong path?");
            }
            catch
            {
                header = PEReader.GetExecutableInfo(path);
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
            }
            finally
            {
                dotnetAssemblyName = null;
                header = null;
            }
            */

            // Load the library
            var handle = UnsafeNativeMethods.LoadLibrary(path);
            if (handle.IsInvalid)
            {
                int hr = Marshal.GetHRForLastWin32Error();
                handle.Dispose();
                handle = null;
                Marshal.ThrowExceptionForHR(hr);
            }
            else
            {
                this._functionPointer = new ConcurrentDictionary<string, IntPtr>();
                this._methods = new ConcurrentDictionary<Type, Delegate>();
                this.libraryHandle = handle;
                System.Text.StringBuilder sb = new System.Text.StringBuilder(Define.MAX_PATH);
                if (UnsafeNativeMethods.GetModuleFileName(handle, sb, sb.Capacity) > 0)
                {
                    this._libpath = sb.ToString();
                }
                else
                {
                    this._libpath = path;
                }

                // Ensure that we're loading libwebp library.
                // Assert decoding
                if (this.IsFunctionExists("WebPGetDecoderVersion"))
                {
                    this._canDecode = true;
                }
                // Assert encoding
                if (this.IsFunctionExists("WebPGetEncoderVersion"))
                {
                    this._canEncode = true;
                }

                if (!this._canEncode && !this._canDecode)
                {
                    this.Dispose();
                    throw new FileLoadException("Cannot find either 'WebPGetDecoderVersion' or 'WebPGetDecoderVersion' function in the library. Wrong library or wrong version?");
                }

                // Check if the library is really a FULL libwebp (with both encode and decode functions)
                if (!preload)
                {
                    // WebPConfigInitInternal
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPConfigInitInternal>("WebPConfigInitInternal");

                    // WebPConfigInitInternal
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetFeaturesInternal>("WebPGetFeaturesInternal");

                    // WebPConfigLosslessPreset
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPConfigLosslessPreset>("WebPConfigLosslessPreset");

                    // WebPValidateConfig
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPValidateConfig>("WebPValidateConfig");

                    // WebPPictureInitInternal
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureInitInternal>("WebPPictureInitInternal");

                    // WebPPictureImportBGR
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGR");

                    // WebPPictureImportBGRA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGRA");

                    // WebPPictureImportRGB
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportRGB");

                    // WebPPictureImportRGBA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportRGBA");

                    // WebPPictureImportBGRX
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGRX");

                    // WebPEncode
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncode>("WebPEncode");

                    // WebPPictureFree
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPPictureFree>("WebPPictureFree");

                    // WebPGetInfo
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetInfo>("WebPGetInfo");

                    // WebPDecodeRGBAInto
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBAInto");

                    // WebPDecodeARGBInto
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeARGBInto");

                    // WebPDecodeBGRAInto
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRAInto");

                    // WebPDecodeRGBInto
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBInto");

                    // WebPDecodeBGRInto
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRInto");

                    // WebPInitDecoderConfigInternal
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPInitDecoderConfigInternal>("WebPInitDecoderConfigInternal");

                    // WebPDecode
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPDecode>("WebPDecode");

                    // WebPFreeDecBuffer
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPFreeDecBuffer>("WebPFreeDecBuffer");

                    // WebPEncodeBGR
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>("WebPEncodeBGR");

                    // WebPEncodeRGB
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>("WebPEncodeRGB");

                    // WebPEncodeBGRA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>("WebPEncodeBGRA");

                    // WebPEncodeRGBA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeAuto>("WebPEncodeRGBA");

                    // WebPEncodeLosslessBGR
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessBGR");

                    // WebPEncodeLosslessBGRA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessBGRA");

                    // WebPEncodeLosslessRGB
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessRGB");

                    // WebPEncodeLosslessRGBA
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessRGBA");

                    // WebPFree
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPFree>("WebPFree");

                    // WebPGetDecoderVersion
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetVersion>("WebPGetDecoderVersion");

                    // WebPGetEncoderVersion
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetVersion>("WebPGetEncoderVersion");

                    // WebPPictureDistortion
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetVersion>("WebPPictureDistortion");
                }
            }
        }

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality">Quality of compresion</param>
        /// <returns>0 if error</returns>
        public int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            if (this.TryGetFunction< NativeDelegates.WebPConfigInitInternal >("WebPConfigInitInternal", out var @delegate))
            {
                return @delegate.Invoke(ref config, preset, quality, Define.WEBP_DECODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPConfigInitInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        public VP8StatusCode WebPGetFeatures(IntPtr rawWebP, uint data_size, ref WebPBitstreamFeatures features)
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetFeaturesInternal>("WebPGetFeaturesInternal", out var @delegate))
            {
                return (VP8StatusCode)@delegate.Invoke(rawWebP, new UIntPtr(data_size), ref features, Define.WEBP_DECODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetFeaturesInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        public VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features) => this.WebPGetFeatures(rawWebP, Convert.ToUInt32(data_size), ref features);

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        public int WebPConfigLosslessPreset(ref WebPConfig config, int level)
        {
            if (this.TryGetFunction<NativeDelegates.WebPConfigLosslessPreset>("WebPConfigLosslessPreset", out var @delegate))
            {
                return @delegate.Invoke(ref config, level);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPConfigLosslessPreset' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <returns>1 if config are OK</returns>
        public int WebPValidateConfig(ref WebPConfig config)
        {
            if (this.TryGetFunction<NativeDelegates.WebPValidateConfig>("WebPValidateConfig", out var @delegate))
            {
                return @delegate.Invoke(ref config);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPValidateConfig' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Init the struct WebPPicture ckecking the dll version</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <returns>1 if not error</returns>
        public int WebPPictureInitInternal(ref WebPPicture wpic)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureInitInternal>("WebPPictureInitInternal", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, Define.WEBP_DECODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureInitInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to BGR data</param>
        /// <param name="stride">stride of BGR data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGR", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, bgr, stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureImportBGR' function from the library. Wrong library or wrong version?");
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
            if (this.TryGetFunction<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGRA", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, rgba, stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureImportBGRA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgb">Point to RGB data</param>
        /// <param name="stride">stride of RGB data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportRGB(ref WebPPicture wpic, IntPtr rgb, int stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportRGB", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, rgb, stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureImportRGB' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Colorspace conversion function to import RGBA samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to RGBA data</param>
        /// <param name="stride">stride of RGBA data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportRGBA(ref WebPPicture wpic, IntPtr rgba, int stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportRGBA", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, rgba, stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureImportRGBA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Colorspace conversion function to import BGRX samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgrx">Point to BGRX data</param>
        /// <param name="stride">stride of BGRX data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgrx, int stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureImportAuto>("WebPPictureImportBGRX", out var @delegate))
            {
                return @delegate.Invoke(ref wpic, bgrx, stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureImportBGRX' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Compress to webp format</summary>
        /// <param name="config">The config struct for compresion parameters</param>
        /// <param name="picture">'picture' hold the source samples in both YUV(A) or ARGB input</param>
        /// <returns>Returns 0 in case of error, 1 otherwise. In case of error, picture->error_code is updated accordingly.</returns>
        public int WebPEncode(ref WebPConfig config, ref WebPPicture picture)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncode>("WebPEncode", out var @delegate))
            {
                return @delegate.Invoke(ref config, ref picture);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncode' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Release the memory allocated by WebPPictureAlloc() or WebPPictureImport*()
        /// Note that this function does _not_ free the memory used by the 'picture' object itself.
        /// Besides memory (which is reclaimed) all other fields of 'picture' are preserved.</summary>
        /// <param name="picture">Picture struct</param>
        public void WebPPictureFree(ref WebPPicture picture)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureFree>("WebPPictureFree", out var @delegate))
            {
                @delegate.Invoke(ref picture);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureFree' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Validate the WebP image header and retrieve the image height and width. Pointers *width and *height can be passed NULL if deemed irrelevant</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        public int WebPGetInfo(IntPtr data, int data_size, out int width, out int height)
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetInfo>("WebPGetInfo", out var @delegate))
            {
                return @delegate.Invoke(data, (UIntPtr)data_size, out width, out height);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetInfo' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decode WEBP image pointed to by *data and returns RGBA samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeRGBAInto(IntPtr data, uint data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBAInto", out var @delegate))
            {
                return @delegate.Invoke(data, new UIntPtr(data_size), output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecodeRGBAInto' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decode WEBP image pointed to by *data and returns ARGB samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeARGBInto(IntPtr data, uint data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeARGBInto", out var @delegate))
            {
                return @delegate.Invoke(data, new UIntPtr(data_size), output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecodeARGBInto' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decode WEBP image pointed to by *data and returns BGRA samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeBGRAInto(IntPtr data, uint data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRAInto", out var @delegate))
            {
                return @delegate.Invoke(data, new UIntPtr(data_size), output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecodeBGRAInto' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decode WEBP image pointed to by *data and returns RGB samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeRGBInto(IntPtr data, uint data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBInto", out var @delegate))
            {
                return @delegate.Invoke(data, new UIntPtr(data_size), output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecodeRGBInto' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public int WebPDecodeBGRInto(IntPtr data, uint data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRInto", out var @delegate))
            {
                return @delegate.Invoke(data, new UIntPtr(data_size), output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecodeBGRInto' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Initialize the configuration as empty. This function must always be called first, unless WebPGetFeatures() is to be called.</summary>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>False in case of mismatched version.</returns>
        public int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig)
        {
            if (this.TryGetFunction<NativeDelegates.WebPInitDecoderConfigInternal>("WebPInitDecoderConfigInternal", out var @delegate))
            {
                return @delegate.Invoke(ref webPDecoderConfig, Define.WEBP_DECODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPInitDecoderConfigInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Decodes the full data at once, taking 'config' into account.</summary>
        /// <param name="data">WebP raw data to decode</param>
        /// <param name="data_size">Size of WebP data </param>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>VP8_STATUS_OK if the decoding was successful</returns>
        public VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecode>("WebPDecode", out var @delegate))
            {
                return @delegate.Invoke(data, (UIntPtr)data_size, ref webPDecoderConfig);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecode' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">WebPDecBuffer</param>
        public void WebPFreeDecBuffer(ref WebPDecBuffer buffer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPFreeDecBuffer>("WebPFreeDecBuffer", out var @delegate))
            {
                @delegate.Invoke(ref buffer);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPFreeDecBuffer' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeAuto>("WebPEncodeBGR", out var @delegate))
            {
                return @delegate.Invoke(bgr, width, height, stride, quality_factor, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeBGR' function from the library. Wrong library or wrong version?");
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
        public int WebPEncodeRGB(IntPtr rgb, int width, int height, int stride, float quality_factor, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeAuto>("WebPEncodeRGB", out var @delegate))
            {
                return @delegate.Invoke(rgb, width, height, stride, quality_factor, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeRGB' function from the library. Wrong library or wrong version?");
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
        public int WebPEncodeBGRA(IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeAuto>("WebPEncodeBGRA", out var @delegate))
            {
                return @delegate.Invoke(bgra, width, height, stride, quality_factor, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeBGRA' function from the library. Wrong library or wrong version?");
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
        public int WebPEncodeRGBA(IntPtr rgba, int width, int height, int stride, float quality_factor, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeAuto>("WebPEncodeRGBA", out var @delegate))
            {
                return @delegate.Invoke(rgba, width, height, stride, quality_factor, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeRGBA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessBGR", out var @delegate))
            {
                return @delegate.Invoke(bgr, width, height, stride, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeLosslessBGR' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessBGRA(IntPtr bgra, int width, int height, int stride, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessBGRA", out var @delegate))
            {
                return @delegate.Invoke(bgra, width, height, stride, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeLosslessBGRA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public int WebPEncodeLosslessRGB(IntPtr rgb, int width, int height, int stride, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessRGB", out var @delegate))
            {
                return @delegate.Invoke(rgb, width, height, stride, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeLosslessRGB' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public int WebPEncodeLosslessRGBA(IntPtr rgba, int width, int height, int stride, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessRGBA", out var @delegate))
            {
                return @delegate.Invoke(rgba, width, height, stride, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeLosslessRGBA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="p">Pointer to memory</param>
        public void WebPFree(IntPtr pointer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPFree>("WebPFree", out var @delegate))
            {
                @delegate.Invoke(pointer);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPFree' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Get the webp decoder version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        public int WebPGetEncoderVersion()
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetVersion>("WebPGetEncoderVersion", out var @delegate))
            {
                return @delegate.Invoke();
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetEncoderVersion' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Get the webp encoder version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        public int WebPGetDecoderVersion()
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetVersion>("WebPGetDecoderVersion", out var @delegate))
            {
                return @delegate.Invoke();
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetDecoderVersion' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures.</summary>
        /// <param name="srcPicture">Picture to measure</param>
        /// <param name="refPicture">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="pResult">dB in the Y/U/V/Alpha/All order</param>
        /// <returns>False in case of error (src and ref don't have same dimension, ...)</returns>
        public int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureDistortion>("WebPPictureDistortion", out var @delegate))
            {
                return @delegate.Invoke(ref srcPicture, ref refPicture, metric_type, pResult);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureDistortion' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// Dynamically invoke a function of the library.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="args">Arguments for the function</param>
        /// <returns></returns>
        public object DynamicInvoke(string functionName, params object[] args)
        {
            this.AssertLibraryCallFailure();
            if (this._functionPointer.TryGetValue(functionName, out var pointer))
            {
                Delegate func = Marshal.GetDelegateForFunctionPointer(pointer, typeof(Delegate));
                var result = func.DynamicInvoke(args);
                func = null;
                return result;
            }
            else
            {
                IntPtr p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                // Failure is a common case, especially for adaptive code.
                if (p == IntPtr.Zero)
                    throw new EntryPointNotFoundException($"Function '{functionName}' not found");

                Delegate function = Marshal.GetDelegateForFunctionPointer(p, typeof(Delegate));
                this._functionPointer.TryAdd(functionName, p);
                var result = function.DynamicInvoke(args);
                function = null;
                return result;
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

            Type delegateType = typeof(TDelegate);

            if ((this._methods.TryGetValue(delegateType, out var outdelegate)) && (outdelegate is TDelegate result))
            {
                function = result;
                return true;
            }
            else
            {
                IntPtr p;
                if (!this._functionPointer.TryGetValue(functionName, out p))
                {
                    p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                    // Failure is a common case, especially for adaptive code.
                    if (p == IntPtr.Zero)
                    {
                        function = default;
                        return false;
                    }

                    this._functionPointer.TryAdd(functionName, p);
                }

                TDelegate foundFunction = (TDelegate)Marshal.GetDelegateForFunctionPointer(p, delegateType);
                this._methods.TryAdd(delegateType, foundFunction);
                function = foundFunction;
                return true;
            }
        }

        /// <summary>
        /// Return a boolean whether the given function name is existed or not.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>Return a boolean whether the given function name is existed or not.</returns>
        public bool IsFunctionExists(string functionName)
        {
            this.AssertLibraryCallFailure();
            if (this._functionPointer.TryGetValue(functionName, out var throwAway))
            {
                return true;
            }
            else
            {
                IntPtr p = UnsafeNativeMethods.GetProcAddress(this.libraryHandle, functionName);
                // Failure is a common case, especially for adaptive code.
                if (p == IntPtr.Zero)
                    return false;

                this._functionPointer.TryAdd(functionName, p);
                return true;
            }
        }

        private void AssertFunctionLoadFailure<TDelegate>(string functionName, bool throwOnNotFound = true) where TDelegate : Delegate
        {
            TDelegate func_delegate;
            if (this.TryGetFunction<TDelegate>(functionName, out func_delegate))
            {
                this._methods.TryAdd(typeof(TDelegate), func_delegate);
            }
            else
            {
                if (throwOnNotFound)
                {
                    this.libraryHandle.Close();
                    this.libraryHandle.Dispose();
                    throw new FileLoadException($"Function '{(typeof(TDelegate)).ToString()}' not found in the library. Wrong library or wrong version?");
                }
            }
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

        /// <summary>
        /// Releases all resources used by the <see cref="Libwebp"/> class and unload the unmanaged library.
        /// </summary>
        public void Dispose()
        {
            if (this.libraryHandle == null || this.libraryHandle.IsClosed || this.libraryHandle.IsInvalid)
                return;

            this._canEncode = false;
            this._canDecode = false;

            if (this._functionPointer != null)
                this._functionPointer.Clear();
            this._functionPointer = null;

            if (this._methods != null)
                this._methods.Clear();
            this._methods = null;

            this.libraryHandle.Close();
            this.libraryHandle.Dispose();
        }
    }
}
