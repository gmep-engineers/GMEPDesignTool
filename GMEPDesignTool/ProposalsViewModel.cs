using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static Amazon.S3.Util.S3EventNotification;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace GMEPDesignTool
{
  public class ProposalsViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private NetSuiteAuth NetSuiteAuth;
    private LoginResponse LoginResponse;

    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      if (name != "WindowTitle")
      {
        Saved = false;
      }
    }

    private bool _Saved = true;
    public bool Saved
    {
      get => _Saved;
      set
      {
        _Saved = value;
        if (_Saved == true)
        {
          WindowTitle = "Proposals";
        }
        else
        {
          WindowTitle = "Proposals*";
        }
      }
    }
    public ObservableCollection<string> YearSelectionOptions { get; set; }

    Database.Database Database { get; set; }

    public ObservableCollection<Proposal> FilteredProposals { get; set; }

    public List<Client> Clients { get; set; }

    public List<Employee> Employees { get; set; }

    private string windowTitle = "Proposals";
    public string WindowTitle
    {
      get => windowTitle;
      set
      {
        if (windowTitle != value)
        {
          windowTitle = value;
          OnPropertyChanged(nameof(WindowTitle));
        }
      }
    }

    public ProposalsViewModel(LoginResponse loginResponse, NetSuiteAuth netSuiteAuth)
    {
      this.NetSuiteAuth = netSuiteAuth;
      this.LoginResponse = loginResponse;
      int currentYear = DateTime.Now.Year;
      YearSelectionOptions = new ObservableCollection<string>();
      while (currentYear >= 2010)
      {
        YearSelectionOptions.Add(currentYear.ToString());
        currentYear--;
      }
      Database = new Database.Database(loginResponse.SqlConnectionString);

      Clients = Database.GetClients();

      FilteredProposals = new ObservableCollection<Proposal>();

      FilteredProposals.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
        Saved = false;

      Employees = new List<Employee>();

      FilterDataGridByYear(DateTime.Now.Year.ToString());

      Saved = true;
    }

    public void FilterDataGridByYear(string year)
    {
      FilteredProposals.Clear();
      ObservableCollection<Proposal> proposals = Database.GetProposalsByYear(year);
      foreach (Proposal proposal in proposals)
      {
        proposal.ProposalsViewModel = this;
        proposal.New = false;
        FilteredProposals.Add(proposal);
      }

      Employees.Clear();
      List<Employee> employees = Database.GetAdminEmployeesByYear(year);
      foreach (Employee employee in employees)
      {
        Employees.Add(employee);
      }
    }

    public void Save()
    {
      List<Proposal> proposals = new List<Proposal>();
      foreach (Proposal proposal in FilteredProposals)
      {
        if (proposal.New)
        {
          Database.CreateProposal(proposal, LoginResponse.EmployeeId, 0, proposal.ProjectId);
          Database.SetProposalWindowProjectValues(proposal);
        }
        else if (proposal.Modified)
        {
          Database.SaveProposal(proposal);
          Database.SetProposalWindowProjectValues(proposal);
        }
      }
    }

    public async Task RefreshNetSuiteToken()
    {
      var formData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("refresh_token", NetSuiteAuth.refresh_token),
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
      };
      string username = LoginResponse.NetSuiteClientId;
      string password = LoginResponse.NetSuiteClientSecret;
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

      NetSuiteAuth.access_token = authRes.access_token;
      NetSuiteAuth.token_type = authRes.token_type;
      NetSuiteAuth.expires_in = authRes.expires_in;
      int expires_in = 0;
      if (Int32.TryParse(authRes.expires_in, out expires_in))
      {
        // calculate datetime of 3600 seconds from now
        NetSuiteAuth.expires_in = DateTime.Now.AddSeconds(expires_in).ToString();
      }
    }

    public async Task<string> GetNetSuiteCompanyId(string companyName)
    {
      string id = "";
      string q = $"SELECT id, companyName FROM customer WHERE companyName = '{companyName}'";
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
      string pattern = $"(?<={companyName},id:)[0-9]+";
      Match m = Regex.Match(resString, pattern);
      netSuiteCompanyId = m.Value;
      return netSuiteCompanyId;
    }

    private static KeyValuePair<string, string> KV(string key, string value)
    {
      return new KeyValuePair<string, string>(key, value);
    }

    private static string TF(bool b)
    {
      return b ? "T" : "F";
    }

    public async void CreateEstimate(Proposal p)
    {
      if (NetSuiteAuth.refresh_token == null || NetSuiteAuth.expires_in == null)
      {
        OAuthLoginWindow oAuthLoginWindow = new OAuthLoginWindow(LoginResponse, NetSuiteAuth);
        oAuthLoginWindow.Show();
      }

      DateTime expiryDate;
      if (DateTime.TryParse(NetSuiteAuth.expires_in, out expiryDate))
      {
        if (expiryDate < DateTime.Now)
        {
          await RefreshNetSuiteToken();
        }
        // get id of company
        if (string.IsNullOrEmpty(p.CompanyName))
        {
          p.CompanyName = Database.GetCompanyName(p.ClientCompanyId);
        }
        string companyId = await GetNetSuiteCompanyId(p.CompanyName);
        if (string.IsNullOrEmpty(companyId))
        {
          MessageBox.Show($"A company with the name {p.ContactName} was not found in NetSuite.");
          return;
        }
        AdminModel project = await Database.GetAdminByProjectId(p.ProjectId);

        //var formData = new List<KeyValuePair<string, string>>
        //{
        //  //KV("submitter", "Save"),
        //  KV("tranid", project.ProjectName), // Estimate #
        //  KV("entity", companyId), // Client
        //  //KV("type", "estimate"), // Client
        //  //KV("entity_display", p.CompanyName),
        //  //KV("inpt_job", ""),
        //  //KV("job", ""),
        //  //KV("title", ""),
        //  //KV("duedate", ""),
        //  KV("trandate", DateTime.Now.ToShortDateString()),
        //  //KV("custbody22", ""),
        //  //KV("custbody23", ""),
        //  //KV("inpt_entitystatus", "Proposal"),
        //  KV("entitystatus", "10"),
        //  //KV("custbody_project_address", project.StreetAddress),
        //  //KV("custbody_project_city", project.City),
        //  //KV("custbody_project_state", project.State),
        //  //KV("custbody_project_zip", project.PostalCode),
        //  //KV("custbody_mechanical", TF(project.IsCheckedM)),
        //  //KV("custbody_electrical", TF(project.IsCheckedE)),
        //  //KV("custbody_plumbing", TF(project.IsCheckedP)),
        //  //KV("custbody_site_lighting", TF(p.Data.HasSiteLighting)),
        //  //KV("custbody_photometric", TF(p.Data.ElectricalScope.ElectricalLightingDesign)),
        //  //KV("custbody_site_visit", TF(p.Data.HasSiteVisit)),

        //};

        //if (p.TypeId == 1)
        //{
        //  formData.Add(KV("inpt_custbody_project_type", "Commercial"));
        //}
        //if (p.TypeId == 2)
        //{
        //  formData.Add(KV("inpt_custbody_project_type", "Residential"));
        //}



        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Accept.Clear();
        //client.DefaultRequestHeaders.Accept.Add(
        //  new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded")
        //);
        client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json")
        );

        client.DefaultRequestHeaders.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            NetSuiteAuth.access_token
          );

        //var form = new FormUrlEncodedContent(formData);

        //HttpResponseMessage response = await client.PostAsync(
        //  "https://5645740.suitetalk.api.netsuite.com/services/rest/record/v1/estimate",
        //  form
        //);


        NetSuiteEstimate estimate = new NetSuiteEstimate()
        {
          tranId = project.ProjectName + "1",
          tranDate = DateTime.Now.ToString("yyyy-MM-dd"),
          entity = companyId,
          entityStatus = new NetSuiteEntityStatus() { id = "10", refName = "Proposal" },
          expectedCloseDate = DateTime.Now.ToString("yyyy-MM-dd"),
          probability = 50,
          item = new NetSuiteEstimateItem()
          {
            items = new List<NetSuiteItem>()
            {
              new NetSuiteItem()
              {
                line = 1,
                item = new NetSuiteLineItem() { id = 5 }, // ID must be 5 for "Consulting"
                rate = 2000,
                quantity = 1,
              },
            },
          },
        };

        var jsonPayload = JsonSerializer.Serialize(estimate);

        Trace.WriteLine(NetSuiteAuth.access_token);

        Trace.WriteLine(jsonPayload);

        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        Trace.WriteLine(content);

        HttpResponseMessage response = await client.PostAsync(
          "https://5645740.suitetalk.api.netsuite.com/services/rest/record/v1/estimate",
          content
        );

        Trace.WriteLine(await response.Content.ReadAsStringAsync());

        response.EnsureSuccessStatusCode();

        // create estimate in net suite
        // use Authorization, Bearer Token where Token = access_token
      }
    }
  }
}
