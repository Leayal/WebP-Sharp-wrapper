using System;

namespace WebPWrapper.LowLevel
{
    /// <summary>
    /// Provides an interface to use unmanaged libwebp library in managed code.
    /// </summary>
    public interface ILibwebp
    {
        /// <summary>
        /// Full path to the imported library
        /// </summary>
        string LibraryPath { get; }

        /// <summary>
        /// Gets a value indicating whether the current loaded library supports WebP encoding.
        /// </summary>
        bool CanEncode { get; }

        /// <summary>
        /// Gets a value indicating whether the current loaded library supports WebP decoding.
        /// </summary>
        bool CanDecode { get; }

        /// <summary>
        /// Attempt to search the function with the given name and type.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate which will define the overload</typeparam>
        /// <param name="functionName">The name to search for the function</param>
        /// <param name="function">The first result of the search if it's found.</param>
        /// <returns>A boolean determine whether the function is found or not</returns>
        /// <exception cref="ObjectDisposedException">The library has been unloaded</exception>
        /// <exception cref="InvalidOperationException">The library wasn not loaded successfully</exception>
        bool TryGetFunction<TDelegate>(in string functionName, out TDelegate function) where TDelegate : Delegate;

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality_factor">Quality factor of compresion</param>
        /// <returns>Non-zero if success, otherwise zero if error</returns>
        /// <remarks>
        /// Should always be called, to initialize a fresh WebPConfig structure before modification.
        /// <see cref="WebPConfigInit"/>() must have succeeded before using the 'config' object.
        /// Note that the default values are '<paramref name="preset"/>'=<seealso cref="WebPPreset.Default"/> and '<paramref name="quality_factor"/>'=75.
        /// </remarks>
        int WebPConfigInit(ref WebPConfig config, in WebPPreset preset, in float quality_factor);

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>Returns <seealso cref="VP8StatusCode.VP8_STATUS_OK"/> if success. Otherwise the error code.</returns>
        VP8StatusCode WebPGetFeatures(in IntPtr rawWebP, in UIntPtr data_size, ref WebPBitstreamFeatures features);

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        int WebPConfigLosslessPreset(ref WebPConfig config, in CompressionLevel level);

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The <seealso cref="WebPConfig"/> struct to check</param>
        /// <returns>1 if config are OK</returns>
        int WebPValidateConfig(ref WebPConfig config);

        /// <summary>Init the struct WebPPicture ckecking the dll version</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <returns>1 if not error</returns>
        int WebPPictureInitInternal(ref WebPPicture wpic);

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to BGR data</param>
        /// <param name="stride">stride of BGR data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportBGR(ref WebPPicture wpic, in IntPtr bgr, in int stride);

        /// <summary>
        /// Colorspace conversion function to import BGRA samples.
        /// </summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgba">Point to BGRA data</param>
        /// <param name="stride">stride of BGRA data</param>
        /// <returns></returns>
        int WebPPictureImportBGRA(ref WebPPicture wpic, in IntPtr rgba, in int stride);

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgb">Point to RGB data</param>
        /// <param name="stride">stride of RGB data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportRGB(ref WebPPicture wpic, in IntPtr rgb, in int stride);

        /// <summary>Colorspace conversion function to import RGBA samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgba">Point to RGBA data</param>
        /// <param name="stride">stride of RGBA data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportRGBA(ref WebPPicture wpic, in IntPtr rgba, in int stride);

        /// <summary>Colorspace conversion function to import BGRX samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgrx">Point to BGRX data</param>
        /// <param name="stride">stride of BGRX data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportBGRX(ref WebPPicture wpic, in IntPtr bgrx, in int stride);

        /// <summary>Compress to webp format</summary>
        /// <param name="config">The config struct for compresion parameters</param>
        /// <param name="picture">'picture' hold the source samples in both YUV(A) or ARGB input</param>
        /// <returns>Returns 0 in case of error, 1 otherwise. In case of error, picture->error_code is updated accordingly.</returns>
        int WebPEncode(ref WebPConfig config, ref WebPPicture picture);

        /// <summary>Release the memory allocated by WebPPictureAlloc() or WebPPictureImport*()
        /// Note that this function does _not_ free the memory used by the 'picture' object itself.
        /// Besides memory (which is reclaimed) all other fields of 'picture' are preserved.</summary>
        /// <param name="picture">Picture struct</param>
        void WebPPictureFree(ref WebPPicture picture);

