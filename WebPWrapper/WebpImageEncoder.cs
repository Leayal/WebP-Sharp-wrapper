using System;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    public sealed class WebpImageEncoder : IDisposable
    {
        private WebPConfig encoderConfig;
        private Libwebp webp;
        private bool disposed;

        private IntPtr outputDataPointer;

        internal WebpImageEncoder(ILibwebp library)
        {
            this.disposed = false;
            this.webp = (Libwebp)library;
            this.webp.IncreaseReferenceCount();
            this.encoderConfig = new WebPConfig();
        }

        public ref WebPConfig Config => ref encoderConfig;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WebpImageEncoder()
        {
            this.Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed) return;
            this.disposed = true;
            ref var o = ref encoderConfig;
            //if (o.u.RGBA.rgba != IntPtr.Zero)
            //{
            //    this.webp.WebPFree(ref o);
            //}
            this.webp.DecreaseReferenceCount();
        }
    }
}
