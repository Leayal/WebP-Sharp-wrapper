using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebPWrapper;
using WebPWrapper.LowLevel;

namespace WebPWrapper_Test
{
    class Test_ProgressiveDecode : IDisposable
    {
        private WebpFactory webp;

        public Test_ProgressiveDecode()
        {
            this.webp = new WebpFactory(Path.Combine("libraries", Environment.Is64BitProcess ? "libwebp-x64.dll" : "libwebp-x86.dll"));
        }

        public void Run(string[] args)
        {
            if (!this.webp.CanDecode)
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
                        using (var decoder = this.webp.CreateDecoderForRGBX(Colorspace.MODE_bgrA))
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
                            if (decoder.GetDecodedImage(out var last_y, out var width, out var height, out var stride, out IntPtr backBuffer) == VP8StatusCode.VP8_STATUS_OK)
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
                            if (!this.webp.TryGetImageInfo(new ReadOnlySpan<byte>(rentedBuffer, 0, headerRead), out imgWidth, out imgHeight))
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
                                var decBuffer = this.webp.CreateDecodeBuffer();
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

                                    using (var decoder = this.webp.CreateDecoder(ref decBuffer))
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
                                    this.webp.Free(ref decBuffer);
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

        public void Dispose() => this.webp.Dispose();
    }
}
