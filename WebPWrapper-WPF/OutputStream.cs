using System;
using System.IO;

#if WPF
namespace WebPWrapper.WPF
#else
namespace WebPWrapper.WinForms
#endif
{
    /// <summary>Wrapper class</summary>
    class OutputStream : IOutputStream
    {
        public Stream BaseStream { get; }
        public OutputStream(Stream basestream)
        {
            this.BaseStream = basestream;
        }

        public void Flush()
        {
            this.BaseStream.Flush();
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
#if NET472 || NET48
            byte[] sharedBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                this.BaseStream.Write(sharedBuffer, 0, buffer.Length);
            }
            finally { System.Buffers.ArrayPool<byte>.Shared.Return(sharedBuffer); }
#else
            this.BaseStream.Write(buffer);
#endif
        }

        public void Dispose()
        {
            this.BaseStream.Dispose();
        }
    }
}
