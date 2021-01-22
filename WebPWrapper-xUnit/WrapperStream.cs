using System;
using System.IO;
using WebPWrapper;

namespace WebPWrapper_xUnit
{
    /// <summary>Actually a <seealso cref="FileStream"/> but coming with <seealso cref="IOutputStream"/> interface.</summary>
    class WrapperStream : FileStream, IOutputStream
    {
        public WrapperStream(string path) : base(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read) { }
    }
}
