using System;
using System.Collections.Generic;
using WebPWrapper;
using WebPWrapper.WinForms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Buffers;

namespace WebPWrapper_Test
{
    class Test_WinForm : IDisposable
    {
        private Webp webp;

        public Test_WinForm()
        {
            this.webp = new Webp(Path.Combine("libraries", Environment.Is64BitProcess ? "libwebp-x64.dll" : "libwebp-x86.dll"));
        }

        public void Run(string[] args)
        {
            string[] test_files = { "Test_lossless.webp" };

            foreach (var filename in test_files)
            {
                using (var fs = File.OpenRead(filename))
                {
                    var decoderOpts = new WindowsDecoderOptions() { PixelFormat = OutputPixelFormat.PreferSmallSize };
                    var encoderOpts = new EncoderOptions(CompressionType.Lossy, CompressionLevel.Highest, WebPPreset.Default, 90f);
                    using (var bitmap = this.webp.Decode(fs, decoderOpts))
                    {
                        using (var output = File.Create(Path.GetFileNameWithoutExtension(filename) + "_re-encode-stream.webp"))
                        {
                            this.webp.Encode(bitmap, output, encoderOpts);
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
                            using (var bitmap = this.webp.Decode(new ReadOnlyMemory<byte>(buffer, 0, length), decoderOpts))
                            {
                                using (var output = File.Create(Path.GetFileNameWithoutExtension(filename) + "_re-encode-buffer.webp"))
                                {
                                    this.webp.Encode(bitmap, output, encoderOpts);
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

        public void Dispose() => this.webp.Dispose();
    }
}
