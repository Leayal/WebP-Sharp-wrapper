using System;
using System.Windows;
using WebPWrapper.LowLevel;

#if WPF
namespace WebPWrapper.WPF
#else
namespace WebPWrapper.WinForms
#endif
{
    /// <summary>WebP Decoding options.</summary>
    public class WindowsDecoderOptions : DecoderOptions
    {
        /// <summary>Initialize a new option instance.</summary>
        public WindowsDecoderOptions()
        {
            this.PixelFormat = OutputPixelFormat.OptimizedForRendering;
        }

        /// <summary>Specify the output pixel format for the decoded image.</summary>
        public OutputPixelFormat PixelFormat { get; set; }
    }
}
