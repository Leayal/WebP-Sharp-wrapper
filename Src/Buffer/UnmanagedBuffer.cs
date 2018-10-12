using System.Runtime.InteropServices;

namespace WebPWrapper.WPF.Buffer
{
    class UnmanagedBuffer : SafeBuffer
    {
        public UnmanagedBuffer(int length) : base(true)
        {
            this.SetHandle(Marshal.AllocHGlobal(length));
            this.Initialize((ulong)length);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(this.handle);
            return true;
        }
    }
}
