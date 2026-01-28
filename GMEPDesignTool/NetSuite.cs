using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public static class NetSuiteHandler
  {
    public static async Task RefreshNetSuiteToken(
      LoginResponse loginResponse,
      NetSuiteAuth netSuiteAuth
    )
    {
      var formData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("refresh_token", netSuiteAuth.refresh_token),
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
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
      netSuiteAuth.token_type = authRes.token_type;
      netSuiteAuth.expires_in = authRes.expires_in;
      int expires_in = 0;
      if (Int32.TryParse(authRes.expires_in, out expires_in))
      {
        // calculate datetime of 3600 seconds from now
        netSuiteAuth.expires_in = DateTime.Now.AddSeconds(expires_in).ToString();
      }
    }
  }

  public class NetSuiteQueryRequest
  {
    public string q;

    public NetSuiteQueryRequest(string _q)
    {
      q = _q;
    }
  };

  public class NetSuiteCollection
  {
    public int count;
    public bool hasMore;
    public int offset;
    public int totalResults;
  }

  public class NetSuiteEstimateAccountBookDetailElement
  {
    public NsResource accountBook;
    public double exchangeRate;
    public NsLink[] links;
    public string refName;
    public bool revRecOnRevCommitment;
    public NetSuiteSubsidiary subsidiary;
    public bool tranIsVsoeBundle;
  }

  public class NetSuiteEstimateAccountBookDetailCollection : NetSuiteCollection
  {
    public NetSuiteEstimateAccountBookDetailElement[] items;
    public NsLink[] links;
  }

  public class NetSuiteEstimate
  {
    public string tranId { get; set; }
    public string entity { get; set; }
    public string tranDate { get; set; }

    public double probability { get; set; }
    public string expectedCloseDate { get; set; }

    public NetSuiteEntityStatus entityStatus { get; set; }

    public NetSuiteEstimateItem item { get; set; }

    public string custbody_project_address { get; set; }
    public string custbody_project_city { get; set; }
    public string custbody_project_state { get; set; }
    public string custbody_project_zip { get; set; }
    public bool custbody_mechanical { get; set; }
    public bool custbody_electrical { get; set; }
    public bool custbody_plumbing { get; set; }
    public bool custbody_energy_calculations_send { get; set; }
    public bool custbody_site_lighting { get; set; }
    public bool custbody_site_visit { get; set; }
  }

  public class NetSuiteEntityStatus
  {
    public string id { get; set; }
    public string refName { get; set; }
  }

  public class NetSuiteEstimateItem
  {
    public List<NetSuiteItem> items { get; set; }
  }

  public class NetSuiteItem
  {
    public int line { get; set; }
    public NetSuiteLineItem item { get; set; }
    public double rate { get; set; }
    public int quantity;
  }

  public class NetSuiteLineItem
  {
    public int id { get; set; }
  }

  public class NetSuiteEstimateItemCollection
  {
    public int count;
    public bool hasMore;
    public NetSuiteEstimateItemElement[] items;
    public NsLink[] links;
    public int offset;
    public int totalResults;
  }

  public class NetSuiteEstimateItemElement { }

  public class NsLink
  {
    public string href;
    public string rel;
  }

  public class NsResource
  {
    public string externalId;
    public string id;
    public NsLink[] links;
    public string refName;
  }

  public class NetSuiteSubsidiary
  {
    public NsObject country;
    public NetSuiteCurrency currency;
    public string email;
    public string externalId;
    public string fax;
    public string fullName;
    public string id;
    public int internalId;
    public bool isElimination;
    public bool isInactive;
    public string lastModifiedDate;
    public string legalName;
    public NsLink[] links;
    public NetSuiteSubsidiaryMainAddress mainAddress;
  }

  public class NetSuiteSubsidiaryMainAddress
  {
    string addr1;
    public string addr2;
    public string addr3;
    public string addrPhone;
    public string addrText;
    public string addressee;
    public string attention;
    public string city;
    public NsObject country;
    public string externalId;
    public string lastModifiedDate;
    public NsLink[] links;
    public bool Override;
    public string refName;
    public string state;
    public string zip;
  }

  public class NsObject
  {
    public string id;
    public string refName;
  }

  public class NetSuiteCurrency
  {
    public int currencyPrecision;
    public string displaySymbol;
    public double exchangeRate;
    public string externalId;
    public string formatSample;
    public NsObject fxRateUpdateTimezone;
    public string id;
    public bool includeFxRateUpdates;
    public bool isAnchorCurrency;
    public bool isBaseCurrency;
    public bool isInactive;
    public string lastModifiedDate;
    public NsLink[] links;
    public NsObject locale;
    public string name;
    public bool overrideCurrencyFormat;
    public string refName;
    public string symbol;
    public NsObject symbolPlacement;
  }
}
