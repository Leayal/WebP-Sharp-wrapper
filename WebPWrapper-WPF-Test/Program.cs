using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WebPWrapper.WPF;

namespace WebPWrapper_WPF_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Version ver;
            using (WebP webp = new WebP())
            {
                var bitmap = webp.DecodeFile(@"F:\All Content\VB_Project\visual studio 2015\libwebp-1.0.0\webp_js\test_webp_js.webp");
                WebPWrapper.WPF.LowLevel.ILibwebp libwebp = webp.GetDirectAccessToLibrary();
                
                webp.EncodeLossyToFile(bitmap, "Test.webp", 100);
            }
            
            Console.WriteLine("Press any keys to close.");
            if (Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
