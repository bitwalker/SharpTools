using System;

namespace SharpTools.Configuration
{
    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message) {}
        public ConfigException(string message, Exception inner) : base(message, inner) {}
    }
}
