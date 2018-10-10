using System;
using WebPWrapper.WPF.LowLevel;

namespace WebPWrapper.WPF
{
    /// <summary>Options for WebP encoder</summary>
    public sealed class EncoderOptions
    {
        private readonly CompressionType _compressionType;
        private WebPPreset? _preset;

        /// <summary>Init new option instance</summary>
        /// <param name="compression">Compression type...self-explain</param>
        /// <param name="level">Compression level profile. A tradeoff of "quality vs. speed".</param>
        /// <param name="preset">Enumerate some predefined settings for WebPConfig, depending on the type of source picture.</param>
        /// <param name="quality">The quality of image. Value between 1 and 100.</param>
        public EncoderOptions(CompressionType compression, CompressionLevel level, WebPPreset preset, float quality)
        {
            this._quality = quality;
            this._preset = preset;
            this._compressionType = compression;
            this.CompressionLevel = level;
        }

        /// <summary>Init new option instance</summary>
        /// <param name="compression">Compression type...self-explain</param>
        /// <param name="preset">Enumerate some predefined settings for WebPConfig, depending on the type of source picture.</param>
        /// <param name="quality">The quality of image. Value between 1 and 100.</param>
        public EncoderOptions(CompressionType compression, WebPPreset preset, float quality) : this(compression, CompressionLevel.Default, preset, quality) { }

        /// <summary>Init new option instance</summary>
        /// <param name="preset">Enumerate some predefined settings for WebPConfig, depending on the type of source picture.</param>
        /// <param name="quality">The quality of image. Value between 1 and 100.</param>
        public EncoderOptions(WebPPreset preset, float quality) : this(CompressionType.Lossy, CompressionLevel.Default, preset, quality) { }

        /// <summary>Init new option instance</summary>
        /// <param name="preset">Enumerate some predefined settings for WebPConfig, depending on the type of source picture.</param>
        public EncoderOptions(WebPPreset preset) : this(preset, 75f) { }

        /// <summary>Init new option instance</summary>
        /// <param name="quality">The quality of image. Value between 1 and 100.</param>
        public EncoderOptions(float quality) : this(WebPPreset.Default, quality) { }

        /// <summary>Init new option instance with default quality value 75 (According to Google)</summary>
        /// <remarks>According to https://github.com/webmproject/libwebp/blob/5c395f1d71f8e753a23f1e256544bf96cb349e3e/src/webp/encode.h#L170</remarks>
        public EncoderOptions() : this(75f) { }

        private int _alpha_quality;
        /// <summary>
        /// The quality of alpha plane. Value between 0 and 100.
        /// </summary>
        public int AlphaQuality
        {
            get => this._alpha_quality;
            set
            {
                if (value < 0 || value > 100)
                    throw new IndexOutOfRangeException("Must be between 0 and 100.");
                this._alpha_quality = value;
            }
        }
        /// <summary>Gets or sets a value whether the encoder should preserve the exact RGB values under transparent area or discard the values for better compression. Null = default of preset.</summary>
        public bool? PreserveRGB { get; set; }
        /// <summary>Gets or sets a value whether the encoder is allowed to automatically adjust filter's strength. Null = default of preset.</summary>
        public bool? AutoFilter { get; set; }
        /// <summary>Gets or sets the value determine whether the encoder is allowed to use multi-threading if available</summary>
        public bool UseMultithreading { get; set; } = true;
        /// <summary>Predictive filtering method for alpha plane. Null = default of preset.</summary>
        public AlphaFiltering? AlphaFiltering { get; set; }
        /// <summary>Gets or sets algorithm for encoding the alpha plane. Null = default of preset.</summary>
        public AlphaCompressionType? AlphaCompression { get; set; }
        /// <summary>Gets or sets flag(s) whether the encoder should reduce memory usage in exchange of CPU consumption (and compression speed).</summary>
        public MemoryAllowance MemoryUsage { get; set; } = MemoryAllowance.AsMuchAsPossible;
        /// <summary>Get or set the preset which .</summary>
        private WebPPreset? Preset { get; set; }

        /// <summary>
        /// Gets compression algorithm that is used to initialize this instance
        /// </summary>
        public CompressionType CompressionType => this._compressionType;
        /// <summary>Quality-speed trade off</summary>
        public CompressionLevel CompressionLevel { get; set; }

        private float _quality;
        /// <summary>The quality of image. Value between 1 and 100.</summary>
        public float Quality
        {
            get => this._quality;
            set
            {
                if (value < 1 || value > 100)
                    throw new IndexOutOfRangeException("Must be between 1 and 100.");
                this._quality = value;
            }
        }
        /// <summary>Only be used if <see cref="AutoFilter"/> is True or <see cref="FilterStrength"/> has non-zero value. Null = default of preset.</summary>
        public FilterType? FilterType { get; set; }
        /// <summary>Hint for image type (Currently is for lossless compression). Null = default of preset.</summary>
        public WebPImageHint? ImageHint { get; set; }
        /// <summary>Gets or set the range for filter sharpness. Null = default of preset.</summary>
        public FilterSharpness? FilterSharpness { get; set; }

        private int _filter_strength;
        /// <summary>The strength of the filter. Value between 0 and 100. Require <see cref="AutoFilter"/> set to False</summary>
        public int FilterStrength
        {
            get => this._filter_strength;
            set
            {
                if (value < 0 || value > 100)
                    throw new IndexOutOfRangeException("Must be between 0 and 100.");
                this._filter_strength = value;
            }
        }

        internal void ApplyConfigStruct(Libwebp libwebp, ref WebPConfig config)
        {
            int val_method = (int)this.CompressionLevel;
            if (this.AlphaCompression.HasValue)
                config.alpha_compression = (int)this.AlphaCompression.Value;

            if (this.AlphaFiltering.HasValue)
                config.alpha_filtering = (int)this.AlphaFiltering.Value;

            config.alpha_quality = this._alpha_quality;

            if (this.PreserveRGB.HasValue)
                config.exact = (this.PreserveRGB.Value ? 1 : 0);

            if (this.AutoFilter.HasValue)
                config.autofilter = (this.AutoFilter.Value ? 1 : 0);

            config.thread_level = (this.UseMultithreading ? 1 : 0);

            config.low_memory = (((this.MemoryUsage & MemoryAllowance.LowMemoryCompressionMode) == MemoryAllowance.LowMemoryCompressionMode) ? 1 : 0);

            // May cause bug which switch to lossless if quality = 0 while selecting NearLossless
            config.near_lossless = ((this._compressionType == CompressionType.NearLossless) ? Convert.ToInt32(this._quality) : 0);

            config.lossless = ((this._compressionType == CompressionType.Lossless) ? 1 : 0);

            config.method = val_method;

            config.quality = this._quality;

            if (this.FilterSharpness.HasValue)
                config.filter_sharpness = (int)this.FilterSharpness.Value;

            config.filter_strength = this._filter_strength;
            if (this.FilterType.HasValue)
                config.filter_type = (int)this.FilterType.Value;

            if (this.ImageHint.HasValue)
                config.image_hint = this.ImageHint.Value;

            // I don't know
            config.pass = val_method + 1;
            config.segments = 4;
            config.partitions = 3;
            if (libwebp.WebPGetDecoderVersion() > 1082) //Old version don´t suport preprocessing 4
                config.preprocessing = 4;
            else
                config.preprocessing = 3;
        }
    }
}
