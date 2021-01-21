using System;
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
        /// <summary>The compression quality</summary>
        /// <remarks>
        /// Between 0 and 100. For lossy, 0 gives the smallest
        /// size and 100 the largest. For lossless, this
        /// parameter is the amount of effort put into the
        /// compression: 0 is the fastest but gives larger
        /// files compared to the slowest, but best, 100.
        /// </remarks>
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

        /// <summary>Quality factor.</summary>
        /// <remarks>Between 0 and 100. For lossy, 0 gives the smallest size and 100 the largest. For lossless, this parameter is the amount of effort put into the compression: 0 is the fastest but gives larger files compared to the slowest, but best, 100.</remarks>
        public float quality;

        /// <summary>Quality/speed trade-off</summary>
        /// <remarks>Range: [0..6] (0=fast, 6=slower-better)</remarks>
        public int method;

        /// <summary>Hint for image type (lossless only for now).</summary>
        public WebPImageHint image_hint;

        /// <summary>If non-zero, set the desired target size in bytes.</summary>
        /// <remarks>Takes precedence over the 'compression' parameter.</remarks>
        public int target_size;

        /// <summary>If non-zero, specifies the minimal distortion to try to achieve.</summary>
        /// <remarks>Takes precedence over <seealso cref="target_size"/>.</remarks>
        public float target_PSNR;

        /// <summary>Maximum number of segments to use.</summary>
        /// <remarks>Range: [1..4]</remarks>
        public int segments;

        /// <summary>Spatial Noise Shaping.</summary>
        /// <remarks>Range: [0..100] (0=off, 100=maximum).</remarks>
        public int sns_strength;

        /// <summary>Specify the filter strength.</summary>
        /// <remarks>Range: [0..100] (0=off, 100=strongest).</remarks>
        public int filter_strength;

        /// <summary>Specify the filter sharpness strength.</summary>
        /// <remarks>Range: [0..7] (0=off, 7=least sharp).</remarks>
        public int filter_sharpness;

        /// <summary>Specify the filter type.</summary>
        /// <remarks>Range: [0..1] (0=simple, 1=strong {only used if filter_strength > 0 or autofilter > 0}).</remarks>
        public int filter_type;

        /// <summary>Auto adjust filter's strength.</summary>
        /// <remarks>Range: [0..1] (0=off, 1=on).</remarks>
        public int autofilter;

        /// <summary>Algorithm for encoding the alpha plane.</summary>
        /// <remarks>Range: [0..1] (0=none, 1=compressed with WebP lossless). Default is 1.</remarks>
        public int alpha_compression;

        /// <summary>Predictive filtering method for alpha plane.</summary>
        /// <remarks>Range: [0..2] (0=none, 1=fast, 2=best). Default is 1.</remarks>
        public int alpha_filtering;

        /// <summary>Quality factor for alpha plane.</summary>
        /// <remarks>Range: [0..100] (0=smallest size, 100=lossless). Default is 100.</remarks>
        public int alpha_quality;

        /// <summary>Number of entropy-analysis passes.</summary>
        /// <remarks>Range: [0..10].</remarks>
        public int pass;

        /// <summary>If non-zero, export the compressed picture back. In-loop filtering is not applied.</summary>
        public int show_compressed;

        /// <summary>Preprocessing filter</summary>
        /// <remarks>Range: [0..4] (0=none, 1=segment-smooth, 2=pseudo-random dithering).</remarks>
        public int preprocessing;

        /// <summary>A log2 of the number of token partitions.</summary>
        /// <remarks>Range: [0..3]. Default is set to 0 for easier progressive decoding.</remarks>
        public int partitions;

        /// <summary>Quality degradation allowed to fit the 512k limit on prediction modes coding.</summary>
        /// <remarks>Range: [0..100] (0=no degradation, 100=maximum possible degradation).</remarks>
        public int partition_limit;

        /// <summary>If non-zero, compression parameters will be remapped to better match the expected output size from JPEG compression.</summary>
        /// <remarks>Generally, the output size will be similar but the degradation will be lower.</remarks>
        public int emulate_jpeg_size;

        /// <summary>If non-zero, try and use multi-threaded encoding.</summary>
        /// <remarks>Range: [0..1] (0=no multi-threaded, 1=use multi-threaded if possible).</remarks>
        public int thread_level;

        /// <summary>If non-zero, reduce memory usage (but increase CPU use).</summary>
        /// <remarks>Range: [0..1] (0=off, 1=on).</remarks>
        public int low_memory;

        /// <summary>Quality factor for [Near lossless] encoding. Requires <seealso cref="lossless"/> is set to 1.</summary>
        /// <remarks>Range: [0..100] (0=max loss, 100=off {lossless mode}). Default is 100.</remarks>
        public int near_lossless;

        /// <summary>If non-zero, preserve the exact RGB values under transparent area. Otherwise, discard this invisible RGB information for better compression.</summary>
        /// <remarks>Default value is 0.</remarks>
        public int exact;

        /// <summary>Reserved for future lossless feature.</summary>
        private int use_delta_palette; // private for now since it's not being actually used.

        /// <summary>If needed, use sharp (and slow) RGB->YUV conversion</summary>
        public int use_sharp_yuv;

        /// <summary>Minimum permissible quality factor.</summary>
        public int qmin;

        /// <summary>Maximum permissible quality factor</summary>
        public int qmax;
    };

    /// <summary>Main exchange structure (input samples, output bytes, statistics)</summary>
    /// <remarks>
    /// Once WebPPictureInit() has been called, it's ok to make all the INPUT fields
    /// (use_argb, y/u/v, argb, ...) point to user-owned data, even if
    /// WebPPictureAlloc() has been called. Depending on the value use_argb,
    /// it's guaranteed that either *argb or *y/*u/*v content will be kept untouched.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPPicture
    {
        /// <summary>Main flag for encoder selecting between ARGB or YUV input. Recommended to use ARGB input (*argb, argb_stride) for lossless, and YUV input (*y, *u, *v, etc.) for lossy</summary>
        public int use_argb;
        /// <summary>colorspace: should be YUV420 for now (=Y'CbCr). Value = 0</summary>
        public WebPEncCSP colorspace;
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
        /// <remarks>The function must be "alive" (Not collected by Garbage Collector) for the whole encoding's process. You should use <seealso cref="WebpDelegate.Create(Delegate)"/> to wrap it or <seealso cref="GCHandle"/> or keeping a reference to the delegate to keep it alive.</remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WebpDelegate.WebPWriterFunction writer;
        /// <summary>Can be used by the writer.</summary>
        public IntPtr custom_ptr;
        /// <summary>map for extra information (only for lossy compression mode)</summary>
        /// <remarks>1: intra type, 2: segment, 3: quant, 4: intra-16 prediction mode, 5: chroma prediction mode, 6: bit cost, 7: distortion</remarks>
        public int extra_info_type;
        /// <summary>if not NULL, points to an array of size ((width + 15) / 16) * ((height + 15) / 16) that will be filled with a macroblock map, depending on extra_info_type.</summary>
        public IntPtr extra_info;
        /// <summary>Pointer to side statistics (updated only if not NULL)</summary>
        public WebPAuxStats stats;
        /// <summary>Error code for the latest error encountered during encoding</summary>
        public WebPEncodingError error_code;
        /// <summary>If not NULL, report progress during encoding.</summary>
        /// <remarks>The function must be "alive" (Not collected by Garbage Collector) for the whole encoding's process. You should use <seealso cref="WebpDelegate.Create(Delegate)"/> to wrap it or <seealso cref="GCHandle"/> or keeping a reference to the delegate to keep it alive.</remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WebpDelegate.WebPProgressHook progress_hook;
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

    /// <summary>Decoding parameters.</summary>
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
        /// <summary>Output buffer parameters for RGB/BGR/RGBA/BGRA.</summary>
        [FieldOffsetAttribute(0)]
        public WebPRGBABuffer RGBA;

        /// <summary>Output buffer parameters for YUV/A.</summary>
        [FieldOffsetAttribute(0)]
        public WebPYUVABuffer YUVA;
    }

    /// <summary>Output buffer parameters for YUV/A.</summary>
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

    /// <summary>Output buffer parameters for RGB/BGR/RGBA/BGRA.</summary>
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
        /// <summary>Padding for later use.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
        private uint[] pad;
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
    unsafe ref struct WebPDecParams
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
        public IntPtr options; // public WebPDecoderOptions* options;

        /// <summary>rescalers</summary>
        public WebPRescaler* scaler_y, scaler_u, scaler_v, scaler_a;
        /// <summary>overall scratch memory for the output work.</summary>
        public IntPtr memory;

        /// <summary>output RGB or YUV samples</summary>
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public NativeDelegates.OutputFunc emit;
        /// <summary>output alpha channel</summary>
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public NativeDelegates.OutputAlphaFunc emit_alpha;
        /// <summary>output one line of rescaled alpha values</summary>
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public NativeDelegates.OutputRowFunc emit_alpha_row;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct VP8Io
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
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        NativeDelegates.VP8IoPutHook put;

        /// <summary>
        /// called just before starting to decode the blocks.
        /// Must return false in case of setup error, true otherwise. If false is
        /// returned, teardown() will NOT be called. But if the setup succeeded
        /// and true is returned, then teardown() will always be called afterward.
        /// </summary>
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        NativeDelegates.VP8IoSetupHook setup;

        /// <summary>
        /// Called just after block decoding is finished (or when an error occurred
        /// during put()). Is NOT called if setup() failed.
        /// </summary>
        /// <remarks>
        /// The delegate/function must be alive (not collected by Garbage Collector) until it is no longer being used.
        /// Keep a reference to it or use <seealso cref="GC.KeepAlive(object)"/> to keep the delegate/function alive.
        /// </remarks>
        [MarshalAs(UnmanagedType.FunctionPtr)]
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
    unsafe ref struct WebPRescaler
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

    /// <summary>Output buffer</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecBuffer
    {
        /// <summary>Colorspace.</summary>
        public Colorspace colorspace;
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
    };

    /// <summary>Output buffer (Read-only)</summary>
    /// <remarks>It's actually a <seealso cref="WebPDecBuffer"/>, but with read-only state.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WebPDecodedDataBuffer
    {
        /// <summary>Colorspace.</summary>
        public readonly Colorspace colorspace;
        /// <summary>Width of image.</summary>
        public readonly int width;
        /// <summary>Height of image.</summary>
        public readonly int height;
        /// <summary>If non-zero, 'internal_memory' pointer is not used. If value is '2' or more, the external memory is considered 'slow' and multiple read/write will be avoided.</summary>
        public readonly int is_external_memory;
        /// <summary>Output buffer parameters.</summary>
        public readonly RGBA_YUVA_Buffer u;
        /// <summary>padding for later use.</summary>
        private readonly UInt32 pad1;
        /// <summary>padding for later use.</summary>
        private readonly UInt32 pad2;
        /// <summary>padding for later use.</summary>
        private readonly UInt32 pad3;
        /// <summary>padding for later use.</summary>
        private readonly UInt32 pad4;
        /// <summary>Internally allocated memory (only when is_external_memory is 0). Should not be used externally, but accessed via WebPRGBABuffer.</summary>
        public readonly IntPtr private_memory;
    };

    /// <summary>
    /// A special WebPWriterFunction that writes to memory using the following WebPMemoryWriter object (to be set as a custom_ptr).
    /// </summary>
    public struct WebPMemoryWriter
    {
        /// <summary>final buffer's pointer (of size 'max_size', larger than 'size').</summary>
        public IntPtr mem;
        /// <summary>final buffer's data length</summary>
        public UIntPtr size;      // final size
        /// <summary>final buffer's capacity</summary>
        public UIntPtr max_size;
        /// <summary>padding for later use</summary>
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
        private uint[] pad;
    };
}
