using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Provides an interface for managed function wrapper for unmanaged libwebp library
    /// </summary>
    public interface ILibwebp
    {
        /// <summary>
        /// Full path to the imported library
        /// </summary>
        string LibraryPath { get; }

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality">Quality of compresion</param>
        /// <returns>0 if error</returns>
        int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality);

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        VP8StatusCode WebPGetFeatures(IntPtr rawWebP, uint data_size, ref WebPBitstreamFeatures features);

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features);

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        int WebPConfigLosslessPreset(ref WebPConfig config, int level);

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The WebPConfig struct</param>
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
        int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride);

        /// <summary>
        /// Colorspace conversion function to import BGRA samples.
        /// </summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgba">Point to BGRA data</param>
        /// <param name="stride">stride of BGRA data</param>
        /// <returns></returns>
        int WebPPictureImportBGRA(ref WebPPicture wpic, IntPtr rgba, int stride);

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="rgb">Point to RGB data</param>
        /// <param name="stride">stride of RGB data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportRGB(ref WebPPicture wpic, IntPtr rgb, int stride);

        /// <summary>Colorspace conversion function to import RGBA samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to RGBA data</param>
        /// <param name="stride">stride of RGBA data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportRGBA(ref WebPPicture wpic, IntPtr rgba, int stride);

        /// <summary>Colorspace conversion function to import BGRX samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgrx">Point to BGRX data</param>
        /// <param name="stride">stride of BGRX data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgrx, int stride);

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

        /// <summary>Validate the WebP image header and retrieve the image height and width. Pointers *width and *height can be passed NULL if deemed irrelevant</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        int WebPGetInfo(IntPtr data, int data_size, out int width, out int height);

        /// <summary>Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        int WebPDecodeBGRInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        /// <summary>Initialize the configuration as empty. This function must always be called first, unless WebPGetFeatures() is to be called.</summary>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>False in case of mismatched version.</returns>
        int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig);

        /// <summary>Decodes the full data at once, taking 'config' into account.</summary>
        /// <param name="data">WebP raw data to decode</param>
        /// <param name="data_size">Size of WebP data </param>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>VP8_STATUS_OK if the decoding was successful</returns>
        VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig);

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">WebPDecBuffer</param>
        void WebPFreeDecBuffer(ref WebPDecBuffer buffer);

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        int WebPEncodeRGB(IntPtr rgb, int width, int height, int stride, float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        int WebPEncodeBGRA(IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr output);

        /// <summary>Lossy encoding images</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        int WebPEncodeRGBA(IntPtr rgba, int width, int height, int stride, float quality_factor, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgra">Pointer to BGRA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        int WebPEncodeLosslessBGRA(IntPtr bgra, int width, int height, int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgb">Pointer to RGB image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        int WebPEncodeLosslessRGB(IntPtr rgb, int width, int height, int stride, out IntPtr output);

        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="rgba">Pointer to RGBA image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        /// <returns></returns>
        int WebPEncodeLosslessRGBA(IntPtr rgba, int width, int height, int stride, out IntPtr output);

        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="p">Pointer to memory</param>
        void WebPFree(IntPtr p);

        /// <summary>Get the webp version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        int WebPGetDecoderVersion();

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures.</summary>
        /// <param name="srcPicture">Picture to measure</param>
        /// <param name="refPicture">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="pResult">dB in the Y/U/V/Alpha/All order</param>
        /// <returns>False in case of error (src and ref don't have same dimension, ...)</returns>
        int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);

        /// <summary>
        /// Invoke a function of the library. (Warning: Low Performance because of <see cref="Delegate.DynamicInvoke(object[])"/>)
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="args">Arguments for the function</param>
        /// <returns></returns>
        object Invoke(string functionName, params object[] args);

        /// <summary>
        /// Return a boolean whether the given function name is existed or not.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>Return a boolean whether the given function name is existed or not.</returns>
        bool IsMethodExists(string functionName);
    }
}