        /// <summary>Validate the WebP image header and retrieve the image height and width.</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        int WebPGetInfo(in IntPtr data, in UIntPtr data_size, out int width, out int height);

        /// <summary>Validate the WebP image header.</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        int WebPGetInfo(in IntPtr data, in UIntPtr data_size);

        /// <summary>Decode WEBP image pointed to by *data and returns RGBA samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to the start of the memory where the decoded pixel will write to</param>
        /// <param name="output_buffer_size">Size of allocated memory</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        IntPtr WebPDecodeRGBAInto(in IntPtr data, in UIntPtr data_size, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

        /// <summary>Decode WEBP image pointed to by *data and returns ARGB samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to the start of the memory where the decoded pixel will write to</param>
        /// <param name="output_buffer_size">Size of allocated memory</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        IntPtr WebPDecodeARGBInto(in IntPtr data, in UIntPtr data_size, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

        /// <summary>Decode WEBP image pointed to by *data and returns BGRA samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to the start of the memory where the decoded pixel will write to</param>
        /// <param name="output_buffer_size">Size of allocated memory</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        IntPtr WebPDecodeBGRAInto(in IntPtr data, in UIntPtr data_size, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

        /// <summary>Decode WEBP image pointed to by *data and returns RGB samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to the start of the memory where the decoded pixel will write to</param>
        /// <param name="output_buffer_size">Size of allocated memory</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        IntPtr WebPDecodeRGBInto(in IntPtr data, in UIntPtr data_size, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

        /// <summary>Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to the start of the memory where the decoded pixel will write to</param>
        /// <param name="output_buffer_size">Size of allocated memory</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        IntPtr WebPDecodeBGRInto(in IntPtr data, in UIntPtr data_size, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

        /// <summary>Initialize the configuration as empty. This function must always be called first, unless WebPGetFeatures() is to be called.</summary>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>False in case of mismatched version.</returns>
        int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig);

        /// <summary>Decodes the full data at once, taking 'config' into account.</summary>
        /// <param name="data">WebP raw data to decode</param>
        /// <param name="data_size">Size of WebP data </param>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>VP8_STATUS_OK if the decoding was successful</returns>
        VP8StatusCode WebPDecode(in IntPtr data, in UIntPtr data_size, ref WebPDecoderConfig webPDecoderConfig);

        /// <summary>Initialize the structure as empty. Must be called before any other use</summary>
        /// <param name="webPDecoderBuffer">The <seealso cref="WebPDecBuffer"/> to init the output buffer</param>
        /// <returns>Returns false in case of version mismatch</returns>
        bool WebPInitDecBuffer(ref WebPDecBuffer webPDecoderBuffer);

