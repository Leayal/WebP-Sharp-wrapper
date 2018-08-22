using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPWrapper.WPF
{
    public sealed class WebPHeader
    {
        public CompressionType CompressionType { get; }
        public bool HasAlpha { get; }
        public bool HasAnimation { get; }
        public int Height { get; }
        public int Width { get; }

        internal WebPHeader(ref WebPBitstreamFeatures features)
        {
            switch (features.format)
            {
                case 1:
                    this.CompressionType = CompressionType.Lossy;
                    break;
                case 2:
                    this.CompressionType = CompressionType.Lossless;
                    break;
                default:
                    this.CompressionType = CompressionType.NearLossless;
                    break;
            }
            this.HasAlpha = (features.has_alpha != 0);
            this.HasAnimation = (features.has_animation != 0);
            this.Width = features.width;
            this.Height = features.height;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CompressionType: {0}", this.CompressionType);
            sb.AppendLine();
            sb.AppendFormat("HasAlpha: {0}", this.HasAlpha);
            sb.AppendLine();
            sb.AppendFormat("HasAnimation: {0}", this.HasAnimation);
            sb.AppendLine();
            sb.AppendFormat("Resolution: {0}x{1}", this.Width, this.Height);
            return sb.ToString();
        }
    }
}
