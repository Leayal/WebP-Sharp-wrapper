using System;
using WebPWrapper.LowLevel;

#if WPF
using System.Windows.Media;
namespace WebPWrapper.WPF
#else
using System.Drawing.Imaging;
namespace WebPWrapper.WinForms
#endif
{
    static class Helper
    {
        public static OutputPixelFormat DecideOutputPixelFormat(OutputPixelFormat format, bool? hasAlpha)
        {
            switch (format)
            {
                case OutputPixelFormat.PreferSmallSize:
                    if (hasAlpha.HasValue)
                    {
                        return (hasAlpha.Value ? OutputPixelFormat.BGRA32 : OutputPixelFormat.BGR24);
                    }
                    else
                    {
                        return OutputPixelFormat.BGRA32;
                    }
                case OutputPixelFormat.OptimizedForRendering:
                    if (hasAlpha.HasValue)
                    {
                        return (hasAlpha.Value ? OutputPixelFormat.PBGRA32 : OutputPixelFormat.BGR32);
                    }
                    else
                    {
                        return OutputPixelFormat.BGRA32;
                    }
                default:
                    return format;
            }
        }

        public static PixelFormat GetPixelFormat(OutputPixelFormat format)
        {
            switch (format)
            {
#if WPF
                case OutputPixelFormat.BGR24:
                    return PixelFormats.Bgr24;
                case OutputPixelFormat.BGR32:
                    return PixelFormats.Bgr32;
                case OutputPixelFormat.BGRA32:
                    return PixelFormats.Bgra32;
                case OutputPixelFormat.PBGRA32:
                    return PixelFormats.Pbgra32;
#else
                case OutputPixelFormat.BGR24:
                    return PixelFormat.Format24bppRgb;
                case OutputPixelFormat.BGR32:
                    return PixelFormat.Format32bppRgb;
                case OutputPixelFormat.BGRA32:
                    return PixelFormat.Format32bppArgb;
                case OutputPixelFormat.PBGRA32:
                    return PixelFormat.Format32bppPArgb;
#endif
            }
            throw new ArgumentOutOfRangeException();
        }

        public static Colorspace GetWebpPixelFormat(OutputPixelFormat format)
        {
            switch (format)
            {
                case OutputPixelFormat.BGR24:
                    return Colorspace.MODE_BGR;
                case OutputPixelFormat.BGR32:
                    return Colorspace.MODE_BGRA;
                case OutputPixelFormat.BGRA32:
                    return Colorspace.MODE_BGRA;
                case OutputPixelFormat.PBGRA32:
                    return Colorspace.MODE_bgrA;
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}
