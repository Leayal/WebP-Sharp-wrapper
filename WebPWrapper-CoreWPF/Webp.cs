using System;
using System.Buffers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper;
using WebPWrapper.LowLevel;

namespace WebPWrapper.WPF
{
    public class Webp : IDisposable
    {
        private WebpFactory webp;
        private bool disposed;

        public Webp(string libraryPath)
        {
            this.disposed = false;
            this.webp = new WebpFactory(libraryPath);
        }

        /// <summary>aaaaa</summary>
        public void Encode(BitmapSource image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (image.IsDownloading)
            {
                throw new ArgumentException("The image is still being downloaded.", nameof(image));
            }
            if (image.Format == PixelFormats.Rgb24)
            {

            }
            var duh = new FormatConvertedBitmap();
        }

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;

            this.webp.Dispose();
        }
    }
}
