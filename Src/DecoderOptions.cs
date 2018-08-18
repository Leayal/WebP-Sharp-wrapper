using System;
using System.Windows;

namespace WebPWrapper
{
    /// <summary>
    /// WebP Decoding options
    /// </summary>
    public sealed class DecoderOptions
    {
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
        private int scaleWidth, scaleHeight;
        /// <summary>
        /// Scale the output (or cropped output).
        /// </summary>
        /// <param name="pixelWidth">Set 0 to disable scaling, or non-negative value for the width scaling in pixel</param>
        /// <param name="pixelHeight">Set 0 to disable scaling, or non-negative value for the height scaling in pixel</param>
        public void SetOutputScale(int pixelWidth, int pixelHeight)
        {
            this.scaleWidth = pixelWidth;
            this.scaleHeight = pixelHeight;
        }
        /// <summary>
        /// Crop the output after decoded
        /// </summary>
        /// <param name="cropOffset"></param>
        public void SetOutputCrop(Int32Rect cropOffset)
        {
            this.crop = cropOffset;
        }

        public Size ScaleSize => new Size(scaleWidth, scaleHeight);
        public Int32Rect CropArea => this.crop;

        internal WebPDecoderOptions GetStruct()
        {
            return new WebPDecoderOptions()
            {
                alpha_dithering_strength = _alpha_dithering_strength,
                bypass_filtering = (this.BypassFiltering ? 1 : 0),
                dithering_strength = _dithering_strength,
                flip = (this.FlipVertically ? 1 : 0),
                no_fancy_upsampling = (this.NoFancyUpsampling ? 1 : 0),
                use_threads = (this.UseMultithreading ? 1 : 0),
                use_cropping = (this.crop == Int32Rect.Empty ? 0 : 1),
                crop_left = this.crop.X,
                crop_top = this.crop.Y,
                crop_width = this.crop.Width,
                crop_height = this.crop.Height,
                use_scaling = (((this.scaleWidth == 0) || (this.scaleHeight == 0)) ? 0 : 1),
                scaled_width = this.scaleWidth,
                scaled_height = this.scaleHeight
            };
        }
    }
}
