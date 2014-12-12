using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Crypto;
    using SharpTools.Functional;
    using SharpTools.Configuration.Errors;

    public class JsonConfigProvider<T> : BaseConfigProvider<T>
        where T : class, IConfig<T>, new()
    {
        private static readonly string _defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config.json");

        private readonly JsonSerializerSettings _settings;
        private readonly Func<string, Either<ParseConfigError<T>, T>> _configReader;

        public JsonConfigProvider() : base()
        {
            // Initialize default json serializer settings
            var settings   = GetDefaultSettings();
            var converters = GetDefaultConverters();
            foreach (var converter in converters)
                settings.Converters.Add(converter);

            _settings = settings;
            _configReader = Function.Compose((string s) => File.ReadAllText(s), Parse);
        }

        public JsonConfigProvider(JsonSerializerSettings settings) : base()
        {
            _settings = settings;
            _configReader = Function.Compose((string s) => File.ReadAllText(s), Parse);
        }

        public override bool IsInitialized()
        {
            return new FileInfo(_defaultPath).Exists;
        }

        public override void Initialize()
        {
            var defaultConfig = new T();
            Save(defaultConfig);
        }

        public override T Read()
        {
            return Read(_defaultPath);
        }

        public override T Read(string source)
        {
            if (!File.Exists(source))
                return new T();

            return _configReader(source).Case(
                error  => { throw new ConfigException(error.Message); },
                config => config
            );
        }

        public override Either<ParseConfigError<T>, T> Parse(string config)
        {
            var deserialized = JsonConvert.DeserializeObject<T>(config, _settings);

            var encryptedProperties = GetPropertiesToEncrypt();
            foreach (var prop in encryptedProperties)
            {
                if (!prop.PropertyType.Equals(typeof(string)))
                    return new ParseConfigError<T>(
                        typeof(JsonConfigProvider<T>).Name,
                        config,
                        "The Encrypt attribute cannot be applied to non-string properties."
                    );

                // Decrypt the value
                var value     = prop.GetValue(deserialized);
                var decrypted = Crypto.Decrypt(value as string);
                prop.SetValue(deserialized, decrypted);
            }

            return deserialized;
        }

        public override void Save(T config)
        {
            Save(config, _defaultPath);
        }

        public override void Save(T config, string source)
        {
            var jobj = TokenizeAndEncrypt(config, _settings, Crypto);

            // Write the json to the output stream
            using (var fs         = File.Create(source))
            using (var writer     = new StreamWriter(fs, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = _settings.Formatting;
                jobj.WriteTo(jsonWriter);
            }
        }

        public override string Serialize(T config)
        {
            string json;
            var jobj = TokenizeAndEncrypt(config, _settings, Crypto);

            using (var ms         = new MemoryStream())
            using (var writer     = new StreamWriter(ms, Encoding.UTF8, 4096, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = _settings.Formatting;
                jobj.WriteTo(jsonWriter);
                json = Encoding.UTF8.GetString(ms.ToArray());
            }
            return json;
        }

        internal static JObject TokenizeAndEncrypt(T config, JsonSerializerSettings settings, CryptoProvider crypto)
        {
            // Generate the tokenized json for the config
            var serializer = JsonSerializer.Create(settings);
            var jobj       = JObject.FromObject(config, serializer);
            // Encrypt the values of all EncryptAttribute-decorated properties
            var encryptedProperties = GetPropertiesToEncrypt();
            foreach (var prop in encryptedProperties)
            {
                if (!prop.PropertyType.Equals(typeof(string)))
                    throw new EncryptConfigException("The Encrypt attribute cannot be applied to non-string properties.");

                var jprop     = jobj.Property(prop.Name);
                var val       = jprop.Value as JValue;
                var encrypted = crypto.Encrypt(val.Value as string);
                val.Value     = encrypted;
                jprop.Value   = val;
            }

            return jobj;
        }

        internal static JsonSerializerSettings GetDefaultSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
            settings.FloatFormatHandling = FloatFormatHandling.String;
            settings.FloatParseHandling = FloatParseHandling.Double;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Error;

            return settings;
        }

        internal static JsonConverter[] GetDefaultConverters()
        {
            return new JsonConverter[]
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter(),
                new RegexConverter(),
                new KeyValuePairConverter(),
                new ExpandoObjectConverter(),
            };
        }
    }
}
