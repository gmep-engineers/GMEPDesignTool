using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Mysqlx.Crud;
using static GMEPDesignTool.ProposalCommercialWindow;
using static MsgReader.Outlook.Storage;

namespace GMEPDesignTool
{
  public partial class ProposalCommercialWindow : Window
  {
    static HttpClient httpClient = new HttpClient();
    public Database.S3 s3 = new Database.S3();
    private string proposal_id;
    private Database.Database database;
    private ObservableCollection<Proposal> proposals;
    string projectId;
    LoginResponse loginResponse;

    Proposal? proposal;

    public class PDFRequest
    {
      public string ProjectAddress { get; set; }
      public string Client { get; set; }
      public string Architect { get; set; }
      public string ProjectDescriptions { get; set; }
      public string ProjectName { get; set; }
      public string TotalPrice { get; set; }
      public string MaxAdminHours { get; set; }
      public string BudgetedAdminHours { get; set; }
      public string RetainerPercent { get; set; }
      public string ClientType { get; set; }
      public string ClientContactName { get; set; }
      public string ClientBusinessName { get; set; }
      public string ClientStreetAddress { get; set; }
      public string ClientCityStateZip { get; set; }
      public string ClientEmail { get; set; }
      public string ClientPhone { get; set; }
      public string DateSent { get; set; }
      public string NumMeetings { get; set; }
      public string TarrarNo { get; set; }
      public bool HasSiteVisit { get; set; }
      public bool NewConstruction { get; set; }
      public bool HasInitialRecommendationsMeeting { get; set; }
      public bool HasCommericalShellConnection { get; set; }
      public bool HasIndoorCommonArea { get; set; }
      public bool HasEmergencyPower { get; set; }
      public bool HasGarageExhaust { get; set; }
      public bool HasSiteLighting { get; set; }
      public string DateDrawingsReceived { get; set; }
      public string StructuralDescriptions { get; set; }
      public string MechanicalDescriptions { get; set; }
      public string ElectricalDescriptions { get; set; }
      public string PlumbingDescriptions { get; set; }
    }

    public ProposalCommercialWindow(
      ProposalCommercialViewModel vm,
      string proposal_id,
      LoginResponse loginResponse,
      AdminViewModel adminVm
    )
    {
      InitializeComponent();

      InitializeWindow(vm, proposal_id, loginResponse);
      vm.WindowTitle = "Proposal Details";
      vm.ProjectNo = adminVm.ProjectNo;
      vm.ProjectName = adminVm.ProjectName;
      vm.ProjectStreetAddress = adminVm.StreetAddress;
      vm.ProjectCity = adminVm.City;
      vm.ProjectState = adminVm.State;
      vm.ProjectPostalCode = adminVm.PostalCode;
      vm.ProjectDescriptions = adminVm.Descriptions;
    }

