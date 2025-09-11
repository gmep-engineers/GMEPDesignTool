using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using GMEPDesignTool.Database;
using Mysqlx.Crud;

namespace GMEPDesignTool
{
  public class AdminViewModel : INotifyPropertyChanged
  {
    private string projectNo;

    public string ProjectNo
    {
      get => projectNo;
      set
      {
        if (projectNo != value)
        {
          projectNo = value;
          OnPropertyChanged(nameof(ProjectNo));
        }
      }
    }

    private string projectName;

    public string ProjectName
    {
      get => projectName;
      set
      {
        if (projectName != value)
        {
          projectName = value;
          OnPropertyChanged(nameof(ProjectName));
        }
      }
    }
    private string client;

    private bool addButtonEnabled;
    public bool AddButtonEnabled
    {
      get => addButtonEnabled;
      set
      {
        if (addButtonEnabled != value)
        {
          addButtonEnabled = value;
          OnPropertyChanged(nameof(AddButtonEnabled));
        }
      }
    }

    public string Client
    {
      get => client;
      set
      {
        if (client != value)
        {
          client = value;
          OnPropertyChanged(nameof(Client));
        }
      }
    }

    private string architect;

    public string Architect
    {
      get => architect;
      set
      {
        if (architect != value)
        {
          architect = value;
          OnPropertyChanged(nameof(Architect));
        }
      }
    }

    private string streetaddress;
    public string StreetAddress
    {
      get => streetaddress;
      set
      {
        if (streetaddress != value)
        {
          streetaddress = value;
          OnPropertyChanged(nameof(StreetAddress));
        }
      }
    }
    private string city;
    public string City
    {
      get => city;
      set
      {
        if (city != value)
        {
          city = value;
          OnPropertyChanged(nameof(City));
        }
      }
    }

    public ObservableCollection<string> States { get; set; } =
      new ObservableCollection<string>
      {
        "AL",
        "AK",
        "AZ",
        "AR",
        "CA",
        "CO",
        "CT",
        "DE",
        "FL",
        "GA",
        "HI",
        "ID",
        "IL",
        "IN",
        "IA",
        "KS",
        "KY",
        "LA",
        "ME",
        "MD",
        "MA",
        "MI",
        "MN",
        "MS",
        "MO",
        "MT",
        "NE",
        "NV",
        "NH",
        "NJ",
        "NM",
        "NY",
        "NC",
        "ND",
        "OH",
        "OK",
        "OR",
        "PA",
        "RI",
        "SC",
        "SD",
        "TN",
        "TX",
        "UT",
        "VT",
        "VA",
        "WA",
        "WV",
        "WI",
        "WY",
      };

    private string state;
    public string State
    {
      get => state;
      set
      {
        if (state != value)
        {
          state = value;
          OnPropertyChanged(nameof(State));
        }
      }
    }
    private string postalCode;
    public string PostalCode
    {
      get => postalCode;
      set
      {
        if (postalCode != value)
        {
          postalCode = value;
          OnPropertyChanged(nameof(PostalCode));
        }
      }
    }

    private string fileDictionary;
    public string FileDictionary
    {
      get => fileDictionary;
      set
      {
        if (fileDictionary != value)
        {
          fileDictionary = value;
          OnPropertyChanged(nameof(FileDictionary));
        }
      }
    }

    private bool isCheckedS;
    public bool IsCheckedS
    {
      get => isCheckedS;
      set
      {
        if (isCheckedS != value)
        {
          isCheckedS = value;
          OnPropertyChanged(nameof(IsCheckedS));
        }
      }
    }

    private bool isCheckedM;
    public bool IsCheckedM
    {
      get => isCheckedM;
      set
      {
        if (isCheckedM != value)
        {
          isCheckedM = value;
          OnPropertyChanged(nameof(IsCheckedM));
        }
      }
    }
    private bool isCheckedE;
    public bool IsCheckedE
    {
      get => isCheckedE;
      set
      {
        if (isCheckedE != value)
        {
          isCheckedE = value;
          OnPropertyChanged(nameof(IsCheckedE));
        }
      }
    }
    private bool isCheckedP;
    public bool IsCheckedP
    {
      get => isCheckedP;
      set
      {
        if (isCheckedP != value)
        {
          isCheckedP = value;
          OnPropertyChanged(nameof(IsCheckedP));
        }
      }
    }
    private string descriptions;
    public string Descriptions
    {
      get => descriptions;
      set
      {
        if (descriptions != value)
        {
          descriptions = value;
          OnPropertyChanged(nameof(Descriptions));
        }
      }
    }

