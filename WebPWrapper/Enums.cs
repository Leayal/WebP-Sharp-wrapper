using System;

namespace WebPWrapper
{
    /// <summary>Flags to determine how the encoder uses the memory</summary>
    [Flags]
    public enum MemoryAllowance
    {
        /// <summary>Allow the encoder use as much memory as possible.</summary>
        AsMuchAsPossible = 0,
        /// <summary>Tell the encoder to use as less memory as possible. May result in slower encode speed. This will overwrite <see cref="AsMuchAsPossible"/>.</summary>
        LowMemoryCompressionMode = 1 << 0,
        /// <summary>
        /// If this flag is set, the encoder will try to allocate contiguous memory block and write the output to it. This flag DOES NOT affects "encode to file" methods.
        /// It is not recommended in general unless you really need it.
        /// </summary>
        /// <remarks>Currently has no meaning or any uses.</remarks>
        ForcedContiguousMemory = 1 << 1
    }
    /// <summary>Compression algorithm</summary>
    public enum CompressionType
    {
        /// <summary>Lossy compression</summary>
        Lossy,
        /// <summary>Lossless compression</summary>
        Lossless,
        /// <summary>Somewhere between the other two compression</summary>
        /// <remarks>Experimental method.</remarks>
        NearLossless
    }
    /// <summary>Predictive filtering method for alpha plane</summary>
    public enum AlphaFiltering
    {
        /// <summary>No alpha-filtering</summary>
        None,
        /// <summary>Default value</summary>
        Fast,
        /// <summary>Slower.</summary>
        Best
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>Compression level profile</summary>
    public enum CompressionLevel
    {
        /// <summary>Fastest compression speed but lowest ratio</summary>
        Level0 = 0,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8,
        /// <summary>Slowest compression speed but highest ratio</summary>
        Level9,
        Lowest = Level0,
        Default = Level5,
        Highest = Level9,
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum FilterSharpness
    {
        /// <summary>Turn off sharpness filter</summary>
        Off = 0,
        /// <summary>Highest sharpness</summary>
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        /// <summary>Lowest sharpness</summary>
        Level7,
        Highest = Level1,
        Lowest = Level7
    }

    public enum FilterType
    {
        Simple,
        Strong
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>Algorithm for encoding the alpha plane</summary>
    public enum AlphaCompressionType
    {
        /// <summary>Turn off alpha plane compression</summary>
        None,
        /// <summary>Lossless compression</summary>
        Lossless
    }

    /// <summary>Enumerate some predefined settings for WebPConfig, depending on the type of source picture. These presets are used when calling WebPConfigPreset().</summary>
    public enum WebPPreset
    {
        /// <summary>Default preset.</summary>
        Default = 0,
        /// <summary>Digital picture, like portrait, inner shot.</summary>
        Picture,
        /// <summary>Outdoor photograph, with natural lighting.</summary>
        Photo,
        /// <summary>Hand or line drawing, with high-contrast details.</summary>
        Drawing,
        /// <summary>Small-sized colorful images.</summary>
        Icon,
        /// <summary>Text-like images.</summary>
        Text
    };

    /// <summary>Image characteristics hint for the underlying encoder.</summary>
    public enum WebPImageHint
    {
        /// <summary>Default preset.</summary>
        Default = 0,
        /// <summary>Digital picture, like portrait, inner shot</summary>
        Picture,
        /// <summary>Outdoor photograph, with natural lighting</summary>
        Photo,
        /// <summary>Discrete tone image (graph, map-tile etc).</summary>
        Graph,
        /// <summary>list terminator. always last.</summary>
        /// <remarks>This value has no use or meaning</remarks>
        Last
    };

    /// <summary>Describes the byte-ordering of packed samples in memory.</summary>
    public enum Colorspace
    {
        /// <summary>Byte-order: R,G,B,R,G,B,...</summary>
        MODE_RGB = 0,
        /// <summary>Byte-order: R,G,B,A,R,G,B,A,...</summary>
        MODE_RGBA = 1,
        /// <summary>Byte-order: B,G,R,B,G,R,...</summary>
        MODE_BGR = 2,
        /// <summary>Byte-order: B,G,R,A,B,G,R,A,...</summary>
        MODE_BGRA = 3,
        /// <summary>Byte-order: A,R,G,B,A,R,G,B,...</summary>
        MODE_ARGB = 4,
        /// <summary>Byte-order: RGB-565: [a4 a3 a2 a1 a0 r5 r4 r3], [r2 r1 r0 g4 g3 g2 g1 g0], ...
        /// WEBP_SWAP_16BITS_CSP is defined, 
        /// Byte-order: RGB-565: [a4 a3 a2 a1 a0 b5 b4 b3], [b2 b1 b0 g4 g3 g2 g1 g0], ...</summary>
        MODE_RGBA_4444 = 5,
        /// <summary>Byte-order: RGB-565: [r4 r3 r2 r1 r0 g5 g4 g3], [g2 g1 g0 b4 b3 b2 b1 b0], ...
        /// WEBP_SWAP_16BITS_CSP is defined, 
        /// Byte-order: [b3 b2 b1 b0 a3 a2 a1 a0], [r3 r2 r1 r0 g3 g2 g1 g0], ...</summary>
        MODE_RGB_565 = 6,
        /// <summary>RGB-premultiplied transparent modes (alpha value is preserved)</summary>
        MODE_rgbA = 7,
        /// <summary>RGB-premultiplied transparent modes (alpha value is preserved)</summary>
        MODE_bgrA = 8,
        /// <summary>RGB-premultiplied transparent modes (alpha value is preserved)</summary>
        MODE_Argb = 9,
        /// <summary>RGB-premultiplied transparent modes (alpha value is preserved)</summary>
        MODE_rgbA_4444 = 10,
        /// <summary>yuv 4:2:0</summary>
        MODE_YUV = 11,
        /// <summary>yuv 4:2:0</summary>
        MODE_YUVA = 12,
        /// <summary>MODE_LAST -> 13</summary>
        MODE_LAST = 13,
    };
}