    public ProposalCommercialWindow(
      ProposalCommercialViewModel vm,
      string proposal_id,
      LoginResponse loginResponse,
      AdminViewModel adminVm,
      ProposalData? proposalData,
      Proposal? proposal
    )
    {
      InitializeComponent();

      InitializeWindow(vm, proposal_id, loginResponse);
      this.proposal = proposal;
      vm.ProjectNo = adminVm.ProjectNo;
      vm.ProjectName = adminVm.ProjectName;
      vm.ProjectStreetAddress = adminVm.StreetAddress;
      vm.ProjectCity = adminVm.City;
      vm.ProjectState = adminVm.State;
      vm.ProjectPostalCode = adminVm.PostalCode;
      vm.ProjectDescriptions = adminVm.Descriptions;
      if (proposal != null)
      {
        vm.SelectedClientCompanyId = proposal.ClientCompanyId;
        vm.ProjectName = proposal.ProjectName;
        vm.TotalPrice = proposal.Fees.ToString();
        vm.Proposal = proposal;
      }
      if (proposalData != null)
      {
        vm.TotalPrice = proposalData.TotalPrice;
        vm.MaxAdminHours = proposalData.MaxAdminHours;
        vm.BudgetedAdminHours = proposalData.BudgetedAdminHours;
        vm.HasSiteVisit = proposalData.HasSiteVisit;
        vm.RetainerPercent = proposalData.RetainerPercent;
        vm.DateSent = proposalData.DateSent;
        vm.NumMeetings = proposalData.NumMeetings;
        vm.TarrarNo = proposalData.TarrarNo;
        vm.DateDrawingsReceived = proposalData.DateDrawingsReceived;
        vm.HasSiteVisit = proposalData.HasSiteVisit;
        vm.NewConstruction = proposalData.NewConstruction;
        vm.HasInitialRecommendationsMeeting = proposalData.HasInitialRecommendationsMeeting;
        vm.HasCommericalShellConnection = proposalData.HasCommercialShellConnection;
        vm.HasEmergencyPower = proposalData.HasEmergencyPower;
        vm.HasIndoorCommonArea = proposalData.HasIndoorCommonArea;
        vm.HasGarageExhaust = proposalData.HasGarageExhaust;
        vm.HasSiteVisit = proposalData.HasSiteVisit;

        vm.MechanicalExhaustSupply = proposalData.MechanicalScope.MechanicalExhaustSupply;
        vm.MechanicalHvacEquipSpec = proposalData.MechanicalScope.MechanicalHvacEquipSpec;
        vm.MechanicalTitle24 = proposalData.MechanicalScope.MechanicalTitle24;

        vm.StructuralPlans = proposalData.StructuralScope.StructuralPlans;
        vm.StructuralDetailsCalculations = proposalData
          .StructuralScope
          .StructuralDetailsCalculations;
        vm.StructuralFramingDepths = proposalData.StructuralScope.StructuralFramingDepths;
        vm.StructuralAnalysis = proposalData.StructuralScope.StructuralAnalysis;
        vm.StructuralCodeCompliance = proposalData.StructuralScope.StructuralCodeCompliance;
        vm.StructuralGeoReport = proposalData.StructuralScope.StructuralGeoReport;

        vm.ElectricalLightingDesign = proposalData.ElectricalScope.ElectricalLightingDesign;
        vm.ElectricalPowerDesign = proposalData.ElectricalScope.ElectricalPowerDesign;
        vm.ElectricalServiceLoadCalc = proposalData.ElectricalScope.ElectricalServiceLoadCalc;
        vm.ElectricalSingleLineDiagram = proposalData.ElectricalScope.ElectricalSingleLineDiagram;
        vm.ElectricalPhotometric = proposalData.ElectricalScope.ElectricalPhotometric;

        vm.PlumbingWasteVent = proposalData.PlumbingScope.PlumbingWasteVent;
        vm.PlumbingHotColdWater = proposalData.PlumbingScope.PlumbingHotColdWater;
      }
      vm.WindowTitle = "Proposal Details";
      vm.Saved = true;
    }

    private void InitializeWindow(
      ProposalCommercialViewModel vm,
      string proposal_id,
      LoginResponse loginResponse
    )
    {
      InitializeComponent();

      httpClient = new HttpClient { BaseAddress = new Uri("http://44.240.61.252:3000/") };
      httpClient.DefaultRequestHeaders.Accept.Clear();
      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
      );

      this.DataContext = vm;
      this.proposal_id = proposal_id;

      database = new Database.Database(loginResponse.SqlConnectionString);
      this.loginResponse = loginResponse;

      string comparableProject = database.GetComparableProject(
        vm.AdminViewModel.StreetAddress,
        vm.AdminViewModel.PostalCode,
        vm.AdminViewModel.ProjectNo
      );
      if (!String.IsNullOrEmpty(comparableProject))
      {
        vm.ComparableProjectNo = comparableProject;
        vm.WarningText = "A project with a similar address already exists: " + comparableProject;
        vm.WarningVisibility = Visibility.Visible;
      }
    }

