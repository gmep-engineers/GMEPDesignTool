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
using System.Windows;
using MySqlX.XDevAPI;

namespace GMEPDesignTool
{
  public class ClientSelectionViewModel : INotifyPropertyChanged
  {
    public Database.Database db { get; set; }
    public Proposal Proposal { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private NetSuiteAuth NetSuiteAuth;

    private Visibility _NetSuiteAuthWarningVisibility = Visibility.Collapsed;
    public Visibility NetSuiteAuthWarningVisibility
    {
      get { return _NetSuiteAuthWarningVisibility; }
      set
      {
        _NetSuiteAuthWarningVisibility = value;
        OnPropertyChanged("NetSuiteAuthWarningVisibility");
      }
    }

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

    private string _currentCompanyName;
    public string CurrentCompanyName
    {
      get { return _currentCompanyName; }
      set
      {
        if (_currentCompanyName != value)
        {
          _currentCompanyName = value;
          OnPropertyChanged("CurrentCompanyName");
        }
      }
    }

    private ClientSearchResult _selectedClient;
    public ClientSearchResult SelectedClient
    {
      get { return _selectedClient; }
      set
      {
        if (_selectedClient != value)
        {
          _selectedClient = value;
          OnPropertyChanged("SelectedClient");
        }
      }
    }

    public string CurrentCompanyId { get; set; }

    private bool SetAsArchitect { get; set; }

    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public ClientSelectionViewModel(
      LoginResponse loginResponse,
      NetSuiteAuth netSuiteAuth,
      Proposal proposal,
      bool setAsArchitect = false
    )
    {
      db = new Database.Database(loginResponse.SqlConnectionString);
      Clients = new ObservableCollection<ClientSearchResult>();
      Proposal = proposal;

      if (setAsArchitect)
      {
        if (!String.IsNullOrEmpty(proposal.ArchitectCompanyName))
        {
          CurrentCompanyName = proposal.ArchitectCompanyName;
          CurrentCompanyId = proposal.ArchitectCompanyId;
        }
      }
      else
      {
        if (!String.IsNullOrEmpty(proposal.ClientCompanyName))
        {
          CurrentCompanyName = proposal.ClientCompanyName;
          CurrentCompanyId = proposal.ClientCompanyId;
        }
      }

      NetSuiteAuth = netSuiteAuth;

      if (string.IsNullOrEmpty(NetSuiteAuth.access_token))
      {
        NetSuiteAuthWarningVisibility = Visibility.Visible;
      }
      SetAsArchitect |= setAsArchitect;
    }

    public async Task SearchNetSuitCompanyNames(string searchStr)
    {
      if (string.IsNullOrEmpty(NetSuiteAuth.access_token))
      {
        return;
      }
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

    private async Task ImportCompanyFromNetSuite(ClientSearchResult c)
    {
      HttpClient httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri("http://44.240.61.252:3000/");
      httpClient.DefaultRequestHeaders.Accept.Clear();
      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
      );
      CustomerJson json = new CustomerJson();
      json.CustomerId = c.Id;
      json.Token = NetSuiteAuth.access_token;
      json.IsArchitect = SetAsArchitect;
      HttpResponseMessage response = await httpClient.PostAsJsonAsync(
        "api/netsuite/sync-client",
        json
      );
      response.EnsureSuccessStatusCode();
    }

    public async Task SetClientCompanyId(ClientSearchResult c)
    {
      if (SetAsArchitect)
      {
        Architect architect = db.GetArchitect(c.Id);
        if (architect == null)
        {
          await ImportCompanyFromNetSuite(c);
          architect = db.GetArchitect(c.Id);
        }
        if (architect != null)
        {
          CurrentCompanyId = architect.CompanyId;
          CurrentCompanyName = architect.CompanyName;
          Proposal.ArchitectCompanyId = CurrentCompanyId;
          Proposal.ArchitectCompanyName = CurrentCompanyName;
        }
      }
      else
      {
        Client? client = db.GetClient(c.Id);
        if (client == null)
        {
          await ImportCompanyFromNetSuite(c);
          client = db.GetClient(c.Id);
        }
        if (client != null)
        {
          CurrentCompanyId = client.CompanyId;
          CurrentCompanyName = client.CompanyName;
          Proposal.ClientCompanyId = CurrentCompanyId;
          Proposal.ClientCompanyName = CurrentCompanyName;
          Proposal.db = db;
          Proposal.Contacts = db.GetProposalContacts(Proposal.ClientCompanyId);
        }
      }
    }
  }

  public class CustomerJson
  {
    public string CustomerId { get; set; }
    public string Token { get; set; }
    public bool IsArchitect { get; set; }
  }

  public class ClientSearchResult
  {
    public string Name { get; set; }
    public string Id { get; set; }
  }
}
