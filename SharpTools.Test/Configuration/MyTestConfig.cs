using System;
using System.Text.RegularExpressions;

using SharpTools.Configuration;
using SharpTools.Configuration.Attributes;

namespace SharpTools.Test.Configuration
{
    public class MyTestConfig : IConfig<MyTestConfig>
    {
        public enum LogLevels
        {
            Debug = 0,
            Info,
            Warn,
            Error
        }

        public LogLevels LogLevel { get; set; }
        public int MaxLogFileSizeMb { get; set; }
        public DateTime EndOfYear { get; set; }
        public Regex ValidUsernameRegex { get; set; }
        [Encrypt]
        public string ConnectionString { get; set; }

        public string GetEncryptionKey()
        {
            return "w^&919dab";
        }
    }
}
