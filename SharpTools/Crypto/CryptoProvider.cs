using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace SharpTools.Crypto
{
    /// <summary>
    /// This class provides a simplified API for symmetric encryption/decryption of text.
    ///
    /// By default, it will use Salsa20 for the symmetric encryption algorithmFactory, and SHA512
    /// for hashing, but this can be configured by providing a CryptoAlgorithmFactory instance to
    /// the appropriate constructor.
    ///
    /// The key for the algorithmFactory will be derived from the password provided + a salt, which
    /// is derived by hashing the password once, then rehashing the hash iteratively 6 more
    /// times. This is not perfect, but will provide an extra layer of security over just the
    /// unsalted password by ensuring there is at least 128 bits available for deriving the key,
    /// and 512 if you use the defaults.
    /// </summary>
    public sealed class CryptoProvider
    {
        /// <summary>
        /// Recommended number of iterations to perform when hashing the provided password
        /// for the key derivation process. It is recommended to make this as high as you can
        /// tolerate.
        ///
        /// Some discussion on this topic is available at the link below:
        /// http://security.stackexchange.com/questions/3959/recommended-of-iterations-when-using-pkbdf2-sha256
        /// </summary>
        public const int RFC2898_ITERATIONS = 20 * 1000;

        private SymmetricAlgorithm _algorithm;
        private CryptoAlgorithmFactory _algorithmsFactory;

        /// <summary>
        /// Creates a new CryptoProvider instance using Salsa20/SHA512.
        /// </summary>
        /// <param name="password">The password to use when deriving the encryption key.</param>
        public CryptoProvider(string password)
            : this(password, RFC2898_ITERATIONS, CryptoAlgorithmFactory.Salsa20) { }

        /// <summary>
        /// Creates a new CryptoProvider instance, using the provided CryptoAlgorithmFactory.
        /// </summary>
        /// <param name="password">The password to use when deriving the encryption key.</param>
        /// <param name="algorithmFactory">The CryptoAlgorithmFactory instance which defines what algorithms to use when encrypting/hashing.</param>
        public CryptoProvider(string password, CryptoAlgorithmFactory algorithmFactory)
            : this(password, RFC2898_ITERATIONS, algorithmFactory) { }

        /// <summary>
        /// Creates a new CryptoProvider instance, using the provided CryptoAlgorithmFactory, and
        /// a custom number of iterations to perform when hashing the password for deriving the key.
        /// </summary>
        /// <param name="password">The password to use when deriving the encryption key.</param>
        /// <param name="iterations">The number of iterations to perform when hashing the password.</param>
        /// <param name="algorithmFactory">The CryptoAlgorithmFactory instance which defines what algorithms to use when encrypting/hashing.</param>
        public CryptoProvider(string password, int iterations, CryptoAlgorithmFactory algorithmFactory)
        {
            // Create algorithmFactory instance
            _algorithmsFactory = algorithmFactory;
            _algorithm  = _algorithmsFactory.CreateSymmetric();

            // Derive the key for this algorithmFactory
            using (var keyInfo = DeriveKey(password, iterations))
            {
                // Prepare the algorithmFactory with the derived key
                _algorithm.Key = keyInfo.Key;
                _algorithm.IV  = keyInfo.IV;
            }
        }

        /// <summary>
        /// Encrypts the provided value, and returns the ciphertext as a base64-encoded string.
        /// </summary>
        /// <param name="value">The string to encrypt</param>
        /// <returns>A base64-encoded string</returns>
        public string Encrypt(string value)
        {
            using (var encryptor    = _algorithm.CreateEncryptor())
            using (var result       = new MemoryStream())
            using (var cryptoStream = new CryptoStream(result, encryptor, CryptoStreamMode.Write))
            {
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(value);
                }

                var bytes = result.ToArray();
                return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
            }
        }

        /// <summary>
        /// Decrypts the provided ciphertext, and returns the decrypted value as a string.
        /// </summary>
        /// <param name="ciphertext">The base64-encoded ciphertext to decrypt.</param>
        /// <returns>A decrypted string</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">
        /// May occur if the wrong password or algorithmFactory is used when decrypting.
        /// </exception>
        public string Decrypt(string ciphertext)
        {
            var bytes = Convert.FromBase64String(ciphertext);

            string decrypted;
            using (var decryptor    = _algorithm.CreateDecryptor())
            using (var result       = new MemoryStream(bytes))
            using (var cryptoStream = new CryptoStream(result, decryptor, CryptoStreamMode.Read))
            {
                using (var reader = new StreamReader(cryptoStream))
                {
                    decrypted = reader.ReadToEnd();
                }
            }

            return decrypted;
        }

        /// <summary>
        /// Derive key material from a provided password and crypto service provider.
        /// </summary>
        /// <param name="password">The password from which the key material will be derived</param>
        /// <param name="iterations">The number of iterations to perform when hashing the password.</param>
        /// <returns>The derived key info</returns>
        private KeyInfo DeriveKey(string password, int iterations)
        {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var saltBytes     = DeriveSalt(passwordBytes);

            // Derive the key using PBKDF-2/HMAC-SHA1
            byte[] key;
            byte[] iv;
            using (var deriveBytes = new Rfc2898DeriveBytes(passwordBytes, saltBytes, iterations))
            {
                key = deriveBytes.GetBytes(_algorithm.KeySize / 8);
                iv  = deriveBytes.GetBytes(_algorithm.IV.Length);
            }

            ClearKeyMaterial(passwordBytes);
            ClearKeyMaterial(saltBytes);

            return KeyInfo.Create(key, iv);
        }

        /// <summary>
        /// Derives a salt from some input.
        /// </summary>
        /// <param name="input">The input bytes to derive the salt from</param>
        private byte[] DeriveSalt(byte[] input)
        {
            // The number of re-hashes to perform
            var nRehashes = 6;
            // Hash with the provided hashing algorithmFactory
            using (var hasher = _algorithmsFactory.CreateHash())
            {
                hasher.Initialize();

                // Hash the input, then rehash `n` times.
                var bytes = hasher.ComputeHash(input);
                for (var i = 0; i < nRehashes; i++)
                {
                    bytes = hasher.ComputeHash(bytes);
                }

                return bytes;
            }
        }

        /// <summary>
        /// Zeroes out a byte[] buffer's contents to remove key derivation material from memory
        /// </summary>
        /// <param name="buffer">The buffer to clear</param>
        private static void ClearKeyMaterial(byte[] buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
        }

        private struct KeyInfo : IDisposable
        {
            public byte[] Key { get; set; }
            public byte[] IV { get; set; }

            public static KeyInfo Create(byte[] key, byte[] iv)
            {
                return new KeyInfo() { Key = key, IV = iv };
            }

            public void Dispose()
            {
                ClearKeyMaterial(Key);
                ClearKeyMaterial(IV);
            }
        }
    }
}
