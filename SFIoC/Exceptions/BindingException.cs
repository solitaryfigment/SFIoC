using System;

namespace SF.IoC
{
    public class BindingException : Exception
    {
        public BindingException(string message) : base(message)
        {
        }
    }
}