namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    /// The result of the fingerprint scan.
    /// </summary>
    public enum FingerprintAuthResult
    {
        /// <summary>
        /// Something happened, but we're not to sure what.
        /// </summary>
        Unknown,
        /// <summary>
        /// The scan failed, but a Help message was provided to help the user.
        /// </summary>
        Help,
        /// <summary>
        /// The scan succeeded.
        /// </summary>
        Success,
        /// <summary>
        /// There was some kind of error with thescan (such as a hardware failure).
        /// </summary>
        Error,
        /// <summary>
        /// The scan did not succeed because the fingerprint was not recognized.
        /// </summary>
        Failure,
        /// <summary>
        /// The fingerprint scan was cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// There was an unexpected problem with the crypto used by the Android FingerprintManager.
        /// </summary>
        CryptoError
    }
}