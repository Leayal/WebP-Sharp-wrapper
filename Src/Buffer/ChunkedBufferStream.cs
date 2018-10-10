using System;
using System.IO;

namespace WebPWrapper.WPF.Buffer
{
    /// <summary>
    /// Based on https://referencesource.microsoft.com/#System.Runtime.Remoting/channels/core/chunkedmemorystream.cs
    /// </summary>
    internal class ChunkedBufferStream : Stream
    {
        // state
        private ManagedMemoryChunk _firstchunks;      // data

        private bool _bClosed = false;   // has the stream been closed.        

        private ManagedMemoryChunk _writeChunk = null; // current chunk to write to
        private int _writeOffset = 0; // offset into chunk to write to
        private ManagedMemoryChunk _readChunk = null; // current chunk to read from
        private int _readOffset = 0;  // offset into chunk to read from
        private bool _isReadOnly;
        private ChunkPool _bufferPool;

        public ChunkedBufferStream(bool isReadOnly) : this(null, false) { }

        public ChunkedBufferStream(ChunkPool bufferPool, bool isReadOnly) : this(bufferPool, null, isReadOnly) { }

        public ChunkedBufferStream(ChunkPool bufferPool, ManagedMemoryChunk initialChunk, bool isReadOnly)
        {
            this._bufferPool = bufferPool;
            this._isReadOnly = isReadOnly;
            this._firstchunks = initialChunk;
            this._writeChunk = initialChunk;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => !this._isReadOnly;

        public override long Length
        {
            get
            {
                if (_bClosed)
                    throw new ObjectDisposedException("ChunkedBuffer");


                int length = 0;
                ManagedMemoryChunk chunk = _firstchunks;
                while (chunk != null)
                {
                    ManagedMemoryChunk next = chunk.Next;
                    if (next != null)
                        length += chunk.Buffer.Length;
                    else
                        length += _writeOffset;

                    chunk = next;
                }

                return (long)length;
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
                ManagedMemoryChunk chunk = _firstchunks;
                while (chunk != _readChunk)
                {
                    pos += chunk.Buffer.Length;
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
                ManagedMemoryChunk backupReadChunk = _readChunk;
                int backupReadOffset = _readOffset;

                _readChunk = null;
                _readOffset = 0;

                int leftUntilAtPos = (int)value;
                ManagedMemoryChunk chunk = _firstchunks;
                while (chunk != null)
                {
                    if ((leftUntilAtPos < chunk.Buffer.Length) ||
                            ((leftUntilAtPos == chunk.Buffer.Length) &&
                             (chunk.Next == null)))
                    {
                        // the desired position is in this chunk
                        _readChunk = chunk;
                        _readOffset = leftUntilAtPos;
                        break;
                    }

                    leftUntilAtPos -= chunk.Buffer.Length;
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
                if (disposing)
                    ReleaseMemoryChunks(_firstchunks);
                _firstchunks = null;
                _writeChunk = null;
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

            byte[] chunkBuffer = _readChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
            if (_readChunk.Next == null)
                chunkSize = _writeOffset;

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
                    chunkBuffer = _readChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                    if (_readChunk.Next == null)
                        chunkSize = _writeOffset;
                }

                int readCount = Math.Min(count, chunkSize - _readOffset);
                System.Buffer.BlockCopy(chunkBuffer, _readOffset, buffer, offset, readCount);
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

            byte[] chunkBuffer = _readChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
            if (_readChunk.Next == null)
                chunkSize = _writeOffset;

            if (_readOffset == chunkSize)
            {
                // exit if no more chunks are currently available
                if (_readChunk.Next == null)
                    return -1;

                _readChunk = _readChunk.Next;
                _readOffset = 0;
                chunkBuffer = _readChunk.Buffer;
                chunkSize = chunkBuffer.Length;
                if (_readChunk.Next == null)
                    chunkSize = _writeOffset;
            }

            return chunkBuffer[_readOffset++];
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (_isReadOnly)
                throw new InvalidOperationException();

            if (_firstchunks == null)
            {
                _firstchunks = AllocateMemoryChunk();
                _writeChunk = _firstchunks;
                _writeOffset = 0;
            }

            byte[] chunkBuffer = _writeChunk.Buffer;
            int chunkSize = chunkBuffer.Length;

            while (count > 0)
            {
                if (_writeOffset == chunkSize)
                {
                    // allocate a new chunk if the current one is full
                    _writeChunk.Next = AllocateMemoryChunk();
                    _writeChunk = _writeChunk.Next;
                    _writeOffset = 0;
                    chunkBuffer = _writeChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                }

                int copyCount = Math.Min(count, chunkSize - _writeOffset);
                System.Buffer.BlockCopy(buffer, offset, chunkBuffer, _writeOffset, copyCount);
                offset += copyCount;
                count -= copyCount;
                _writeOffset += copyCount;
            }

        } // Write

        public override void WriteByte(byte value)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (_isReadOnly)
                throw new InvalidOperationException();

            if (_firstchunks == null)
            {
                _firstchunks = AllocateMemoryChunk();
                _writeChunk = _firstchunks;
                _writeOffset = 0;
            }

            byte[] chunkBuffer = _writeChunk.Buffer;
            int chunkSize = chunkBuffer.Length;

            if (_writeOffset == chunkSize)
            {
                // allocate a new chunk if the current one is full
                _writeChunk.Next = AllocateMemoryChunk();
                _writeChunk = _writeChunk.Next;
                _writeOffset = 0;
                chunkBuffer = _writeChunk.Buffer;
                chunkSize = chunkBuffer.Length;
            }

            chunkBuffer[_writeOffset++] = value;
        } // WriteByte


        // copy entire buffer into an array
        public virtual byte[] ToArray()
        {
            int length = (int)Length; // this will throw if stream is closed
            byte[] copy = new byte[Length];

            ManagedMemoryChunk backupReadChunk = _readChunk;
            int backupReadOffset = _readOffset;

            _readChunk = _firstchunks;
            _readOffset = 0;
            Read(copy, 0, length);

            _readChunk = backupReadChunk;
            _readOffset = backupReadOffset;

            return copy;
        } // ToArray      


        // write remainder of this stream to another stream
        public virtual void WriteTo(Stream stream)
        {
            if (_bClosed)
                throw new ObjectDisposedException("ChunkedBuffer");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (_readChunk == null)
            {
                if (_firstchunks == null)
                    return;

                _readChunk = _firstchunks;
                _readOffset = 0;
            }

            byte[] chunkBuffer = _readChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
            if (_readChunk.Next == null)
                chunkSize = _writeOffset;

            // following code mirrors Read() logic (_readChunk/_readOffset should
            //   point just past last byte of last chunk when done)

            while (true) // loop until end of chunks is found
            {
                if (_readOffset == chunkSize)
                {
                    // exit if no more chunks are currently available
                    if (_readChunk.Next == null)
                        break;

                    _readChunk = _readChunk.Next;
                    _readOffset = 0;
                    chunkBuffer = _readChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                    if (_readChunk.Next == null)
                        chunkSize = _writeOffset;
                }

                int writeCount = chunkSize - _readOffset;
                stream.Write(chunkBuffer, _readOffset, writeCount);
                _readOffset = chunkSize;
            }

        }

        internal void SetReadOnlyCore(bool val)
        {
            this._isReadOnly = val;
        }

        private ManagedMemoryChunk AllocateMemoryChunk()
        {
            if (this._bufferPool != null)
            {
                return this._bufferPool.RequestChunk();
            }
            else
            {
                return new ManagedMemoryChunk(Helper.RuntimeValue.DefaultBufferSize);
            }
        }

        private void ReleaseMemoryChunks(ManagedMemoryChunk chunk)
        {
            // If the buffer pool always allocates a new buffer,
            //   there's no point to trying to return all of the buffers. 
            if (this._bufferPool == null)
                return;

            while (chunk != null)
            {
                this._bufferPool.ReturnToPool(chunk);
                chunk = chunk.Next;
            }

            this._bufferPool = null;
        }
    }
}
