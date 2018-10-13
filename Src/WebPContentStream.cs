using System;
using System.IO;

namespace WebPWrapper.WPF
{
    /// <summary>
    /// Dispose this stream ASAP when you don't use it anymore. Or wrap it within a using block.
    /// </summary>
    public abstract class WebPContentStream : UnmanagedMemoryStream
    {
        private IntPtr startPointer;

        protected unsafe WebPContentStream(IntPtr memoryPointer, int length) : base((byte*)(memoryPointer.ToPointer()), length, length, FileAccess.Read)
        {
            this.startPointer = memoryPointer;
        }

        internal virtual ref IntPtr GetPointer() => ref this.startPointer;
    }
}
