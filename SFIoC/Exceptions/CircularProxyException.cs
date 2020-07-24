using System;

namespace SF.IoC
{
    public class CircularProxyException : Exception
    {
        public CircularProxyException(string message) : base(message)
        {
        }
    }
}
