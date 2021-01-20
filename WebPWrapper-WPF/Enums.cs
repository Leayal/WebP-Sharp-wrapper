using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WPF
namespace WebPWrapper.WPF
#else
namespace WebPWrapper.WinForms
#endif
{
    /// <summary>Specifies the format of the color data for each pixel in the output.</summary>
    public enum OutputPixelFormat
    {
        /// <summary>Specifies that the format should be optimized for rendering operations.</summary>
        /// <remarks>Will be <seealso cref="PBGRA32"/> if the image has no alpha, otherwise, <seealso cref="BGR32"/></remarks>
        OptimizedForRendering = 0,
        /// <summary>Specifies that the format should be small if possible.</summary>
        /// <remarks>Will be <seealso cref="BGR24"/> if the image has no alpha, otherwise, <seealso cref="BGRA32"/>. However, in case where the decoder doesn't know whether the image has alpha or not, <seealso cref="BGRA32"/> will be used.</remarks>
        PreferSmallSize,
        /// <summary>Specifies that the format is 24 bits per pixel; 8 bits each are used for the red, green, and blue components.</summary>
        BGR24,
        /// <summary>Specifies that the format is 32 bits per pixel; 8 bits each are used for the red, green, and blue components. The remaining 8 bits are not used.</summary>
        BGR32,
        /// <summary> Specifies that the format is 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue components.</summary>
        BGRA32,
        /// <summary>Specifies that the format is 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue components. The red, green, and blue components are premultiplied, according to the alpha component, alpha component will be preserved.</summary>
        PBGRA32
    }
}
