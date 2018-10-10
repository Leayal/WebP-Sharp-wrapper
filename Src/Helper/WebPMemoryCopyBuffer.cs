using System;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.Buffer;
using WebPWrapper.WPF.LowLevel;

namespace WebPWrapper.WPF.Helper
{
    class WebPMemoryCopyBuffer : WebPContentBuffer
    {
        private ChunkedBufferStream contentStream;
        private ChunkPool pool;

        public void ToReadOnly()
        {
            this.contentStream.Position = 0;
            this.contentStream.SetReadOnlyCore(true);
        }

        internal override int MyWriter([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            int blocksize = (int)data_size;

            if (this.contentStream == null)
            {
                int contentSizeWithoutRIFFHeader = Marshal.ReadInt32(data, 4);
                contentSizeWithoutRIFFHeader += 8; // Because the we got above is starting from offset 8 of the real file.
                try
                {
                    ManagedMemoryChunk bigChunkIndeed = new ManagedMemoryChunk(contentSizeWithoutRIFFHeader);
                    this.contentStream = new ChunkedBufferStream(this.pool, bigChunkIndeed, false);
                }
                catch
                {
                    // Pre-allocate size on disk failed. Either not enough space or I don't know.
                    return 0;
                }
            }

            unsafe
            {
                byte* b = (byte*)(data.ToPointer());
                try
                {
                    for (int i = 0; i < blocksize; i++)
                    {
                        // May cause overhead
                        this.contentStream.WriteByte(b[i]);
                    }
                }
                catch
                {
                    // In case someone freed the buffer in the memory while the memory copy is on going.
                    // Or the memory cannot allocate more.
                    return 0;
                }
            }

            return 1;
        }

        /// <summary>
        /// Lazily allocate the buffer according to the size.
        /// </summary>
        internal WebPMemoryCopyBuffer(ChunkPool chunkpool, bool isContiguousMemory)
        {
            this.pool = chunkpool;
            if (!isContiguousMemory)
                this.contentStream = new ChunkedBufferStream(chunkpool, false);
        }

        private bool _disposed;

        public override bool CanRead => true;

        public override bool CanSeek => this.contentStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => this.contentStream.Length;

        public override long Position { get => this.contentStream.Position; set => this.contentStream.Position = value; }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._disposed) return;
                    this._disposed = true;
                    this.contentStream.Dispose();
                    this.pool = null;
                    this.contentStream = null;
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.contentStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            return this.contentStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.contentStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.contentStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Flush() => this.contentStream.Flush();

        public override long Seek(long offset, SeekOrigin origin) => this.contentStream.Seek(offset, origin);

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
