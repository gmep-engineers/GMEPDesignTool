using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Windows;

namespace GMEPDesignTool
{

    public partial class ProposalCommercialWindow : Window
    {
        static HttpClient client = new HttpClient();
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

            
            public string StructuralDescriptions { get; set; }           
            public string MechanicalDescriptions { get; set; }
            public string ElectricalDescriptions { get; set; }
            public string PlumbingDescriptions { get; set; }



        }
        public ProposalCommercialWindow(ProposalCommercialViewModel vm)
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://44.240.61.252:3000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            this.DataContext = vm;
        }

        private async void generate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click OK to get PDF");
            PDFRequest pdfRequest = new PDFRequest();
            var vm = DataContext as ProposalCommercialViewModel;
            if (vm == null) {
                
                    MessageBox.Show("DataContext is null or not of expected type.");
                    return;              
            }
            try
            {
                pdfRequest.MechanicalDescriptions = "MEP design, Title 24 calculations";

                pdfRequest.TotalPrice = TotalPriceBox.Text;
                pdfRequest.RetainerPercent = RetainerPercentBox.Text;
                pdfRequest.ClientType = vm.ClientType;
                pdfRequest.ClientContactName = ClientContactNameBox.Text;
                pdfRequest.ClientBusinessName = ClientBusinessNameBox.Text;
                pdfRequest.ClientStreetAddress = ClientStreetAddressBox.Text;

                string clientCityStateZip = ClientCityBox.Text + " , " + ClientStateBox.Text + "  " + ClientZipcodeBox.Text;
                pdfRequest.ClientCityStateZip = clientCityStateZip;
                pdfRequest.DateSent = vm.DateSent.Value.ToString("yyyy-MM-dd");
                pdfRequest.NumMeetings = NumMeetingsBox.Text;
                pdfRequest.TarrarNo = TarrarNoBox.Text;

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
                string projectAddress = vm.AdminViewModel.StreetAddress+", " + vm.AdminViewModel.City + ", " + vm.AdminViewModel.State + " " + vm.AdminViewModel.PostalCode;
                pdfRequest.ProjectAddress = projectAddress;
                pdfRequest.ProjectDescriptions = vm.AdminViewModel.Descriptions;
                pdfRequest.ProjectName = vm.AdminViewModel.ProjectName;

                string mechanicalDescriptions = "Tenant Improvement: engineering for HVAC design";
                if (vm.MechanicalExhaustSupply) mechanicalDescriptions += ", exhaust and supply";
                if (vm.MechanicalHvacEquipSpec) mechanicalDescriptions += ", HVAC equipment specifications";
                if (vm.MechanicalTitle24) mechanicalDescriptions += ", Title 24";
                pdfRequest.MechanicalDescriptions = mechanicalDescriptions;

                string structuralDescriptions = "Tenant Improvement: engineering for Structural design";
                if (vm.StructuralGeoReport) structuralDescriptions += ", Review geotechnical report and define foundation type";
                if (vm.StructuralFramingDepths) structuralDescriptions += ", Perform structural analysis and design for all gravity and lateral load resisting elements";
                if (vm.StructuralAnalysis) structuralDescriptions += ", Title 24";
                if (vm.StructuralPlans) structuralDescriptions += ", Structural Plans";
                if (vm.StructuralDetailsCalculations) structuralDescriptions += ", Details and calculations";
                if (vm.StructuralCodeCompliance) structuralDescriptions += ", Building structural to comply with code";
                pdfRequest.StructuralDescriptions = structuralDescriptions;

                string electricalDescriptions = "Tenant Improvement: engineering for Electrica design";
                if (vm.ElectricalPowerDesign) electricalDescriptions += ", Electrical power design";
                if (vm.ElectricalServiceLoadCalc) electricalDescriptions += ", Service load calculation";
                if (vm.ElectricalSingleLineDiagram) electricalDescriptions += ", Single line diagrams";
                if (vm.ElectricalLightingDesign) electricalDescriptions += ", Electrical lighting design";
                pdfRequest.ElectricalDescriptions = electricalDescriptions;

                string plumbingDescriptions = "Tenant Improvement: engineering for Plumbing design";
                if (vm.PlumbingHotColdWater) plumbingDescriptions += ", Hot and cold water piping design";
                if (vm.PlumbingWasteVent) plumbingDescriptions += ", Sewer and vent piping design";
                pdfRequest.PlumbingDescriptions = plumbingDescriptions;

                HttpResponseMessage response;

                switch (vm.SelectProposalTypeViewModel.TypeId)
                {
                    case 1:
                        response =  await client.PostAsJsonAsync("api/wkhtmltopdf/commercial", pdfRequest);
                        break;
                    case 2:
                        response = await client.PostAsJsonAsync("api/wkhtmltopdf/residential", pdfRequest);
                        break;
                    case 3:
                        response = await client.PostAsJsonAsync("api/wkhtmltopdf/t24", pdfRequest);
                        break;
                    case 4:
                        response = await client.PostAsJsonAsync("api/wkhtmltopdf/site-lighting-tarrar", pdfRequest);
                        break;
                    case 5:
                        response = await client.PostAsJsonAsync("api/wkhtmltopdf/2019", pdfRequest);
                        break;
                    default:
                        MessageBox.Show("Invalid proposal type selected.");
                        return;
                }


                response.EnsureSuccessStatusCode();
                //MessageBox.Show($"Click OK to check PDF--- \n" +
                //    $"HasInitialRecommendationsMeeting: {pdfRequest.HasInitialRecommendationsMeeting}\n" + 
                //    $"HasCommericalShellConnection:{pdfRequest.HasCommericalShellConnection}\n" +
                //    $"HasEmergencyPower:{pdfRequest.HasEmergencyPower}\n" +
                //    $"HasIndoorCommonArea:{pdfRequest.HasIndoorCommonArea} \n" +
                //    $"HasGarageExhaust:{pdfRequest.HasGarageExhaust} \n" +
                //    $"HasSiteLighting:{pdfRequest.HasSiteLighting} \n" +
                //    $"HasSiteVisit:{pdfRequest.HasSiteVisit} \n" +
                //    $"TotalPrice:{pdfRequest.TotalPrice}\n" +
                //    $"RetainerPercent:{pdfRequest.RetainerPercent} \n" +
                //    $"ClientType:{pdfRequest.ClientType} \n" +
                //    $"ClientBusinessName:{pdfRequest.ClientBusinessName} \n" +
                //    $"ClientStreetAddress:{pdfRequest.ClientStreetAddress}\n" +
                //    $"ClientCityStateZip:{pdfRequest.ClientCityStateZip}\n" +
                //    $"DateSent:{pdfRequest.DateSent}\n" +
                //    $"NumMeetings:{pdfRequest.NumMeetings}\n" +
                //    $"TarrarNo:{pdfRequest.TarrarNo}\n" +
                //    $"MechanicalDescriptions:{pdfRequest.MechanicalDescriptions}\n" +
                //    $"StructuralDescriptions:{pdfRequest.StructuralDescriptions}\n" +
                //    $"ElectricalDescriptions:{pdfRequest.ElectricalDescriptions}\n" +
                //    $"PlumbingDescriptions:{pdfRequest.PlumbingDescriptions}\n" +
                //    $"Client:{pdfRequest.Client}\n" +
                //    $"Architect:{pdfRequest.Architect}\n" +
                //    $"ProjectAddress:{pdfRequest.ProjectAddress}\n" +
                //    $"ProjectDescriptions:{pdfRequest.ProjectDescriptions}\n" +
                //    $"TypeId:{vm.SelectProposalTypeViewModel.TypeId}\n", 
                //    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                string tempFilePath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"{Guid.NewGuid()}.pdf"
                );
                System.IO.File.WriteAllBytes(tempFilePath, pdfBytes);
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send PDF request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

    }
}
