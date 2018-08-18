using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebPWrapper
{
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

    internal class WebPMemoryBuffer : IDisposable
    {
        internal IntPtr allocPointer;
        internal int capacity;
        private int position;

        internal int MyWriter([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            int blocksize = (int)data_size;
            // The first memory block that decoder push will be the webp file header (which contains the WebPContent size in bytes).
            // Total of 12 bytes:
            // 4: ASCII "RIFF"
            // 4: WebP Content Size
            // 4: ASCII "WEBP"
            if (this.capacity == 0)
            {
                // Get the WebPContent size in bytes
                int contentSizeWithoutRIFFHeader = Marshal.ReadInt32(data, 4);
                this.capacity = contentSizeWithoutRIFFHeader + 8; // Because the we got above is starting from offset 8 of the real file.
                this.allocPointer = Marshal.AllocHGlobal(this.capacity);
            }

            IntPtr newAdd = new IntPtr(allocPointer.ToInt32() + this.position);
            this.position += blocksize;

            unsafe
            {
                // Buffer.MemoryCopy only exists in .NET 4.6 and up
                // Buffer.MemoryCopy(data.ToPointer(), newAdd.ToPointer(), blocksize, blocksize);
                UnsafeNativeMethods.MemoryCopy(newAdd.ToPointer(), data.ToPointer(), blocksize);
            }
            // Marshal.Copy(data, this.writer.data, this.writer.size, (int)data_size);

            return 1;
        }

        [Obsolete("While this should work, it will waste the memory as it allocate the size of raw pixel data. The newer one (lazily allocate) is better.", true)]
        internal WebPMemoryBuffer(int length)
        {
            this.allocPointer = Marshal.AllocHGlobal(length);
            this.capacity = length;
            this.position = 0;
        }

        /// <summary>
        /// Lazily allocate the buffer according to the size.
        /// </summary>
        internal WebPMemoryBuffer()
        {
            this.allocPointer = IntPtr.Zero;
            this.capacity = 0;
            this.position = 0;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;
            if (this.allocPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.allocPointer);
                this.allocPointer = IntPtr.Zero;
            }
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
                // Get the WebPContent size in bytes
                int contentSizeWithoutRIFFHeader = Marshal.ReadInt32(data, 4);
                contentSizeWithoutRIFFHeader += 8; // Because the we got above is starting from offset 8 of the real file.
                this.fs.SetLength(contentSizeWithoutRIFFHeader);
            }

            unsafe
            {
                byte* b = (byte*)(data.ToPointer());
                for (int i = 0; i < blocksize; i++)
                {
                    // WriteByte may cause overhead but Marshal.Copy to use Write(byte[], int, int) is not good in case, either.
                    // WriteByte method still uses FileStream's buffer, by the way.
                    this.fs.WriteByte(b[i]);
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
