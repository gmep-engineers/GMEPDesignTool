using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for OAuthLoginWindow.xaml
  /// </summary>
  ///

  public partial class OAuthLoginWindow : Window
  {
    public OAuthLoginWindow(LoginResponse loginResponse, NetSuiteAuth netSuiteAuth)
    {
      InitializeComponent();
      webView.CoreWebView2InitializationCompleted += (sender, args) =>
      {
        webView.CoreWebView2.CookieManager.DeleteAllCookies();
      };

      var netSuitUri = new Uri(
        $"https://5645740.app.netsuite.com/app/login/oauth2/authorize.nl?response_type=code&client_id={loginResponse.NetSuiteClientId}&redirect_uri=https%3A%2F%2Flocalhost&scope=rest_webservices&state=Kz5ZkB6smLpk3Y5t456SG0ETr6evxUtL"
      );

      webView.Source = netSuitUri;
      webView.ContentLoading += async (sender, args) =>
      {
        var queryParams = HttpUtility.ParseQueryString(webView.Source.Query);
        string? authorizationCode = queryParams["code"];
        if (
          authorizationCode != null
          && webView.Source.OriginalString.StartsWith("https://localhost")
        )
        {
          var formData = new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", "https://localhost"),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
          };
          string username = loginResponse.NetSuiteClientId;
          string password = loginResponse.NetSuiteClientSecret;
          string credentials = $"{username}:{password}";

          byte[] credentialBytes = System.Text.Encoding.ASCII.GetBytes(credentials);
          string base64Credentials = Convert.ToBase64String(credentialBytes);

          HttpClient client = new HttpClient();

          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded")
          );

          client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);

          var form = new FormUrlEncodedContent(formData);

          HttpResponseMessage response = await client.PostAsync(
            "https://5645740.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token",
            form
          );

          response.EnsureSuccessStatusCode();

          NetSuiteAuth authRes = await response.Content.ReadAsAsync<NetSuiteAuth>();

          netSuiteAuth.access_token = authRes.access_token;
          netSuiteAuth.refresh_token = authRes.refresh_token;
          netSuiteAuth.token_type = authRes.token_type;
          netSuiteAuth.expires_in = authRes.expires_in;
          int expires_in = 0;
          if (Int32.TryParse(authRes.expires_in, out expires_in))
          {
            // calculate datetime of 3600 seconds from now
            netSuiteAuth.expires_in = DateTime.Now.AddSeconds(expires_in).ToString();
          }

          this.Close();
        }
      };
    }
  }
}
