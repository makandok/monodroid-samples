using Javax.Crypto;

namespace FingerprintAuthSampleActivity.FingerprintAuthHelper
{
    public interface ICipherHelper
    {
        Cipher GetCipherFor(string keyName);
    }
}