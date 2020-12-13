using System;
using System.Runtime.InteropServices;
using System.Security;

namespace WebPWrapper.LowLevel
{
    [SuppressUnmanagedCodeSecurity]
    static class NativeDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPGetVersion();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPConfigInitInternal(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPGetFeaturesInternal(IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPConfigLosslessPreset(ref WebPConfig config, int level);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPValidateConfig(ref WebPConfig config);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPPictureInitInternal(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPPictureImportAuto(ref WebPPicture wpic, IntPtr rgba, int stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPEncode(ref WebPConfig config, ref WebPPicture picture);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WebPPictureFree(ref WebPPicture wpic);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPGetInfo(IntPtr data, UIntPtr data_size, out int width, out int height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPDecodeAutoInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPInitDecoderConfigInternal(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPInitDecBufferInternal(ref WebPDecBuffer webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate VP8StatusCode WebPDecode(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WebPFreeDecBuffer(ref WebPDecBuffer buffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPEncodeAuto(IntPtr pixelData, int width, int height, int stride, float quality_factor, out IntPtr outputData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPEncodeLosslessAuto(IntPtr pixelData, int width, int height, int stride, out IntPtr outputData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WebPFree(IntPtr pointer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);

        /// <summary>The writer type for output compress data</summary>
        /// <param name="data">Data returned</param>
        /// <param name="data_size">Size of data returned</param>
        /// <param name="wpic">Picture struct</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPDataWriterCallback(IntPtr data, UIntPtr data_size, ref WebPPicture wpic);


        #region "Progressive decode"
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPINewDecoder(ref WebPDecBuffer wpic);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPINewDecoderFromPointer(IntPtr pointer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPINewRGB(WEBP_CSP_MODE colorspace, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPINewYUVA(IntPtr luma, UIntPtr luma_size, int luma_stride,
             IntPtr u, UIntPtr u_size, int u_stride,
             IntPtr v, UIntPtr v_size, int v_stride,
             IntPtr a, UIntPtr a_size, int a_stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WebPIDelete(IntPtr wpic);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate VP8StatusCode WebPIAppendOrUpdate(IntPtr idec, IntPtr data, UIntPtr data_size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPIDecGetRGB(IntPtr idec, ref int last_y, ref int width, ref int height, ref int stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr WebPIDecGetYUVA(IntPtr idec, ref int last_y, ref IntPtr u, ref IntPtr v, ref IntPtr a, ref int width, ref int height, ref int stride, ref int uv_stride, ref int a_stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate WebPDecBuffer WebPIDecodedArea(IntPtr idec, ref int left, ref int top, ref int width, ref int height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int OutputFunc(ref VP8Io io, ref WebPDecParams p);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int OutputAlphaFunc(ref VP8Io io, ref WebPDecParams p, int expected_num_out_lines);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int OutputRowFunc(ref WebPDecParams p, int y_pos, int max_out_lines);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int VP8IoPutHook(ref VP8Io io);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int VP8IoSetupHook(ref VP8Io io);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void VP8IoTeardownHook(ref VP8Io io);
        #endregion

        /*
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x86(ref WebPDecBuffer buffer);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x64(ref WebPDecBuffer buffer);
        */
    }
}
