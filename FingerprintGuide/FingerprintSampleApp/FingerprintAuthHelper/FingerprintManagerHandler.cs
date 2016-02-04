namespace FingerprintAuthSampleActivity.FingerprintAuthHelper
{
    /// <summary>
    ///     A delegate that can be called when the fingerprint scanner is finished to
    ///     return the results to the calling application.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">The <code>EventArgs</code> with the results of the Fingerprint scan.</param>
    public delegate void FingerprintManagerHandler(object sender, FingerprintAuthEventArgs e);
}