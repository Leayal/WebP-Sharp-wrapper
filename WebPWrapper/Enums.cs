using System;

namespace WebPWrapper
{
    /// <summary>Flags to determine how the encoder uses the memory</summary>
    [Flags]
    public enum MemoryAllowance
    {
        /// <summary>
        /// Allow the encoder use as much memory as possible.
        /// </summary>
        AsMuchAsPossible = 0,
        /// <summary>
        /// Tell the encoder to use as less memory as possible. May result in slower encode speed. This will overwrite <see cref="AsMuchAsPossible"/>.
        /// </summary>
        LowMemoryCompressionMode = 1 << 0,
        /// <summary>
        /// If this flag is set, the encoder will try to allocate contiguous memory block and write the output to it. This flag DOES NOT affects "encode to file" methods.
        /// It is not recommended in general unless you really need it.
        /// </summary>
        ForcedContiguousMemory = 1 << 1
    }
    /// <summary>Compression method</summary>
    public enum CompressionType
    {
        /// <summary>[Experimental] Somewhere between the other two compression</summary>
        NearLossless,
        /// <summary>Lossy compression</summary>
        Lossy,
        /// <summary>Lossless compression</summary>
        Lossless
    }
    /// <summary>Predictive filtering method for alpha plane</summary>
    public enum AlphaFiltering
    {
        /// <summary>No alpha-filtering</summary>
        None,
        /// <summary>Default value</summary>
        Fast,
        Best
    }

    /// <summary>Compression level profile</summary>
    public enum CompressionLevel
    {
        /// <summary>Fastest compression speed but lowest quality</summary>
        Level0,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8,
        /// <summary>Slowest compression speed but highest quality</summary>
        Level9,
        Default = Level5,
        Fastest = Level0,
        Best = Level9,
    }

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
        Last
    };
}
