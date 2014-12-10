using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpTools.Test.Crypto
{
    using SharpTools.Crypto;

    [TestClass]
    public class CryptoProviderTest
    {
        private const string ENCRYPTION_PASSWORD = "w^dv!abc";
        private const string REALLY_SHORT_TEXT   = "hello";
        private const string UNICODE_TEXT        = "∀ ∁ ∂ ∃ ∄ ∅ ∆ ∇ ∈ ∉ ∊ ∋";
        private const string LONG_FORM_TEXT      = @"
        Lorem Ipsum is simply dummy text of the printing and typesetting industry.
        Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when
        an unknown printer took a galley of type and scrambled it to make a type specimen
        book. It has survived not only five centuries, but also the leap into electronic
        typesetting, remaining essentially unchanged. It was popularised in the 1960s with the
        release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop
        publishing software like Aldus PageMaker including versions of Lorem Ipsum.
        ";

        [TestMethod]
        public void CanEncryptThenDecrypt()
        {
            var provider    = new CryptoProvider(ENCRYPTION_PASSWORD);
            var shortCipher = provider.Encrypt(REALLY_SHORT_TEXT);
            var longCipher  = provider.Encrypt(LONG_FORM_TEXT);
            var utfCipher   = provider.Encrypt(UNICODE_TEXT);

            var shortText = provider.Decrypt(shortCipher);
            var longText  = provider.Decrypt(longCipher);
            var utfText   = provider.Decrypt(utfCipher);

            Assert.AreEqual(REALLY_SHORT_TEXT, shortText);
            Assert.AreEqual(LONG_FORM_TEXT, longText);
            Assert.AreEqual(UNICODE_TEXT, utfText);
        }

        [TestMethod]
        public void CanEncryptThenDecryptWithAlternateAlgorithm()
        {
            var provider = new CryptoProvider(ENCRYPTION_PASSWORD, CryptoAlgorithmFactory.AES);
            var shortCipher = provider.Encrypt(REALLY_SHORT_TEXT);
            var longCipher  = provider.Encrypt(LONG_FORM_TEXT);
            var utfCipher   = provider.Encrypt(UNICODE_TEXT);

            var shortText = provider.Decrypt(shortCipher);
            var longText  = provider.Decrypt(longCipher);
            var utfText   = provider.Decrypt(utfCipher);

            Assert.AreEqual(REALLY_SHORT_TEXT, shortText);
            Assert.AreEqual(LONG_FORM_TEXT, longText);
            Assert.AreEqual(UNICODE_TEXT, utfText);
        }

        [TestMethod]
        public void CanEncryptThenDecryptUsingDifferentInstances()
        {
            var encryptor = new CryptoProvider(ENCRYPTION_PASSWORD, CryptoAlgorithmFactory.AES);
            var shortCipher = encryptor.Encrypt(REALLY_SHORT_TEXT);
            var longCipher  = encryptor.Encrypt(LONG_FORM_TEXT);
            var utfCipher   = encryptor.Encrypt(UNICODE_TEXT);

            var decryptor = new CryptoProvider(ENCRYPTION_PASSWORD, CryptoAlgorithmFactory.AES);
            var shortText = decryptor.Decrypt(shortCipher);
            var longText  = decryptor.Decrypt(longCipher);
            var utfText   = decryptor.Decrypt(utfCipher);

            Assert.AreEqual(REALLY_SHORT_TEXT, shortText);
            Assert.AreEqual(LONG_FORM_TEXT, longText);
            Assert.AreEqual(UNICODE_TEXT, utfText);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void CannotEncryptThenDecryptWithInvalidPassword()
        {
            var encryptor = new CryptoProvider(ENCRYPTION_PASSWORD, CryptoAlgorithmFactory.AES);
            var shortCipher = encryptor.Encrypt(REALLY_SHORT_TEXT);
            var longCipher  = encryptor.Encrypt(LONG_FORM_TEXT);
            var utfCipher   = encryptor.Encrypt(UNICODE_TEXT);

            var decryptor = new CryptoProvider(ENCRYPTION_PASSWORD + "a", CryptoAlgorithmFactory.AES);
            var shortText = decryptor.Decrypt(shortCipher);
            var longText  = decryptor.Decrypt(longCipher);
            var utfText   = decryptor.Decrypt(utfCipher);

            Assert.AreEqual(REALLY_SHORT_TEXT, shortText);
            Assert.AreEqual(LONG_FORM_TEXT, longText);
            Assert.AreEqual(UNICODE_TEXT, utfText);
        }
    }
}
