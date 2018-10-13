using System;
using System.Runtime.Serialization;

namespace WebPWrapper.WPF
{
    [Serializable]
    public class InvalidOptionValueException : Exception
    {
        internal InvalidOptionValueException() : this("Invalid option") { }

        internal InvalidOptionValueException(string message) : base(message) { }

        // Constructor needed for serialization 
        // when exception propagates from a remoting server to the client.
        protected InvalidOptionValueException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
