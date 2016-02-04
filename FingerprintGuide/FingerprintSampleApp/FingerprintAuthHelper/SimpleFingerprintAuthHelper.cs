using System;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V4.OS;
using Android.Util;
using Java.Lang;
using Javax.Crypto;
using Exception = System.Exception;

namespace FingerprintAuthSampleActivity.FingerprintAuthHelper
{
    /// <summary>
    ///     This class is a wrapper around the boilerplate code
    /// </summary>
    public class SimpleFingerprintAuthHelper
    {
        static readonly string TAG = "X:" + typeof (SimpleFingerprintAuthHelper).Name;

        protected FingerprintManagerCompat _fingerprintManager;

        protected SimpleFingerprintAuthHelper(Context context, ICryptoBuilder cryptobuilder)
        {
            Context = context;
            CryptoBuilder = cryptobuilder;
            ScanInProgress = false;
            _fingerprintManager = FingerprintManagerCompat.From(context);
        }

        protected ICryptoBuilder CryptoBuilder { get; }
        protected Context Context { get; }
        protected CancellationSignal CancellationSignal { get; set; }

        public bool ScanInProgress { get; protected set; }
        public event EventHandler<FingerprintAuthEventArgs> AuthenticationFinished = delegate { };

        public virtual void Start(FingerprintManagerCompat.AuthenticationCallback callback = null)
        {
            AssertCanDoFingerprintScan();
            ScanInProgress = true;
            CancellationSignal = new CancellationSignal();

            Log.Debug(TAG, "Starting up the fingerprint scanner.");
            FingerprintManagerCompat.CryptoObject crypto = CryptoBuilder.CreateCryptoObject();
            _fingerprintManager.Authenticate(crypto,
                                             0, /* flags - always zero for now*/
                                             CancellationSignal,
                                             callback ?? new EventRaisingFingerprintAuthenticationCallback(this, crypto),
                                             null);
        }

        public virtual void Stop()
        {
            if (!ScanInProgress)
            {
                return;
            }

            if (CancellationSignal != null)
            {
                CancellationSignal.Cancel();
                Log.Verbose(TAG, "Sent the CancellationSignal.");
            }
            ScanInProgress = false;
        }

        public virtual bool HasPermission()
        {
            return ContextCompat.CheckSelfPermission(Context, Manifest.Permission.UseFingerprint) != Permission.Denied;
        }

        public virtual bool HasEnrolledFingerprints()
        {
            return _fingerprintManager.HasEnrolledFingerprints;
        }

        public virtual bool IsHardwareDetected()
        {
            return _fingerprintManager.IsHardwareDetected;
        }

        protected virtual void AssertCanDoFingerprintScan()
        {

            // TODO [TO20160106@1307] Localize these strings
            if (!IsHardwareDetected())
            {
                throw new FingerprintAuthException(
                    "Device does not have the required hardware for fingerprint authentication.");
            }
            if (!HasEnrolledFingerprints())
            {
                throw new FingerprintAuthException(
                    "There are no fingerprints enrolled - cannot use fingerprint authentication.");
            }
            if (!HasPermission())
            {
                throw new FingerprintAuthException("The app has not been granted permission to scan for hardware.");
            }
        }

        protected virtual void OnFingerprintScanFinished(FingerprintAuthEventArgs args)
        {
            AuthenticationFinished(this, args);
        }

        /// <summary>
        ///     Factory method to instantiate the Fingerprint authenticator.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="keyName">The name of the key to use.</param>
        /// <returns></returns>
        public static SimpleFingerprintAuthHelper From(Context context, string keyName)
        {
            ICryptoBuilder cryptoBuilder = new CipherCryptoBuilder(keyName);
            return new SimpleFingerprintAuthHelper(context, cryptoBuilder);
        }

        /// <summary>
        ///     This is the default callback class. It consolidates everything to a C# event instead of the
        ///     callback interface that a Java based Android application would use).
        /// </summary>
        class EventRaisingFingerprintAuthenticationCallback : FingerprintManagerCompat.AuthenticationCallback
        {
            static readonly byte[] SECRET_BYTES = {11, 22, 33, 44, 55, 66, 77, 88};
            static readonly string TAG = "X:" + typeof (EventRaisingFingerprintAuthenticationCallback).Name;
            readonly SimpleFingerprintAuthHelper _authHelper;
            readonly FingerprintManagerCompat.CryptoObject _cryptoObject;

