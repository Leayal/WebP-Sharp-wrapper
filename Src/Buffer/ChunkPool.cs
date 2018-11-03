using System.Collections.Concurrent;
using System;

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

        public int BlockSize => this._chunkSize;

        public void ReturnToPool(ManagedMemoryChunk chunk)
        {
            this.chunkList.Add(chunk);
        }

        public ManagedMemoryChunk RequestChunk()
        {
            ManagedMemoryChunk chunk;
            if (this.chunkList.TryTake(out chunk))
            {
                // Array.Clear(chunk.Buffer, 0, chunk.Buffer.Length);
                return chunk;
            }
            else
            {
                return new ManagedMemoryChunk(this._chunkSize);
            }
        }

        public void Dispose()
        {
            this.chunkList = null;
            GC.SuppressFinalize(this);
        }
    }
}
