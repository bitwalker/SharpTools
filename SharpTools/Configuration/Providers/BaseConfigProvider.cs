using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Crypto;
    using SharpTools.Functional;
    using SharpTools.Configuration.Errors;
    using SharpTools.Configuration.Attributes;

    public abstract class BaseConfigProvider<T> : IProvider<T>, IDisposable
        where T : class, IConfig<T>, new()
    {
        protected CryptoProvider Crypto { get; private set; }

        protected BaseConfigProvider()
        {
            if (GetPropertiesToEncrypt().Length > 0)
            {
                // Load the encryption key
                var key = new T().GetEncryptionKey();
                if (string.IsNullOrWhiteSpace(key))
                    throw new EncryptConfigException("Invalid encryption key! Must not be null or empty.");

                Crypto = new CryptoProvider(key);
            }
        }

        public abstract bool IsInitialized();
        public abstract void Initialize();
        public abstract T Read();
        public abstract T Read(string source);
        public abstract Either<ParseConfigError<T>, T> Parse(string config);
        public abstract void Save(T config);
        public abstract void Save(T config, string source);
        public abstract string Serialize(T config);

        protected static PropertyInfo[] GetConfigProperties()
        {
            return typeof (T).GetProperties()
                .Where(p => p.CanWrite)
                .Where(p => !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)))
                .Where(p => !Attribute.IsDefined(p, typeof(XmlIgnoreAttribute)))
                .ToArray();
        }

        protected static PropertyInfo[] GetPropertiesToEncrypt()
        {
            return GetConfigProperties()
                .Where(p => Attribute.IsDefined(p, typeof (EncryptAttribute)))
                .ToArray();
        }

        public virtual void Dispose()
        {
            if (Crypto != null)
            {
                Crypto.Dispose();
                Crypto = null;
            }
        }
    }
}
