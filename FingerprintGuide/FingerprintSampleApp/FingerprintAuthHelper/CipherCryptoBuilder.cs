using Android.Security.Keystore;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Util;
using Java.Security;
using Javax.Crypto;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    ///     Builds a <code>CryptoObject<code> for use with the Fingerprint that is a wrapper around a </code></code>
    ///     <code>Cipher</code>
    ///     object.
    /// </summary>
    public class CipherCryptoBuilder : ICryptoBuilder
    {
        static readonly string TAG = "X:" + typeof (CipherCryptoBuilder).Name;

        /// <summary>
        /// The name of the Android Keystore Provider.
        /// </summary>
        static readonly string KEYSTORE_PROVIDER_NAME = "AndroidKeyStore";  // This value doesn't change - is the name of the provider.

        /// <summary>
        /// Use AES for encryption.
        /// </summary>
        static readonly string ALGORITHM = KeyProperties.KeyAlgorithmAes;

        /// <summary>
        /// Use the Cipher Block Chaining (CBC) as part of the encryption algorithm.
        /// </summary>
        static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;

        /// <summary>
        /// Use Public Key Cryptography Standard #7 for padding each of the blocks.
        /// </summary>
        static readonly string PADDING = KeyProperties.EncryptionPaddingPkcs7;

        /// <summary>
        /// String together the pieces the Cipher needs to encrypt things.
        /// </summary>
        static readonly string TRANSFORMATION = ALGORITHM + "/" + BLOCK_MODE + "/" + PADDING;

        readonly KeyStore _keystore;


        /// <summary>
        /// Instantiate an instance of <code>CipherCryptoBuilder</code> for a given key name.
        /// </summary>
        /// <param name="keyname"></param>
        public CipherCryptoBuilder(string keyname)
        {
            _keystore = KeyStore.GetInstance(KEYSTORE_PROVIDER_NAME);
            _keystore.Load(null);
            KeyName = keyname;
        }


        /// <summary>
        ///     The name of the key used for the Cipher
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        ///     Instantiates the CryptoObject for a given <code>KeyName</code>. Will create the key if it does not exist.
        /// </summary>
        /// <returns>A <code>CryptoObject</code> to use with the the Android fingerprint APIs.</returns>
        public FingerprintManagerCompat.CryptoObject CreateCryptoObject()
        {
            return CreateCryptoObject(KeyName);
        }

        /// <summary>
        ///     Instantiates the CryptoObject for a given <code>keyname</code>. Will create the key if it does not exist.
        /// </summary>
        /// <returns>A <code>CryptoObject</code> to use with the the Android fingerprint APIs.</returns>
        public FingerprintManagerCompat.CryptoObject CreateCryptoObject(string keyname)
        {
            if (!_keystore.IsKeyEntry(keyname))
            {
                CreateKey(keyname);
            }

            IKey secretKey = _keystore.GetKey(keyname, null);
            Cipher cipher = Cipher.GetInstance(TRANSFORMATION);
            cipher.Init(CipherMode.EncryptMode, secretKey);

            FingerprintManagerCompat.CryptoObject crypto = new FingerprintManagerCompat.CryptoObject(cipher);

            return crypto;
        }


        void CreateKey(string keyname)
        {
            KeyGenerator keyGenerator = KeyGenerator.GetInstance(ALGORITHM, KEYSTORE_PROVIDER_NAME);
            KeyGenParameterSpec spec =
                new KeyGenParameterSpec.Builder(keyname, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(BLOCK_MODE)
                    .SetEncryptionPaddings(PADDING)
                    .SetUserAuthenticationRequired(true)
                    .Build();
            keyGenerator.Init(spec);
            keyGenerator.GenerateKey();

            Log.Debug(TAG, "Created the key '{0}' for the Cipher.", keyname);
        }
    }
}