using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPWrapper.WPF.Buffer
{
    internal class ManagedMemoryChunk
    {
        public ManagedMemoryChunk Next = null;
        public readonly byte[] Buffer;

        public ManagedMemoryChunk(int size)
        {
            this.Buffer = new byte[size];
        }
    }

    internal class UnmanagedMemoryChunk
    {
        public UnmanagedMemoryChunk Next;
        public readonly IntPtr Pointer;
        public readonly int Size;

        public UnmanagedMemoryChunk(IntPtr pointer, int size)
        {
            this.Next = null;
            this.Pointer = pointer;
            this.Size = size;
        }
    }
}
