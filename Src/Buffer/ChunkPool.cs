using System;
using System.Collections.Concurrent;

namespace WebPWrapper.WPF.Buffer
{
    class ChunkPool : IDisposable
    {
        private readonly int _chunkSize;
        private ConcurrentBag<ManagedMemoryChunk> chunkList;

        internal ChunkPool(int chunkSize)
        {
            this._chunkSize = chunkSize;
            this.chunkList = new ConcurrentBag<ManagedMemoryChunk>();
        }

        public void ReturnToPool(ManagedMemoryChunk chunk)
        {
            this.chunkList?.Add(chunk);
        }

        public ManagedMemoryChunk RequestChunk()
        {
            if (this.chunkList == null)
                throw new ObjectDisposedException("ChunkPool");
            ManagedMemoryChunk chunk;
            if (this.chunkList.TryTake(out chunk))
            {
                // Array.Clear(chunk.Buffer, 0, chunk.Buffer.Length);
                return chunk;
            }
            else
                return new ManagedMemoryChunk(this._chunkSize);
        }

        public void Dispose()
        {
            this.chunkList = null;
        }
    }
}
