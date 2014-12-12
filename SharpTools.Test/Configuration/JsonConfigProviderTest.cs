using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpTools.Test.Configuration
{
    using SharpTools.Configuration;
    using SharpTools.Configuration.Attributes;
    using SharpTools.Configuration.Providers;

    [TestClass]
    public class JsonConfigProviderTest
    {
        [TestMethod]
        public void CanSerializeWithEncryption()
        {
            var config = new MyTestConfig();
            config.LogLevel           = MyTestConfig.LogLevels.Warn;
            config.MaxLogFileSizeMb   = 42;
            config.EndOfYear          = new DateTime(2014, 12, 31);
            config.ValidUsernameRegex = new Regex(@"^[A-Za-z]{1}[A-Za-z-_0-9]{7,}$");
            config.ConnectionString   = "Server=somehost;Database=goldmine;User Id=sa;Password=supersecret";

            var provider = new JsonConfigProvider<MyTestConfig>();
            var json = provider.Serialize(config);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var deserialized = provider.Read(json);

            Assert.IsNotNull(deserialized);

            Assert.AreEqual(config.LogLevel, deserialized.LogLevel);
            Assert.AreEqual(config.MaxLogFileSizeMb, deserialized.MaxLogFileSizeMb);
            Assert.AreEqual(config.EndOfYear, deserialized.EndOfYear);
            Assert.AreEqual(config.ValidUsernameRegex.ToString(), deserialized.ValidUsernameRegex.ToString());
            Assert.AreEqual(config.ConnectionString, deserialized.ConnectionString);
        }

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
}
