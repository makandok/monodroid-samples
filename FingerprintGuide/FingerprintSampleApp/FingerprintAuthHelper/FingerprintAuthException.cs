using System;
using System.Runtime.Serialization;

namespace Xamarin.FingerprintSample.FingerprintAuthHelper
{
    /// <summary>
    /// This exception is raised when there is some kind of problem with 
    /// Fingerprint Authentication.
    /// </summary>
    [Serializable]
    public class FingerprintAuthException: Exception
    {
        public FingerprintAuthException() {}
        public FingerprintAuthException(string message) : base(message) {}
        protected FingerprintAuthException(SerializationInfo info, StreamingContext context) : base(info, context) {}
        public FingerprintAuthException(string message, Exception innerException) : base(message, innerException) {}
    }
}