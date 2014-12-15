using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml.XPath;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Functional;
    using SharpTools.Configuration.Errors;

    public class AppSettingsConfigProvider<T> : BaseConfigProvider<T>
        where T : class, IConfig<T>, new()
    {
        private const string ADD_SETTING_FORMAT = "<add key=\"{0}\" value=\"{1}\" />";

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
            return Read(null);
        }

        public override T Read(string source)
        {
            var appSettings = GetAppSettings(source);
            return ConvertSettingsToConfig(appSettings);
        }

        public override Either<ParseConfigError<T>, T> Parse(string config)
        {
            if (string.IsNullOrWhiteSpace(config))
                throw new ArgumentException("appSettings xml cannot be null or empty.", "config");

            try
            {
                var xml = XElement.Parse(config);
                if (xml == null)
                    throw new Exception("Unable to parse provided appSettings configuration.");

                var appSettings = xml.XPathSelectElement("//appSettings");
                if (appSettings == null)
                    throw new Exception("Invalid config. The provided xml does not contain an appSettings element.");

                var settings = appSettings
                    .Descendants("add")
                    .ToDictionary(
                        x => x.Attribute("key").Value,
                        x => x.Attribute("value").Value
                    );

                return ConvertSettingsToConfig(settings);
            }
            catch (Exception ex)
            {
                return new ParseConfigError<T>(typeof (AppSettingsConfigProvider<T>).Name, config, ex.Message);
            }
        }

        public override void Save(T config)
        {
            Save(config, null);
        }

        public override void Save(T config, string source)
        {
            // Tokenize as a JSON object
            var jobj = JsonConfigProvider<T>.TokenizeAndEncrypt(config, _settings, Crypto);

            // Create a dictionary of properties to stringified values
            var settings   = new Dictionary<string, string>();
            foreach (JToken token in jobj.Descendants())
            {
                if (token.HasValues && token.Type == JTokenType.Property)
                {
                    var prop   = token as JProperty;
                    var values = token.Values().ToArray();
                    if (values.Length == 1)
                    {
                        var value = prop.Value
                            .ToString(Formatting.None)
                            .Replace("\"", "\\\"");
                        settings.Add(prop.Path, value);
                    }
                    else if (token.First.Type == JTokenType.Array)
                    {

                        var array = token.First
                            .ToString(Newtonsoft.Json.Formatting.None)
                            .Replace("\"", "\\\"");
                        settings.Add(prop.Path, array);
                    }
                }
            }

            PersistAppSettings(settings, source);
        }

        public override string Serialize(T config)
        {
            // Tokenize as a JSON object
            var jobj = JsonConfigProvider<T>.TokenizeAndEncrypt(config, _settings, Crypto);

            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.AppendLine("<configuration>");
            builder.AppendLine("  <appSettings>");
            foreach (JToken token in jobj.Descendants())
            {
                if (token.HasValues && token.Type == JTokenType.Property)
                {
                    var prop   = token as JProperty;
                    var values = token.Values().ToArray();
                    if (values.Length == 1)
                    {
                        var value = prop
                            .ToString(Formatting.None)
                            .Replace("\"", "\\\"");
                        builder.AppendFormat("    " + ADD_SETTING_FORMAT, prop.Path, prop.Value.ToString());
                        builder.AppendLine();
                    }
                    else if (token.First.Type == JTokenType.Array)
                    {
                        var array = token.First
                            .ToString(Newtonsoft.Json.Formatting.None)
                            .Replace("\"", "\\\"");
                        builder.AppendFormat("    " + ADD_SETTING_FORMAT, prop.Path, array);
                        builder.AppendLine();
                    }
                }
            }
            builder.AppendLine("  </appSettings>");
            builder.AppendLine("</configuration>");
            return builder.ToString();
        }

        private Dictionary<string, string> GetAppSettings(string configPath = "")
        {
            var prefix      = typeof (T).Name;
            var appSettings = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(configPath))
            {
                string expandedPath;

                var configMap = new ExeConfigurationFileMap();
                if (!Path.IsPathRooted(configPath))
                {
                    expandedPath = Path.GetFullPath(configPath);
                    configMap.ExeConfigFilename = Path.GetFullPath(configPath);
                }
                else
                {
                    expandedPath = configPath;
                    configMap.ExeConfigFilename = configPath;
                }

                if (!File.Exists(expandedPath))
                    throw new FileNotFoundException("The provided config file path does not exist!", expandedPath);

                var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                var keys = config.AppSettings.Settings
                    .AllKeys
                    .Where(key => key.StartsWith(prefix + "."))
                    .ToArray();
                var result = new Dictionary<string, string>();
                foreach (var key in keys)
                {
                    var setting = config.AppSettings.Settings[key];
                    appSettings.Add(key.Replace(prefix + ".", ""), setting.Value);
                }
            }
            else
            {
                var keys = ConfigurationManager.AppSettings
                    .AllKeys
                    .Where(key => key.StartsWith(prefix + "."))
                    .ToArray();

                foreach (var key in keys)
                    appSettings.Add(key.Replace(prefix + ".", ""), ConfigurationManager.AppSettings[key]);
            }

            return appSettings;
        }

        private void PersistAppSettings(IDictionary<string, string> settings, string configPath = "")
        {
            var prefix = typeof (T).Name;

            System.Configuration.Configuration config;
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                string expandedPath;

                var configMap = new ExeConfigurationFileMap();
                if (!Path.IsPathRooted(configPath))
                {
                    expandedPath = Path.GetFullPath(configPath);
                    configMap.ExeConfigFilename = Path.GetFullPath(configPath);
                }
                else
                {
                    expandedPath = configPath;
                    configMap.ExeConfigFilename = configPath;
                }

                if (!File.Exists(expandedPath))
                    throw new FileNotFoundException("The provided config file path does not exist!", expandedPath);

                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            }
            else
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            var keys = settings.Keys;
            foreach (var key in keys)
            {
                var prefixedKey = prefix + "." + key;
                var value       = settings[key];

                config.AppSettings.Settings.Remove(prefixedKey);
                config.AppSettings.Settings.Add(prefixedKey, value);
            }
            config.Save(ConfigurationSaveMode.Modified);
        }

        private T ConvertSettingsToConfig(IDictionary<string, string> appSettings)
        {
            var dict     = appSettings.ToDictionary(k => k.Key, v => (object) v.Value);
            dynamic obj  = BuildObjectFromDictionary(dict);
            var json     = JsonConvert.SerializeObject((object) obj, Formatting.None, _settings);
            var provider = new JsonConfigProvider<T>(_settings);
            return provider.Parse(json).Case(
                e      => { throw new Exception(e.Message); },
                parsed => parsed
            );
        }

        private static dynamic BuildObjectFromDictionary(IDictionary<string, object> props, dynamic obj = null)
        {
            if (obj == null)
                obj = new System.Dynamic.ExpandoObject();
            foreach (var prop in props)
            {
                var levels = prop.Key.Split(new[] { '.' }, 2);
                if (levels.Length == 1)
                {
                    ((IDictionary<string, object>) obj).Add(prop.Key, prop.Value);
                }
                else
                {
                    var propName = levels[0];
                    var adding 	 = new Dictionary<string, object>() { { levels[1], prop.Value } };
                    if (((IDictionary<string, object>) obj).ContainsKey(propName))
                    {
                        var existing = ((IDictionary<string, object>) obj)[propName];
                        ((IDictionary<string, object>) obj)[propName] = BuildObjectFromDictionary(adding, existing);
                    }
                    else
                    {
                        var value = BuildObjectFromDictionary(adding);
                        ((IDictionary<string, object>) obj).Add(propName, value);
                    }
                }
            }
            return obj;
        }
    }
}