        /// <summary>Convenience allocation / deallocation based on picture->width/height. Allocate y/u/v buffers as per colorspace/width/height specification.</summary>
        /// <param name="wpic">The <see cref="WebPPicture"/> structure for the allocation.</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        /// <remarks>This function will free the previous buffer if needed.</remarks>
        int WebPPictureAlloc(ref WebPPicture wpic);

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">The <seealso cref="WebPDecBuffer"/> to free the associated memory.</param>
        /// <remarks>External memory will not be touched.</remarks>
        void WebPFreeDecBuffer(ref WebPDecBuffer buffer);

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        UIntPtr WebPEncodeBGR(in IntPtr bgr, in int width, in int height, in int stride, in float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        UIntPtr WebPEncodeRGB(in IntPtr rgb, in int width, in int height, in int stride, in float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        UIntPtr WebPEncodeBGRA(in IntPtr bgra, in int width, in int height, in int stride, in float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        UIntPtr WebPEncodeRGBA(in IntPtr rgba, in int width, in int height, in int stride, in float quality_factor, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        UIntPtr WebPEncodeLosslessBGR(in IntPtr bgr, in int width, in int height, in int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        UIntPtr WebPEncodeLosslessBGRA(in IntPtr bgra, in int width, in int height, in int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        UIntPtr WebPEncodeLosslessRGB(in IntPtr rgb, in int width, in int height, in int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        UIntPtr WebPEncodeLosslessRGBA(in IntPtr rgba, in int width, in int height, in int stride, out IntPtr output);

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
        IntPtr WebPINewDecoder(ref WebPDecBuffer output_buffer);

        /// <summary>Creates a new incremental decoder with default settings (Output with MODE_RGB)</summary>
        /// <returns>Returns NULL if the allocation failed</returns>
        IntPtr WebPINewDecoder();

        /// <summary>Creates a new incremental decoder with the given config</summary>
        /// <param name="input_buffer">The memory pointer to the Webp image data buffer. Can be NULL.</param>
        /// <param name="input_buffer_size">The size of the Webp image data buffer.</param>
        /// <param name="config">The configuration for the decoder.</param>
        /// <returns>Returns NULL if the allocation failed</returns>
        /// <remarks>In case <paramref name="input_buffer"/> is NULL, <paramref name="input_buffer_size"/> is ignored and the function.</remarks>
        IntPtr WebPIDecode(in IntPtr input_buffer, in UIntPtr input_buffer_size, ref WebPDecoderConfig config);

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
        IntPtr WebPINewRGB(in WEBP_CSP_MODE colorspace, in IntPtr output_buffer, in UIntPtr output_buffer_size, in int output_stride);

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
        IntPtr WebPINewYUVA(in IntPtr luma, in UIntPtr luma_size, in int luma_stride,
            in IntPtr u, in UIntPtr u_size, in int u_stride,
            in IntPtr v, in UIntPtr v_size, in int v_stride,
            in IntPtr a, in UIntPtr a_size, in int a_stride);

        /// <summary>Deletes the WebPIDecoder object and associated memory</summary>
        /// <param name="idec">The reference to <see cref="WebPIDecoder"/> which will be deleted.</param>
        /// <remarks>Must always be called if WebPINewDecoder, WebPINewRGB or WebPINewYUV succeeded.</remarks>
        void WebPIDelete(in IntPtr idec);

        /// <summary>Copies and decodes the next available data</summary>
        /// <param name="idec"></param>
        /// <param name="data"></param>
        /// <param name="data_size"></param>
        /// <returns>
        /// Returns <see cref="VP8StatusCode.VP8_STATUS_OK"/> when
        /// the image is successfully decoded. Returns <see cref="VP8StatusCode.VP8_STATUS_SUSPENDED"/> when more
        /// data is expected. Returns error in other cases.
        /// </returns>
        VP8StatusCode WebPIAppend(in IntPtr idec, in IntPtr data, in UIntPtr data_size);

        /// <summary>
        /// A variant of the <see cref="WebPIAppend(in IntPtr, in IntPtr, in UIntPtr)"/> to be used when data buffer contains
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
        VP8StatusCode WebPIUpdate(in IntPtr idec, in IntPtr data, in UIntPtr data_size);

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
        IntPtr WebPIDecGetRGB(in IntPtr idec, ref int last_y, ref int width, ref int height, ref int stride);

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
        IntPtr WebPIDecGetYUVA(in IntPtr idec, ref int last_y, ref IntPtr u, ref IntPtr v, ref IntPtr a, ref int width, ref int height, ref int stride, ref int uv_stride, ref int a_stride);

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
        IntPtr WebPIDecodedArea(in IntPtr idec, ref int left, ref int top, ref int width, ref int height);

        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="p">Pointer to memory</param>
        void WebPFree(in IntPtr p);

        /// <summary>Get the webp decoder version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        int WebPGetDecoderVersion();

        /// <summary>Get the webp encoder version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        int WebPGetEncoderVersion();

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures.</summary>
        /// <param name="srcPicture">Picture to measure</param>
        /// <param name="refPicture">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="pResult">dB in the Y/U/V/Alpha/All order</param>
        /// <returns>False in case of error (src and ref don't have same dimension, ...)</returns>
        int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, in int metric_type, in IntPtr pResult);

        /// <summary>
        /// Dynamically invoke a function of the library. (Warning: Low Performance because of <see cref="Delegate.DynamicInvoke(object[])"/>)
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="args">Arguments for the function</param>
        /// <returns>The object returned by the invoked function. Or null if the function doesn't return anything.</returns>
        object DynamicInvoke(in string functionName, params object[] args);

        /// <summary>
        /// Return a boolean whether the given function name is existed or not.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>Return a boolean whether the given function name is existed or not.</returns>
        bool IsFunctionExists(in string functionName);
    }
}
