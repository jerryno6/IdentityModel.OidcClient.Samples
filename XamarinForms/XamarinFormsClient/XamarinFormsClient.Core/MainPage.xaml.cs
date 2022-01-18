using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinFormsClient.Core
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class MainPage : ContentPage
    {
		//private const string AuthUrl = "https://pkce-xamarinformsclients.auth.eu-central-1.amazoncognito.com";
		private const string AuthUrl = "https://demo.identityserver.io";
		private const string ClientId = "1knsantudc41sm66v1t01u2gr6";
		private const string CallBackUrl = "xamarinformsclients://callback";
		private const string AuthorizePath = "/oauth2/authorize";
	    private const string TokenPath = "/oauth2/token";
	    private const string UserInfoPath = "/oauth2/userInfo";
	    private const string Scope = "openid";

	    private readonly OidcClient _client;
        private LoginResult _result;
        private readonly Lazy<HttpClient> _apiClient;

		public MainPage()
        {
            InitializeComponent();

            Login.Clicked += Login_Clicked;
            CallApi.Clicked += CallApi_Clicked;

			//Initialize OpenidConnectClient
            var browser = DependencyService.Get<IBrowser>();

			var options = new OidcClientOptions
			{
				Authority = AuthUrl,
				ClientId = ClientId,
				RedirectUri = CallBackUrl,
				Scope = Scope,
				Browser = browser,

				ProviderInformation = new ProviderInformation()
				{
					IssuerName = "name of issuer", //any value you want except null/empty
					KeySet = new IdentityModel.Jwk.JsonWebKeySet(),
					AuthorizeEndpoint = $"{AuthUrl}{AuthorizePath}",
					TokenEndpoint = $"{AuthUrl}{TokenPath}",
					UserInfoEndpoint = $"{AuthUrl}{UserInfoPath}",
				}
			};

			_client = new OidcClient(options);

			//Initialize apiClient to get data from web api
			_apiClient = new Lazy<HttpClient>(() => new HttpClient());
			_apiClient.Value.BaseAddress = new Uri(AuthUrl);
		}

		private async void Login_Clicked(object sender, EventArgs e)
		{
			//Login PKCE flow
			_result = await _client.LoginAsync(new LoginRequest());

			//Display result
			if (_result.IsError)
			{
				OutputText.Text = _result.Error;
				return;
			}

			var sb = new StringBuilder(128);
			foreach (var claim in _result.User.Claims)
			{
				sb.AppendFormat("{0}: {1}\n", claim.Type, claim.Value);
			}

			sb.AppendFormat("\n{0}: {1}\n", "refresh token", _result?.RefreshToken ?? "none");
			sb.AppendFormat("\n{0}: {1}\n", "access token", _result.AccessToken);

			OutputText.Text = sb.ToString();

			_apiClient.Value.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _result?.AccessToken ?? "");
		}

		private async void CallApi_Clicked(object sender, EventArgs e)
        {

            var result = await _apiClient.Value.GetAsync("api/test");

            if (result.IsSuccessStatusCode)
            {
                OutputText.Text = JsonDocument.Parse(await result.Content.ReadAsStringAsync()).RootElement.GetRawText();
            }
            else
            {
                OutputText.Text = result.ReasonPhrase;
            }
        }
    }
}