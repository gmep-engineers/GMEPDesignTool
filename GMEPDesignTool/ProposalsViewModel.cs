using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        if (proposal.Modified)
        {
          Database.SaveProposal(proposal);
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

    public async void CreateEstimate()
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
        // create estimate in net suite
        // use Authorization, Bearer Token where Token = access_token
      }
    }
  }
}
