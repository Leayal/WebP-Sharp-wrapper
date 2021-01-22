using System;
using System.IO;
using Xunit;
using WebPWrapper;
using WebPWrapper.WinForms;
using System.Buffers;
using WebPWrapper.LowLevel;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebPWrapper_xUnit
{
    public class UnitTest : IDisposable
    {
        private readonly WebpFactory factory;

        public UnitTest()
        {
            this.factory = new WebpFactory(Path.Combine("libraries", Environment.Is64BitProcess ? "libwebp-x64.dll" : "libwebp-x86.dll"));
        }

        [Fact]
        public void Test_ProgressiveDecode()
        {
            if (!this.factory.CanDecode)
            {
                throw new BadImageFormatException("Dll is not libwebp or it doesn't contain decode functions.");
            }

            string[] test_files = { "Test_lossy100.webp", "Test_lossy75.webp", "Test_lossless.webp" };

            var borrowed = ArrayPool<byte>.Shared.Rent(4096);
            var buffer = new Span<byte>(borrowed);
            try
            {
                foreach (var filename in test_files)
                {
                    using (var fs = File.OpenRead(filename))
                    {
                        // Test using internal preallocated buffer by decoder.
                        using (var decoder = this.factory.CreateDecoderForRGBX(Colorspace.MODE_bgrA))
                        {
                            var readbyte = fs.Read(buffer);
                            var status = VP8StatusCode.VP8_STATUS_NOT_ENOUGH_DATA;
                            while (readbyte != 0)
                            {
                                status = decoder.AppendEncodedData(buffer.Slice(0, readbyte));
                                if (status != VP8StatusCode.VP8_STATUS_OK && status != VP8StatusCode.VP8_STATUS_SUSPENDED)
                                {
                                    break;
                                }
                                readbyte = fs.Read(buffer);
                            }
                            int last_y = 0;
                            if (decoder.GetDecodedImage(ref last_y, out var width, out var height, out var stride, out IntPtr backBuffer) == VP8StatusCode.VP8_STATUS_OK)
                            {
                                using (var bm = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, backBuffer))
                                {
                                    bm.Save(Path.ChangeExtension(filename, ".internalbuffer.png"), System.Drawing.Imaging.ImageFormat.Png);
                                }
                            }
                        }

                        // Reset stream
                        fs.Position = 0;
                        // Test using external buffer.

                        // Buffering data to get image header. The longest possible is 30?
                        var rentedBuffer = ArrayPool<byte>.Shared.Rent(30);
                        int imgWidth = 0, imgHeight = 0;

                        int headerRead = fs.Read(rentedBuffer);
                        if (headerRead != 0)
                        {
                            if (!this.factory.TryGetImageSize(new ReadOnlySpan<byte>(rentedBuffer, 0, headerRead), out imgWidth, out imgHeight))
                            {
                                // Error!!!
                                continue;
                            }
                        }

                        try
                        {
                            using (var bm = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                            {
                                var lockedBm = bm.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                                var decBuffer = this.factory.CreateDecodeBuffer();
                                VP8StatusCode status;
                                try
                                {
                                    decBuffer.colorspace = Colorspace.MODE_bgrA;
                                    decBuffer.is_external_memory = 1;
                                    decBuffer.u.RGBA.rgba = lockedBm.Scan0;
                                    decBuffer.u.RGBA.stride = lockedBm.Stride;
                                    decBuffer.u.RGBA.size = new UIntPtr(Convert.ToUInt32(lockedBm.Stride * lockedBm.Height));

                                    // Not necessary
                                    // decBuffer.width = lockedBm.Width;
                                    // decBuffer.height = lockedBm.Height;

                                    using (var decoder = this.factory.CreateDecoder(ref decBuffer))
                                    {
                                        status = decoder.AppendEncodedData(new ReadOnlySpan<byte>(rentedBuffer, 0, headerRead));
                                        if (status == VP8StatusCode.VP8_STATUS_SUSPENDED)
                                        {
                                            var readbyte = fs.Read(buffer);
                                            while (readbyte != 0)
                                            {
                                                status = decoder.AppendEncodedData(buffer.Slice(0, readbyte));
                                                if (status != VP8StatusCode.VP8_STATUS_OK && status != VP8StatusCode.VP8_STATUS_SUSPENDED)
                                                {
                                                    break;
                                                }
                                                readbyte = fs.Read(buffer);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    bm.UnlockBits(lockedBm);
                                    this.factory.Free(ref decBuffer);
                                }
                                if (status == VP8StatusCode.VP8_STATUS_OK)
                                {
                                    bm.Save(Path.ChangeExtension(filename, ".externalbuffer.png"), System.Drawing.Imaging.ImageFormat.Png);
                                }
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(rentedBuffer);
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(borrowed);
            }
        }

        [Fact]
        public void Test_Encode_Buffer()
        {
            if (!this.factory.CanEncode)
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
                                    this.factory.EncodeRGB(lockedData.Scan0, lockedData.Width, lockedData.Height, lockedData.Stride, true, outputFileStream, opts);
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

        [Fact]
        public void Test_WinForms()
        {
            using (var webp = new Webp(this.factory, false))
            {
                string[] test_files = { "Test_lossless.webp" };

                foreach (var filename in test_files)
                {
                    using (var fs = File.OpenRead(filename))
                    {
                        var decoderOpts = new WindowsDecoderOptions() { PixelFormat = OutputPixelFormat.PreferSmallSize };
                        var encoderOpts = new EncoderOptions(CompressionType.Lossy, CompressionLevel.Highest, WebPPreset.Default, 90f);
                        using (var bitmap = webp.Decode(fs, decoderOpts))
                        {
                            using (var output = File.Create(Path.GetFileNameWithoutExtension(filename) + "_re-encode-stream.webp"))
                            {
                                webp.Encode(bitmap, output, encoderOpts);
                                output.Flush();
                            }
                        }

                        fs.Position = 0;
                        var length = (int)fs.Length;
                        var buffer = ArrayPool<byte>.Shared.Rent(length);
                        try
                        {
                            if (fs.Read(buffer, 0, buffer.Length) == length)
                            {
                                using (var bitmap = webp.Decode(new ReadOnlyMemory<byte>(buffer, 0, length), decoderOpts))
                                {
                                    using (var output = File.Create(Path.GetFileNameWithoutExtension(filename) + "_re-encode-buffer.webp"))
                                    {
                                        webp.Encode(bitmap, output, encoderOpts);
                                        output.Flush();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.factory.Dispose();
        }
    }
}
