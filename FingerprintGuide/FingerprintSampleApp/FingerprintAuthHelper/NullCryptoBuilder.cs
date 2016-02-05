using Android.Support.V4.Hardware.Fingerprint;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    ///     Returns null for the CryptoObject.
    /// </summary>
    /// <remarks>
    /// This class is used when a secure fingerprint scan is not necessary.
    /// </remarks>
    public class NullCryptoBuilder : ICryptoBuilder
    {
        public FingerprintManagerCompat.CryptoObject CreateCryptoObject()
        {
            return null;
        }

        public FingerprintManagerCompat.CryptoObject CreateCryptoObject(string keyname)
        {
            return CreateCryptoObject();
        }
    }
}