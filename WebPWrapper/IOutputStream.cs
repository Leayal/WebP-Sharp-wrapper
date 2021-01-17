using System;
using System.Collections.Generic;
using System.Text;

namespace WebPWrapper
{
    /// <summary>Interface for output stream to write data to.</summary>
    /// <remarks>The instance must be writable when implementing this interface.</remarks>
    public interface IOutputStream : IDisposable
    {
        /// <summary>Write data buffer to the stream</summary>
        /// <param name="buffer">The data buffer to write data from.</param>
        void Write(ReadOnlySpan<byte> buffer);

        /// <summary>Attempts to flush all the buffered written data to the stream.</summary>
        /// <remarks>Not really necessary to call</remarks>
        void Flush();
    }
}
