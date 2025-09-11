using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Animation;
using static GMEPDesignTool.ProposalCommercialWindow;

namespace GMEPDesignTool
{
  public partial class ProposalCommercialWindow : Window
  {
    static HttpClient httpClient = new HttpClient();
    public Database.S3 s3 = new Database.S3();
    private string proposal_id;
    private Database.Database database;

    public class PDFRequest
    {
      public string ProjectAddress { get; set; }
      public string Client { get; set; }
      public string Architect { get; set; }
      public string ProjectDescriptions { get; set; }
      public string ProjectName { get; set; }
      public string TotalPrice { get; set; }
      public string RetainerPercent { get; set; }
      public string ClientType { get; set; }
      public string ClientContactName { get; set; }
      public string ClientBusinessName { get; set; }
      public string ClientStreetAddress { get; set; }
      public string ClientCityStateZip { get; set; }
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
    }

    private ProposalData? GetProposalData()
    {
      try
      {
        ProposalData proposalData = new ProposalData();

        var vm = DataContext as ProposalCommercialViewModel;
        if (vm == null)
        {
          MessageBox.Show("Please complete the missing fields.");
          return null;
        }

        proposalData.TotalPrice = TotalPriceBox.Text;
        proposalData.HasSiteVisit = vm.HasSiteVisit;
        proposalData.RetainerPercent = RetainerPercentBox.Text;

        proposalData.DateSent = vm.DateSent.Value;

        proposalData.NumMeetings = NumMeetingsBox.Text;

        proposalData.TarrarNo = TarrarNoBox.Text;
        proposalData.DateDrawingsReceived = vm.DateDrawingsReceived.Value;

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
      ProposalData? proposalData = GetProposalData();
      if (proposalData == null)
      {
        MessageBox.Show("Please complete the missing fields.");
        return;
      }

      string jsonString = JsonSerializer.Serialize(proposalData);

      database.SetProposalData(proposal_id, jsonString);
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

      pdfRequest.TotalPrice = TotalPriceBox.Text;

      pdfRequest.RetainerPercent = RetainerPercentBox.Text;

      string selectedClientId = ClientNameComboBox.SelectedValue.ToString();

      Client client = database.GetClient(selectedClientId);

      if (client == null)
      {
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
      pdfRequest.ClientContactName = client.PrimaryContactName;
      pdfRequest.ClientBusinessName = client.CompanyName;
      pdfRequest.ClientStreetAddress = client.StreetAddress;
      string clientCityStateZip = client.City + ", " + client.State + "  " + client.PostalCode;
      pdfRequest.ClientCityStateZip = clientCityStateZip;
      pdfRequest.DateSent = vm.DateSent.Value.ToString("yyyy-MM-dd");
      pdfRequest.NumMeetings = NumMeetingsBox.Text;
      pdfRequest.TarrarNo = TarrarNoBox.Text;
      pdfRequest.DateDrawingsReceived = vm.DateDrawingsReceived.Value.ToString("yyyy-MM-dd");
      pdfRequest.HasSiteVisit = vm.HasSiteVisit;

      pdfRequest.NewConstruction = vm.NewConstruction;

      pdfRequest.HasInitialRecommendationsMeeting = vm.HasInitialRecommendationsMeeting;

      pdfRequest.HasCommericalShellConnection = vm.HasCommericalShellConnection;

      pdfRequest.HasEmergencyPower = vm.HasEmergencyPower;

      pdfRequest.HasIndoorCommonArea = vm.HasIndoorCommonArea;

      pdfRequest.HasGarageExhaust = vm.HasGarageExhaust;

      pdfRequest.HasSiteLighting = vm.HasSiteLighting;

      pdfRequest.Client = vm.AdminViewModel.Client;
      pdfRequest.Architect = vm.AdminViewModel.Architect;
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

      switch (vm.SelectProposalTypeViewModel.TypeId)
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

      response.EnsureSuccessStatusCode();
      var pdfBytes = await response.Content.ReadAsByteArrayAsync();
      string keyName = $"{vm.SelectProposalTypeViewModel.TypeId}-{Guid.NewGuid()}.pdf";
      database.SetProposalPdf(proposal_id, keyName);
      database.SetProposalData(proposal_id, jsonString);
      string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), keyName);
      System.IO.File.WriteAllBytes(tempFilePath, pdfBytes);
      Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
      await s3.UploadFileAsync(keyName, tempFilePath);
    }

    private void TextBox_TextChanged(
      object sender,
      System.Windows.Controls.TextChangedEventArgs e
    ) { }
  }
}
