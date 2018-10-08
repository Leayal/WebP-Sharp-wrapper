using System;
using WebPWrapper.WPF.Buffer;

namespace WebPWrapper.WPF
{
    class SimpleWebPContentStream : ChunkedBufferStream
    {
        internal SimpleWebPContentStream(IntPtr memoryPointer, int length) : base(false)
        {
            unsafe
            {
                byte* b = (byte*)(memoryPointer.ToPointer());
                for (int i = 0; i < length; i++)
                    this.WriteByte(b[i]);
            }
            this.SetReadOnlyCore(true);
        }
    }
}