    private async Task<AdminModel?> GetProjectData()
    {
      try
      {
        AdminModel projectData = new AdminModel();

        var vm = DataContext as ProposalCommercialViewModel;
        if (vm == null)
        {
          MessageBox.Show("Please complete the missing fields.");
          return null;
        }

        Dictionary<int, string> projectIds = await database.GetProjectIds(vm.ProjectNo);
        projectId = projectIds.First().Value;
        AdminModel fixedProjectData = await database.GetAdminByProjectId(projectId);

        projectData.ProjectNo = vm.ProjectNo;
        projectData.ProjectName = vm.ProjectName;
        projectData.StreetAddress = vm.ProjectStreetAddress;
        projectData.City = vm.ProjectCity;
        projectData.State = vm.ProjectState;
        projectData.PostalCode = vm.ProjectPostalCode;
        projectData.Descriptions = vm.ProjectDescriptions;
        projectData.Client = fixedProjectData.Client;
        projectData.Architect = fixedProjectData.Architect;
        projectData.Directory = fixedProjectData.Directory;

        projectData.IsCheckedS = false;
        projectData.IsCheckedM = false;
        projectData.IsCheckedE = false;
        projectData.IsCheckedP = false;
        if (
          vm.StructuralAnalysis
          || vm.StructuralCodeCompliance
          || vm.StructuralDetailsCalculations
          || vm.StructuralFramingDepths
          || vm.StructuralGeoReport
          || vm.StructuralPlans
        )
        {
          projectData.IsCheckedS = true;
        }
        if (vm.MechanicalExhaustSupply || vm.MechanicalHvacEquipSpec || vm.MechanicalTitle24)
        {
          projectData.IsCheckedM = true;
        }
        if (
          vm.ElectricalLightingDesign
          || vm.ElectricalPowerDesign
          || vm.ElectricalServiceLoadCalc
          || vm.ElectricalSingleLineDiagram
          || vm.ElectricalPhotometric
        )
        {
          projectData.IsCheckedE = true;
        }
        if (vm.PlumbingHotColdWater || vm.PlumbingWasteVent)
        {
          projectData.IsCheckedP = true;
        }
        return projectData;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    private ProposalData? GetProposalData()
    {
      try
      {
        ProposalData proposalData = new ProposalData();

        var vm = DataContext as ProposalCommercialViewModel;
        if (vm == null)
        {
          return null;
        }

        proposalData.TotalPrice = TotalPriceBox.Text;
        proposalData.BudgetedAdminHours = vm.BudgetedAdminHours;
        proposalData.MaxAdminHours = vm.MaxAdminHours;
        proposalData.HasSiteVisit = vm.HasSiteVisit;
        proposalData.RetainerPercent = RetainerPercentBox.Text;

        proposalData.DateSent = vm.DateSent != null ? vm.DateSent.Value : DateTime.MinValue;

        proposalData.NumMeetings = NumMeetingsBox.Text;

        proposalData.TarrarNo = TarrarNoBox.Text;
        proposalData.DateDrawingsReceived =
          vm.DateDrawingsReceived != null ? vm.DateDrawingsReceived.Value : DateTime.MinValue;

        proposalData.HasSiteVisit = vm.HasSiteVisit;

        proposalData.NewConstruction = vm.NewConstruction;

        proposalData.HasInitialRecommendationsMeeting = vm.HasInitialRecommendationsMeeting;

        proposalData.HasCommercialShellConnection = vm.HasCommericalShellConnection;

        proposalData.HasEmergencyPower = vm.HasEmergencyPower;

        proposalData.HasIndoorCommonArea = vm.HasIndoorCommonArea;

        proposalData.HasGarageExhaust = vm.HasGarageExhaust;

        proposalData.HasSiteLighting = vm.HasSiteLighting;

        MechanicalScope mechanicalScope = new MechanicalScope();
        mechanicalScope.MechanicalExhaustSupply = vm.MechanicalExhaustSupply;
        mechanicalScope.MechanicalHvacEquipSpec = vm.MechanicalHvacEquipSpec;
        mechanicalScope.MechanicalTitle24 = vm.MechanicalTitle24;
        proposalData.MechanicalScope = mechanicalScope;

        StructuralScope structuralScope = new StructuralScope();
        structuralScope.StructuralGeoReport = vm.StructuralGeoReport;
        structuralScope.StructuralFramingDepths = vm.StructuralFramingDepths;
        structuralScope.StructuralAnalysis = vm.StructuralAnalysis;
        structuralScope.StructuralPlans = vm.StructuralPlans;
        structuralScope.StructuralDetailsCalculations = vm.StructuralDetailsCalculations;
        structuralScope.StructuralCodeCompliance = vm.StructuralCodeCompliance;
        proposalData.StructuralScope = structuralScope;

        ElectricalScope electricalScope = new ElectricalScope();
        electricalScope.ElectricalPowerDesign = vm.ElectricalPowerDesign;
        electricalScope.ElectricalLightingDesign = vm.ElectricalLightingDesign;
        electricalScope.ElectricalSingleLineDiagram = vm.ElectricalSingleLineDiagram;
        electricalScope.ElectricalServiceLoadCalc = vm.ElectricalServiceLoadCalc;
        electricalScope.ElectricalPhotometric = vm.ElectricalPhotometric;
        proposalData.ElectricalScope = electricalScope;

        PlumbingScope plumbingScope = new PlumbingScope();
        plumbingScope.PlumbingHotColdWater = vm.PlumbingHotColdWater;
        plumbingScope.PlumbingWasteVent = vm.PlumbingWasteVent;
        proposalData.PlumbingScope = plumbingScope;

        return proposalData;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    private void SaveClick(object sender, EventArgs e)
    {
      Save();
    }

    private async void Save(CancelEventArgs? e = null)
    {
      ProposalData? proposalData = GetProposalData();
      if (proposalData == null)
      {
        MessageBox.Show("Please complete the missing fields.");
        if (e != null)
        {
          e.Cancel = true;
        }
        return;
      }

      AdminModel? projectData = await GetProjectData();
      if (projectData == null)
      {
        MessageBox.Show("Please complete the missing fields.");
        if (e != null)
        {
          e.Cancel = true;
        }
        return;
      }

      string jsonString = JsonSerializer.Serialize(proposalData);

      database.SetProposalData(proposal_id, jsonString);
      await database.UpdateAdminProject(projectData, projectId);

      if (proposal != null)
      {
        proposal.Data = proposalData;
        var totalPrice = 0;
        Int32.TryParse(proposalData.TotalPrice, out totalPrice);
        proposal.Fees = totalPrice;
      }

      var vm = DataContext as ProposalCommercialViewModel;
      if (proposal != null)
      {
        proposal.ProjectName = vm.ProjectName;
        proposal.ProjectNo = vm.ProjectNo;
      }
      vm.Saved = true;
    }

    private void ProposalGrid_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
      {
        Save();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      var vm = DataContext as ProposalCommercialViewModel;
      if (vm != null && !vm.Saved)
      {
        MessageBoxResult result = MessageBox.Show(
          "Save changes?",
          "Confirmation",
          MessageBoxButton.YesNoCancel
        );
        if (result == MessageBoxResult.Yes)
        {
          Save(e);
          base.OnClosing(e);
        }
        else if (result == MessageBoxResult.No)
        {
          base.OnClosing(e);
        }
        else
        {
          e.Cancel = true;
        }
      }
      else
      {
        base.OnClosing(e);
      }
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
      PDFRequest pdfRequest = new PDFRequest();
      var vm = DataContext as ProposalCommercialViewModel;
      if (vm == null)
      {
        MessageBox.Show("Please complete the missing fields.");
        return;
      }
      ProposalData? proposalData = GetProposalData();
      if (proposalData == null)
      {
        MessageBox.Show("Please complete the missing fields.");
        return;
      }
      if (vm.WarningVisibility == Visibility.Visible)
      {
        MessageBox.Show("Resolve warnings before continuing");
      }

      pdfRequest.TotalPrice = TotalPriceBox.Text.Trim();

      pdfRequest.MaxAdminHours = MaxAdminHours.Text.Trim();

      if (
        !string.IsNullOrEmpty(pdfRequest.MaxAdminHours)
        && !Int32.TryParse(pdfRequest.MaxAdminHours, out _)
      )
      {
        MessageBox.Show("Max Admin Hours must be a number");
        return;
      }

      pdfRequest.BudgetedAdminHours = BudgetedAdminHours.Text.Trim();

      if (
        !string.IsNullOrEmpty(pdfRequest.BudgetedAdminHours)
        && !Int32.TryParse(pdfRequest.MaxAdminHours, out _)
      )
      {
        MessageBox.Show("Budgeted Admin Hours must be a number");
        return;
      }

      pdfRequest.BudgetedAdminHours = BudgetedAdminHours.Text.Trim();

      pdfRequest.RetainerPercent = RetainerPercentBox.Text.Trim();

      if (String.IsNullOrEmpty(pdfRequest.TotalPrice))
      {
        MessageBox.Show("Project must have a total price");
      }

      if (ClientNameComboBox.SelectedItem == null)
      {
        MessageBox.Show("Client name not set.");
        return;
      }

      string selectedClientCompanyId = ClientNameComboBox.SelectedValue.ToString();

      Client client = database.GetClient(selectedClientCompanyId);

      if (client == null)
      {
        MessageBox.Show("Client not found in database.");
        return;
      }

      if (client.LoyaltyTypeId == 1)
      {
        pdfRequest.ClientType = "loyal";
      }
      if (client.LoyaltyTypeId == 2)
      {
        pdfRequest.ClientType = "returning";
      }
      if (client.LoyaltyTypeId == 3)
      {
        pdfRequest.ClientType = "new";
      }

      pdfRequest.ClientPhone = client.CompanyPhone.ToString();
      pdfRequest.ClientEmail = client.CompanyEmail;

      if (proposal != null && proposal.ContactId != client.PrimaryContactId)
      {
        Contact? contact = database.GetContact(proposal.ContactId);
        if (contact != null)
        {
          pdfRequest.ClientContactName = contact.FirstName + " " + contact.LastName;
          if (!string.IsNullOrEmpty(contact.EmailAddress))
          {
            pdfRequest.ClientEmail = contact.EmailAddress;
          }

          if (contact.PhoneNumber != null)
          {
            pdfRequest.ClientPhone = contact.PhoneNumber.ToString();
          }
        }
      }
      else
      {
        pdfRequest.ClientContactName = client.PrimaryContactName;
        if (!string.IsNullOrEmpty(client.CompanyEmail))
        {
          pdfRequest.ClientEmail = client.CompanyEmail;
        }

        if (client.CompanyPhone != null)
        {
          pdfRequest.ClientPhone = client.CompanyPhone.ToString();
        }
      }

      pdfRequest.ClientBusinessName = client.CompanyName;
      pdfRequest.ClientStreetAddress = client.StreetAddress;
      string clientCityStateZip = client.City + ", " + client.State + "  " + client.PostalCode;
      pdfRequest.ClientCityStateZip = clientCityStateZip;
      if (vm.DateSent == null || vm.DateSent == DateTime.MinValue)
      {
        MessageBox.Show("Please fill in the Date Sent.");
        return;
      }
      pdfRequest.DateSent =
        vm.DateSent != null
          ? vm.DateSent.Value.ToString("yyyy-MM-dd")
          : DateTime.MinValue.ToString("yyyy-MM-dd");

      pdfRequest.NumMeetings = NumMeetingsBox.Text;
      pdfRequest.TarrarNo = TarrarNoBox.Text;
      if (vm.DateDrawingsReceived == null || vm.DateDrawingsReceived == DateTime.MinValue)
      {
        MessageBox.Show("Please fill in the Date Drawings Received.");
        return;
      }
      pdfRequest.DateDrawingsReceived =
        vm.DateDrawingsReceived != null
          ? vm.DateDrawingsReceived.Value.ToString("yyyy-MM-dd")
          : DateTime.MinValue.ToString("yyyy-MM-dd");
      pdfRequest.HasSiteVisit = vm.HasSiteVisit;

      pdfRequest.NewConstruction = vm.NewConstruction;

      pdfRequest.HasInitialRecommendationsMeeting = vm.HasInitialRecommendationsMeeting;

      pdfRequest.HasCommericalShellConnection = vm.HasCommericalShellConnection;

      pdfRequest.HasEmergencyPower = vm.HasEmergencyPower;

      pdfRequest.HasIndoorCommonArea = vm.HasIndoorCommonArea;

      pdfRequest.HasGarageExhaust = vm.HasGarageExhaust;

      pdfRequest.HasSiteLighting = vm.HasSiteLighting;

      pdfRequest.Client =
        (proposal != null ? proposal.ClientCompanyName : null) ?? vm.AdminViewModel.Client;
      pdfRequest.Architect =
        (proposal != null ? proposal.ArchitectCompanyName : null) ?? vm.AdminViewModel.Architect;
      if (string.IsNullOrEmpty(vm.AdminViewModel.StreetAddress))
      {
        MessageBox.Show("Please fill in street address");
        return;
      }
      if (string.IsNullOrEmpty(vm.AdminViewModel.City))
      {
        MessageBox.Show("Please fill in city");
        return;
      }
      if (string.IsNullOrEmpty(vm.AdminViewModel.State))
      {
        MessageBox.Show("Please fill in state");
        return;
      }
      if (string.IsNullOrEmpty(vm.AdminViewModel.PostalCode))
      {
        MessageBox.Show("Please fill in postal code");
        return;
      }
      string projectAddress =
        vm.AdminViewModel.StreetAddress
        + ", "
        + vm.AdminViewModel.City
        + ", "
        + vm.AdminViewModel.State
        + " "
        + vm.AdminViewModel.PostalCode;

      pdfRequest.ProjectAddress = projectAddress;
      pdfRequest.ProjectDescriptions = vm.AdminViewModel.Descriptions;
      pdfRequest.ProjectName = vm.AdminViewModel.ProjectName;

      string mechanicalDescriptions;
      if (vm.MechanicalExhaustSupply || vm.MechanicalHvacEquipSpec || vm.MechanicalTitle24)
      {
        if (vm.NewConstruction)
        {
          mechanicalDescriptions = "New Construction: engineering for Mechanical design";
        }
        else
        {
          mechanicalDescriptions = "Tenant Improvement: engineering for HVAC design";
        }
        if (vm.MechanicalExhaustSupply)
          mechanicalDescriptions += ", exhaust and supply";
        if (vm.MechanicalHvacEquipSpec)
          mechanicalDescriptions += ", HVAC equipment specifications";
        if (vm.MechanicalTitle24)
          mechanicalDescriptions += ", Title 24";
        pdfRequest.MechanicalDescriptions = mechanicalDescriptions;
      }

      string structuralDescriptions;
      if (
        vm.StructuralGeoReport
        || vm.StructuralFramingDepths
        || vm.StructuralAnalysis
        || vm.StructuralPlans
        || vm.StructuralDetailsCalculations
        || vm.StructuralCodeCompliance
      )
      {
        if (vm.NewConstruction)
        {
          structuralDescriptions = "New Construction: engineering for Structural design";
        }
        else
        {
          structuralDescriptions = "Tenant Improvement: engineering for Structural design";
        }
        if (vm.StructuralGeoReport)
          structuralDescriptions += ", Review geotechnical report and define foundation type";
        if (vm.StructuralFramingDepths)
          structuralDescriptions +=
            ", Perform structural analysis and design for all gravity and lateral load resisting elements";
        if (vm.StructuralAnalysis)
          structuralDescriptions += ", Title 24";
        if (vm.StructuralPlans)
          structuralDescriptions += ", Structural Plans";
        if (vm.StructuralDetailsCalculations)
          structuralDescriptions += ", Details and calculations";
        if (vm.StructuralCodeCompliance)
          structuralDescriptions += ", Building structural to comply with code";
        pdfRequest.StructuralDescriptions = structuralDescriptions;
      }

      string electricalDescriptions = "New Construction: engineering for Electrical design";
      if (
        vm.ElectricalPowerDesign
        || vm.ElectricalServiceLoadCalc
        || vm.ElectricalSingleLineDiagram
        || vm.ElectricalLightingDesign
        || vm.ElectricalPhotometric
      )
      {
        if (vm.NewConstruction)
        {
          electricalDescriptions = "New Construction: engineering for Electrical design";
        }
        else
        {
          electricalDescriptions = "Tenant Improvement: engineering for Electrical design";
        }
        if (vm.ElectricalPowerDesign)
          electricalDescriptions += ", Electrical power design";
        if (vm.ElectricalServiceLoadCalc)
          electricalDescriptions += ", Service load calculation";
        if (vm.ElectricalSingleLineDiagram)
          electricalDescriptions += ", Single line diagrams";
        if (vm.ElectricalLightingDesign)
          electricalDescriptions += ", Electrical lighting design";
        if (vm.ElectricalPhotometric)
          electricalDescriptions += ", Electrical photometric";

        if (
          electricalDescriptions.EndsWith(
            "engineering for Electrical design, Electrical photometric"
          )
        )
        {
          electricalDescriptions = electricalDescriptions.Replace(
            "engineering for Electrical design, Electrical photometric",
            "Electrical photometric"
          );
        }
        pdfRequest.ElectricalDescriptions = electricalDescriptions;
      }

      string plumbingDescriptions;
      if (vm.PlumbingHotColdWater || vm.PlumbingWasteVent)
      {
        if (vm.NewConstruction)
        {
          plumbingDescriptions = "New Construction: engineering for Plumbing design";
        }
        else
        {
          plumbingDescriptions = "Tenant Improvement: engineering for Plumbing design";
        }
        if (vm.PlumbingHotColdWater)
          plumbingDescriptions += ", Hot and cold water piping design";
        if (vm.PlumbingWasteVent)
          plumbingDescriptions += ", Sewer and vent piping design";
        pdfRequest.PlumbingDescriptions = plumbingDescriptions;
      }

      string jsonString = JsonSerializer.Serialize(proposalData);

      HttpResponseMessage response;

      int typeId = vm.SelectProposalTypeViewModel.TypeId;

      if (proposal != null)
      {
        typeId = proposal.TypeId;
      }

      switch (typeId)
      {
        case 1:
          response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/commercial", pdfRequest);
          break;
        case 2:
          response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/residential", pdfRequest);
          break;
        case 3:
          response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/t24", pdfRequest);
          break;
        case 4:
          response = await httpClient.PostAsJsonAsync(
            "api/wkhtmltopdf/site-lighting-tarrar",
            pdfRequest
          );
          break;
        case 5:
          response = await httpClient.PostAsJsonAsync("api/wkhtmltopdf/2019", pdfRequest);
          break;
        default:
          MessageBox.Show("Invalid proposal type selected.");
          return;
      }

      try
      {
        response.EnsureSuccessStatusCode();
      }
      catch (System.Net.Http.HttpRequestException ex)
      {
        MessageBox.Show(
          "Could not generate pdf. Check for missing fields and ensure at least one of the SMEP checkboxes are selected."
        );
        return;
      }
      var pdfBytes = await response.Content.ReadAsByteArrayAsync();
      string keyName = $"{pdfRequest.ProjectName} Proposal.pdf";

      proposal_id = await database.DuplicateProposal(proposal_id, loginResponse.EmployeeId);

      database.SetProposalPdf(proposal_id, keyName);
      database.SetProposalData(proposal_id, jsonString);

      if (proposal != null)
      {
        proposal.Id = proposal_id;
        proposal.PdfName = keyName;
        proposal.Data = proposalData;
      }

      string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), keyName);
      try
      {
        System.IO.File.WriteAllBytes(tempFilePath, pdfBytes);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Could not save PDF. Check if file with the same name is currently open.");
        return;
      }
      Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
      await s3.UploadFileAsync(keyName, tempFilePath);
      int currentYear = DateTime.Now.Year;
      try
      {
        Directory.CreateDirectory(
          $"S:\\Projects\\Projects\\{currentYear}\\{client.CompanyName}\\{pdfRequest.ProjectName}"
        );
      }
      catch (Exception ex) { }
      string destinationPath =
        $"S:\\Projects\\Projects\\{currentYear}\\{client.CompanyName}\\{pdfRequest.ProjectName}\\${keyName}";
      try
      {
        System.IO.File.Copy(tempFilePath, destinationPath, true);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    private void TextBox_TextChanged(
      object sender,
      System.Windows.Controls.TextChangedEventArgs e
    ) { }
  }
}
