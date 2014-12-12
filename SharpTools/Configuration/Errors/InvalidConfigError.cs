using System;

namespace SharpTools.Configuration.Errors
{
    public class InvalidConfigError<T> where T : class, IConfig<T>, new()
    {
        public T Config { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public InvalidConfigError(string message)
        {
            Message = message;
        }

        public InvalidConfigError(string message, params object[] args)
        {
            Message = string.Format(message, args);
        }

        public InvalidConfigError(T config, string message)
        {
            Config = config;
            Message = message;
        }

        public InvalidConfigError(T config, string message, params object[] args)
        {
            Config = config;
            Message = string.Format(message, args);
        }

        public InvalidConfigError(T config, Exception ex, string message)
        {
            Config = config;
            Message = message;
            Exception = ex;
        }

        public InvalidConfigError(T config, Exception ex, string message, params object[] args)
        {
            Config = config;
            Message = string.Format(message, args);
            Exception = ex;
        }
    }
}
