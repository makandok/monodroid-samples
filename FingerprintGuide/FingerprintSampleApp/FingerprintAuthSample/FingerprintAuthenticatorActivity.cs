using System;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.FingerprintSample;
using Xamarin.FingerprintSample.FingerprintAuthHelper;

namespace BasicFingerPrintSample.FingerprintAuthSample
{
    [Activity(Label = "@string/activity_fingeprintauthenticatoractivity_title")]
    public class FingerprintAuthenticatorActivity : Activity
    {
        // ReSharper disable once InconsistentNaming
        static readonly string TAG = "X:" + typeof (FingerprintAuthenticatorActivity).Name;
        View _errorPanel, _authenticatedPanel, _initialPanel;
        TextView _errorTextView1, _errorTextView2, _scanFingerPrintTextView;
        SimpleFingerprintAuthHelper _fingerprintAuthenticator;
        Button _scanButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_fingerprintmanager_api);
            _fingerprintAuthenticator = SimpleFingerprintAuthHelper.From(this, "cipher_key_name");
            InitializeViewReferences();
        }

        protected override void OnResume()
        {
            base.OnResume();
            FingerprintScanReadyUI();
            if (_fingerprintAuthenticator.HasPermission())
            {
                _fingerprintAuthenticator.AuthenticationFinished += OnFingerprintAuthenticationFinished;
            }
        }

        protected override void OnPause()
        {
            _fingerprintAuthenticator.Stop();
            _fingerprintAuthenticator.AuthenticationFinished -= OnFingerprintAuthenticationFinished;
            base.OnPause();
        }

        void InitializeViewReferences()
        {
//            _errorPanel = FindViewById(Resource.Id.error_panel);
//            _authenticatedPanel = FindViewById(Resource.Id.authenticated_panel);
//
//            _errorTextView1 = FindViewById<TextView>(Resource.Id.error_text1);
//            _errorTextView2 = FindViewById<TextView>(Resource.Id.error_text2);
//
//            _scanFingerPrintTextView = FindViewById<TextView>(Resource.Id.start_scan_button);
//
//            _scanButton = FindViewById<Button>(Resource.Id.start_scan_button);
        }

        void OnFingerprintAuthenticationFinished(object sender, FingerprintAuthEventArgs eventArgs)
        {
            Log.Debug(TAG, "AuthenticationFinished with the authenticator.");
            ShowError(eventArgs.AuthResult.ToString());
        }

        void FingerprintScanReadyUI()
        {
            _scanButton.Click -= StopFingerprintScanHandler;
            _scanButton.Click += StartFingerprintScanHandler;

            _scanButton.SetText(Resource.String.scan_fingerprint_start);
            _scanFingerPrintTextView.SetText(Resource.String.scan_fingerprint_start);

            _initialPanel.Visibility = ViewStates.Visible;
            _authenticatedPanel.Visibility = ViewStates.Gone;
            _errorPanel.Visibility = ViewStates.Gone;
        }

        void StartFingerprintScanHandler(object sender, EventArgs e)
        {
            if (_fingerprintAuthenticator.ScanInProgress)
            {
                return;
            }
            _fingerprintAuthenticator.Start(null); // [TO20160106@1308] By sending NULL
            _scanButton.Click -= StartFingerprintScanHandler;
            _scanButton.Click += StopFingerprintScanHandler;
            _scanFingerPrintTextView.SetText(Resource.String.scan_fingerprint_in_progress);
            _scanButton.SetText(Resource.String.scan_fingerprint_stop);
        }

        void StopFingerprintScanHandler(object sender, EventArgs e)
        {
            _fingerprintAuthenticator.Stop();

            FingerprintScanReadyUI();
        }

        void ShowError(string text1, string text2 = "")
        {
            _errorTextView1.Text = text1;
            _errorTextView2.Text = text2;

            _initialPanel.Visibility = ViewStates.Gone;
            _authenticatedPanel.Visibility = ViewStates.Gone;
            _errorPanel.Visibility = ViewStates.Visible;
        }
    }
}