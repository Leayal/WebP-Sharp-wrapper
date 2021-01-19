using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebPWrapper.WPF
{
    class PinnedBuffers : IDisposable
    {
        private readonly Dictionary<byte[], GCHandle> handles;

        public PinnedBuffers()
        {
            this.handles = new Dictionary<byte[], GCHandle>();
        }

        public IntPtr Pin(byte[] buffer)
        {
            if (this.handles.TryGetValue(buffer, out var gchandle))
            {
                return gchandle.AddrOfPinnedObject();
            }
            else
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                this.handles.Add(buffer, handle);
                return handle.AddrOfPinnedObject();
            }
        }

        public bool GetPinned(byte[] buffer, out IntPtr handle)
        {
            if (this.handles.TryGetValue(buffer, out var gchandle))
            {
                handle = gchandle.AddrOfPinnedObject();
                return true;
            }
            else
            {
                handle = IntPtr.Zero;
                return false;
            }
        }

        public void Dispose()
        {
            foreach (var item in this.handles.Values)
            {
                item.Free();
            }
            this.handles.Clear();
        }
    }
}
