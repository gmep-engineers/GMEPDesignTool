using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace GMEPDesignTool
{

    public partial class ProposalCommercialWindow : Window
    {
        static HttpClient client = new HttpClient();
        public class PDFRequest
        {
            //public bool NewConstruction { get; set; }
            public string TotalPrice { get; set; }
            //public string ProjectAddress { get; set; }
            //public string Client { get; set; }
            //public string Architect { get; set; }
            //public int DateSent { get; set; }
            //public string ProjectDescriptions { get; set; }
            public string MechanicalDescriptions { get; set; }
            //public string ElectricalDescriptions { get; set; }
            //public string PlumbingDescriptions { get; set; }
            //public string StructuralDescriptions { get; set; }
            //public int TotalPrice { get; set; }
            //public string ClientType { get; set; }
            //public int RetainerPercent { get; set; }
            //public int NumMeetings { get; set; }
            //public bool HasInitialRecommendationsMeeting { get; set; }
            public string ClientContactName { get; set; }
            //public string ClientBusinessName { get; set; }
            //public string ClientStreetAddress { get; set; }
            //public string ClientCityStateZip { get; set; }
            //public string TarrarNo { get; set; }
            //public bool HasCommericalShellConnection { get; set; }
            //public bool HasIndoorCommonArea { get; set; }
            //public bool HasEmergencyPower { get; set; }
            //public bool HasGarageExhaust { get; set; }
            //public bool HasSiteLighting { get; set; }

        }
        public ProposalCommercialWindow()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://44.240.61.252:3000/");
            //client.BaseAddress = new Uri("http://localhost:3000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }


        private async void generate_Click(object sender, RoutedEventArgs e)
        {
            PDFRequest pdfRequest = new PDFRequest();
            try
            {

                pdfRequest.TotalPrice = TotalPriceBox.Text;
                pdfRequest.ClientContactName = ClientContactNameBox.Text;
                pdfRequest.MechanicalDescriptions = "ssss";
                HttpResponseMessage response = await client.PostAsJsonAsync("api/wkhtmltopdf/commercial", pdfRequest);
                response.EnsureSuccessStatusCode();
                MessageBox.Show($"PDF request sent successfully!--- ProjectName: {pdfRequest.TotalPrice}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        public ProposalCommercialWindow(ProposalCommercialViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
