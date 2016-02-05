using Javax.Crypto;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    public interface ICipherHelper
    {
        Cipher GetCipherFor(string keyName);
    }
}