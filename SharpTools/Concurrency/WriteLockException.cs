using System;

namespace SharpTools.Concurrency
{
    public class WriteLockException : Exception
    {
        public WriteLockException(string message)
            : base(message) {}

        public WriteLockException(string format, params object[] args)
            : base(string.Format(format, args)) {}

        public WriteLockException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