            /// <summary>
            /// </summary>
            /// <param name="authHelper">The Android Context.</param>
            /// <param name="cryptoObject">
            ///     The <code>CryptoObject</code> that was passed to the Android/Java
            ///     <code>FingerprintManager</code>.
            /// </param>
            public EventRaisingFingerprintAuthenticationCallback(SimpleFingerprintAuthHelper authHelper,
                                                                 FingerprintManagerCompat.CryptoObject cryptoObject)
            {
                _authHelper = authHelper;
                _cryptoObject = cryptoObject;
            }

            /// <summary>
            ///     Invoked by the Android OS when the fingerprint scan succeeds.
            /// </summary>
            /// <param name="result"></param>
            public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
            {
                FingerprintAuthValues authValues = DoFinalOnCipher();
                OnFingerprintScanFinished(authValues);
            }

            /// <summary>
            ///     If a Cipher was used, then call DoFinal on the the Cipher to finish things up.
            /// </summary>
            /// <returns></returns>
            FingerprintAuthValues DoFinalOnCipher()
            {
                FingerprintAuthValues authValues = new FingerprintAuthValues(FingerprintAuthResult.Success,
                                                                             int.MinValue,
                                                                             string.Empty);
                if (_cryptoObject == null)
                {
                    return authValues;
                }
                if (_cryptoObject.Cipher == null)
                {
                    return authValues;
                }

                // It seems that the fingerprint scan was done using a Cipher. In this case, call 
                // .DoFinal on the Cipher to complete the encryption of the fingerprint data.
                try
                {
                    byte[] doFinalResult = _cryptoObject.Cipher.DoFinal(SECRET_BYTES);
                    Log.Debug(TAG, "Fingerprint Authentication with Cipher succeeded, {0}",
                              Convert.ToBase64String(doFinalResult));
                    // [TO20160105@1815] Not to sure if it makes sense to provide doFinalResults to the caller as a part of the scan. 
                }
                catch (BadPaddingException bpe)
                {
                    // This exception is thrown when there is something wrong with the Cipher
                    // used to encrypt the data.
                    Log.Error(TAG, "BadPaddingException when trying to encrypt the fingerprint scan data: {0}.", bpe);
                    authValues.Result = FingerprintAuthResult.Crypto;
                }
                catch (IllegalBlockSizeException ibe)
                {
                    // This exception is thrown when there is something wrong with the Cipher
                    // used to encrypt the data.
                    Log.Error(TAG,
                              "IllegalBlockSizeException when trying to encrypt the fingerprint scan data: {0}.",
                              ibe);
                    authValues.Result = FingerprintAuthResult.Crypto;
                }
                catch (Exception e)
                {
                    throw new FingerprintAuthException("Some unknown error occured with the Cipher.", e);
                }

                return authValues;
            }

            public override void OnAuthenticationError(int errMsgId, ICharSequence errString)
            {
                Log.Info(TAG, "Authentication error; errMsgId={0}, errString='{1}'", errMsgId, errString);
                FingerprintAuthValues values = new FingerprintAuthValues(FingerprintAuthResult.Error,
                                                                         errMsgId,
                                                                         errString.ToString());
                OnFingerprintScanFinished(values);
            }

            public override void OnAuthenticationFailed()
            {
                Log.Info(TAG, "Authentication failed.");
                FingerprintAuthValues values = new FingerprintAuthValues(FingerprintAuthResult.Error,
                                                                         int.MinValue,
                                                                         string.Empty);
                OnFingerprintScanFinished(values);
            }

            public override void OnAuthenticationHelp(int helpMsgId, ICharSequence helpString)
            {
                Log.Info(TAG, "Authentication help; helpMsgId={0}, helpString='{1}'.", helpMsgId, helpString);
                FingerprintAuthValues values = new FingerprintAuthValues(FingerprintAuthResult.Help,
                                                                         helpMsgId,
                                                                         helpString.ToString());
                OnFingerprintScanFinished(values);
            }

            void OnFingerprintScanFinished(FingerprintAuthValues values)
            {
                EventHandler<FingerprintAuthEventArgs> handler = _authHelper.AuthenticationFinished;

                if (handler == null)
                {
                    return;
                }
                FingerprintAuthEventArgs args = new FingerprintAuthEventArgs(values.Result,
                                                                             _cryptoObject,
                                                                             values.MessageId,
                                                                             values.Message);
                handler(_authHelper, args);
            }

            /// <summary>
            ///     This <code>struct</code> holds the data that will be raised by the event
            ///     when authentication is finished.
            /// </summary>
            struct FingerprintAuthValues
            {
                public FingerprintAuthValues(FingerprintAuthResult result, int id, string msg)
                {
                    Result = result;
                    MessageId = id;
                    Message = msg;
                }

                public FingerprintAuthResult Result { get; set; }
                public int MessageId { get; }
                public string Message { get; }
            }
        }
    }
}