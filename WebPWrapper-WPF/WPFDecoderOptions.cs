using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WebPWrapper.WPF
{
    /// <summary>Webp Decoder Option for WPF.</summary>
    public class WPFDecoderOptions : DecoderOptions
    {
        /// <summary>Determines whether choosing pixel buffer that is optimized for rendering or not.</summary>
        /// <remarks>True = <seealso cref="PixelFormats.Pbgra32"/>. False = <seealso cref="PixelFormats.Bgra32"/></remarks>
        public bool OptimizeForRendering { get; set; }

        public WPFDecoderOptions() : base()
        {
            this.OptimizeForRendering = true;
        }
    }
}
