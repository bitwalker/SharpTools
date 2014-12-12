using System;

namespace SharpTools.Configuration.Errors
{
    public class ParseConfigError<T> where T : class, IConfig<T>, new()
    {
        public string ProviderName { get; set; }
        public string Config { get; set; }
        public string Message { get; set; }

        public ParseConfigError(string providerName, string config, string message = "The provided configuration is invalid.")
        {
            ProviderName = providerName;
            Config = config;
            Message = message;
        }
    }
}
