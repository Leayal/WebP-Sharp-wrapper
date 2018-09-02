using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;

namespace WebPWrapper.WPF
{
    class SimpleWebPContentStream : WebPContentStream
    {
        private IntPtr myPointer;

        internal SimpleWebPContentStream(IntPtr memoryPointer, int length) : base(memoryPointer, length)
        {
            this.myPointer = memoryPointer;
        }

        protected bool _closed;
        public override void Close()
        {
            base.Close();
            if (this._closed) return;
            this._closed = true;
            if (this.myPointer != IntPtr.Zero)
            {
                UnsafeNativeMethods.WebPFree(this.myPointer);
                this.myPointer = IntPtr.Zero;
            }
        }
    }
}
