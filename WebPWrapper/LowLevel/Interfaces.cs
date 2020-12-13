using System;
using System.Runtime.InteropServices;

namespace WebPWrapper.LowLevel
{
    interface Interfaces
    {
        int MyWriter([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture);
    }
}
