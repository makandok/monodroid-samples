using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    ///     A helper that abstracts away the logic for createing the Cipher.
    /// </summary>
    public class CipherHelper : ICipherHelper
    {
        static readonly string TAG = "X:" + typeof (CipherHelper).Name;
        static readonly string KEYSTORE_PROVIDER_NAME = "AndroidKeyStore";
        static readonly string ALGORITHM = KeyProperties.KeyAlgorithmAes;
        static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;
        static readonly string PADDING = KeyProperties.EncryptionPaddingPkcs7;
        static readonly string TRANSFORMATION = ALGORITHM + "/" + BLOCK_MODE + "/" + PADDING;

        static KeyStore _keystore;
        static volatile CipherHelper _instance;
        static readonly object LockObject = new object();

        CipherHelper()
        {
            _keystore = KeyStore.GetInstance(KEYSTORE_PROVIDER_NAME);
            _keystore.Load(null);
        }

        public static CipherHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new CipherHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        public Cipher GetCipherFor(string keyName)
        {
            if (!_keystore.IsKeyEntry(keyName))
            {
                CreateKey(keyName);
            }
            IKey secretKey = _keystore.GetKey(keyName, null);
            Cipher cipher = Cipher.GetInstance(TRANSFORMATION);
            cipher.Init(CipherMode.EncryptMode, secretKey);
            return cipher;
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
        }
    }
}