using System;
using System.IO;
using System.Runtime.InteropServices;
using WebPWrapper.WPF.LowLevel;

namespace WebPWrapper.WPF.Helper
{
    /// <summary>
    /// Use old-school buffering enqueue system
    /// </summary>
    internal class WebPFileWriter : IDisposable
    {
        private readonly FileStream fs;
        private bool _preallocateOnDisk;

        internal int MyFileWriter([In] IntPtr data, [param:MarshalAs(UnmanagedType.SysUInt)]UIntPtr data_size, ref WebPPicture picture)
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
