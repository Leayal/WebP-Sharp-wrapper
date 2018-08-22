using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebPWrapper.WPF
{
    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed partial class UnsafeNativeMethods
    {
        public static unsafe void MemoryCopy(void* dest, void* src, int count)
        {
            int block;

            block = count >> 3;

            long* pDest = (long*)dest;
            long* pSrc = (long*)src;

            for (int i = 0; i < block; i++)
            {
                *pDest = *pSrc; pDest++; pSrc++;
            }
            dest = pDest;
            src = pSrc;
            count = count - (block << 3);

            if (count > 0)
            {
                byte* pDestB = (byte*)dest;
                byte* pSrcB = (byte*)src;
                for (int i = 0; i < count; i++)
                {
                    *pDestB = *pSrcB; pDestB++; pSrcB++;
                }
            }
        }

        public static PixelBuffer WebPPictureImportAuto(BitmapSource bmp, ref WebPPicture wpic)
        {
            PixelBuffer pixelBuffer;

            wpic.width = bmp.PixelWidth;
            wpic.height = bmp.PixelHeight;
            wpic.use_argb = 1;

            if (bmp.Format == PixelFormats.Bgr24)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (UnsafeNativeMethods.WebPPictureImportBGRX(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32);
                if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                pixelBuffer = new PixelBuffer(bmp);
                if (UnsafeNativeMethods.WebPPictureImportRGB(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportRGB");
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32);
                if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                    throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    pixelBuffer = new PixelBuffer(bmp);
                    if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                        throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGR");
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    pixelBuffer = new PixelBuffer(bmp);
                    if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, pixelBuffer.GetPointer(), pixelBuffer.BackBufferStride) != 1)
                        throw new OutOfMemoryException("Can´t allocate memory in WebPPictureImportBGRA");
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return pixelBuffer;
        }

        public static int WebPPictureImportAuto(PixelFormat pixelFormat, ref WebPPicture wpic, IntPtr buffer, int stride)
        {
            if (pixelFormat == PixelFormats.Bgr24)
                return UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Bgr32)
                return UnsafeNativeMethods.WebPPictureImportBGRX(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Bgra32)
                return UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Pbgra32)
                return UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Rgb24)
                return UnsafeNativeMethods.WebPPictureImportRGB(ref wpic, buffer, stride);
            else if (pixelFormat == PixelFormats.Indexed8)
                return UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, buffer, stride);
            else
            {
                if (pixelFormat.BitsPerPixel == 24)
                    return UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, buffer, stride);
                else if (pixelFormat.BitsPerPixel == 32)
                    return UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, buffer, stride);
                else
                    throw new NotSupportedException("Image format not supported.");
            }
        }

        public static int WebPEncodeSimple(BitmapSource bmp, float quality, out IntPtr output)
        {
            int size = 0;
            if (bmp.Format == PixelFormats.Bgr24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                    size = UnsafeNativeMethods.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = UnsafeNativeMethods.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeRGB(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = UnsafeNativeMethods.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                        size = UnsafeNativeMethods.WebPEncodeBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                        size = UnsafeNativeMethods.WebPEncodeBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, quality, out output);
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return size;
        }

        public static int WebPEncodeLosslessSimple(BitmapSource bmp, out IntPtr output)
        {
            int size = 0;
            if (bmp.Format == PixelFormats.Bgr24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Bgr32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Bgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Pbgra32)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Rgb24)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp))
                    size = UnsafeNativeMethods.WebPEncodeLosslessRGB(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else if (bmp.Format == PixelFormats.Indexed8)
            {
                using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
            }
            else
            {
                if (bmp.Format.BitsPerPixel == 24)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgr24))
                        size = UnsafeNativeMethods.WebPEncodeLosslessBGR(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
                }
                else if (bmp.Format.BitsPerPixel == 32)
                {
                    using (PixelBuffer pixelBuffer = new PixelBuffer(bmp, PixelFormats.Bgra32))
                        size = UnsafeNativeMethods.WebPEncodeLosslessBGRA(pixelBuffer.GetPointer(), pixelBuffer.PixelWidth, pixelBuffer.PixelHeight, pixelBuffer.BackBufferStride, out output);
                }
                else
                {
                    throw new NotSupportedException("Image format not supported.");
                }
            }
            return size;
        }

        public const int WEBP_DECODER_ABI_VERSION = 0x0208;

        /// <summary>This function will initialize the configuration according to a predefined set of parameters (referred to by 'preset') and a given quality factor.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="preset">Type of image</param>
        /// <param name="quality">Quality of compresion</param>
        /// <returns>0 if error</returns>
        public static int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPConfigInitInternal_x86(ref config, preset, quality, WEBP_DECODER_ABI_VERSION);
                case 8:
                    return WebPConfigInitInternal_x64(ref config, preset, quality, WEBP_DECODER_ABI_VERSION);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }



        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x86(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x64(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);

        /// <summary>Get info of WepP image</summary>
        /// <param name="rawWebP">Bytes[] of webp image</param>
        /// <param name="data_size">Size of rawWebP</param>
        /// <param name="features">Features of WebP image</param>
        /// <returns>VP8StatusCode</returns>
        public static VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetFeaturesInternal_x86(rawWebP, (UIntPtr)data_size, ref features, WEBP_DECODER_ABI_VERSION);
                case 8:
                    return WebPGetFeaturesInternal_x64(rawWebP, (UIntPtr)data_size, ref features, WEBP_DECODER_ABI_VERSION);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x86([InAttribute()] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x64([InAttribute()] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        /// <summary>Activate the lossless compression mode with the desired efficiency.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <param name="level">between 0 (fastest, lowest compression) and 9 (slower, best compression)</param>
        /// <returns>0 in case of parameter errorr</returns>
        public static int WebPConfigLosslessPreset(ref WebPConfig config, int level)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPConfigLosslessPreset_x86(ref config, level);
                case 8:
                    return WebPConfigLosslessPreset_x64(ref config, level);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x86(ref WebPConfig config, int level);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x64(ref WebPConfig config, int level);

        /// <summary>Check that 'config' is non-NULL and all configuration parameters are within their valid ranges.</summary>
        /// <param name="config">The WebPConfig struct</param>
        /// <returns>1 if config are OK</returns>
        public static int WebPValidateConfig(ref WebPConfig config)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPValidateConfig_x86(ref config);
                case 8:
                    return WebPValidateConfig_x64(ref config);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x86(ref WebPConfig config);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x64(ref WebPConfig config);

        /// <summary>Init the struct WebPPicture ckecking the dll version</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <returns>1 if not error</returns>
        public static int WebPPictureInitInternal(ref WebPPicture wpic)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureInitInternal_x86(ref wpic, WEBP_DECODER_ABI_VERSION);
                case 8:
                    return WebPPictureInitInternal_x64(ref wpic, WEBP_DECODER_ABI_VERSION);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x86(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x64(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to BGR data</param>
        /// <param name="stride">stride of BGR data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public static int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportBGR_x86(ref wpic, bgr, stride);
                case 8:
                    return WebPPictureImportBGR_x64(ref wpic, bgr, stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x86(ref WebPPicture wpic, IntPtr bgr, int stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x64(ref WebPPicture wpic, IntPtr bgr, int stride);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wpic"></param>
        /// <param name="rgba"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        public static int WebPPictureImportBGRA(ref WebPPicture wpic, IntPtr rgba, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportBGRA_x86(ref wpic, rgba, stride);
                case 8:
                    return WebPPictureImportBGRA_x64(ref wpic, rgba, stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRA")]
        private static extern int WebPPictureImportBGRA_x86(ref WebPPicture wpic, IntPtr rgba, int stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRA")]
        private static extern int WebPPictureImportBGRA_x64(ref WebPPicture wpic, IntPtr rgba, int stride);


        public static int WebPPictureImportRGB(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportRGB_x86(ref wpic, bgr, stride);
                case 8:
                    return WebPPictureImportRGB_x64(ref wpic, bgr, stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportRGB")]
        private static extern int WebPPictureImportRGB_x86(ref WebPPicture wpic, IntPtr bgr, int stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportRGB")]
        private static extern int WebPPictureImportRGB_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        public static int WebPPictureImportRGBA(ref WebPPicture wpic, IntPtr rgba, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportRGBA_x86(ref wpic, rgba, stride);
                case 8:
                    return WebPPictureImportRGBA_x64(ref wpic, rgba, stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportRGBA")]
        private static extern int WebPPictureImportRGBA_x86(ref WebPPicture wpic, IntPtr rgba, int stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportRGBA")]
        private static extern int WebPPictureImportRGBA_x64(ref WebPPicture wpic, IntPtr rgba, int stride);
        /// <summary>Colorspace conversion function to import RGB samples.</summary>
        /// <param name="wpic">The WebPPicture struct</param>
        /// <param name="bgr">Point to BGR data</param>
        /// <param name="stride">stride of BGR data</param>
        /// <returns>Returns 0 in case of memory error.</returns>
        public static int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportBGRX_x86(ref wpic, bgr, stride);
                case 8:
                    return WebPPictureImportBGRX_x64(ref wpic, bgr, stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x86(ref WebPPicture wpic, IntPtr bgr, int stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        /// <summary>The writer type for output compress data</summary>
        /// <param name="data">Data returned</param>
        /// <param name="data_size">Size of data returned</param>
        /// <param name="wpic">Picture struct</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WebPMemoryWrite([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture wpic);
        // public delegate WebPMemoryWrite OnCallback(WebP.MemoryWriter writer);

        /// <summary>Compress to webp format</summary>
        /// <param name="config">The config struct for compresion parameters</param>
        /// <param name="picture">'picture' hold the source samples in both YUV(A) or ARGB input</param>
        /// <returns>Returns 0 in case of error, 1 otherwise. In case of error, picture->error_code is updated accordingly.</returns>
        public static int WebPEncode(ref WebPConfig config, ref WebPPicture picture)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncode_x86(ref config, ref picture);
                case 8:
                    return WebPEncode_x64(ref config, ref picture);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x86(ref WebPConfig config, ref WebPPicture picture);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x64(ref WebPConfig config, ref WebPPicture picture);

        /// <summary>Release the memory allocated by WebPPictureAlloc() or WebPPictureImport*()
        /// Note that this function does _not_ free the memory used by the 'picture' object itself.
        /// Besides memory (which is reclaimed) all other fields of 'picture' are preserved.</summary>
        /// <param name="picture">Picture struct</param>
        public static void WebPPictureFree(ref WebPPicture picture)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPPictureFree_x86(ref picture);
                    break;
                case 8:
                    WebPPictureFree_x64(ref picture);
                    break;
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x86(ref WebPPicture wpic);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x64(ref WebPPicture wpic);

        /// <summary>Validate the WebP image header and retrieve the image height and width. Pointers *width and *height can be passed NULL if deemed irrelevant</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <returns>1 if success, otherwise error code returned in the case of (a) formatting error(s).</returns>
        public static int WebPGetInfo(IntPtr data, int data_size, out int width, out int height)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetInfo_x86(data, (UIntPtr)data_size, out width, out height);
                case 8:
                    return WebPGetInfo_x64(data, (UIntPtr)data_size, out width, out height);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x86([InAttribute()] IntPtr data, UIntPtr data_size, out int width, out int height);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x64([InAttribute()] IntPtr data, UIntPtr data_size, out int width, out int height);

        /// <summary>Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer</summary>
        /// <param name="data">Pointer to WebP image data</param>
        /// <param name="data_size">This is the size of the memory block pointed to by data containing the image data</param>
        /// <param name="output_buffer">Pointer to decoded WebP image</param>
        /// <param name="output_buffer_size">Size of allocated buffer</param>
        /// <param name="output_stride">Specifies the distance between scanlines</param>
        /// <returns>output_buffer if function succeeds; NULL otherwise</returns>
        public static int WebPDecodeBGRInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPDecodeBGRInto_x86(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride);
                case 8:
                    return WebPDecodeBGRInto_x64(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x86([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x64([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        /// <summary>Initialize the configuration as empty. This function must always be called first, unless WebPGetFeatures() is to be called.</summary>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>False in case of mismatched version.</returns>
        public static int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPInitDecoderConfigInternal_x86(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION);
                case 8:
                    return WebPInitDecoderConfigInternal_x64(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x86(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x64(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        /// <summary>Decodes the full data at once, taking 'config' into account.</summary>
        /// <param name="data">WebP raw data to decode</param>
        /// <param name="data_size">Size of WebP data </param>
        /// <param name="webPDecoderConfig">Configuration struct</param>
        /// <returns>VP8_STATUS_OK if the decoding was successful</returns>
        public static VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPDecode_x86(data, (UIntPtr)data_size, ref webPDecoderConfig);
                case 8:
                    return WebPDecode_x64(data, (UIntPtr)data_size, ref webPDecoderConfig);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode WebPDecode_x86(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode WebPDecode_x64(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        /// <summary>Free any memory associated with the buffer. Must always be called last. Doesn't free the 'buffer' structure itself.</summary>
        /// <param name="buffer">WebPDecBuffer</param>
        public static void WebPFreeDecBuffer(ref WebPDecBuffer buffer)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFreeDecBuffer_x86(ref buffer);
                    break;
                case 8:
                    WebPFreeDecBuffer_x64(ref buffer);
                    break;
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x86(ref WebPDecBuffer buffer);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x64(ref WebPDecBuffer buffer);


        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x86(ref WebPDecBuffer buffer);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPINewDecoder")]
        private static extern void WebPINewDecoder_x64(ref WebPDecBuffer buffer);

        /// <summary>Lossy encoding images</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="quality_factor">Ranges from 0 (lower quality) to 100 (highest quality). Controls the loss and quality during compression</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public static int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeBGR_x86(bgr, width, height, stride, quality_factor, out output);
                case 8:
                    return WebPEncodeBGR_x64(bgr, width, height, stride, quality_factor, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="quality_factor"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeRGB(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeRGB_x86(bgr, width, height, stride, quality_factor, out output);
                case 8:
                    return WebPEncodeRGB_x64(bgr, width, height, stride, quality_factor, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeRGB")]
        private static extern int WebPEncodeRGB_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeRGB")]
        private static extern int WebPEncodeRGB_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="quality_factor"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeBGRA(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeBGRA_x86(bgr, width, height, stride, quality_factor, out output);
                case 8:
                    return WebPEncodeBGRA_x64(bgr, width, height, stride, quality_factor, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGRA")]
        private static extern int WebPEncodeBGRA_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGRA")]
        private static extern int WebPEncodeBGRA_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="quality_factor"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeRGBA(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeRGBA_x86(bgr, width, height, stride, quality_factor, out output);
                case 8:
                    return WebPEncodeRGBA_x64(bgr, width, height, stride, quality_factor, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeRGBA")]
        private static extern int WebPEncodeRGBA_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeRGBA")]
        private static extern int WebPEncodeRGBA_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);
        /// <summary>Lossless encoding images pointed to by *data in WebP format</summary>
        /// <param name="bgr">Pointer to BGR image data</param>
        /// <param name="width">The range is limited currently from 1 to 16383</param>
        /// <param name="height">The range is limited currently from 1 to 16383</param>
        /// <param name="stride">Specifies the distance between scanlines</param>
        /// <param name="output">output_buffer with WebP image</param>
        /// <returns>Size of WebP Image or 0 if an error occurred</returns>
        public static int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeLosslessBGR_x86(bgr, width, height, stride, out output);
                case 8:
                    return WebPEncodeLosslessBGR_x64(bgr, width, height, stride, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeLosslessBGRA(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeLosslessBGRA_x86(bgr, width, height, stride, out output);
                case 8:
                    return WebPEncodeLosslessBGRA_x64(bgr, width, height, stride, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGRA")]
        private static extern int WebPEncodeLosslessBGRA_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGRA")]
        private static extern int WebPEncodeLosslessBGRA_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeLosslessRGB(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeLosslessRGB_x86(bgr, width, height, stride, out output);
                case 8:
                    return WebPEncodeLosslessRGB_x64(bgr, width, height, stride, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessRGB")]
        private static extern int WebPEncodeLosslessRGB_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessRGB")]
        private static extern int WebPEncodeLosslessRGB_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int WebPEncodeLosslessRGBA(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeLosslessRGBA_x86(bgr, width, height, stride, out output);
                case 8:
                    return WebPEncodeLosslessRGBA_x64(bgr, width, height, stride, out output);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessRGBA")]
        private static extern int WebPEncodeLosslessRGBA_x86([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessRGBA")]
        private static extern int WebPEncodeLosslessRGBA_x64([InAttribute()] IntPtr bgr, int width, int height, int stride, out IntPtr output);
        /// <summary>Releases memory returned by the WebPEncode</summary>
        /// <param name="p">Pointer to memory</param>
        public static void WebPFree(IntPtr p)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFree_x86(p);
                    break;
                case 8:
                    WebPFree_x64(p);
                    break;
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x86(IntPtr p);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x64(IntPtr p);

        /// <summary>Get the webp version library</summary>
        /// <returns>8bits for each of major/minor/revision packet in integer. E.g: v2.5.7 is 0x020507</returns>
        public static int WebPGetDecoderVersion()
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetDecoderVersion_x86();
                case 8:
                    return WebPGetDecoderVersion_x64();
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x86();
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x64();

        /// <summary>Compute PSNR, SSIM or LSIM distortion metric between two pictures.</summary>
        /// <param name="srcPicture">Picture to measure</param>
        /// <param name="refPicture">Reference picture</param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="pResult">dB in the Y/U/V/Alpha/All order</param>
        /// <returns>False in case of error (src and ref don't have same dimension, ...)</returns>
        public static int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureDistortion_x86(ref srcPicture, ref refPicture, metric_type, pResult);
                case 8:
                    return WebPPictureDistortion_x64(ref srcPicture, ref refPicture, metric_type, pResult);
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }
        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x86(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);
        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x64(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);
    }
}
