﻿using System;
using System.Runtime.InteropServices;

namespace WebPWrapper.LowLevel
{
    /// <summary>Features gathered from the bitstream</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPBitstreamFeatures
    {
        /// <summary>Width in pixels, as read from the bitstream.</summary>
        public int width;
        /// <summary>Height in pixels, as read from the bitstream.</summary>
        public int height;
        /// <summary>True if the bitstream contains an alpha channel.</summary>
        public int has_alpha;
        /// <summary>True if the bitstream is an animation.</summary>
        public int has_animation;
        /// <summary>0 = undefined (/mixed), 1 = lossy, 2 = lossless</summary>
        public int format;
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
        private uint[] pad;
    };

    /// <summary>Compression parameters. (For WebP release before v1.1.0). This is for compatibility reason.</summary>
    [StructLayout(LayoutKind.Sequential)]
    struct WebPConfigOld_1_0_3
    {
        /// <summary>Lossless encoding (0=lossy(default), 1=lossless).</summary>
        public int lossless;
        /// <summary>Between 0 (smallest file) and 100 (biggest)</summary>
        public float quality;
        /// <summary>Quality/speed trade-off (0=fast, 6=slower-better)</summary>
        public int method;
        /// <summary>Hint for image type (lossless only for now).</summary>
        public WebPImageHint image_hint;
        /// <summary>If non-zero, set the desired target size in bytes. Takes precedence over the 'compression' parameter.</summary>
        public int target_size;
        /// <summary>If non-zero, specifies the minimal distortion to try to achieve. Takes precedence over target_size.</summary>
        public float target_PSNR;
        /// <summary>Maximum number of segments to use, in [1..4]</summary>
        public int segments;
        /// <summary>Spatial Noise Shaping. 0=off, 100=maximum.</summary>
        public int sns_strength;
        /// <summary>Range: [0 = off .. 100 = strongest]</summary>
        public int filter_strength;
        /// <summary>Range: [0 = off .. 7 = least sharp]</summary>
        public int filter_sharpness;
        /// <summary>Filtering type: 0 = simple, 1 = strong (only used if filter_strength > 0 or autofilter > 0)</summary>
        public int filter_type;
        /// <summary>Auto adjust filter's strength [0 = off, 1 = on]</summary>
        public int autofilter;
        /// <summary>Algorithm for encoding the alpha plane (0 = none, 1 = compressed with WebP lossless). Default is 1.</summary>
        public int alpha_compression;
        /// <summary>Predictive filtering method for alpha plane. 0: none, 1: fast, 2: best. Default is 1.</summary>
        public int alpha_filtering;
        /// <summary>Between 0 (smallest size) and 100 (lossless). Default is 100.</summary>
        public int alpha_quality;
        /// <summary>Number of entropy-analysis passes (in [1..10]).</summary>
        public int pass;
        /// <summary>If true, export the compressed picture back. In-loop filtering is not applied.</summary>
        public int show_compressed;
        /// <summary>Preprocessing filter (0=none, 1=segment-smooth)</summary>
        public int preprocessing;
        /// <summary>Log2(number of token partitions) in [0..3] Default is set to 0 for easier progressive decoding.</summary>
        public int partitions;
        /// <summary>Quality degradation allowed to fit the 512k limit on prediction modes coding (0: no degradation, 100: maximum possible degradation).</summary>
        public int partition_limit;
        /// <summary>If true, compression parameters will be remapped to better match the expected output size from JPEG compression. Generally, the output size will be similar but the degradation will be lower.</summary>
        public int emulate_jpeg_size;
        /// <summary>If non-zero, try and use multi-threaded encoding.</summary>
        public int thread_level;
        /// <summary>If set, reduce memory usage (but increase CPU use).</summary>
        public int low_memory;
        /// <summary>Near lossless encoding [0 = off(default) .. 100]. This feature is experimental.</summary>
        public int near_lossless;
        /// <summary>If non-zero, preserve the exact RGB values under transparent area. Otherwise, discard this invisible RGB information for better compression. The default value is 0.</summary>
        public int exact;
        /// <summary>Reserved for future lossless feature</summary>
        public int delta_palettization;
        /// <summary>Padding for later use.</summary>
        private int pad1;
        /// <summary>Padding for later use.</summary>
        private int pad2;
    };

