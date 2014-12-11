using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Crypto;
    using SharpTools.Configuration.Attributes;

    public class JsonConfigProvider<T> : IProvider<T>
        where T : class, IConfig<T>, new()
    {
        private readonly Stream _output;
        private readonly JsonSerializerSettings _settings;
        private CryptoProvider _crypto;

        public Stream Output { get { return _output; } }

        public JsonConfigProvider() : this(new MemoryStream())
        {
        }

        public JsonConfigProvider(Stream output)
        {
            if (output == null)
                throw new ArgumentNullException("output", "Output stream cannot be null!");

            // Initialize default json serializer settings
            var settings   = GetDefaultSettings();
            var converters = GetDefaultConverters();
            foreach (var converter in converters)
                settings.Converters.Add(converter);

            _output   = output;
            _settings = settings;
        }

        public JsonConfigProvider(Stream output, JsonSerializerSettings settings)
        {
            if (output == null)
                throw new ArgumentNullException("output", "Output stream cannot be null!");

            _output   = output;
            _settings = settings;
        }

        /// <summary>
        /// NOTE: This simply returns a default config instance.
        /// </summary>
        public T Read()
        {
            return new T();
        }

        public Task<T> ReadAsync()
        {
            return Task.FromResult(Read());
        }

        public T Read(string config)
        {
            var deserialized = JsonConvert.DeserializeObject<T>(config, _settings);

            var configProperties = typeof (T).GetProperties()
                .Where(p => p.CanWrite)
                .Where(p => !p.GetIndexParameters().Any())
                .Where(p => Attribute.IsDefined(p, typeof(EncryptAttribute)));

            var encryptionKey = new T().GetEncryptionKey();
            InitializeCrypto(encryptionKey);

            foreach (var prop in configProperties)
            {
                if (!prop.PropertyType.Equals(typeof(string)))
                    throw new ReadConfigException("The Encrypt attribute cannot be applied to non-string properties.");

                // Decrypt the value if this is an encrypted property
                var value     = prop.GetValue(deserialized);
                var decrypted = _crypto.Decrypt(value as string);
                prop.SetValue(deserialized, decrypted);
            }

            return deserialized;
        }

        public Task<T> ReadAsync(string config)
        {
            return Task.FromResult(Read(config));
        }

        public void Save(T config)
        {
            InitializeCrypto(config.GetEncryptionKey());

            // Generate the tokenized json for the config
            var serializer = JsonSerializer.Create(_settings);
            var jobj       = JObject.FromObject(config, serializer);

            // Load the encryption key
            var encryptionKey = config.GetEncryptionKey();
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new EncryptConfigException("Invalid encryption key! Must not be null or empty.");

            // Encrypt the values of all EncryptAttribute-decorated properties
            var encryptedProperties = GetPropertiesToEncrypt();
            foreach (var prop in encryptedProperties)
            {
                if (!prop.PropertyType.Equals(typeof(string)))
                    throw new WriteConfigException("The Encrypt attribute cannot be applied to non-string properties.");

                var jprop     = jobj.Property(prop.Name);
                var val       = jprop.Value as JValue;
                var encrypted = _crypto.Encrypt(val.Value as string);
                val.Value     = encrypted;
                jprop.Value   = val;
            }

            // Write the json to the output stream
            using (var writer     = new StreamWriter(_output, Encoding.UTF8, 4096, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = _settings.Formatting;
                jobj.WriteTo(jsonWriter);
            }

            // Seek back to the beginning for reading
            _output.Seek(0, SeekOrigin.Begin);
        }

        public Task SaveAsync(T config)
        {
            return Task.Run(() => Save(config));
        }

        private PropertyInfo[] GetPropertiesToEncrypt()
        {
            return typeof (T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof (EncryptAttribute)))
                .ToArray();
        }

        private void InitializeCrypto(string password)
        {
            // Make sure the crypto provider is reset for this configuration
            if (_crypto != null)
            {
                _crypto.Dispose();
                _crypto = null;
            }
            _crypto = new CryptoProvider(password);
        }

        private JsonSerializerSettings GetDefaultSettings()
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

        private JsonConverter[] GetDefaultConverters()
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
