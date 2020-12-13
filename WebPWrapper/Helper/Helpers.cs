using System;
using System.Runtime.InteropServices;

namespace WebPWrapper.Helper
{
    static class RuntimeValue
    {
        internal static readonly bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        internal static readonly bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        internal static readonly bool isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    static class ScaleHelper
    {
        internal static void GetScaledWidthAndHeight(BitmapSizeOptions instance, int width, int height, out int newWidth, out int newHeight)
        {
            int _pixelWidth = instance.PixelWidth,
                _pixelHeight = instance.PixelHeight;
            if (_pixelWidth == 0 && _pixelHeight != 0)
            {
                newWidth = (_pixelHeight * width / height);
                newHeight = _pixelHeight;
            }
            else if (_pixelWidth != 0 && _pixelHeight == 0)
            {
                newWidth = _pixelWidth;
                newHeight = (_pixelWidth * height / width);
            }
            else if (_pixelWidth != 0 && _pixelHeight != 0)
            {
                newWidth = _pixelWidth;
                newHeight = _pixelHeight;
            }
            else
            {
                newWidth = width;
                newHeight = height;
            }
        }
    }
}
