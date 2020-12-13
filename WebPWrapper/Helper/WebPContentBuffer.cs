using System;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.LowLevel;

namespace WebPWrapper.WPF.Helper
{
    internal abstract class WebPContentBuffer : Stream
    {
        internal virtual int MyWriter([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            
            return 0;
        }
    }
}
