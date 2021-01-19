using System;
using System.Drawing.Imaging;

namespace WebPWrapper.WinForms
{
    /// <summary>Webp Decoder Option for Windows Forms.</summary>
    public class WinFormsDecoderOptions : DecoderOptions
    {
        /// <summary>Determines whether choosing pixel buffer that is optimized for rendering or not.</summary>
        /// <remarks>True = <seealso cref="PixelFormat.Format32bppPArgb"/>. False = <seealso cref="PixelFormat.Format32bppArgb"/></remarks>
        public bool OptimizeForRendering { get; set; }

        public WinFormsDecoderOptions() : base()
        {
            this.OptimizeForRendering = true;
        }
    }
}
