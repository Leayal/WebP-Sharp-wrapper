using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPWrapper
{
    public sealed class EncoderOptions
    {
        internal WebPConfig config;
        public EncoderOptions(CompressionType compression, CompressionLevel level, WebPPreset preset, float quality)
        {
            //test dll version
            if (compression == CompressionType.NearLossless)
                if (UnsafeNativeMethods.WebPGetDecoderVersion() <= 1082)
                    throw new Exception("This dll version not suport EncodeNearLossless");

            this.config = new WebPConfig();
            if (UnsafeNativeMethods.WebPConfigInit(ref this.config, preset, quality) == 0)
                throw new Exception("Can´t config preset");
            this._compressionType = compression;
            switch (compression)
            {
                case CompressionType.NearLossless:
                    if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, (int)level) == 0)
                        throw new Exception("Can´t config lossless preset");
                    break;
                case CompressionType.Lossless:
                    if (UnsafeNativeMethods.WebPGetDecoderVersion() > 1082)
                    {
                        if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, (int)level) == 0)
                            throw new Exception("Can´t config lossless preset");
                    }
                    break;
            }

            this._alpha_quality = config.alpha_quality;
            this._quality = config.quality;
            this.PreserveRGB = (config.exact != 0);
            this.AutoFilter = (config.autofilter != 0);
            this.AlphaFiltering = (AlphaFiltering)(config.alpha_filtering);
            this.AlphaCompression = (AlphaCompressionType)(config.alpha_compression);
            this.CompressionLevel = (CompressionLevel)(config.method);
            this.FilterType = (FilterType)(config.filter_type);
            this.ImageHint = config.image_hint;
            this.FilterSharpness = (FilterSharpness)(config.filter_sharpness);
            this.FilterStrength = config.filter_strength;
        }

        public EncoderOptions(CompressionType compression, WebPPreset preset, float quality) : this(compression, CompressionLevel.Default, preset, quality) { }
        public EncoderOptions(WebPPreset preset, float quality) : this(CompressionType.Lossy, CompressionLevel.Default, preset, quality) { }
        public EncoderOptions(WebPPreset preset) : this(preset, 75f) { }
        public EncoderOptions(float quality) : this(WebPPreset.WEBP_PRESET_DEFAULT, quality) { }
        public EncoderOptions() : this(WebPPreset.WEBP_PRESET_DEFAULT) { }

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
        /// <summary>Gets or sets a value whether the encoder is allowed to automatically adjust filter's strength</summary>
        public bool AutoFilter { get; set; } = true;
        /// <summary>Gets or sets the value determine whether the encoder is allowed to use multi-threading if available</summary>
        public bool UseMultithreading { get; set; } = true;
        /// <summary>Predictive filtering method for alpha plane</summary>
        public AlphaFiltering AlphaFiltering { get; set; } = AlphaFiltering.Fast;
        /// <summary>Gets or sets algorithm for encoding the alpha plane</summary>
        public AlphaCompressionType AlphaCompression { get; set; } = AlphaCompressionType.Lossless;
        /// <summary>Gets or sets a value whether the encoder should reduce memory usage in exchange of CPU consumption</summary>
        public bool LowMemoryUsage { get; set; } = false;
        private CompressionType _compressionType;
        /// <summary>
        /// Gets compression algorithm that is used to initialize this instance
        /// </summary>
        public CompressionType CompressionType => this._compressionType;
        /// <summary>Quality-speed trade off</summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Default;

        private float _quality;
        /// <summary>
        /// The quality of alpha plane. Value between 0 and 100.
        /// </summary>
        public float Quality
        {
            get => this._quality;
            set
            {
                if (value < 0 || value > 100)
                    throw new IndexOutOfRangeException("Must be between 0 and 100.");
                this._quality = value;
            }
        }
        /// <summary>Only be used if <see cref="AutoFilter"/> is True or <see cref="FilterStrength"/> has non-zero value</summary>
        public FilterType FilterType { get; set; } = FilterType.Simple;
        /// <summary>Hint for image type (Currently is for lossless compression)</summary>
        public WebPImageHint ImageHint { get; set; } = WebPImageHint.WEBP_HINT_DEFAULT;
        /// <summary>Gets or set the range for filter sharpness</summary>
        public FilterSharpness FilterSharpness { get; set; } = FilterSharpness.Off;

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

        internal WebPConfig GetConfigStruct()
        {
            this.config.alpha_compression = (int)this.AlphaCompression;
            this.config.alpha_filtering = (int)AlphaFiltering;
            this.config.alpha_quality = this._alpha_quality;
            this.config.exact = (this.PreserveRGB ? 1 : 0);
            this.config.autofilter = (this.AutoFilter ? 1 : 0);
            this.config.thread_level = (this.UseMultithreading ? 1 : 0);
            this.config.low_memory = (this.LowMemoryUsage ? 1 : 0);
            // May cause bug which switch to lossless if quality = 0 while selecting NearLossless
            this.config.near_lossless = ((this._compressionType == CompressionType.NearLossless) ? Convert.ToInt32(this._quality) : 0);
            this.config.lossless = ((this._compressionType == CompressionType.Lossless) ? 1 : 0);
            this.config.method = (int)this.CompressionLevel;
            this.config.quality = this._quality;
            this.config.filter_sharpness = 1;
            this.config.filter_strength = this._filter_strength;
            this.config.filter_type = (int)this.FilterType;
            this.config.image_hint = this.ImageHint;

            // I don't know
            this.config.pass = this.config.method + 1;
            this.config.segments = 4;
            this.config.partitions = 3;
            if (UnsafeNativeMethods.WebPGetDecoderVersion() > 1082) //Old version don´t suport preprocessing 4
                this.config.preprocessing = 4;
            else
                this.config.preprocessing = 3;

            return this.config;
        }
    }
}
