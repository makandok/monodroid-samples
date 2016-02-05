using System;
using Android.Support.V4.Hardware.Fingerprint;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    /// An <code>EventArg</code> holding the results of the fingerprint scan.
    /// </summary>
    public class FingerprintAuthEventArgs : EventArgs
    {
        public FingerprintAuthEventArgs(FingerprintAuthResult scanResult, FingerprintManagerCompat.CryptoObject cryptoObject,
                                    int resultCode, string message)
        {
            AuthResult = scanResult;
            Message = message;
            ResultCode = resultCode;
            CryptoObject = cryptoObject;
        }

        /// <summary>
        /// The result code associated with the fingerprint scan.
        /// </summary>
        public FingerprintAuthResult AuthResult { get; }

        /// <summary>
        /// A user-friendly message from the results.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The result code from the Android fingerprint scanner service.
        /// </summary>
        public int ResultCode { get; }

        /// <summary>
        /// The CryptoObject that was used when the fingerprint authentication was kicked off.
        /// </summary>
        public FingerprintManagerCompat.CryptoObject CryptoObject { get; private set; }

        public override int GetHashCode()
        {
            return ResultCode + Message.GetHashCode() * 32 + AuthResult.GetHashCode() * 64;
        }

        public override string ToString()
        {
            return string.Format("AuthResult={0}, ResultCode={1}, Message='{2}'", AuthResult, ResultCode, Message);
        }
    }
}