using Android.Support.V4.Hardware.Fingerprint;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    ///     This interface is implemented by classes that will instantiate a CryptoObject wrapper
    ///     for the FingerprintAuthenticator.
    /// </summary>
    public interface ICryptoBuilder
    {
        /// <summary>
        /// Instantiates the CryptoObject for a given <code>KeyName</code>. Will create the key if it does not exist.
        /// </summary>
        /// <returns>A <code>CryptoObject</code> to use with the the Android fingerprint APIs.</returns>
        FingerprintManagerCompat.CryptoObject CreateCryptoObject();

        /// <summary>
        /// Instantiates the CryptoObject for a given <code>keyname</code>. Will create the key if it does not exist.
        /// </summary>
        /// <returns>A <code>CryptoObject</code> to use with the the Android fingerprint APIs.</returns>
        FingerprintManagerCompat.CryptoObject CreateCryptoObject(string keyname);
    }
}