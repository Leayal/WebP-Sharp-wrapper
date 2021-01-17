using System;

namespace WebPWrapper.LowLevel
{
    /// <summary>Encoding error conditions.</summary>
    public enum WebPEncodingError
    {
        /// <summary>No error.</summary>
        VP8_ENC_OK = 0,
        /// <summary>Memory error allocating objects.</summary>
        VP8_ENC_ERROR_OUT_OF_MEMORY,
        /// <summary>Memory error while flushing bits.</summary>
        VP8_ENC_ERROR_BITSTREAM_OUT_OF_MEMORY,
        /// <summary>A  pointer parameter is NULL.</summary>
        VP8_ENC_ERROR_NULL_PARAMETER,
        /// <summary>Configuration is invalid.</summary>
        VP8_ENC_ERROR_INVALID_CONFIGURATION,
        /// <summary>Picture has invalid width/height.</summary>
        VP8_ENC_ERROR_BAD_DIMENSION,
        /// <summary>Partition is bigger than 512k.</summary>
        VP8_ENC_ERROR_PARTITION0_OVERFLOW,
        /// <summary>Partition is bigger than 16M.</summary>
        VP8_ENC_ERROR_PARTITION_OVERFLOW,
        /// <summary>Error while flushing bytes.</summary>
        VP8_ENC_ERROR_BAD_WRITE,
        /// <summary>File is bigger than 4G.</summary>
        VP8_ENC_ERROR_FILE_TOO_BIG,
        /// <summary>Abort request by user.</summary>
        VP8_ENC_ERROR_USER_ABORT,
        /// <summary>List terminator. always last.</summary>
        VP8_ENC_ERROR_LAST,
    };

    /// <summary>The returning value in webp picture writer to determine whether the writer should continue or abort.</summary>
    public enum WEBP_WRITER_RESPONSE : int
    {
        /// <summary>Abort the operation.</summary>
        ABORT = 0,
        /// <summary>Continue the operation.</summary>
        CONTINUE
    };

    /// <summary>Enumeration of the status codes.</summary>
    public enum VP8StatusCode
    {
        /// <summary>No error.</summary>
        VP8_STATUS_OK = 0,
        /// <summary>Memory error allocating objects.</summary>
        VP8_STATUS_OUT_OF_MEMORY,
        VP8_STATUS_INVALID_PARAM,
        VP8_STATUS_BITSTREAM_ERROR,
        /// <summary>Configuration is invalid.</summary>
        VP8_STATUS_UNSUPPORTED_FEATURE,
        VP8_STATUS_SUSPENDED,
        /// <summary>Abort request by user.</summary>
        VP8_STATUS_USER_ABORT,
        VP8_STATUS_NOT_ENOUGH_DATA,
    };

    /// <summary>
    /// Decoding states. State normally flows as:
    /// WEBP_HEADER->VP8_HEADER->VP8_PARTS0->VP8_DATA->DONE for a lossy image, and
    /// WEBP_HEADER->VP8L_HEADER->VP8L_DATA->DONE for a lossless image.
    /// If there is any error the decoder goes into state ERROR.
    /// </summary>
    enum DecState
    {
        /// <summary>All the data before that of the VP8/VP8L chunk.</summary>
        STATE_WEBP_HEADER,
        /// <summary>The VP8 Lossy Frame header (within the VP8 chunk)</summary>
        STATE_VP8_HEADER,
        STATE_VP8_PARTS0,
        STATE_VP8_DATA,
        /// <summary>The VP8 Lossless Frame header (within the VP8L chunk)</summary>
        STATE_VP8L_HEADER,
        STATE_VP8L_DATA,
        STATE_DONE,
        STATE_ERROR
    };

    enum MemBufferMode
    {
        MEM_MODE_NONE = 0,
        MEM_MODE_APPEND,
        MEM_MODE_MAP
    };

    /// <summary>
    /// Encoder's colorspace bitwise flags.
    /// </summary>
    [Flags]
    public enum WebPEncCSP
    {
        /// <summary>chroma sampling 4:2:0</summary>
        WEBP_YUV420 = 0,
        /// <summary>chroma sampling 4:2:0 with alpha channel</summary>
        WEBP_YUV420A = 4,
        /// <summary>bit-mask to get the UV sampling factors</summary>
        WEBP_CSP_UV_MASK = 3,
        /// <summary>bit that is set if alpha is present</summary>
        WEBP_CSP_ALPHA_BIT = 4
    }
}
