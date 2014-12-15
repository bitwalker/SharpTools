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
        public void CanSerializeAndDeserializeConfig()
        {
            var expected = new MyTestConfig();
            expected.LogLevel           = MyTestConfig.LogLevels.Warn;
            expected.MaxLogFileSizeMb   = 42;
            expected.EndOfYear          = new DateTime(2014, 12, 31);
            expected.ValidUsernameRegex = new Regex(@"^[A-Za-z]{1}[A-Za-z-_0-9]{7,}$");
            expected.ConnectionString   = "Server=somehost;Database=goldmine;User Id=sa;Password=supersecret";

            var provider = new JsonConfigProvider<MyTestConfig>();
            var json = provider.Serialize(expected);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var deserialized = provider.Parse(json);

            Assert.IsNotNull(deserialized);

            deserialized.Case(
                error => Assert.Fail(error.Message),
                config =>
                {
                    Assert.AreEqual(config.LogLevel, config.LogLevel);
                    Assert.AreEqual(config.MaxLogFileSizeMb, config.MaxLogFileSizeMb);
                    Assert.AreEqual(config.EndOfYear, config.EndOfYear);
                    Assert.AreEqual(config.ValidUsernameRegex.ToString(), config.ValidUsernameRegex.ToString());
                    Assert.AreEqual(config.ConnectionString, config.ConnectionString);
                });
        }
    }
}
