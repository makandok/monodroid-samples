using System;
using Android.App;
using Android.Hardware.Fingerprints;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Javax.Crypto;
using CancellationSignal = Android.Support.V4.OS.CancellationSignal;
using Debug = System.Diagnostics.Debug;

namespace BasicFingerPrintSample.FingerprintManagerAPISample
{
    /// <summary>
    ///     This is an example of using a DialogFragment for the FingerprintManager API.
    /// </summary>
    public class FingerprintManagerApiDialogFragment : DialogFragment
    {
        static readonly string TAG = "X:" + typeof (FingerprintManagerApiDialogFragment).Name;

        Button _cancelButton;
        CancellationSignal _cancellationSignal;
        FingerprintManagerCompat _fingerprintManager;

        bool ScanForFingerprintsInOnResume { get; set; } = true;

        bool UserCancelledScan { get; set; }

        CryptoObjectHelper CryptObjectHelper { get; set;  }

        bool IsScanningForFingerprints
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _cancellationSignal != null; }
        }

        public static FingerprintManagerApiDialogFragment NewInstance(FingerprintManagerCompat fingerprintManager)
        {
            FingerprintManagerApiDialogFragment frag = new FingerprintManagerApiDialogFragment
                                                       {
                                                           _fingerprintManager = fingerprintManager
                                                       };
            return frag;
        }

        public void Init( bool startScanning = true)
        {
            ScanForFingerprintsInOnResume = startScanning;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            CryptObjectHelper = new CryptoObjectHelper();
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeMaterialLightDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.SetTitle(Resource.String.sign_in);

            View v = inflater.Inflate(Resource.Layout.dialog_scanning_for_fingerprint, container, false);

            _cancelButton = v.FindViewById<Button>(Resource.Id.cancel_button);
            _cancelButton.Click += (sender, args) =>
                                   {
                                       UserCancelledScan = true;
                                       StopListeningForFingerprints();
                                   };

            return v;
        }

        public override void OnResume()
        {
            base.OnResume();
            Log.Debug(TAG, "OnResume: ScanForFingerprintsInOnResume={0}", ScanForFingerprintsInOnResume);
            if (!ScanForFingerprintsInOnResume)
            {
                return;
            }

            Debug.Assert(_cancellationSignal == null);
            UserCancelledScan = false;
            _cancellationSignal = new CancellationSignal();
            _fingerprintManager.Authenticate(CryptObjectHelper.BuildCryptoObject(),
                                             (int) FingerprintAuthenticationFlags.None, /* flags */
                                             _cancellationSignal,
                                             new SimpleAuthCallbacks(this),
                                             null);
        }

        public override void OnPause()
        {
            base.OnPause();
            Log.Debug(TAG, "OnPause: IsScanningForFingerprints={0}", IsScanningForFingerprints);
            if (IsScanningForFingerprints)
            {
                StopListeningForFingerprints(true);
            }
        }

        void AuthenticationFailed()
        {
            FingerprintManagerApiActivity activity = Activity as FingerprintManagerApiActivity;
            if (activity != null)
            {
                string msg = Resources.GetString(Resource.String.authentication_failed_message);
                activity.ShowError(msg);
            }
            Dismiss();
        }

        void AuthenticationError(int errMsgId, string errorMessage)
        {
            FingerprintManagerApiActivity activity = Activity as FingerprintManagerApiActivity;
            if (activity != null)
            {
                activity.ShowError(errorMessage, string.Format("Error message id {0}.", errMsgId));
            }
            Dismiss();
        }

        void AuthenticationSuccess()
        {
            FingerprintManagerApiActivity activity = Activity as FingerprintManagerApiActivity;
            if (activity != null)
            {
                activity.AuthenticationSuccessful();
            }
            Dismiss();
        }

        void StopListeningForFingerprints(bool butStartListeningAgainInOnResume = false)
        {
            if (_cancellationSignal != null)
            {
                _cancellationSignal.Cancel();
                _cancellationSignal = null;
                Log.Debug(TAG, "StopListeningForFingerprints: _cancellationSignal.Cancel();");
            }
            ScanForFingerprintsInOnResume = butStartListeningAgainInOnResume;
            Log.Debug(TAG,
                      "StopListeningForFingerprints: Stopped listening for fingerprints, _scanForFingerprintsInOnResume={0}.",
                      ScanForFingerprintsInOnResume);
        }

        public override void OnDestroyView()
        {
            // see https://code.google.com/p/android/issues/detail?id=17423
            if (Dialog != null && RetainInstance)
            {
                Dialog.SetDismissMessage(null);
            }
            base.OnDestroyView();
        }

        class SimpleAuthCallbacks : FingerprintManagerCompat.AuthenticationCallback
        {
            static readonly byte[] SECRET_BYTES = {1, 2, 3, 4, 5, 6, 7, 8, 9};
            static readonly string TAG = "X:" + typeof (SimpleAuthCallbacks).Name;
            readonly FingerprintManagerApiDialogFragment _fragment;

            public SimpleAuthCallbacks(FingerprintManagerApiDialogFragment frag)
            {
                _fragment = frag;
            }

            public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
            {
                Log.Debug(TAG, "OnAuthenticationSucceeded");
                if (result.CryptoObject.Cipher != null)
                {
                    try
                    {
                        // Calling DoFinal on the Cipher ensures that the encryption worked.
                        byte[] doFinalResult = result.CryptoObject.Cipher.DoFinal(SECRET_BYTES);
                        Log.Debug(TAG, "Fingerprint authentication succeeded: {0}",
                                  Convert.ToBase64String(doFinalResult));
                        _fragment.AuthenticationSuccess();
                    }
                    catch (BadPaddingException bpe)
                    {
                        Log.Error(TAG, "Failed to encrypt the data with the generated key." + bpe);
                        _fragment.AuthenticationFailed();
                    }
                    catch (IllegalBlockSizeException ibse)
                    {
                        Log.Error(TAG, "Failed to encrypt the data with the generated key." + ibse);
                        _fragment.AuthenticationFailed();
                    }
                }
                else
                {
                    // No cipher used, assume that everything went well and trust the results.
                    Log.Debug(TAG, "Fingerprint authentication succeeded.");
                    _fragment.AuthenticationSuccess();
                }
            }

            public override void OnAuthenticationError(int errMsgId, ICharSequence errString)
            {
                // There are some situations where we don't care about the error. 
                bool reportError = (errMsgId == (int) FingerprintState.ErrorCanceled) &&
                                   !_fragment.ScanForFingerprintsInOnResume;

                string debugMsg = string.Format("OnAuthenticationError: {0}:`{1}`.", errMsgId, errString);

                if (_fragment.UserCancelledScan)
                {
                    string msg = _fragment.Resources.GetString(Resource.String.scan_cancelled_by_user);
                    _fragment.AuthenticationError(-1, msg);
                }
                else if (reportError)
                {
                    _fragment.AuthenticationError(errMsgId, errString.ToString());
                    debugMsg += " Reporting the error.";
                }
                else
                {
                    debugMsg += " Ignoring the error.";
                }
                Log.Debug(TAG, debugMsg);
            }

            public override void OnAuthenticationFailed()
            {
                Log.Info(TAG, "Authentication failed.");
                _fragment.AuthenticationFailed();
            }

            public override void OnAuthenticationHelp(int helpMsgId, ICharSequence helpString)
            {
                Log.Debug(TAG, "OnAuthenticationHelp: {0}:`{1}`", helpString, helpMsgId);
                _fragment.AuthenticationError(helpMsgId, helpString.ToString());
            }
        }
    }
}