using System;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.Buffer;

namespace WebPWrapper.WPF.Helper
{
    internal static class RuntimeValue
    {
        internal static readonly bool is64bit = Environment.Is64BitProcess;

        internal static string StringDependsArchitecture(string x86, string x64)
        {
            if (is64bit)
                return x64;
            else
                return x86;
        }
    }
    /*
        private MemoryWriter webpMemory2;

        private int MyWriter([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            Marshal.Copy(data, webpMemory.data, webpMemory.size, (int)data_size);
            webpMemory.size += (int)data_size;
            return 1;
        }

        
    //private delegate int MyWriterDelegate([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture);
    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct MemoryWriter
    {
        public int size;                    // Size of webP data
        public byte[] data;                 // Data of WebP Image
    }
    */

    internal abstract class WebPContentBuffer : Stream
    {
        internal virtual int MyWriter([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            return 0;
        }
    }


    class WebPMemoryCopyBuffer : WebPContentBuffer
    {
        private ChunkedBufferStream contentStream;
        private ChunkPool pool;

        public void ToReadOnly()
        {
            this.contentStream.Position = 0;
            this.contentStream.SetReadOnlyCore(true);
        }

        internal override int MyWriter([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
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

    /// <summary>
    /// Use old-school buffering enqueue system
    /// </summary>
    internal class WebPFileWriter : IDisposable
    {
        private readonly FileStream fs;
        private bool _preallocateOnDisk;

        internal int MyFileWriter([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            int blocksize = (int)data_size;

            if (this._preallocateOnDisk)
            {
                this._preallocateOnDisk = false;

                // The first memory block that decoder push will be the webp file header (which contains the WebPContent size in bytes).
                // Total of 12 bytes:
                // 4: ASCII "RIFF"
                // 4: WebP Content Size
                // 4: ASCII "WEBP"

                // Get the WebPContent size in bytes
                int contentSizeWithoutRIFFHeader = Marshal.ReadInt32(data, 4);
                contentSizeWithoutRIFFHeader += 8; // Because the we got above is starting from offset 8 of the real file.
                try
                {
                    this.fs.SetLength(contentSizeWithoutRIFFHeader);
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
                        // WriteByte may cause overhead but Marshal.Copy to use Write(byte[], int, int) is not good in case, either.
                        // WriteByte method still uses FileStream's buffer, by the way.
                        this.fs.WriteByte(b[i]);
                    }
                }
                catch
                {
                    // Tell the native library to stop because we got an error here.
                    // Either the file has been removed or the write permission has been cut off by someone (in case it's a network file)
                    // Or locked by anti-virus or anything.
                    return 0;
                }
            }

            return 1;
        }

        internal WebPFileWriter(string filepath) : this(filepath, true) { }

        internal WebPFileWriter(string filepath, bool preAllocateOnDisk)
        {
            this._preallocateOnDisk = preAllocateOnDisk;
            this.fs = File.Create(filepath);
        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            this.fs.Flush();
            if (this.fs != null)
                this.fs.Dispose();
        }
    }
}
