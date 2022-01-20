using IdentityModel.OidcClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using IdentityModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private const string AuthUrl = "https://pkce-xamarinformsclients.auth.eu-central-1.amazoncognito.com";
        private const string AuthorizePath = "/oauth2/authorize";
        private const string TokenPath = "/oauth2/token";
        private const string UserInfoPath = "/oauth2/userInfo";

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            var options = new OidcClientOptions
            {
                Authority = AuthUrl,
                ClientId = "1knsantudc41sm66v1t01u2gr6",
                RedirectUri = "xamarinformsclients://callback",
                Scope = OidcConstants.StandardScopes.OpenId,
                Browser = new EmbeddedBrowser(loginDialog),

                ProviderInformation = new ProviderInformation()
                {
                    IssuerName = "name of issuer", //any value you want except null/empty
                    KeySet = new IdentityModel.Jwk.JsonWebKeySet(),
                    AuthorizeEndpoint = $"{AuthUrl}{AuthorizePath}",
                    TokenEndpoint = $"{AuthUrl}{TokenPath}",
                    UserInfoEndpoint = $"{AuthUrl}{UserInfoPath}",
                }
            };

            var client = new OidcClient(options);
            var result = await client.LoginAsync(new LoginRequest());
            var a = result.User.Claims.Where(x=>x.Type=="username");
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                textBox.Text = result.Error;

                return;
            }

            var sb = new StringBuilder();

            foreach (var claim in result.User.Claims)
            {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            sb.AppendLine($"refresh token: {result.RefreshToken}");
            sb.AppendLine($"access token: {result.AccessToken}");

            textBox.Text = sb.ToString();
        }
    }
}
