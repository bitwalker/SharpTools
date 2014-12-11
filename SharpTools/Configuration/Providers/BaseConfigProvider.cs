using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SharpTools.Configuration.Providers
{
    using SharpTools.Crypto;
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

        public abstract T Read();
        public abstract Task<T> ReadAsync();
        public abstract T Read(string config);
        public abstract Task<T> ReadAsync(string config);
        public abstract void Save(T config);
        public abstract Task SaveAsync(T config);

        protected PropertyInfo[] GetConfigProperties()
        {
            return typeof (T).GetProperties()
                .Where(p => p.CanWrite)
                .Where(p => !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)))
                .Where(p => !Attribute.IsDefined(p, typeof(XmlIgnoreAttribute)))
                .ToArray();
        }

        protected PropertyInfo[] GetPropertiesToEncrypt()
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
