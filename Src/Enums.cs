﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPWrapper
{
    public enum CompressionType
    {
        NearLossless,
        Lossy,
        Lossless
    }
    /// <summary>Predictive filtering method for alpha plane</summary>
    public enum AlphaFiltering
    {
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
        /// <summary>Slowest compression speed but highest quality</summary>
        Level6,
        Default = Level5,
        Fastest = Level0,
        Best = Level6,
    }

    public enum FilterSharpness
    {
        Off,
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
        None,
        Lossless
    }

    /// <summary>Enumerate some predefined settings for WebPConfig, depending on the type of source picture. These presets are used when calling WebPConfigPreset().</summary>
    public enum WebPPreset
    {
        /// <summary>Default preset.</summary>
        WEBP_PRESET_DEFAULT = 0,
        /// <summary>Digital picture, like portrait, inner shot.</summary>
        WEBP_PRESET_PICTURE,
        /// <summary>Outdoor photograph, with natural lighting.</summary>
        WEBP_PRESET_PHOTO,
        /// <summary>Hand or line drawing, with high-contrast details.</summary>
        WEBP_PRESET_DRAWING,
        /// <summary>Small-sized colorful images.</summary>
        WEBP_PRESET_ICON,
        /// <summary>Text-like.</summary>
        WEBP_PRESET_TEXT
    };

    /// <summary>Encoding error conditions.</summary>
    public enum WebPEncodingError
    {
        /// <summary>No error.</summary>
        VP8_ENC_OK = 0,
        /// <summary>Memory error allocating objects.</summary>
        VP8_ENC_ERROR_OUT_OF_MEMORY,
        /// <summary>Memory error while flushing bits.</summary>
        VP8_ENC_ERROR_BITSTREAM_OUT_OF_MEMORY,
        /// <summary>A  pointer parameter is NULL.</summary>
        VP8_ENC_ERROR_NULL_PARAMETER,
        /// <summary>Configuration is invalid.</summary>
        VP8_ENC_ERROR_INVALID_CONFIGURATION,
        /// <summary>Picture has invalid width/height.</summary>
        VP8_ENC_ERROR_BAD_DIMENSION,
        /// <summary>Partition is bigger than 512k.</summary>
        VP8_ENC_ERROR_PARTITION0_OVERFLOW,
        /// <summary>Partition is bigger than 16M.</summary>
        VP8_ENC_ERROR_PARTITION_OVERFLOW,
        /// <summary>Error while flushing bytes.</summary>
        VP8_ENC_ERROR_BAD_WRITE,
        /// <summary>File is bigger than 4G.</summary>
        VP8_ENC_ERROR_FILE_TOO_BIG,
        /// <summary>Abort request by user.</summary>
        VP8_ENC_ERROR_USER_ABORT,
        /// <summary>List terminator. always last.</summary>
        VP8_ENC_ERROR_LAST,
    }

    /// <summary>Enumeration of the status codes.</summary>
    public enum VP8StatusCode
    {
        /// <summary>No error.</summary>
        VP8_STATUS_OK = 0,
        /// <summary>Memory error allocating objects.</summary>
        VP8_STATUS_OUT_OF_MEMORY,
        VP8_STATUS_INVALID_PARAM,
        VP8_STATUS_BITSTREAM_ERROR,
        /// <summary>Configuration is invalid.</summary>
        VP8_STATUS_UNSUPPORTED_FEATURE,
        VP8_STATUS_SUSPENDED,
        /// <summary>Abort request by user.</summary>
        VP8_STATUS_USER_ABORT,
        VP8_STATUS_NOT_ENOUGH_DATA,
    }

    /// <summary>Image characteristics hint for the underlying encoder.</summary>
    public enum WebPImageHint
    {
        /// <summary>Default preset.</summary>
        WEBP_HINT_DEFAULT = 0,
        /// <summary>Digital picture, like portrait, inner shot</summary>
        WEBP_HINT_PICTURE,
        /// <summary>Outdoor photograph, with natural lighting</summary>
        WEBP_HINT_PHOTO,
        /// <summary>Discrete tone image (graph, map-tile etc).</summary>
        WEBP_HINT_GRAPH,
        /// <summary>list terminator. always last.</summary>
        WEBP_HINT_LAST
    };

    /// <summary>Describes the byte-ordering of packed samples in memory.</summary>
    enum WEBP_CSP_MODE
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
    }

    /// <summary>
    /// Decoding states. State normally flows as:
    /// WEBP_HEADER->VP8_HEADER->VP8_PARTS0->VP8_DATA->DONE for a lossy image, and
    /// WEBP_HEADER->VP8L_HEADER->VP8L_DATA->DONE for a lossless image.
    /// If there is any error the decoder goes into state ERROR.
    /// </summary>
    enum DecState
    {
        STATE_WEBP_HEADER,  // All the data before that of the VP8/VP8L chunk.
        STATE_VP8_HEADER,   // The VP8 Frame header (within the VP8 chunk).
        STATE_VP8_PARTS0,
        STATE_VP8_DATA,
        STATE_VP8L_HEADER,
        STATE_VP8L_DATA,
        STATE_DONE,
        STATE_ERROR
    }
}
