using System;
using System.Collections.Concurrent;

namespace WebPWrapper.WPF.Buffer
{
    class ChunkPool : IDisposable
    {
        private object locking;
        private readonly int _chunkSize;
        private ConcurrentBag<ManagedMemoryChunk> chunkList;

        internal ChunkPool(int chunkSize)
        {
            this.locking = new object();
            this._chunkSize = chunkSize;
            this.chunkList = new ConcurrentBag<ManagedMemoryChunk>();
        }

        public void ReturnToPool(ManagedMemoryChunk chunk)
        {
            this.chunkList?.Add(chunk);
        }

        public ManagedMemoryChunk RequestChunk()
        {
            ConcurrentBag<ManagedMemoryChunk> list;
            lock (this.locking)
            {
                if (this.chunkList == null)
                    throw new ObjectDisposedException("ChunkPool");

                list = chunkList;
            }
            ManagedMemoryChunk chunk;
            if (list.TryTake(out chunk))
            {
                // Array.Clear(chunk.Buffer, 0, chunk.Buffer.Length);
                list = null;
                return chunk;
            }
            else
            {
                list = null;
                return new ManagedMemoryChunk(this._chunkSize);
            }
        }

        public void Dispose()
        {
            lock (this.locking)
            {
                this.chunkList = null;
            }
        }
    }
}
