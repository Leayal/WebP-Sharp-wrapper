using System;
using System.Runtime.InteropServices;
using System.Security;

namespace WebPWrapper.WPF.LowLevel
{
    [SuppressUnmanagedCodeSecurity]
    static class NativeDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPGetVersion();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPConfigInitInternal(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPGetFeaturesInternal([In] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

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
        internal delegate int WebPGetInfo([In] IntPtr data, UIntPtr data_size, out int width, out int height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPDecodeAutoInto([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPInitDecoderConfigInternal(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate VP8StatusCode WebPDecode(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void WebPFreeDecBuffer(ref WebPDecBuffer buffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPEncodeAuto([In] IntPtr pixelData, int width, int height, int stride, float quality_factor, out IntPtr outputData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WebPEncodeLosslessAuto([In] IntPtr pixelData, int width, int height, int stride, out IntPtr outputData);

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
        internal delegate int WebPDataWriterCallback([In] IntPtr data, UIntPtr data_size, ref WebPPicture wpic);

        /*
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x86(ref WebPDecBuffer buffer);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x64(ref WebPDecBuffer buffer);
        */
    }
}
