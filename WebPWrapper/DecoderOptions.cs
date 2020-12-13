using System;
using System.Windows;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>
    /// WebP Decoding options
    /// </summary>
    public sealed class DecoderOptions
    {
        internal static readonly DecoderOptions Default = new DecoderOptions();
        /// <summary>
        /// Initialize a new option instance
        /// </summary>
        public DecoderOptions()
        {
            this._alpha_dithering_strength = 0;
            this._dithering_strength = 100;
        }

        private int _alpha_dithering_strength;
        /// <summary>Set to 0 to disable alpha dithering</summary>
        public int AlphaDitheringStrength
        {
            get => this._alpha_dithering_strength;
            set
            {
                if (value < 0 || value > 100)
                    throw new IndexOutOfRangeException("Value must be from 0 to 100");
                this._alpha_dithering_strength = value;
            }
        }

        private int _dithering_strength;
        /// <summary>Set to 0 to disable dithering</summary>
        public int DitheringStrength
        {
            get => this._dithering_strength;
            set
            {
                if (value < 0 || value > 100)
                    throw new IndexOutOfRangeException("Value must be from 0 to 100");
                this._dithering_strength = value;
            }
        }

        /// <summary>Gets or sets the value determine whether in-loop filtering will be skipped</summary>
        public bool BypassFiltering { get; set; } = false;
        /// <summary>Gets or sets the value determine whether the output image will be flipped vertically</summary>
        public bool FlipVertically { get; set; } = false;
        /// <summary>Gets or sets the value determine whether fancy upsampling (slower) will be skipped</summary>
        public bool NoFancyUpsampling { get; set; } = false;
        /// <summary>Gets or sets the value determine whether the decoder is allowed to use multi-threading if available</summary>
        public bool UseMultithreading { get; set; } = true;

        private Int32Rect crop;
        /// <summary>
        /// Crop the output after decoded
        /// </summary>
        /// <param name="cropOffset"></param>
        public void SetOutputCrop(Int32Rect cropOffset)
        {
            if (cropOffset.IsEmpty || cropOffset.Width == 0 || cropOffset.Height == 0)
                throw new ArgumentException("Invalid crop size. Why do you encode the image but take nothing from it (Crop size: 0x0)?", nameof(cropOffset));
            this.crop = cropOffset;
        }

        /// <summary>Gets or sets the scale option for the output (after cropping, if <see cref="CropArea"/> is set)</summary>
        public BitmapSizeOptions ScaleSize { get; set; }
        public Int32Rect CropArea => this.crop;

        internal bool HasScaling
        {
            get
            {
                if (this.ScaleSize == null)
                    return false;

                if (this.ScaleSize.PixelWidth == 0 && this.ScaleSize.PixelHeight == 0)
                    return false;

                return true;
            }
        }

        internal void ApplyOptions(ref WebPDecoderConfig config)
        {
            ref var options = ref config.options;

            options.alpha_dithering_strength = this._alpha_dithering_strength;
            options.bypass_filtering = (this.BypassFiltering ? 1 : 0);
            options.dithering_strength = this._dithering_strength;
            options.flip = (this.FlipVertically ? 1 : 0);
            options.no_fancy_upsampling = (this.NoFancyUpsampling ? 1 : 0);
            options.use_threads = (this.UseMultithreading ? 1 : 0);
            options.use_cropping = (this.crop == Int32Rect.Empty ? 0 : 1);
            
            if (options.use_cropping != 0)
            {
                if (options.crop_left + options.crop_width > config.input.width || options.crop_top + options.crop_height > config.input.height)
                {
                    throw new ArgumentOutOfRangeException("Crop options exceded WebP image dimensions");
                }
                options.crop_left = this.crop.X;
                options.crop_top = this.crop.Y;
                options.crop_width = this.crop.Width;
                options.crop_height = this.crop.Height;
            }
            
            // We will deal with scaling later
            /*
            use_scaling = (isScaling ? 0 : 1),
            scaled_width = (isScaling ? this._scale.PixelWidth : 0),
            scaled_height = (isScaling ? this._scale.PixelHeight : 0)
            */
            if (HasScaling)
            {
                config.options.use_scaling = 1;
                Helper.ScaleHelper.GetScaledWidthAndHeight(this.ScaleSize, config.input.width, config.input.height, out options.scaled_width, out options.scaled_height);
            }
            else
            {
                options.use_scaling = 0;
            }
        }
    }
}