    /// <summary>Compression parameters.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPConfig
    {
        /// <summary>Lossless encoding (0=lossy(default), 1=lossless).</summary>
        public int lossless;
        /// <summary>Between 0 (smallest file) and 100 (biggest)</summary>
        public float quality;
        /// <summary>Quality/speed trade-off (0=fast, 6=slower-better)</summary>
        public int method;
        /// <summary>Hint for image type (lossless only for now).</summary>
        public WebPImageHint image_hint;
        /// <summary>If non-zero, set the desired target size in bytes. Takes precedence over the 'compression' parameter.</summary>
        public int target_size;
        /// <summary>If non-zero, specifies the minimal distortion to try to achieve. Takes precedence over target_size.</summary>
        public float target_PSNR;
        /// <summary>Maximum number of segments to use, in [1..4]</summary>
        public int segments;
        /// <summary>Spatial Noise Shaping. 0=off, 100=maximum.</summary>
        public int sns_strength;
        /// <summary>Range: [0 = off .. 100 = strongest]</summary>
        public int filter_strength;
        /// <summary>Range: [0 = off .. 7 = least sharp]</summary>
        public int filter_sharpness;
        /// <summary>Filtering type: 0 = simple, 1 = strong (only used if filter_strength > 0 or autofilter > 0)</summary>
        public int filter_type;
        /// <summary>Auto adjust filter's strength [0 = off, 1 = on]</summary>
        public int autofilter;
        /// <summary>Algorithm for encoding the alpha plane (0 = none, 1 = compressed with WebP lossless). Default is 1.</summary>
        public int alpha_compression;
        /// <summary>Predictive filtering method for alpha plane. 0: none, 1: fast, 2: best. Default is 1.</summary>
        public int alpha_filtering;
        /// <summary>Between 0 (smallest size) and 100 (lossless). Default is 100.</summary>
        public int alpha_quality;
        /// <summary>Number of entropy-analysis passes (in [1..10]).</summary>
        public int pass;
        /// <summary>If true, export the compressed picture back. In-loop filtering is not applied.</summary>
        public int show_compressed;
        /// <summary>Preprocessing filter (0=none, 1=segment-smooth)</summary>
        public int preprocessing;
        /// <summary>Log2(number of token partitions) in [0..3] Default is set to 0 for easier progressive decoding.</summary>
        public int partitions;
        /// <summary>Quality degradation allowed to fit the 512k limit on prediction modes coding (0: no degradation, 100: maximum possible degradation).</summary>
        public int partition_limit;
        /// <summary>If true, compression parameters will be remapped to better match the expected output size from JPEG compression. Generally, the output size will be similar but the degradation will be lower.</summary>
        public int emulate_jpeg_size;
        /// <summary>If non-zero, try and use multi-threaded encoding.</summary>
        public int thread_level;
        /// <summary>If set, reduce memory usage (but increase CPU use).</summary>
        public int low_memory;
        /// <summary>Near lossless encoding [0 = off(default) .. 100]. This feature is experimental.</summary>
        public int near_lossless;
        /// <summary>If non-zero, preserve the exact RGB values under transparent area. Otherwise, discard this invisible RGB information for better compression. The default value is 0.</summary>
        public int exact;
        /// <summary>Reserved for future lossless feature</summary>
        public int delta_palettization;
        /// <summary>Padding for later use.</summary>
        private int pad1;
        /// <summary>Padding for later use.</summary>
        private int pad2;
    };

