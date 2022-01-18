
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using IdentityModel.OidcClient.Browser;

namespace XamarinFormsClient.UWP
{
//    public class SystemBrowser2 : IBrowser
//    {
//        public static TaskCompletionSource<WebAuthenticationResult> BrowserAuthenticationTaskCompletionSource { get; private set; }

//        private ContentDialog webViewDialog;

//        private Uri _callBackUri;

//        public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
//        {
//            return AuthenticateAsync(webAuthenticatorOptions.Url, webAuthenticatorOptions.CallbackUrl);
//        }

//        public async Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
//        {
//            _callBackUri = callbackUrl;

//            //Show webview dialog
//            BrowserAuthenticationTaskCompletionSource = new TaskCompletionSource<WebAuthenticationResult>();
//            webViewDialog = CreateWebViewDialog(url);

//#pragma warning disable CS4014
//            //we do not call await because in some cases, the task completes by jumping to the OnActivated method
//            webViewDialog.ShowAsync();
//#pragma warning restore CS4014

//            //Wait the webView to return the callback url
//            var result = await BrowserAuthenticationTaskCompletionSource.Task;

//            //Once we get result from the webViewDialog, hide it
//            webViewDialog.Hide();

//            //Return result base on status
//            switch (result.ResponseStatus)
//            {
//                case WebAuthenticationStatus.Success:
//                    // For GET requests this is a URI:
//                    var resultUri = new Uri(result.ResponseData);
//                    return new WebAuthenticatorResult(resultUri);
//                case WebAuthenticationStatus.UserCancel:
//                    throw new TaskCanceledException();
//                case WebAuthenticationStatus.ErrorHttp:
//                    throw new HttpRequestException("Error: " + result.WebErrorStatus);
//                default:
//                    throw new ArgumentOutOfRangeException("Response: " + result.ResponseData + "\nStatus: " + result.ResponseStatus);
//            }
//        }

//        private ContentDialog CreateWebViewDialog(Uri url)
//        {
//            var dialog = new WebviewDialog
//            {
//                CloseButtonText = AppResources.Close,
//            };

//            dialog.WebView.Source = url;

//            dialog.WebView.NavigationCompleted += (s, e) =>
//            {
//                OnCallbackUrlReturned(e, _callBackUri);
//            };

//            dialog.CloseButtonClick += (sender, arg) =>
//            {
//                var result = new WebAuthenticationResult(null, WebErrorStatus.Unknown, WebAuthenticationStatus.UserCancel);
//                BrowserAuthenticationTaskCompletionSource.TrySetResult(result);
//            };

//            return dialog;
//        }

//        private void OnCallbackUrlReturned(WebViewNavigationCompletedEventArgs args, Uri callbackUri)
//        {
//            var returnedUrl = args.Uri.AbsoluteUri;

//            //validate input
//            if (string.IsNullOrEmpty(returnedUrl) ||
//                !returnedUrl.StartsWith(callbackUri.AbsoluteUri))
//            {
//                return;
//            }

//            //return parameters from backend via CompletionSource
//            var result = new WebAuthenticationResult(args.Uri.AbsoluteUri, args.WebErrorStatus, WebAuthenticationStatus.Success);
//            BrowserAuthenticationTaskCompletionSource.TrySetResult(result);
//        }
//    }

    public class SystemBrowser : IBrowser
    {
        public static TaskCompletionSource<BrowserResult> BrowserAuthenticationTaskCompletionSource { get; private set; }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            BrowserAuthenticationTaskCompletionSource = new TaskCompletionSource<BrowserResult>(cancellationToken);

            await OpenBrowser(options.StartUrl);
            
            try
            {
                var result = await BrowserAuthenticationTaskCompletionSource.Task;

                return result;
            }
            catch (TaskCanceledException ex)
            {
                return new BrowserResult {ResultType = BrowserResultType.Timeout, Error = ex.Message};
            }
            catch (Exception ex)
            {
                return new BrowserResult {ResultType = BrowserResultType.UnknownError, Error = ex.Message};
            }
        }

        public static async Task OpenBrowser(string url)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
