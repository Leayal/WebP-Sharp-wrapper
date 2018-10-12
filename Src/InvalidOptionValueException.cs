using System;

namespace WebPWrapper.WPF
{
    public class InvalidOptionValueException : Exception
    {
        internal InvalidOptionValueException() : this("Invalid option") { }

        internal InvalidOptionValueException(string message) : base(message) { }
    }
}