    /// <summary>Main exchange structure (input samples, output bytes, statistics)</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPPicture
    {
        /// <summary>Main flag for encoder selecting between ARGB or YUV input. Recommended to use ARGB input (*argb, argb_stride) for lossless, and YUV input (*y, *u, *v, etc.) for lossy</summary>
        public int use_argb;
        /// <summary>colorspace: should be YUV420 for now (=Y'CbCr). Value = 0</summary>
        public UInt32 colorspace;
        /// <summary>Width of picture (less or equal to WEBP_MAX_DIMENSION)</summary>
        public int width;
        /// <summary>Height of picture (less or equal to WEBP_MAX_DIMENSION)</summary>
        public int height;
        /// <summary>Pointer to luma plane.</summary>
        public IntPtr y;
        /// <summary>Pointer to chroma U plane.</summary>
        public IntPtr u;
        /// <summary>Pointer to chroma V plane.</summary>
        public IntPtr v;
        /// <summary>Luma stride.</summary>
        public int y_stride;
        /// <summary>Chroma stride.</summary>
        public int uv_stride;
        /// <summary>Pointer to the alpha plane</summary>
        public IntPtr a;
        /// <summary>stride of the alpha plane</summary>
        public int a_stride;
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private uint[] pad1;
        /// <summary>Pointer to argb (32 bit) plane.</summary>
        public IntPtr argb;
        /// <summary>This is stride in pixels units, not bytes.</summary>
        public int argb_stride;
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
        private uint[] pad2;
        /// <summary>Byte-emission hook, to store compressed bytes as they are ready.</summary>
        public IntPtr writer;
        /// <summary>Can be used by the writer.</summary>
        public IntPtr custom_ptr;
        // map for extra information (only for lossy compression mode)
        /// <summary>1: intra type, 2: segment, 3: quant, 4: intra-16 prediction mode, 5: chroma prediction mode, 6: bit cost, 7: distortion</summary>
        public int extra_info_type;
        /// <summary>if not NULL, points to an array of size ((width + 15) / 16) * ((height + 15) / 16) that will be filled with a macroblock map, depending on extra_info_type.</summary>
        public IntPtr extra_info;
        /// <summary>Pointer to side statistics (updated only if not NULL)</summary>
        public IntPtr stats;
        /// <summary>Error code for the latest error encountered during encoding</summary>
        public UInt32 error_code;
        /// <summary>If not NULL, report progress during encoding.</summary>
        public IntPtr progress_hook;
        /// <summary>this field is free to be set to any value and used during callbacks (like progress-report e.g.).</summary>
        public IntPtr user_data;
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13, ArraySubType = UnmanagedType.U4)]
        private uint[] pad3;
        /// <summary>Row chunk of memory for yuva planes</summary>
        private IntPtr memory_;
        /// <summary>row chunk of memory for argb planes</summary>
        private IntPtr memory_argb_;
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private uint[] pad4;
    };

    /// <summary>Structure for storing auxiliary statistics (mostly for lossy encoding).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPAuxStats
    {
        /// <summary>Final size</summary>
        public int coded_size;
        /// <summary>Peak-signal-to-noise ratio for Y</summary>
        public float PSNRY;
        /// <summary>Peak-signal-to-noise ratio for U</summary>
        public float PSNRU;
        /// <summary>Peak-signal-to-noise ratio for V</summary>
        public float PSNRV;
        /// <summary>Peak-signal-to-noise ratio for All</summary>
        public float PSNRALL;
        /// <summary>Peak-signal-to-noise ratio for Alpha</summary>
        public float PSNRAlpha;
        /// <summary>Number of intra4</summary>
        public int block_count_intra4;
        /// <summary>Number of intra16</summary>
        public int block_count_intra16;
        /// <summary>Number of skipped macroblocks</summary>
        public int block_count_skipped;
        /// <summary>Approximate number of bytes spent for header</summary>
        public int header_bytes;
        /// <summary>Approximate number of bytes spent for  mode-partition #0</summary>
        public int mode_partition_0;
        /// <summary>Approximate number of bytes spent for DC coefficients for segment 0.</summary>
        public int residual_bytes_DC_segments0;
        /// <summary>Approximate number of bytes spent for AC coefficients for segment 0.</summary>
        public int residual_bytes_AC_segments0;
        /// <summary>Approximate number of bytes spent for uv coefficients for segment 0.</summary>
        public int residual_bytes_uv_segments0;
        /// <summary>Approximate number of bytes spent for DC coefficients for segment 1.</summary>
        public int residual_bytes_DC_segments1;
        /// <summary>Approximate number of bytes spent for AC coefficients for segment 1.</summary>
        public int residual_bytes_AC_segments1;
        /// <summary>Approximate number of bytes spent for uv coefficients for segment 1.</summary>
        public int residual_bytes_uv_segments1;
        /// <summary>Approximate number of bytes spent for DC coefficients for segment 2.</summary>
        public int residual_bytes_DC_segments2;
        /// <summary>Approximate number of bytes spent for AC coefficients for segment 2.</summary>
        public int residual_bytes_AC_segments2;
        /// <summary>Approximate number of bytes spent for uv coefficients for segment 2.</summary>
        public int residual_bytes_uv_segments2;
        /// <summary>Approximate number of bytes spent for DC coefficients for segment 3.</summary>
        public int residual_bytes_DC_segments3;
        /// <summary>Approximate number of bytes spent for AC coefficients for segment 3.</summary>
        public int residual_bytes_AC_segments3;
        /// <summary>Approximate number of bytes spent for uv coefficients for segment 3.</summary>
        public int residual_bytes_uv_segments3;
        /// <summary>Number of macroblocks in segments 0</summary>
        public int segment_size_segments0;
        /// <summary>Number of macroblocks in segments 1</summary>
        public int segment_size_segments1;
        /// <summary>Number of macroblocks in segments 2</summary>
        public int segment_size_segments2;
        /// <summary>Number of macroblocks in segments 3</summary>
        public int segment_size_segments3;
        /// <summary>Quantizer values for segment 0</summary>
        public int segment_quant_segments0;
        /// <summary>Quantizer values for segment 1</summary>
        public int segment_quant_segments1;
        /// <summary>Quantizer values for segment 2</summary>
        public int segment_quant_segments2;
        /// <summary>Quantizer values for segment 3</summary>
        public int segment_quant_segments3;
        /// <summary>Filtering strength for segment 0 [0..63]</summary>
        public int segment_level_segments0;
        /// <summary>Filtering strength for segment 1 [0..63]</summary>
        public int segment_level_segments1;
        /// <summary>Filtering strength for segment 2 [0..63]</summary>
        public int segment_level_segments2;
        /// <summary>Filtering strength for segment 3 [0..63]</summary>
        public int segment_level_segments3;
        /// <summary>Size of the transparency data</summary>
        public int alpha_data_size;
        /// <summary>Size of the enhancement layer data</summary>
        public int layer_data_size;

        // lossless encoder statistics
        /// <summary>bit0:predictor bit1:cross-color transform bit2:subtract-green bit3:color indexing</summary>
        public Int32 lossless_features;
        /// <summary>Number of precision bits of histogram</summary>
        public int histogram_bits;
        /// <summary>Precision bits for transform</summary>
        public int transform_bits;
        /// <summary>Number of bits for color cache lookup</summary>
        public int cache_bits;
        /// <summary>Number of color in palette, if used</summary>
        public int palette_size;
        /// <summary>Final lossless size</summary>
        public int lossless_size;
        /// <summary>Lossless header (transform, huffman etc) size</summary>
        public int lossless_hdr_size;
        /// <summary>Lossless image data size</summary>
        public int lossless_data_size;
        /// <summary>Padding for later use.</summary>
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private uint[] pad;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderConfig
    {
        /// <summary>Immutable bitstream features (optional)</summary>
        public WebPBitstreamFeatures input;
        /// <summary>Output buffer (can point to external mem)</summary>
        public WebPDecBuffer output;
        /// <summary>Decoding options</summary>
        public WebPDecoderOptions options;
    }

    /// <summary>Union of buffer parameters</summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct RGBA_YUVA_Buffer
    {
        [FieldOffsetAttribute(0)]
        public WebPRGBABuffer RGBA;

        [FieldOffsetAttribute(0)]
        public WebPYUVABuffer YUVA;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPYUVABuffer
    {
        /// <summary>Pointer to luma samples</summary>
        public IntPtr y;
        /// <summary>Pointer to chroma U samples</summary>
        public IntPtr u;
        /// <summary>Pointer to chroma V samples</summary>
        public IntPtr v;
        /// <summary>Pointer to alpha samples</summary>
        public IntPtr a;
        /// <summary>luma stride</summary>
        public int y_stride;
        /// <summary>chroma U stride</summary>
        public int u_stride;
        /// <summary>chroma V stride</summary>
        public int v_stride;
        /// <summary>alpha stride</summary>
        public int a_stride;
        /// <summary>luma plane size</summary>
        public UIntPtr y_size;
        /// <summary>chroma plane U size</summary>
        public UIntPtr u_size;
        /// <summary>chroma plane V size</summary>
        public UIntPtr v_size;
        /// <summary>alpha plane size</summary>
        public UIntPtr a_size;
    }

    /// <summary>Generic structure for describing the output sample buffer.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPRGBABuffer
    {
        /// <summary>pointer to RGBA samples.</summary>
        public IntPtr rgba;
        /// <summary>stride in bytes from one scanline to the next.</summary>
        public int stride;
        /// <summary>total size of the rgba buffer.</summary>
        public UIntPtr size;
    }

    /// <summary>Decoding options</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderOptions
    {
        /// <summary>if true, skip the in-loop filtering.</summary>
        public int bypass_filtering;
        /// <summary>if true, use faster pointwise upsampler.</summary>
        public int no_fancy_upsampling;
        /// <summary>if true, cropping is applied _first_</summary>
        public int use_cropping;
        /// <summary>left position for cropping. Will be snapped to even values.</summary>
        public int crop_left;
        /// <summary>top position for cropping. Will be snapped to even values.</summary>
        public int crop_top;
        /// <summary>width of the cropping area</summary>
        public int crop_width;
        /// <summary>height of the cropping area</summary>
        public int crop_height;
        /// <summary>if true, scaling is applied _afterward_</summary>
        public int use_scaling;
        /// <summary>final width</summary>
        public int scaled_width;
        /// <summary>final height</summary>
        public int scaled_height;
        /// <summary>if true, use multi-threaded decoding</summary>
        public int use_threads;
        /// <summary>dithering strength (0=Off, 100=full)</summary>
        public int dithering_strength;
        /// <summary>flip output vertically</summary>
        public int flip;
        /// <summary>alpha dithering strength in [0..100]</summary>
        public int alpha_dithering_strength;
        /// <summary>padding for later use.</summary>
        private UInt32 pad1;
        /// <summary>padding for later use.</summary>
        private UInt32 pad2;
        /// <summary>padding for later use.</summary>
        private UInt32 pad3;
        /// <summary>padding for later use.</summary>
        private UInt32 pad4;
        /// <summary>padding for later use.</summary>
        private UInt32 pad5;
    };

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct MemBuffer
    {
        MemBufferMode mode_;  // Operation mode
        UIntPtr start_;        // start location of the data to be decoded
        UIntPtr end_;          // end location
        UIntPtr buf_size_;     // size of the allocated buffer
        IntPtr buf_;        // We don't own this buffer in case WebPIUpdate()

        UIntPtr part0_size_;         // size of partition #0
        IntPtr part0_buf_;  // buffer to store partition #0
    };

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct WebPDecParams
    {
        /// <summary>output buffer.</summary>
        public WebPDecBuffer output;
        /// <summary>
        /// cache for the fancy upsampler or used for tmp rescaling
        /// </summary>
        public IntPtr tmp_y, tmp_u, tmp_v;
        /// <summary>coordinate of the line that was last output</summary>
        public int last_y;
        /// <summary>if not NULL, use alt decoding features</summary>
        public WebPDecoderOptions* options;

        /// <summary>rescalers</summary>
        public WebPRescaler* scaler_y, scaler_u, scaler_v, scaler_a;
        /// <summary>overall scratch memory for the output work.</summary>
        public IntPtr memory;

        /// <summary>output RGB or YUV samples</summary>
        public NativeDelegates.OutputFunc emit;
        /// <summary>output alpha channel</summary>
        public NativeDelegates.OutputAlphaFunc emit_alpha;
        /// <summary>output one line of rescaled alpha values</summary>
        public NativeDelegates.OutputRowFunc emit_alpha_row;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct VP8Io
    {
        /// <summary>
        /// picture dimensions, in pixels (invariable).
        /// These are the original, uncropped dimensions.
        /// The actual area passed to put() is stored
        /// in mb_w / mb_h fields.
        /// </summary>
        /// <remarks>set by VP8GetHeaders()</remarks>
        public int width, height;

        /// <summary>position of the current rows (in pixels)</summary>
        /// <remarks>set before calling put()</remarks>
        public int mb_y;
        /// <summary>number of columns in the sample</summary>
        /// <remarks>set before calling put()</remarks>
        public int mb_w;
        /// <summary>number of rows in the sample</summary>
        /// <remarks>set before calling put()</remarks>
        public int mb_h;
        /// <summary>rows to copy (in yuv420 format)</summary>
        public IntPtr y, u, v;
        /// <summary>row stride for luma</summary>
        public int y_stride;
        /// <summary>row stride for chroma</summary>
        public int uv_stride;

        /// <summary>user data</summary>
        public IntPtr opaque;

        /// <summary>
        /// called when fresh samples are available. Currently, samples are in
        /// YUV420 format, and can be up to width x 24 in size (depending on the
        /// in-loop filtering level, e.g.). Should return false in case of error
        /// or abort request. The actual size of the area to update is mb_w x mb_h
        /// in size, taking cropping into account.
        /// </summary>
        NativeDelegates.VP8IoPutHook put;

        /// <summary>
        /// called just before starting to decode the blocks.
        /// Must return false in case of setup error, true otherwise. If false is
        /// returned, teardown() will NOT be called. But if the setup succeeded
        /// and true is returned, then teardown() will always be called afterward.
        NativeDelegates.VP8IoSetupHook setup;

        /// <summary>
        /// Called just after block decoding is finished (or when an error occurred
        /// during put()). Is NOT called if setup() failed.
        /// </summary>
        NativeDelegates.VP8IoTeardownHook teardown;

        /// <summary>
        /// this is a recommendation for the user-side yuv->rgb converter. This flag
        /// is set when calling setup() hook and can be overwritten by it. It then
        /// can be taken into consideration during the put() method.
        /// </summary>
        public int fancy_upsampling;

        /// <summary>Input buffer</summary>
        public UIntPtr data_size;
        /// <summary>Input buffer</summary>
        public IntPtr data;

        /// <summary>
        /// If true, in-loop filtering will not be performed even if present in the
        /// bitstream. Switching off filtering may speed up decoding at the expense
        /// of more visible blocking. Note that output will also be non-compliant
        /// with the VP8 specifications.
        /// </summary>
        public int bypass_filtering;

        /// <summary>Cropping parameters</summary>
        public int use_cropping;
        /// <summary>Cropping parameters</summary>
        public int crop_left, crop_right, crop_top, crop_bottom;

        /// <summary>Scaling parameters</summary>
        public int use_scaling;
        /// <summary>Scaling parameters</summary>
        public int scaled_width, scaled_height;

        /// <summary>
        /// If non NULL, pointer to the alpha data (if present) corresponding to the
        /// start of the current row (That is: it is pre-offset by mb_y and takes
        /// cropping into account).
        /// </summary>
        public IntPtr a;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe ref struct WebPRescaler
    {
        /// <summary>true if we're expanding in the x direction</summary>
        int x_expand;
        /// <summary>true if we're expanding in the y direction</summary>
        int y_expand;
        /// <summary>bytes to jump between pixels</summary>
        int num_channels;
        /// <summary>fixed-point scaling factors</summary>
        uint fx_scale;
        /// <summary>fixed-point scaling factors</summary>
        uint fy_scale;
        /// <summary>fixed-point scaling factors</summary>
        uint fxy_scale;
        /// <summary>vertical accumulator</summary>
        int y_accum;
        /// <summary>vertical increments</summary>
        int y_add, y_sub;
        /// <summary>horizontal increments</summary>
        int x_add, x_sub;
        /// <summary>source dimensions</summary>
        int src_width, src_height;
        /// <summary>destination dimensions</summary>
        int dst_width, dst_height;
        /// <summary>row counters for input and output</summary>
        int src_y, dst_y;
        IntPtr dst;
        int dst_stride;
        /// <summary>work buffer</summary>
        uint* irow, frow;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WebPIDecoder
    {
        /// <summary>current decoding state</summary>
        internal DecState state_;
        /// <summary>Params to store output info</summary>
        internal WebPDecParams params_;
        /// <summary>for down-casting 'dec_'</summary>
        public int is_lossless_;
        /// <summary>either a VP8Decoder or a VP8LDecoder instance</summary>
        public IntPtr dec_;
        public VP8Io io_;

        /// <summary>input memory buffer</summary>
        internal MemBuffer mem_;
        /// <summary>output buffer (when no external one is supplied or if the external one has slow-memory)</summary>
        public WebPDecBuffer output_;
        /// <summary>Slow-memory output to copy to eventually</summary>
        public WebPDecBuffer* final_output_;
        /// <summary>Compressed VP8/VP8L size extracted from Header</summary>
        public UIntPtr chunk_size_;
        /// <summary>last row reached for intra-mode decoding</summary>
        public int last_mb_y_;
    };

    /// <summary>Output buffer</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecBuffer
    {
        /// <summary>Colorspace.</summary>
        public WEBP_CSP_MODE colorspace;
        /// <summary>Width of image.</summary>
        public int width;
        /// <summary>Height of image.</summary>
        public int height;
        /// <summary>If non-zero, 'internal_memory' pointer is not used. If value is '2' or more, the external memory is considered 'slow' and multiple read/write will be avoided.</summary>
        public int is_external_memory;
        /// <summary>Output buffer parameters.</summary>
        public RGBA_YUVA_Buffer u;
        /// <summary>padding for later use.</summary>
        private UInt32 pad1;
        /// <summary>padding for later use.</summary>
        private UInt32 pad2;
        /// <summary>padding for later use.</summary>
        private UInt32 pad3;
        /// <summary>padding for later use.</summary>
        private UInt32 pad4;
        /// <summary>Internally allocated memory (only when is_external_memory is 0). Should not be used externally, but accessed via WebPRGBABuffer.</summary>
        public IntPtr private_memory;
    }
}