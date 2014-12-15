using System;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpTools.Test.Configuration
{
    using SharpTools.Configuration.Providers;

    [TestClass]
    public class AppSettingsConfigProviderTest
    {
        private const string ASSET_DIR = @"Assets\Configuration";
        private const string TEST_CONFIG = @"SampleApp.config";

        [TestMethod]
        [DeploymentItem(ASSET_DIR, ASSET_DIR)]
        public void CanReadConfigFromPath()
        {
            var expected = new MyTestConfig();
            expected.LogLevel = MyTestConfig.LogLevels.Warn;
            expected.MaxLogFileSizeMb = 42;
            expected.EndOfYear = new DateTime(2014, 12, 31);
            expected.ValidUsernameRegex = new Regex(@"^[A-Za-z]{1}[A-Za-z-_0-9]{7,}$");
            expected.ConnectionString = "Server=somehost;Database=goldmine;User Id=sa;Password=supersecret";

            var provider = new AppSettingsConfigProvider<MyTestConfig>();

            var configPath = Path.Combine(ASSET_DIR, TEST_CONFIG);
            var config = provider.Read(configPath);

            Assert.AreEqual(expected.LogLevel, config.LogLevel);
            Assert.AreEqual(expected.MaxLogFileSizeMb, config.MaxLogFileSizeMb);
            Assert.AreEqual(expected.EndOfYear, config.EndOfYear);
            Assert.AreEqual(expected.ValidUsernameRegex.ToString(), config.ValidUsernameRegex.ToString());
            Assert.AreEqual(expected.ConnectionString, config.ConnectionString);
        }

        [TestMethod]
        public void CanSerializeAndDeserializeConfig()
        {
            var config = new MyTestConfig();
            config.LogLevel = MyTestConfig.LogLevels.Warn;
            config.MaxLogFileSizeMb = 42;
            config.EndOfYear = new DateTime(2014, 12, 31);
            config.ValidUsernameRegex = new Regex(@"^[A-Za-z]{1}[A-Za-z-_0-9]{7,}$");
            config.ConnectionString = "Server=somehost;Database=goldmine;User Id=sa;Password=supersecret";

            var provider = new AppSettingsConfigProvider<MyTestConfig>();
            var xml = provider.Serialize(config);

            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));

            var result = provider.Parse(xml);

            result.Case(
                e => Assert.Fail(e.Message),
                deserialized =>
                {
                    Assert.IsNotNull(deserialized);

                    Assert.AreEqual(config.LogLevel, deserialized.LogLevel);
                    Assert.AreEqual(config.MaxLogFileSizeMb, deserialized.MaxLogFileSizeMb);
                    Assert.AreEqual(config.EndOfYear, deserialized.EndOfYear);
                    Assert.AreEqual(config.ValidUsernameRegex.ToString(), deserialized.ValidUsernameRegex.ToString());
                    Assert.AreEqual(config.ConnectionString, deserialized.ConnectionString);
                });
        }
    }
}
