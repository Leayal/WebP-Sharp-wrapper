using System;
using System.IO;

namespace WebPWrapper
{
    public abstract class WebPContentStream : UnmanagedMemoryStream
    {
        protected bool __closed;
        protected IntPtr startPointer;

        protected unsafe WebPContentStream(IntPtr memoryPointer, int length) : base((byte*)(memoryPointer.ToPointer()), length, length, FileAccess.Read)
        {
            this.startPointer = memoryPointer;
            this.__closed = false;
        }

        internal virtual ref IntPtr GetPointer() => ref this.startPointer;
    }
}
