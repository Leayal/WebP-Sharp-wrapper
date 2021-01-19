using System;
using System.Collections.Generic;
using WebPWrapper;
using WebPWrapper.WinForms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
                    using (var bitmap = this.webp.Decode(fs, new WinFormsDecoderOptions() { OptimizeForRendering = false }))
                    {
                        using (var mem = this.webp.Encode(bitmap, new EncoderOptions(CompressionType.Lossy, CompressionLevel.Highest, WebPPreset.Default, 90f)))
                        using (var output = File.Create("Test_ReencodeLossless.webp"))
                        {
                            mem.WriteTo(output);
                            output.Flush();
                        }
                    }
                }
            }
        }

        public void Dispose() => this.webp.Dispose();
    }
}
