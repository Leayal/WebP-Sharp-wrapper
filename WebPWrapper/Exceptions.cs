using System;
using WebPWrapper.LowLevel;

namespace WebPWrapper
{
    /// <summary>Represents errors that occur during webp encoding operation.</summary>
    public class WebpEncodeException : Exception
    {
        /// <summary>The error code of the operation.</summary>
        public WebPEncodingError ErrorCode { get; }

        /// <summary>Initializes a new instance of the <see cref="WebpEncodeException"/> class with a specified error code and error message.</summary>
        /// <param name="code">The error code of the operation.</param>
        /// <param name="message">The error message of the operation.</param>
        public WebpEncodeException(WebPEncodingError code, string message) : base(message)
        {
            this.ErrorCode = code;
        }

        /// <summary>Initializes a new instance of the <see cref="WebpEncodeException"/> class with a specified error code.</summary>
        /// <param name="code">The error code of the operation.</param>
        public WebpEncodeException(WebPEncodingError code) : base()
        {
            this.ErrorCode = code;
        }
    }

    /// <summary>Represents errors that occur during webp decoding operation.</summary>
    public class WebpDecodeException : Exception
    {
        /// <summary>The error code of the operation.</summary>
        public VP8StatusCode ErrorCode { get; }

        /// <summary>Initializes a new instance of the <see cref="WebpDecodeException"/> class with a specified error code and error message.</summary>
        /// <param name="code">The error code of the operation.</param>
        /// <param name="message">The error message of the operation.</param>
        public WebpDecodeException(VP8StatusCode code, string message) : base(message)
        {
            this.ErrorCode = code;
        }

        /// <summary>Initializes a new instance of the <see cref="WebpDecodeException"/> class with a specified error code.</summary>
        /// <param name="code">The error code of the operation.</param>
        public WebpDecodeException(VP8StatusCode code) : base()
        {
            this.ErrorCode = code;
        }
    }
}
