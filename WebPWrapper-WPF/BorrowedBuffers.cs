using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPWrapper.WPF
{
    class BorrowedBuffers : IDisposable
    {
        private readonly List<byte[]> borrowed;

        public BorrowedBuffers()
        {
            this.borrowed = new List<byte[]>();
        }

        public byte[] Rent(int minimumLength)
        {
            var result = ArrayPool<byte>.Shared.Rent(minimumLength);
            this.borrowed.Add(result);
            return result;
        }

        public void Dispose()
        {
            for (int i = 0; i < this.borrowed.Count; i++)
            {
                ArrayPool<byte>.Shared.Return(this.borrowed[i], true);
            }
            this.borrowed.Clear();
        }
    }
}
