using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ClientSelectionViewModel : INotifyPropertyChanged
  {
    Database.Database db { get; set; }
    Proposal Proposal { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private NetSuiteAuth NetSuiteAuth;
    private ObservableCollection<ClientSearchResult> _clients;
    public ObservableCollection<ClientSearchResult> Clients
    {
      get { return _clients; }
      set
      {
        _clients = value;
        OnPropertyChanged("Client");
      }
    }

    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public ClientSelectionViewModel(
      LoginResponse loginResponse,
      NetSuiteAuth netSuiteAuth,
      Proposal proposal
    )
    {
      db = new Database.Database(loginResponse.SqlConnectionString);
      Clients = new ObservableCollection<ClientSearchResult>();
      Proposal = proposal;

      NetSuiteAuth = netSuiteAuth;
    }

    public async Task SearchNetSuitCompanyNames(string searchStr)
    {
      Clients.Clear();
      string id = "";
      string q = $"SELECT id, companyName FROM customer WHERE companyName LIKE '%{searchStr}%'";
      NetSuiteQueryRequest query = new NetSuiteQueryRequest(q);

      HttpClient client = new HttpClient();

      client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", NetSuiteAuth.access_token);

      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
      );
      client.DefaultRequestHeaders.Add("Prefer", "transient");
      HttpResponseMessage response = await client.PostAsJsonAsync(
        "https://5645740.suitetalk.api.netsuite.com/services/rest/query/v1/suiteql?limit=5",
        query
      );

      response.EnsureSuccessStatusCode();
      string resString = await response.Content.ReadAsStringAsync();
      string netSuiteCompanyId = "";
      resString = resString.Replace("\"", "");
      int startIndex = resString.IndexOf("companyname:");
      if (startIndex == -1)
      {
        return;
      }
      startIndex += "companyname:".Length;
      int endIndex = resString.IndexOf(",id:");
      if (endIndex == -1)
      {
        return;
      }
      string companyName = resString.Substring(startIndex, endIndex - startIndex);
      string idPattern = $"(?<={companyName},id:)[0-9]+";
      Match m = Regex.Match(resString, idPattern);
      netSuiteCompanyId = m.Value;
      ClientSearchResult res = new ClientSearchResult();
      res.Name = companyName;
      res.Id = netSuiteCompanyId;
      Clients.Add(res);
    }

    public async Task SetClientCompanyId(ClientSearchResult c)
    {
      Client? client = db.GetClient(c.Id);
      if (client == null)
      {
        // HERE import into database from netsuite
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://44.240.61.252:3000/");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json")
        );
        CustomerJson json = new CustomerJson();
        json.CustomerId = c.Id;
        json.Token = NetSuiteAuth.access_token;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
          "api/netsuite/sync-client",
          json
        );
        response.EnsureSuccessStatusCode();
        client = db.GetClient(c.Id);
      }
      if (client != null)
      {
        Proposal.ClientCompanyId = client.CompanyId;
      }
    }
  }

  public class CustomerJson
  {
    public string CustomerId { get; set; }
    public string Token { get; set; }
  }

  public class ClientSearchResult
  {
    public string Name { get; set; }
    public string Id { get; set; }
  }
}
