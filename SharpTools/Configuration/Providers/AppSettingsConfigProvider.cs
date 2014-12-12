using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

using Newtonsoft.Json;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Functional;
    using SharpTools.Configuration.Errors;
    using SharpTools.Configuration.Attributes;

    public class AppSettingsConfigProvider<T> : BaseConfigProvider<T>
        where T : class, IConfig<T>, new()
    {
        private JsonSerializerSettings _settings;

        public AppSettingsConfigProvider()
        {
            // Initialize serializer settings
            var settings = JsonConfigProvider<T>.GetDefaultSettings();
            var converters = JsonConfigProvider<T>.GetDefaultConverters();
            foreach (var converter in converters)
                settings.Converters.Add(converter);
            _settings = settings;
        }

        public override bool IsInitialized()
        {
            return true;
        }

        public override void Initialize()
        {
            return;
        }

        public override T Read()
        {
            var appSettings = GetAppSettings();
            var properties  = GetConfigProperties();

            var result = new T();

            foreach (var key in appSettings.Keys)
            {
                var value = appSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var prop = properties.FirstOrDefault(
                    p => p.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)
                );
                if (prop == null)
                    continue;

                if (Attribute.IsDefined(prop, typeof(EncryptAttribute)))
                {
                    var decrypted = Crypto.Decrypt(value);
                    prop.SetValue(result, decrypted);
                }
                else
                {
                    var deserialized = JsonConvert.DeserializeObject(
                        value,
                        prop.PropertyType,
                        _settings
                    );
                    prop.SetValue(result, deserialized);
                }
            }

            return result;
        }

        public override T Read(string source)
        {
            throw new NotImplementedException();
        }

        public override Either<ParseConfigError<T>, T> Parse(string config)
        {
            throw new NotImplementedException();
        }

        public override void Save(T config)
        {
            throw new NotImplementedException();
        }

        public override void Save(T config, string source)
        {
            throw new NotImplementedException();
        }

        public override string Serialize(T config)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, string> GetAppSettings()
        {
            var prefix = typeof (T).Name;
            var keys = ConfigurationManager.AppSettings
                .AllKeys
                .Where(key => key.StartsWith(prefix + "."))
                .ToArray();

            var result = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                result.Add(key.Replace(prefix + ".", ""), ConfigurationManager.AppSettings[key]);
            }

            return result;
        }

        private void PersistAppSettings(IDictionary<string, string> settings)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var prefix = typeof (T).Name;
            var keys = settings.Keys;
            foreach (var key in keys)
            {
                var prefixedKey = prefix + "." + key;
                config.AppSettings.Settings.Remove(prefixedKey);
                config.AppSettings.Settings.Add(prefixedKey, settings[key]);
            }
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
