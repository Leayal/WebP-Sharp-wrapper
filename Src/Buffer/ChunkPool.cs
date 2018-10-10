using System.Collections.Concurrent;

namespace WebPWrapper.WPF.Buffer
{
    class ChunkPool
    {
        private object locking;
        private readonly int _chunkSize;
        private ConcurrentBag<ManagedMemoryChunk> chunkList;

        internal ChunkPool(int chunkSize)
        {
            this._chunkSize = chunkSize;
            this.chunkList = new ConcurrentBag<ManagedMemoryChunk>();
        }

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
    }
}
