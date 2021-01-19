using System;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>Options for WebP encoder</summary>
    /// <remarks>Just a convenient class. You still need to call <seealso cref="ApplyOptions(ILibwebp, ref WebPConfig)"/> and use the <seealso cref="WebPConfig"/> structure.</remarks>
    public class EncoderOptions
    {
        private const int CompressionLevel_Lowest = (int)CompressionLevel.Lowest, CompressionLevel_Highest = (int)CompressionLevel.Highest;
        /// <summary>Init new option instance</summary>
        /// <param name="compression">Compression type...self-explain</param>
        /// <param name="level">Compression level profile. A tradeoff of "quality vs. speed".</param>
        /// <param name="preset">Enumerate some predefined settings for WebPConfig, depending on the type of source picture.</param>
        /// <param name="quality">The quality of image. Value between 1 and 100.</param>
        public EncoderOptions(CompressionType compression, CompressionLevel level, WebPPreset preset, float quality)
        {
            this._quality = quality;
            this.Preset = preset;
            this.CompressionType = compression;
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
        /// <remarks>
        /// According to https://github.com/webmproject/libwebp/blob/5c395f1d71f8e753a23f1e256544bf96cb349e3e/src/webp/encode.h#L170
        /// </remarks>
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
        /// <summary>Gets or sets a value whether the encoder should preserve the exact RGB values under transparent area or discard the values for better compression</summary>
        public bool PreserveRGB { get; set; } = false;
        /// <summary>Gets or sets a value whether the encoder is allowed to automatically adjust filter's strength. Null = default of preset.</summary>
        public bool? AutoFilter { get; set; }
        /// <summary>Gets or sets the value determine whether the encoder is allowed to use multi-threading if available</summary>
        public bool UseMultithreading { get; set; } = true;
        /// <summary>Gets or sets the value to allow the encoded image to support progressive decoding.</summary>
        public bool SupportProgressiveDecoding { get; set; } = true;

        /// <summary>Predictive filtering method for alpha plane.</summary>
        public AlphaFiltering AlphaFiltering { get; set; } = AlphaFiltering.Fast;
        /// <summary>Gets or sets algorithm for encoding the alpha plane.</summary>
        public AlphaCompressionType AlphaCompression { get; set; } = AlphaCompressionType.Lossless;
        /// <summary>Gets or sets flag(s) whether the encoder should reduce memory usage in exchange of CPU consumption.</summary>
        public MemoryAllowance MemoryUsage { get; set; } = MemoryAllowance.AsMuchAsPossible;
        /// <summary>The compression preset</summary>
        public WebPPreset Preset { get; set; }

        /// <summary>The compression algorithm.</summary>
        public CompressionType CompressionType { get; set; }

        private CompressionLevel _compressionLevel;
        /// <summary>Quality-speed trade off</summary>
        public CompressionLevel CompressionLevel
        {
            get => this._compressionLevel;
            set
            {
                if (this._compressionLevel != value)
                {
                    var int_val = (int)value;
                    if (int_val < CompressionLevel_Lowest || int_val > CompressionLevel_Highest)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    this._compressionLevel = value;
                }
            }
        }

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
        /// <summary>The strength of the filter.</summary>
        /// <remarks>Value between 0 and 100. If '<see cref="AutoFilter"/>' is true, this setting will not be used.</remarks>
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

        /// <summary>Applies the option values from this instance to a <seealso cref="WebPConfig"/> structure.</summary>
        /// <param name="factory">The factory of the native library to validate the config.</param>
        /// <param name="config">The <seealso cref="WebPConfig"/> structure to apply the values on.</param>
        /// <returns>Returns true if the option values are valid. Otherwise false.</returns>
        public bool ApplyOptions(WebpFactory factory, ref WebPConfig config) => this.ApplyOptions(factory.GetUnmanagedInterface(), ref config);

        /// <summary>Applies the option values from this instance to a <seealso cref="WebPConfig"/> structure.</summary>
        /// <param name="libwebp">The interface of the native library to validate the config.</param>
        /// <param name="config">The <seealso cref="WebPConfig"/> structure to apply the values on.</param>
        /// <returns>Returns true if the option values are valid. Otherwise false.</returns>
        public bool ApplyOptions(ILibwebp libwebp, ref WebPConfig config)
        {
            if (libwebp == null)
            {
                throw new ArgumentNullException(nameof(libwebp));
            }
            int val_level = (int)this.CompressionLevel;
            bool val_isLossy = (this.CompressionType == CompressionType.Lossy),
                val_isLossless = (this.CompressionType == CompressionType.Lossless),
                val_isNearLossless = (this.CompressionType == CompressionType.NearLossless);

            // This is something work for all compression type
            if (libwebp.WebPConfigInit(ref config, this.Preset, this.Quality) == 0)
                throw new InvalidOperationException("Can't config preset");
            if (!val_isLossy)
            {
                if (libwebp.IsFunctionExists("WebPConfigLosslessPreset"))
                {
                    if (libwebp.WebPConfigLosslessPreset(ref config, this.CompressionLevel) == 0)
                        throw new InvalidOperationException("Can't config lossless preset");
                }
            }
            if (val_level > 6)
            {
                config.method = 6;
            }
            else if (val_level < 0)
            {
                config.method = 0;
            }
            else
            {
                config.method = val_level;
            }
            if (val_isLossy)
            {
                config.lossless = 0;
                config.segments = 4;
                config.preprocessing = 4;
            }
            else if (val_isLossless)
            {
                config.lossless = 1;
                config.near_lossless = 0;
            }
            else if (val_isNearLossless)
            {
                // config.lossless = 1;
                config.near_lossless = Convert.ToInt32(this._quality);
            }

            config.partitions = this.SupportProgressiveDecoding ? 0 : 3;

            config.alpha_compression = (int)this.AlphaCompression;

            config.alpha_filtering = (int)this.AlphaFiltering;

            config.alpha_quality = this._alpha_quality;

            config.exact = (this.PreserveRGB ? 1 : 0);

            if (this.AutoFilter.HasValue)
            {
                config.autofilter = (this.AutoFilter.Value ? 1 : 0);
            }

            config.thread_level = (this.UseMultithreading ? 1 : 0);

            config.low_memory = (((this.MemoryUsage & MemoryAllowance.LowMemoryCompressionMode) == MemoryAllowance.LowMemoryCompressionMode) ? 1 : 0);

            config.quality = this._quality;

            if (this.FilterSharpness.HasValue)
            {
                config.filter_sharpness = (int)this.FilterSharpness.Value;
            }

            config.filter_strength = this._filter_strength;
            if (this.FilterType.HasValue)
            {
                config.filter_type = (int)this.FilterType.Value;
            }

            if (this.ImageHint.HasValue)
            {
                config.image_hint = this.ImageHint.Value;
            }

            var num_pass = 1 + val_level;
            if (num_pass < 1)
            {
                num_pass = 1;
            }
            else if (num_pass > 10)
            {
                num_pass = 10;
            }
            config.pass = num_pass;

            if (libwebp.WebPValidateConfig(ref config) == 1)
            {
                return true;
            }
            else
            {
                config.preprocessing = 3;
                return (libwebp.WebPValidateConfig(ref config) == 1);
            }
        }
    }
}
