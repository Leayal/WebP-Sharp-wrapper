using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebPWrapper.WPF.Buffer
{
    /// <summary>
    /// Based on https://referencesource.microsoft.com/#System.Runtime.Remoting/channels/core/chunkedmemorystream.cs
    /// </summary>
    internal class UnmanagedChunkedBufferStream : Stream
    {
        // state
        private UnmanagedMemoryChunk _firstchunks = null;      // data

        private bool _bClosed = false;   // has the stream been closed.        

        private UnmanagedMemoryChunk _readChunk = null; // current chunk to read from
        private int _readOffset = 0;  // offset into chunk to read from
        private readonly long length;

        public UnmanagedChunkedBufferStream(UnmanagedMemoryChunk initialChunk)
        {
            this._firstchunks = initialChunk;
            UnmanagedMemoryChunk unmanagedChunk = initialChunk;
            long totalLength = 0;
            while (unmanagedChunk != null)
            {
                totalLength += unmanagedChunk.Size;
                unmanagedChunk = unmanagedChunk.Next;
            }
            this.length = totalLength;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                if (_bClosed)
                    throw new ObjectDisposedException("ChunkedBuffer");

                return this.length;
            }
        } // Length        

        public override long Position
        {
            get
            {
                if (_bClosed)
                    throw new ObjectDisposedException("ChunkedBuffer");

                if (_readChunk == null)
                    return 0;

                int pos = 0;
                UnmanagedMemoryChunk chunk = _firstchunks;
                while (chunk != _readChunk)
                {
                    pos += chunk.Size;
                    chunk = chunk.Next;
                }
                pos += _readOffset;

                return (long)pos;
            }

            set
            {
                if (_bClosed)
                    throw new ObjectDisposedException("ChunkedBuffer");

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                // back up current position in case new position is out of range
                UnmanagedMemoryChunk backupReadChunk = _readChunk;
                int backupReadOffset = _readOffset;

                _readChunk = null;
                _readOffset = 0;

                int leftUntilAtPos = (int)value;
                UnmanagedMemoryChunk chunk = _firstchunks;
                while (chunk != null)
                {
                    if ((leftUntilAtPos < chunk.Size) ||
                            ((leftUntilAtPos == chunk.Size) &&
                             (chunk.Next == null)))
                    {
                        // the desired position is in this chunk
                        _readChunk = chunk;
                        _readOffset = leftUntilAtPos;
                        break;
                    }

                    leftUntilAtPos -= chunk.Size;
                    chunk = chunk.Next;
                }

                if (_readChunk == null)
                {
                    // position is out of range
                    _readChunk = backupReadChunk;
                    _readOffset = backupReadOffset;
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        } // Position

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return Position;
        } // Seek


        public override void SetLength(long value) { throw new NotSupportedException(); }

        protected override void Dispose(bool disposing)
        {
            try
            {
                _bClosed = true;
                _firstchunks = null;
                _readChunk = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        } // Close

        public override void Flush()
        {
        } // Flush


        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (_readChunk == null)
            {
                if (_firstchunks == null)
                    return 0;
                _readChunk = _firstchunks;
                _readOffset = 0;
            }

            int chunkSize = _readChunk.Size;

            int bytesRead = 0;

            while (count > 0)
            {
                if (_readOffset == chunkSize)
                {
                    // exit if no more chunks are currently available
                    if (_readChunk.Next == null)
                        break;

                    _readChunk = _readChunk.Next;
                    _readOffset = 0;
                    chunkSize = _readChunk.Size;
                }

                int readCount = Math.Min(count, chunkSize - _readOffset);
                unsafe
                {
                    UnsafeNativeMethods.Memcpy(buffer, offset, (byte*)(_readChunk.Pointer.ToPointer()), _readOffset, readCount);
                }
                offset += readCount;
                count -= readCount;
                _readOffset += readCount;
                bytesRead += readCount;
            }

            return bytesRead;
        } // Read

        public override int ReadByte()
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (_readChunk == null)
            {
                if (_firstchunks == null)
                    return 0;
                _readChunk = _firstchunks;
                _readOffset = 0;
            }

            int chunkSize = _readChunk.Size;

            if (_readOffset == chunkSize)
            {
                // exit if no more chunks are currently available
                if (_readChunk.Next == null)
                    return -1;

                _readChunk = _readChunk.Next;
                _readOffset = 0;
                chunkSize = _readChunk.Size;
            }

            byte result;
            unsafe
            {
                result = Marshal.ReadByte(new IntPtr(((byte*)_readChunk.Pointer.ToPointer()) + _readOffset));
            }
            _readOffset++;
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        } // Write

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        } // WriteByte


        // copy entire buffer into an array
        public virtual byte[] ToArray()
        {
            int length = (int)Length; // this will throw if stream is closed
            byte[] copy = new byte[length];

            UnmanagedMemoryChunk backupReadChunk = _readChunk;
            int backupReadOffset = _readOffset;

            _readChunk = _firstchunks;
            _readOffset = 0;
            Read(copy, 0, length);

            _readChunk = backupReadChunk;
            _readOffset = backupReadOffset;

            return copy;
        } // ToArray      


        // write remainder of this stream to another stream
        public void WriteTo(Stream stream)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (stream == null)
                throw new ArgumentNullException("stream");

            int readbyte = this.ReadByte();
            while (readbyte != -1) // loop until end of chunks is found
            {
                stream.WriteByte((byte)readbyte);
            }
        }

        public void WriteTo(Stream stream, int bufferSize)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (bufferSize < 8)
                bufferSize = 8;
            byte[] buffer = new byte[bufferSize];
            int readbyte = this.Read(buffer, 0, buffer.Length);
            while (readbyte > 0) // loop until end of chunks is found
            {
                stream.Write(buffer, 0, readbyte);
                readbyte = this.Read(buffer, 0, buffer.Length);
            }
        }
    }
}
