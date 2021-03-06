﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace WebPWrapper.LowLevel
{
    /// <summary>
    /// Provides managed function wrapper for unmanaged libwebp library
    /// </summary>
    /// <remarks>
    /// Internal use only. Using reflection or another way to access this class, you will have to take care of close library/handle by yourself.
    /// </remarks>
    public class Libwebp : ILibwebp, IDisposable
    {
        private static readonly ConcurrentDictionary<string, Libwebp> cache_library = new ConcurrentDictionary<string, Libwebp>(StringComparer.OrdinalIgnoreCase);
        
        private int partners;
        private SafeLibraryHandle libraryHandle;
        private ConcurrentDictionary<DelegateIdentity, Delegate> _methods;
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
        private Libwebp(string myLibPath, bool preload)
        {
            if (string.IsNullOrWhiteSpace(myLibPath))
                throw new ArgumentNullException("myLibPath");
            // if (!File.Exists(myLibPath)) throw new FileNotFoundException("Library not found", myLibPath);

            this._canEncode = false;
            this._canDecode = false;

            this.LoadLib(myLibPath, preload);
            Interlocked.Exchange(ref this.partners, 0);
        }

        /// <summary>Load the libwebp native library file from the given path.</summary>
        /// <param name="library_path">The path to the libwebp native library.</param>
        /// <returns>Returns an <seealso cref="ILibwebp"/> interface which can be used.</returns>
        /// <remarks>Use <seealso cref="Deinit(ILibwebp)"/> once you've finished using to unload the library.</remarks>
        public static ILibwebp Init(string library_path)
        {
            if (string.IsNullOrWhiteSpace(library_path))
                throw new ArgumentNullException("library_path");

            Libwebp result;
            if (!cache_library.TryGetValue(library_path, out result))
            {
                // Let's leave the full-load library for another time.
                // Make use of LoadLibrary to search for the library
                var loadedLib = new Libwebp(library_path, false);

                // Check if the library is already existed in cache
                var addedOrNot = cache_library.GetOrAdd(loadedLib.LibraryPath, loadedLib);

                if (addedOrNot == loadedLib)
                {
                    // It's added successfully. Which means loadedLib is (in a valid way) the first-arrive in cache.
                    // Let's use it.
                    result = loadedLib;
                }
                else
                {
                    // It's failed. Which means another library is already added before this.
                    // Let's dispose the current one (unload library) and use the already existed one.
                    loadedLib.Dispose();
                    result = addedOrNot;
                }
            }

            result.IncreaseReferenceCount();
            return result;
        }

        /// <summary>Attempt to decrease the reference count of the underlying native library of the <seealso cref="ILibwebp"/> interface.</summary>
        /// <param name="library">The <seealso cref="ILibwebp"/> interface to attempt</param>
        /// <remarks>Only the default implementation has this. The underlying native library will also be unloaded if the reference count reaches 0.</remarks>
        public static void Deinit(ILibwebp library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            if (library is Libwebp obj)
            {
                obj.DecreaseReferenceCount();
            }
        }

        /// <summary>Increase reference count to this object by one count.</summary>
        /// <remarks>Extremely unsafe. Do not use this if you don't know what it is.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int IncreaseReferenceCount()
        {
            return Interlocked.Increment(ref this.partners);
        }

        /// <summary>Decrease reference count to this object by one count.</summary>
        /// <remarks>Extremely unsafe. Do not use this if you don't know what it is.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int DecreaseReferenceCount()
        {
            var refCount = Interlocked.Decrement(ref this.partners);
            if (refCount == 0)
            {
                if (cache_library.TryRemove(this.LibraryPath, out var result))
                {
                    result.Dispose();
                }
            }
            return refCount;
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
                Marshal.ThrowExceptionForHR(hr);
            }
            else
            {
                this._functionPointer = new ConcurrentDictionary<string, IntPtr>();
                this._methods = new ConcurrentDictionary<DelegateIdentity, Delegate>();
                this.libraryHandle = handle;
                this._libpath = UnsafeNativeMethods.GetModuleFileName(handle);
                if (string.IsNullOrEmpty(this._libpath))
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
                if (preload)
                {
                    // WebPConfigInitInternal
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPConfigInitInternal>("WebPConfigInitInternal");

                    // WebPGetFeaturesInternal
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

                    // WebPPictureDistortion
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPGetVersion>("WebPPictureDistortion");

                    // WebPINewDecoder
                    this.AssertFunctionLoadFailure<NativeDelegates.WebPINewDecoder>("WebPINewDecoder");
                }
            }
        }

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality">Quality of compresion</param>
        /// <returns>Non-zero if success, otherwise zero if error</returns>
        /// <remarks>
        /// Should always be called, to initialize a fresh WebPConfig structure before modification.
        /// <see cref="WebPConfigInit"/>() must have succeeded before using the 'config' object.
        /// Note that the default values are '<paramref name="preset"/>'=<seealso cref="WebPPreset.Default"/> and '<paramref name="quality"/>'=75.
        /// </remarks>
        public int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            if (this.TryGetFunction< NativeDelegates.WebPConfigInitInternal >("WebPConfigInitInternal", out var @delegate))
            {
                return @delegate.Invoke(ref config, preset, quality, Define.WEBP_ENCODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPConfigInitInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if success. Otherwise the error code.</returns>
        public VP8StatusCode WebPGetFeatures(IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features)
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetFeaturesInternal>("WebPGetFeaturesInternal", out var @delegate))
            {
                return (VP8StatusCode)@delegate.Invoke(rawWebP, data_size, ref features, Define.WEBP_DECODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetFeaturesInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        public int WebPConfigLosslessPreset(ref WebPConfig config, CompressionLevel level)
        {
            if (this.TryGetFunction<NativeDelegates.WebPConfigLosslessPreset>("WebPConfigLosslessPreset", out var @delegate))
            {
                return @delegate.Invoke(ref config, (int)level);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPConfigLosslessPreset' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The <seealso cref="WebPConfig"/> struct to check</param>
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
                return @delegate.Invoke(ref wpic, Define.WEBP_ENCODER_ABI_VERSION);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureInitInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Convenience allocation / deallocation based on picture->width/height. Allocate y/u/v buffers as per colorspace/width/height specification.</summary>
        /// <param name="wpic">The <see cref="WebPPicture"/> structure for the allocation.</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        /// <remarks>This function will free the previous buffer if needed.</remarks>
        public int WebPPictureAlloc(ref WebPPicture wpic)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureAlloc>("WebPPictureAlloc", out var @delegate))
            {
                return @delegate.Invoke(ref wpic);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureAlloc' function from the library. Wrong library or wrong version?");
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
        /// <param name="rgba">Point to RGBA data</param>
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

        /// <summary>Validate the WebP image header and retrieve the image height and width.</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The width (in pixels) of the image</param>
        /// <param name="height">The height (in pixels) of the image</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        public int WebPGetInfo(IntPtr data, UIntPtr data_size, out int width, out int height)
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetInfo>("WebPGetInfo", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, out width, out height);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPGetInfo' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Validate the WebP image header.</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        public int WebPGetInfo(IntPtr data, UIntPtr data_size)
        {
            if (this.TryGetFunction<NativeDelegates.WebPGetInfoWithPointer>("WebPGetInfo", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, IntPtr.Zero, IntPtr.Zero);
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
        public IntPtr WebPDecodeRGBAInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBAInto", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, output_buffer, output_buffer_size, output_stride);
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
        public IntPtr WebPDecodeARGBInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeARGBInto", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, output_buffer, output_buffer_size, output_stride);
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
        public IntPtr WebPDecodeBGRAInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRAInto", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, output_buffer, output_buffer_size, output_stride);
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
        public IntPtr WebPDecodeRGBInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeRGBInto", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, output_buffer, output_buffer_size, output_stride);
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
        public IntPtr WebPDecodeBGRInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecodeAutoInto>("WebPDecodeBGRInto", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, output_buffer, output_buffer_size, output_stride);
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
        public VP8StatusCode WebPDecode(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            if (this.TryGetFunction<NativeDelegates.WebPDecode>("WebPDecode", out var @delegate))
            {
                return @delegate.Invoke(data, data_size, ref webPDecoderConfig);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPDecode' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Initialize the structure as empty. Must be called before any other use</summary>
        /// <param name="webPDecoderBuffer">The <seealso cref="WebPDecBuffer"/> to init the output buffer</param>
        /// <returns>Returns false in case of version mismatch</returns>
        public bool WebPInitDecBuffer(ref WebPDecBuffer webPDecoderBuffer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPInitDecBufferInternal>("WebPInitDecBufferInternal", out var @delegate))
            {
                return (@delegate.Invoke(ref webPDecoderBuffer, Define.WEBP_DECODER_ABI_VERSION) != 0);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPInitDecBufferInternal' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">WebPDecBuffer</param>
        /// <remarks>External memory will not be touched.</remarks>
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public UIntPtr WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public UIntPtr WebPEncodeRGB(IntPtr rgb, int width, int height, int stride, float quality_factor, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public UIntPtr WebPEncodeBGRA(IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public UIntPtr WebPEncodeRGBA(IntPtr rgba, int width, int height, int stride, float quality_factor, out IntPtr outputData)
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
        /// <param name="outputData">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public UIntPtr WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public UIntPtr WebPEncodeLosslessBGRA(IntPtr bgra, int width, int height, int stride, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public UIntPtr WebPEncodeLosslessRGB(IntPtr rgb, int width, int height, int stride, out IntPtr outputData)
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
        /// <param name="outputData">The output buffer's pointer which contains WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        public UIntPtr WebPEncodeLosslessRGBA(IntPtr rgba, int width, int height, int stride, out IntPtr outputData)
        {
            if (this.TryGetFunction<NativeDelegates.WebPEncodeLosslessAuto>("WebPEncodeLosslessRGBA", out var @delegate))
            {
                return @delegate.Invoke(rgba, width, height, stride, out outputData);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPEncodeLosslessRGBA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="pointer">Pointer to memory</param>
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
        /// <returns>1 if success, 0 in case of error (src and ref don't have same dimension, ...)</returns>
        public int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult)
        {
            if (this.TryGetFunction<NativeDelegates.WebPPictureDistortion>("WebPPictureDistortion", out var @delegate))
            {
                return @delegate.Invoke(ref srcPicture, ref refPicture, metric_type, pResult);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPPictureDistortion' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// Creates a new incremental decoder with the supplied buffer parameter
        /// </summary>
        /// <param name="output_buffer">The data of <see cref="WebPDecBuffer"/> to create decoder</param>
        /// <returns>Returns NULL if the allocation failed</returns>
        /// <remarks>
        /// The supplied 'output_buffer' content MUST NOT be changed between calls to
        /// WebPIAppend() or WebPIUpdate() unless 'output_buffer.is_external_memory' is
        /// not set to 0. In such a case, it is allowed to modify the pointers, size and
        /// stride of output_buffer.u.RGBA or output_buffer.u.YUVA, provided they remain
        /// within valid bounds.
        /// All other fields of WebPDecBuffer MUST remain constant between calls.
        /// </remarks>
        public IntPtr WebPINewDecoder(ref WebPDecBuffer output_buffer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPINewDecoder>("WebPINewDecoder", out var @delegate))
            {
                return @delegate.Invoke(ref output_buffer);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPINewDecoder' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Creates a new incremental decoder with the given config</summary>
        /// <param name="input_buffer">The memory pointer to the Webp image data buffer. Can be NULL.</param>
        /// <param name="input_buffer_size">The size of the Webp image data buffer.</param>
        /// <param name="config">The configuration for the decoder.</param>
        /// <returns>Returns NULL if the allocation failed</returns>
        /// <remarks>In case <paramref name="input_buffer"/> is NULL, <paramref name="input_buffer_size"/> is ignored and the function.</remarks>
        public IntPtr WebPIDecode(IntPtr input_buffer, UIntPtr input_buffer_size, ref WebPDecoderConfig config)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIDecode>("WebPIDecode", out var @delegate))
            {
                return @delegate.Invoke(input_buffer, input_buffer_size, ref config);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIDecode' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Creates a new incremental decoder with default settings (Output with MODE_RGB)</summary>
        /// <returns>Returns NULL if the allocation failed</returns>
        public IntPtr WebPINewDecoder()
        {
            if (this.TryGetFunction<NativeDelegates.WebPINewDecoderFromPointer>("WebPINewDecoder", out var @delegate))
            {
                return @delegate.Invoke(IntPtr.Zero);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPINewDecoder' function from the library. Wrong library or wrong version?");
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
        /// <returns>Returns NULL if the allocation failed, or if some parameters are invalid</returns>
        public IntPtr WebPINewRGB(Colorspace colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPINewRGB>("WebPINewRGB", out var @delegate))
            {
                return @delegate.Invoke(colorspace, output_buffer, output_buffer_size, output_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPINewRGB' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// This function allocates and initializes an incremental-decoder object, which
        /// will output the raw luma/chroma samples into a preallocated planes if supplied.
        /// </summary>
        /// <param name="luma">The pointer to the luma plane. Can be passed NULL if no preallocated planes are supplied</param>
        /// <param name="luma_size">The size of the luma plane</param>
        /// <param name="luma_stride">The stride of the luma plane</param>
        /// <param name="u">The pointer to the chroma-u</param>
        /// <param name="u_size">The size of the chroma-u</param>
        /// <param name="u_stride">The stride of the chroma-u</param>
        /// <param name="v">The pointer to the chroma-v</param>
        /// <param name="v_size">The size of the chroma-v</param>
        /// <param name="v_stride">The stride of the chroma-v</param>
        /// <param name="a">The pointer to the alpha plane. Can be passed NULL to ignore alpha.</param>
        /// <param name="a_size">The size of the alpha plane</param>
        /// <param name="a_stride">The stride of the alpha plane</param>
        /// <remarks>
        /// If '<paramref name="luma"/>' is passed NULL. In this case, the output buffer will be automatically allocated (using
        /// MODE_YUVA) when decoding starts. All parameters are then ignored.
        /// </remarks>
        /// <returns>Returns NULL if the allocation failed or if a parameter is invalid</returns>
        public IntPtr WebPINewYUVA(IntPtr luma, UIntPtr luma_size, int luma_stride,
            IntPtr u, UIntPtr u_size, int u_stride,
            IntPtr v, UIntPtr v_size, int v_stride,
            IntPtr a, UIntPtr a_size, int a_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPINewYUVA>("WebPINewYUVA", out var @delegate))
            {
                return @delegate.Invoke(luma, luma_size, luma_stride, u, u_size, u_stride, v, v_size, v_stride, a, a_size, a_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPINewYUVA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Deletes the WebPIDecoder object and associated memory</summary>
        /// <param name="idec">The reference to <see cref="WebPIDecoder"/> which will be deleted.</param>
        /// <remarks>Must always be called if WebPINewDecoder, WebPINewRGB or WebPINewYUV succeeded.</remarks>
        public void WebPIDelete(IntPtr idec)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIDelete>("WebPIDelete", out var @delegate))
            {
                @delegate.Invoke(idec);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIDelete' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Copies and decodes the next available data</summary>
        /// <param name="idec"></param>
        /// <param name="data"></param>
        /// <param name="data_size"></param>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode WebPIAppend(IntPtr idec, IntPtr data, UIntPtr data_size)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIAppendOrUpdate>("WebPIAppend", out var @delegate))
            {
                return @delegate.Invoke(idec, data, data_size);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIAppend' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// A variant of the <see cref="WebPIAppend(IntPtr, IntPtr, UIntPtr)"/> to be used when data buffer contains
        /// partial data from the beginning. In this case data buffer is not copied
        /// to the internal memory
        /// </summary>
        /// <param name="idec"></param>
        /// <param name="data"></param>
        /// <param name="data_size"></param>
        /// <remarks>
        /// Note that the value of the '<paramref name="data"/>' pointer can change between calls to WebPIUpdate, for instance when the data buffer is resized to fit larger data.
        /// </remarks>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        public VP8StatusCode WebPIUpdate(IntPtr idec, IntPtr data, UIntPtr data_size)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIAppendOrUpdate>("WebPIUpdate", out var @delegate))
            {
                return @delegate.Invoke(idec, data, data_size);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIUpdate' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// Returns the RGB/A image decoded so far. The RGB/A output type corresponds to the colorspace specified during call to <see cref="WebPINewDecoder()"/> or <see cref="WebPINewRGB"/>.
        /// </summary>
        /// <param name="idec"></param>
        /// <param name="last_y">The index of last decoded row in raster scan order</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <remarks>
        /// Some pointers (<paramref name="last_y"/>, <paramref name="width"/> etc.) can be zero if corresponding information is not
        /// needed. The values in these pointers are only valid on successful (non-NULL) return.
        /// </remarks>
        /// <returns>Returns <see cref="IntPtr.Zero"/> if output params are not initialized yet</returns>
        public IntPtr WebPIDecGetRGB(IntPtr idec, ref int last_y, ref int width, ref int height, ref int stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIDecGetRGB>("WebPIDecGetRGB", out var @delegate))
            {
                IntPtr result;
                unsafe
                {
                    result = @delegate.Invoke(idec, ref last_y, ref width, ref height, ref stride);
                }
                return result;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIDecGetRGB' function from the library. Wrong library or wrong version?");
        }

        /// <summary>
        /// Returns the YUVA image decoded so far. The YUVA output type corresponds to the colorspace specified during call to <see cref="WebPINewDecoder()"/> or <see cref="WebPINewRGB"/>.
        /// </summary>
        /// <param name="idec"></param>
        /// <param name="last_y"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="uv_stride"></param>
        /// <param name="a_stride"></param>
        /// <remarks>
        /// If there is no alpha information the alpha pointer '<paramref name="a"/>' will be returned <see cref="IntPtr.Zero"/>
        /// </remarks>
        /// <returns>Returns pointer to the luma plane or <see cref="IntPtr.Zero"/> in case of error</returns>
        public IntPtr WebPIDecGetYUVA(IntPtr idec, ref int last_y, ref IntPtr u, ref IntPtr v, ref IntPtr a, ref int width, ref int height, ref int stride, ref int uv_stride, ref int a_stride)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIDecGetYUVA>("WebPIDecGetYUVA", out var @delegate))
            {
                return @delegate.Invoke(idec, ref last_y, ref u, ref v, ref a, ref width, ref height, ref stride, ref uv_stride, ref a_stride);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIDecGetYUVA' function from the library. Wrong library or wrong version?");
        }

        /// <summary>Generic call to retrieve information about the displayable area</summary>
        /// <param name="idec"></param>
        /// <param name="left">The left value are filled with the visible rectangular area so far</param>
        /// <param name="top">The top value are filled with the visible rectangular area so far</param>
        /// <param name="width">The width value are filled with the visible rectangular area so far</param>
        /// <param name="height">The height value are filled with the visible rectangular area so far</param>
        /// <remarks>
        /// If result is non-NULL, the left/right/width/height values are filled with the visible rectangular area so far
        /// </remarks>
        /// <returns>
        /// Returns NULL in case the incremental decoder object is in an invalid state.
        /// Otherwise returns the pointer to the internal representation. This structure
        /// is read-only, tied to <seealso cref="WebPIDecoder"/>'s lifespan and should not be modified.
        /// </returns>
        public IntPtr WebPIDecodedArea(IntPtr idec, ref int left, ref int top, ref int width, ref int height)
        {
            if (this.TryGetFunction<NativeDelegates.WebPIDecodedArea>("WebPIDecodedArea", out var @delegate))
            {
                return @delegate.Invoke(idec, ref left, ref top, ref width, ref height);
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPIDecodedArea' function from the library. Wrong library or wrong version?");
        }

        /// <summary>The following must be called first before any use.</summary>
        /// <param name="writer">The <see cref="WebPMemoryWriter" /> structure to initialize</param>
        public void WebPMemoryWriterInit(ref WebPMemoryWriter writer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPMemoryWriterInit>("WebPMemoryWriterInit", out var @delegate))
            {
                @delegate.Invoke(ref writer);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPMemoryWriterInit' function from the library. Wrong library or wrong version?");
        }

        /// <summary>The following must be called to deallocate associated memory. The 'writer' object itself is not deallocated.</summary>
        /// <param name="writer">The <see cref="WebPMemoryWriter" /> structure to free the associated allocated memory.</param>
        public void WebPMemoryWriterClear(ref WebPMemoryWriter writer)
        {
            if (this.TryGetFunction<NativeDelegates.WebPMemoryWriterClear>("WebPMemoryWriterClear", out var @delegate))
            {
                @delegate.Invoke(ref writer);
                return;
            }
            throw new EntryPointNotFoundException("Cannot find 'WebPMemoryWriterClear' function from the library. Wrong library or wrong version?");
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
            IntPtr pointer = this._functionPointer.GetOrAdd(functionName, (entryPoint) => UnsafeNativeMethods.FindFunction(this.libraryHandle, entryPoint));
            if (pointer == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException($"Function '{functionName}' not found");
            }
            Delegate func = Marshal.GetDelegateForFunctionPointer(pointer, typeof(Delegate));
            var result = func.DynamicInvoke(args);
            return result;
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

            var result = this._methods.GetOrAdd(new DelegateIdentity(typeof(TDelegate), functionName), (identity) =>
            {
                IntPtr p = this._functionPointer.GetOrAdd(identity.FunctionName, (entryPoint) => UnsafeNativeMethods.FindFunction(this.libraryHandle, entryPoint));
                if (p == IntPtr.Zero)
                {
                    return null;
                }

                return (TDelegate)Marshal.GetDelegateForFunctionPointer(p, identity.DelegateType);
            });
            if (result == null)
            {
                function = default;
                return false;
            }
            else
            {
                function = (TDelegate)result;
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
            IntPtr p = this._functionPointer.GetOrAdd(functionName, (entryPoint) => UnsafeNativeMethods.FindFunction(this.libraryHandle, entryPoint));
            if (p == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AssertFunctionLoadFailure<TDelegate>(string functionName, bool throwOnNotFound = true) where TDelegate : Delegate
        {
            if (!this.TryGetFunction<TDelegate>(functionName, out _))
            {
                if (throwOnNotFound)
                {
                    this.libraryHandle.Close();
                    this.libraryHandle.Dispose();
                    throw new FileLoadException($"Function '{(typeof(TDelegate))}' not found in the library. Wrong library or wrong version?");
                }
            }
        }

        /// <summary>Throw exception if the library has been unloaded or has not been loaded yet.</summary>
        /// <remarks>Should be used to check before finding function pointers or functions of the library.</remarks>
        protected void AssertLibraryCallFailure()
        {
            if (this.libraryHandle == null || this.libraryHandle.IsInvalid)
                throw new InvalidOperationException("The library was not loaded successfully.");
            if (this.libraryHandle.IsClosed)
                throw new ObjectDisposedException(nameof(Libwebp), "The library has been disposed. Cannot revive the library. Please load the library again.");
        }

        /// <summary>Releases all resources used by the <see cref="Libwebp"/> class and unload the unmanaged library regardless of the reference count.</summary>
        /// <remarks>Extremely unsafe. Do not use this if you don't know what it is. Usually you should use <seealso cref="Deinit(ILibwebp)"/> instead.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._functionPointer != null)
                    this._functionPointer.Clear();

                if (this._methods != null)
                    this._methods.Clear();

                if (this.libraryHandle != null)
                {
                    if (!this.libraryHandle.IsClosed && !this.libraryHandle.IsInvalid)
                    {
                        this.libraryHandle.Close();
                    }

                    this.libraryHandle.Dispose();
                }
            }
            this._canEncode = false;
            this._canDecode = false;

            this._functionPointer = null;
            this._methods = null;

            this.libraryHandle = null;
        }

        /// <summary>Destructor. Used by GC</summary>
        ~Libwebp()
        {
            this.Dispose(false);
        }
    }
}
