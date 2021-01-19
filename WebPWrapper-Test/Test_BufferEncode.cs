using System;
using System.Drawing;
using System.IO;
using WebPWrapper;
using WebPWrapper.LowLevel;
using System.Drawing.Imaging;

namespace WebPWrapper_Test
{
    class Test_BufferEncode : IDisposable
    {
        private WebpFactory webp;

        public Test_BufferEncode()
        {
            this.webp = new WebpFactory(Path.Combine("libraries", Environment.Is64BitProcess ? "libwebp-x64.dll" : "libwebp-x86.dll"));
        }

        /// <summary>
        /// Actually a <seealso cref="FileStream"/> but coming with <seealso cref="IOutputStream"/> interface.
        /// </summary>
        class WrapperStream : FileStream, IOutputStream
        {
            public WrapperStream(string path) : base(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read) { }
        }

        public void Run(string[] args)
        {
            if (!this.webp.CanEncode)
            {
                throw new BadImageFormatException("Dll is not libwebp or it doesn't contain encode functions.");
            }

            string[] test_files = { "Test1.png" };

            foreach (var filename in test_files)
            {
                using (var fs = File.OpenRead(filename))
                {
                    // Test using the shared buffer from .NET instead of copying into unmanaged buffer of WebP's encoder.
                    using (var bitmap = new Bitmap(fs, false))
                    {
                        var wholeImg = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                        Bitmap bm;
                        if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            bm = bitmap;
                        }
                        else
                        {
                            bm = bitmap.Clone(wholeImg, PixelFormat.Format32bppArgb);
                        }
                        try
                        {
                            var opts = new EncoderOptions(CompressionType.Lossy, CompressionLevel.Highest, WebPPreset.Default, 90f);
                            var lockedData = bm.LockBits(wholeImg, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            try
                            {
                                using (var outputFileStream = new WrapperStream(Path.ChangeExtension(filename, ".webp")))
                                {
                                    webp.EncodeRGB(lockedData.Scan0, lockedData.Width, lockedData.Height, lockedData.Stride, true, outputFileStream, opts);
                                }
                            }
                            finally
                            {
                                bm.UnlockBits(lockedData);
                            }
                        }
                        finally
                        {
                            bm.Dispose();
                        }
                    }
                }
            }
        }

        public void Dispose() => this.webp.Dispose();
    }
}
