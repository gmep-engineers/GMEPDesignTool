using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using GMEPDesignTool.Database;
using Microsoft.Win32;
using MsgReader.Outlook;
using Mysqlx.Crud;
using static GMEPDesignTool.ProposalCommercialWindow;

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

    private bool projectNameReadOnly = true;
    public bool ProjectNameReadOnly
    {
      get => projectNameReadOnly;
      set
      {
        if (projectNameReadOnly != value)
        {
          projectNameReadOnly = value;
          OnPropertyChanged(nameof(ProjectNameReadOnly));
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

    private string selectedClientCompanyId;
    public string SelectedClientCompanyId
    {
      get => selectedClientCompanyId;
      set
      {
        if (selectedClientCompanyId != value)
        {
          selectedClientCompanyId = value;
          OnPropertyChanged(nameof(SelectedClientCompanyId));
          if (
            !string.IsNullOrEmpty(selectedClientCompanyId)
            && !string.IsNullOrEmpty(selectedArchitectCompanyId)
          )
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

    private string selectedArchitectCompanyId;
    public string SelectedArchitectCompanyId
    {
      get => selectedArchitectCompanyId;
      set
      {
        if (selectedArchitectCompanyId != value)
          selectedArchitectCompanyId = value;
        OnPropertyChanged(nameof(SelectedArchitectCompanyId));
        if (
          !string.IsNullOrEmpty(selectedClientCompanyId)
          && !string.IsNullOrEmpty(selectedArchitectCompanyId)
        )
        {
          AddButtonEnabled = true;
        }
        else
        {
          AddButtonEnabled = false;
        }
      }
    }

    private bool enableDownloadRfp = false;
    public bool EnableDownloadRfp
    {
      get => enableDownloadRfp;
      set
      {
        if (enableDownloadRfp != value)
        {
          enableDownloadRfp = value;
          OnPropertyChanged(nameof(EnableDownloadRfp));
        }
      }
    }

    public Proposal? SelectedProposal { get; set; }

    public AdminViewModel(string projectId, LoginResponse loginResponse)
    {
      LoadProjectInfoAsync(projectId);
      if (loginResponse.AccessLevelId == 1)
      {
        ProjectNameReadOnly = false;
      }
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
      SelectedClientCompanyId = ProjectInfo.ClientCompanyId;
      Architect = ProjectInfo.Architect;
      SelectedArchitectCompanyId = ProjectInfo.ArchitectCompanyId;
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

      if (
        !string.IsNullOrEmpty(selectedClientCompanyId)
        && !string.IsNullOrEmpty(selectedArchitectCompanyId)
      )
      {
        AddButtonEnabled = true;
      }
      else
      {
        AddButtonEnabled = false;
      }

      string filename = db.GetLatestRfpFilename(projectId);
      if (!string.IsNullOrEmpty(filename))
      {
        EnableDownloadRfp = true;
      }
    }

    public async void DownloadProposal()
    {
      if (SelectedProposal != null)
      {
        if (!String.IsNullOrEmpty(SelectedProposal.PdfName))
        {
          S3 s3 = new S3();
          string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
          string downloadPath = System.IO.Path.Combine(desktopPath, SelectedProposal.PdfName);
          await s3.DownloadAndOpenFileAsync(SelectedProposal.PdfName, downloadPath);
          return;
        }
        var db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);
        Proposal? proposal = await db.GetProposalById(SelectedProposal.Id);
        if (proposal == null || proposal.Data == null)
        {
          return;
        }

        ProposalData d = proposal.Data;

        HttpClient httpClient = new HttpClient
        {
          BaseAddress = new Uri("http://44.240.61.252:3000/"),
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json")
        );

        HttpResponseMessage response;

        PDFRequest r = new PDFRequest();

        r.TotalPrice = d.TotalPrice;
        r.RetainerPercent = d.RetainerPercent;

        Client client = db.GetClient(SelectedClientCompanyId);

        if (client == null)
        {
          return;
        }

        if (client.LoyaltyTypeId == 1)
        {
          r.ClientType = "loyal";
        }
        if (client.LoyaltyTypeId == 2)
        {
          r.ClientType = "returning";
        }
        if (client.LoyaltyTypeId == 3)
        {
          r.ClientType = "new";
        }
        r.ClientContactName = client.PrimaryContactName;
        r.ClientBusinessName = client.CompanyName;
        r.ClientStreetAddress = client.StreetAddress;
        string clientCityStateZip = client.City + ", " + client.State + "  " + client.PostalCode;
        r.ClientCityStateZip = clientCityStateZip;

        r.DateSent = d.DateSent.ToString("yyyy-MM-dd");
        r.NumMeetings = d.NumMeetings;
        r.TarrarNo = d.TarrarNo;
        r.DateDrawingsReceived = d.DateDrawingsReceived.ToString("yyyy-MM-dd");
        r.HasSiteVisit = d.HasSiteVisit;
        r.NewConstruction = d.NewConstruction;
        r.HasInitialRecommendationsMeeting = d.HasInitialRecommendationsMeeting;
        r.HasCommericalShellConnection = d.HasCommercialShellConnection;
        r.HasEmergencyPower = d.HasEmergencyPower;
        r.HasIndoorCommonArea = d.HasIndoorCommonArea;
        r.HasGarageExhaust = d.HasGarageExhaust;
        r.HasSiteLighting = d.HasSiteLighting;
        r.Client = Client;
        r.Architect = Architect;
        string projectAddress = StreetAddress + ", " + City + ", " + State + " " + PostalCode;
        r.ProjectAddress = projectAddress;
        r.ProjectDescriptions = Descriptions;
        r.ProjectName = ProjectName;

        string mechanicalDescriptions;
        if (
          d.MechanicalScope.MechanicalExhaustSupply
          || d.MechanicalScope.MechanicalHvacEquipSpec
          || d.MechanicalScope.MechanicalTitle24
        )
        {
          if (d.NewConstruction)
          {
            mechanicalDescriptions = "New Construction: engineering for Mechanical design";
          }
          else
          {
            mechanicalDescriptions = "Tenant Improvement: engineering for HVAC design";
          }
          if (d.MechanicalScope.MechanicalExhaustSupply)
            mechanicalDescriptions += ", exhaust and supply";
          if (d.MechanicalScope.MechanicalHvacEquipSpec)
            mechanicalDescriptions += ", HVAC equipment specifications";
          if (d.MechanicalScope.MechanicalTitle24)
            mechanicalDescriptions += ", Title 24";
          r.MechanicalDescriptions = mechanicalDescriptions;
        }

        string structuralDescriptions;
        if (
          d.StructuralScope.StructuralGeoReport
          || d.StructuralScope.StructuralFramingDepths
          || d.StructuralScope.StructuralAnalysis
          || d.StructuralScope.StructuralPlans
          || d.StructuralScope.StructuralDetailsCalculations
          || d.StructuralScope.StructuralCodeCompliance
        )
        {
          if (d.NewConstruction)
          {
            structuralDescriptions = "New Construction: engineering for Structural design";
          }
          else
          {
            structuralDescriptions = "Tenant Improvement: engineering for Structural design";
          }
          if (d.StructuralScope.StructuralGeoReport)
            structuralDescriptions += ", Review geotechnical report and define foundation type";
          if (d.StructuralScope.StructuralFramingDepths)
            structuralDescriptions +=
              ", Perform structural analysis and design for all gravity and lateral load resisting elements";
          if (d.StructuralScope.StructuralAnalysis)
            structuralDescriptions += ", Title 24";
          if (d.StructuralScope.StructuralPlans)
            structuralDescriptions += ", Structural Plans";
          if (d.StructuralScope.StructuralDetailsCalculations)
            structuralDescriptions += ", Details and calculations";
          if (d.StructuralScope.StructuralCodeCompliance)
            structuralDescriptions += ", Building structural to comply with code";
          r.StructuralDescriptions = structuralDescriptions;
        }

        string electricalDescriptions = "New Construction: engineering for Electrical design";
        if (
          d.ElectricalScope.ElectricalPowerDesign
          || d.ElectricalScope.ElectricalServiceLoadCalc
          || d.ElectricalScope.ElectricalSingleLineDiagram
          || d.ElectricalScope.ElectricalLightingDesign
        )
        {
          if (d.NewConstruction)
          {
            electricalDescriptions = "New Construction: engineering for Electrical design";
          }
          else
          {
            electricalDescriptions = "Tenant Improvement: engineering for Electrical design";
          }
          if (d.ElectricalScope.ElectricalPowerDesign)
            electricalDescriptions += ", Electrical power design";
          if (d.ElectricalScope.ElectricalServiceLoadCalc)
            electricalDescriptions += ", Service load calculation";
          if (d.ElectricalScope.ElectricalSingleLineDiagram)
            electricalDescriptions += ", Single line diagrams";
          if (d.ElectricalScope.ElectricalLightingDesign)
            electricalDescriptions += ", Electrical lighting design";
          r.ElectricalDescriptions = electricalDescriptions;
        }

        string plumbingDescriptions;
        if (d.PlumbingScope.PlumbingHotColdWater || d.PlumbingScope.PlumbingWasteVent)
        {
          if (d.NewConstruction)
          {
            plumbingDescriptions = "New Construction: engineering for Plumbing design";
          }
          else
          {
            plumbingDescriptions = "Tenant Improvement: engineering for Plumbing design";
          }
          if (d.PlumbingScope.PlumbingHotColdWater)
            plumbingDescriptions += ", Hot and cold water piping design";
          if (d.PlumbingScope.PlumbingWasteVent)
            plumbingDescriptions += ", Sewer and vent piping design";
          r.PlumbingDescriptions = plumbingDescriptions;
        }

        switch (proposal.TypeId)
        {
          case 1:
            response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/commercial", r);
            break;
          case 2:
            response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/residential", r);
            break;
          case 3:
            response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/t24", r);
            break;
          case 4:
            response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/site-lighting-tarrar", r);
            break;
          case 5:
            response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/2019", r);
            break;
          default:
            return;
        }

        response.EnsureSuccessStatusCode();

        string jsonString = JsonSerializer.Serialize(proposal.Data);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        string keyName = $"{proposal.TypeId}-{Guid.NewGuid()}.pdf";
        db.SetProposalPdf(SelectedProposal.Id, keyName);
        db.SetProposalData(SelectedProposal.Id, jsonString);
        string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), keyName);
        System.IO.File.WriteAllBytes(tempFilePath, pdfBytes);
        Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
        Database.S3 s3db = new Database.S3();
        await s3db.UploadFileAsync(keyName, tempFilePath);
      }
      else { }
    }

    public async void UploadRfp(string projectId, Database.Database db)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Email Files (*.msg;*.eml)|*.msg;*.eml|All files (*.*)|*.*";
      openFileDialog.Multiselect = false;
      openFileDialog.InitialDirectory = Environment.GetFolderPath(
        Environment.SpecialFolder.MyDocuments
      );

      if (openFileDialog.ShowDialog() == true)
      {
        string filePath = openFileDialog.FileName;
        string fileName = openFileDialog.SafeFileName;
        Database.S3 s3 = new Database.S3();
        string storedFilename = Guid.NewGuid().ToString().Substring(0, 6) + "-" + fileName;
        await s3.UploadFileAsync(storedFilename, filePath);

        db.CreateRfp(projectId, storedFilename);

        EnableDownloadRfp = true;

        string companyId = string.Empty;
        if (filePath.EndsWith(".msg"))
        {
          FileInfo fileInfo = new FileInfo(filePath);
          Storage.Message msg = new Storage.Message(filePath);

          string sender = msg.Sender.Email;

          companyId = db.GetContactCompanyIdByEmail(sender);
        }
        if (filePath.EndsWith(".eml"))
        {
          Storage.Message eml = new Storage.Message(filePath);
          if (eml.Headers != null)
          {
            string sender = eml.Headers.Sender.ToString();
            companyId = db.GetContactCompanyIdByEmail(sender);
          }
        }

        if (!string.IsNullOrEmpty(companyId))
        {
          SelectedClientCompanyId = companyId;
        }

        //using (var stream = File.OpenRead(filePath))
        //{
        //  var message = MimeMessage.Load(stream);
        //  Trace.WriteLine(message.Subject);
        //  Trace.WriteLine(message.Body);
        //  Trace.WriteLine(message.From);
        //}
      }
    }

    public async void DownloadRfp(string projectId, Database.Database db)
    {
      string filename = db.GetLatestRfpFilename(projectId);
      if (string.IsNullOrEmpty(filename))
      {
        return;
      }
      string filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{filename}");
      Database.S3 s3 = new Database.S3();
      await s3.DownloadAndOpenFileAsync(filename, filePath);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
