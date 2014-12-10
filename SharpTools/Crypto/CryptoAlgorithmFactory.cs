using System;
using System.Security.Cryptography;

namespace SharpTools.Crypto
{
    using SharpTools.Crypto.Engines;

    /// <summary>
    /// This class provides a type-safe API around available symmetric cryptography
    /// algorithms provided by the .NET framework, as well as the Salsa20 streaming
    /// symmetric block cipher algorithm. It also defines safe default values for the
    /// chosen algorithm (CFB-mode, ISO10126 padding, maximum key size).
    ///
    /// It also pairs the symmetric algorithm with a hash algorithm, as they often go
    /// hand in hand. By default, SHA512 is used, but this can be customized if needed,
    /// by creating an instance yourself and providing a valid hash algorithm name in
    /// the constructor.
    ///
    /// Usage is simple: choose one of the static instances for the algorithm you want,
    /// or create a new one if necessary, then use the CreateSymmetric and CreateHash methods
    /// to generate SymmetricAlgorithm and HashAlgorithm instances respectively. The
    /// HashAlgorithm will be ready to go, but the SymmetricAlgorithm will need to have it's
    /// Key property set, and possibly other configuration as needed. This class is simply a
    /// way to get those instances easily.
    /// </summary>
    public class CryptoAlgorithmFactory
    {
        private const string INVALID_NAME = "Invalid algorithm name. Valid values are AES, 3DES, DES, RC2.";
        private const string INVALID_HASH = "Invalid hash algorithm name. Valid values are SHA1, SHA256, SHA384, SHA512.";

        public static CryptoAlgorithmFactory AES       = new CryptoAlgorithmFactory("AES");
        public static CryptoAlgorithmFactory TripleDES = new CryptoAlgorithmFactory("3DES");
        public static CryptoAlgorithmFactory DES       = new CryptoAlgorithmFactory("DES");
        public static CryptoAlgorithmFactory RC2       = new CryptoAlgorithmFactory("RC2");
        public static CryptoAlgorithmFactory Salsa20   = new CryptoAlgorithmFactory("Salsa20");

        /// <summary>
        /// The name of the symmetric algorithm which will be used.
        /// </summary>
        public string SymmetricAlgorithm { get; private set; }
        /// <summary>
        /// The name of the hashing algorithm which will be used.
        /// </summary>
        public string HashingAlgorithm { get; private set; }

        /// <summary>
        /// Create a new CryptoAlgorithmFactory instance.
        ///
        /// Valid symmetric encryption algorithm names:
        ///     Aes, 3DES, DES, RC2, Salsa20
        /// Valid hashing algorithm names:
        ///     SHA, SHA1, SHA256, SHA384, SHA512
        ///
        /// By default, SHA512 is used for the hashing algorithm, if one
        /// is not provided.
        /// </summary>
        /// <param name="symmetricAlgorithm">The name of the symmetric algorithm</param>
        /// <param name="hashAlgorithm">The name of the hashing algorithm</param>
        public CryptoAlgorithmFactory(string symmetricAlgorithm, string hashAlgorithm = "SHA512")
        {
            var algorithm = symmetricAlgorithm.Trim();
            var hash      = hashAlgorithm.Trim();

            if (!ValidateAlgorithm(algorithm))
                throw new ArgumentException(INVALID_NAME);
            if (!ValidateHashAlgorithm(hash))
                throw new ArgumentException(INVALID_HASH);

            SymmetricAlgorithm = algorithm;
            HashingAlgorithm   = hash;
        }

        /// <summary>
        /// Create a SymmetricAlgorithm instance of the type defined by this CryptoAlgorithmFactory instance.
        /// Some defaults will be set for you (CFB mode, ISO10126 padding, max key size). Tweak these as you
        /// see fit.
        /// </summary>
        /// <returns>SymmetricAlgorithm</returns>
        public SymmetricAlgorithm CreateSymmetric()
        {
            if (SymmetricAlgorithm.ToUpperInvariant() == "SALSA20")
                return new Salsa20CryptoServiceProvider();

            var algorithm     = System.Security.Cryptography.SymmetricAlgorithm.Create(SymmetricAlgorithm);
            algorithm.Mode    = CipherMode.CFB;
            algorithm.KeySize = algorithm.LegalKeySizes[algorithm.LegalKeySizes.Length - 1].MaxSize;
            algorithm.Padding = PaddingMode.ISO10126;

            return algorithm;
        }

        /// <summary>
        /// Create a HashAlgorithm instance of the type defined by this CryptoAlgorithmFactory instance.
        /// The algorithm will be initialized for you, so it can be used immediately.
        /// </summary>
        /// <returns>HashAlgorithm</returns>
        public HashAlgorithm CreateHash()
        {
            var hashAlgorithm = HashAlgorithm.Create(HashingAlgorithm);
            hashAlgorithm.Initialize();
            return hashAlgorithm;
        }

        private static bool ValidateAlgorithm(string algorithmName)
        {
            switch (algorithmName.ToUpperInvariant())
            {
                case "AES":
                case "3DES":
                case "DES":
                case "RC2":
                case "SALSA20":
                    return true;
                default:
                    return false;
            }
        }

        private static bool ValidateHashAlgorithm(string algorithmName)
        {
            switch (algorithmName.ToUpperInvariant())
            {
                case "SHA":
                case "SHA1":
                case "SHA256":
                case "SHA384":
                case "SHA512":
                    return true;
                default:
                    return false;
            }
        }
    }
}