    private List<ComboData> clientData = new List<ComboData>();
    public List<ComboData> ClientData
    {
      get { return clientData; }
    }

    private string selectedClientId;
    public string SelectedClientId
    {
      get => selectedClientId;
      set
      {
        if (selectedClientId != value)
        {
          selectedClientId = value;
          OnPropertyChanged(nameof(SelectedClientId));
          if (!string.IsNullOrEmpty(selectedClientId) && !string.IsNullOrEmpty(selectedArchitectId))
          {
            AddButtonEnabled = true;
          }
          else
          {
            AddButtonEnabled = false;
          }
        }
      }
    }

    private List<ComboData> architectData = new List<ComboData>();
    public List<ComboData> ArchitectData
    {
      get { return architectData; }
    }

    private string selectedArchitectId;
    public string SelectedArchitectId
    {
      get => selectedArchitectId;
      set
      {
        if (selectedArchitectId != value)
          selectedArchitectId = value;
        OnPropertyChanged(nameof(SelectedArchitectId));
        if (!string.IsNullOrEmpty(selectedClientId) && !string.IsNullOrEmpty(selectedArchitectId))
        {
          AddButtonEnabled = true;
        }
        else
        {
          AddButtonEnabled = false;
        }
      }
    }

    public Proposal? SelectedProposal { get; set; }

    public AdminViewModel(string projectId)
    {
      LoadProjectInfoAsync(projectId);
    }

    private async void LoadProjectInfoAsync(string projectId)
    {
      var db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);

      var clients = db.GetClients();
      foreach (var client in clients)
      {
        clientData.Add(new ComboData { Id = client.CompanyId, Value = client.CompanyName });
      }
      var architects = db.GetArchitects();
      foreach (var architect in architects)
      {
        architectData.Add(
          new ComboData { Id = architect.CompanyId, Value = architect.CompanyName }
        );
      }

      AdminModel ProjectInfo = await db.GetAdminByProjectId(projectId);
      ProjectNo = ProjectInfo.ProjectNo;
      ProjectName = ProjectInfo.ProjectName;
      Client = ProjectInfo.Client;
      SelectedClientId = ProjectInfo.ClientCompanyId;
      Architect = ProjectInfo.Architect;
      SelectedArchitectId = ProjectInfo.ArchitectCompanyId;
      StreetAddress = ProjectInfo.StreetAddress;
      City = ProjectInfo.City;
      State = ProjectInfo.State;
      PostalCode = ProjectInfo.PostalCode;
      FileDictionary = ProjectInfo.Directory;
      IsCheckedS = ProjectInfo.IsCheckedS;
      IsCheckedM = ProjectInfo.IsCheckedM;
      IsCheckedE = ProjectInfo.IsCheckedE;
      IsCheckedP = ProjectInfo.IsCheckedP;
      Descriptions = ProjectInfo.Descriptions;

      if (!string.IsNullOrEmpty(selectedClientId) && !string.IsNullOrEmpty(selectedArchitectId))
      {
        AddButtonEnabled = true;
      }
      else
      {
        AddButtonEnabled = false;
      }
    }

    public async void DownloadProposal()
    {
      if (SelectedProposal != null)
      {
        if (!String.IsNullOrEmpty(SelectedProposal.PdfName))
        {
          S3 s3 = new S3();
          Trace.WriteLine("namae " + SelectedProposal.PdfName);
          string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
          string downloadPath = System.IO.Path.Combine(desktopPath, SelectedProposal.PdfName);
          await s3.DownloadAndOpenFileAsync(SelectedProposal.PdfName, downloadPath);
          return;
        }
        var db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString); // HERE change this
        Proposal? proposal = await db.GetProposalById(SelectedProposal.Id);
        if (proposal == null)
        {
          return;
        }
        // HERE generate pdf in server and download -or- show error
      }
      else { }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
