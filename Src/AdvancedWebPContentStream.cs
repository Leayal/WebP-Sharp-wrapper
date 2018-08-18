using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebPWrapper
{
    class AdvancedWebPContentStream : WebPContentStream
    {
        private WebPPicture pictureStruct;

        internal unsafe AdvancedWebPContentStream(ref WebPPicture picturedata, WebPMemoryBuffer memoryWriter) : base(memoryWriter.allocPointer, memoryWriter.capacity)
        {
            this.pictureStruct = picturedata;
        }

        public override void Close()
        {
            base.Close();
            if (this.__closed) return;
            this.__closed = true;
            if (this.pictureStruct.argb != IntPtr.Zero)
            {
                UnsafeNativeMethods.WebPPictureFree(ref this.pictureStruct);
            }
        }
    }
}
