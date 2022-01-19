using IdentityModel.OidcClient.Browser;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WinUI3
{
    internal class EmbeddedBrowser : IBrowser
    {
        private readonly ContentDialog _dialog;

        public EmbeddedBrowser(ContentDialog dialog)
        {
            _dialog = dialog;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            var semaphoreSlim = new SemaphoreSlim(0, 1); ;

            var browserResult = new BrowserResult()
            {
                ResultType = BrowserResultType.UserCancel
            };

            var webView = new WebView2
            {
                Width = 500,
                Height = 600
            };
            _dialog.Content = webView;

            webView.NavigationStarting += (s, e) =>
            {
                if (IsBrowserNavigatingToRedirectUri(new Uri(e.Uri), options.EndUrl))
                {
                    e.Cancel = true;

                    browserResult = new BrowserResult()
                    {
                        ResultType = BrowserResultType.Success,
                        Response = new Uri(e.Uri).AbsoluteUri
                    };

                    semaphoreSlim.Release();
                    _dialog.Content = null;
                    _dialog.Hide();
                }
            };

            // Initialization
            await webView.EnsureCoreWebView2Async();

            // Delete existing Cookies so previous logins won't remembered
            webView.CoreWebView2.CookieManager.DeleteAllCookies();

            // Navigate
            webView.CoreWebView2.Navigate(options.StartUrl);

            await _dialog.ShowAsync();

            await semaphoreSlim.WaitAsync();

            return browserResult;
        }

        private bool IsBrowserNavigatingToRedirectUri(Uri uri, string endUrl)
        {
            return uri.AbsoluteUri.StartsWith(endUrl);
        }
    }
}
