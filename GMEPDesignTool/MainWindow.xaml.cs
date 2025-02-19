using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.S3.Model;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public class LogoutResponse
    {
        public string EmployeeId { get; set; }
    }

    public partial class MainWindow
    {
        public ViewModel MainWindowViewModel { get; set; }
        public string SessionId { get; set; }

        static HttpClient client = new HttpClient();

        public MainWindow(LoginResponse loginResponse)
        {
            SessionId = loginResponse.SessionId;
            client.BaseAddress = new Uri("http://44.240.61.252:3000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            MainWindowViewModel = new ViewModel(loginResponse);
            DataContext = MainWindowViewModel;
            InitializeComponent();
        }

        static async Task<LogoutResponse> AttemptLogout(string sessionId)
        {
            LogoutResponse logoutResponse = null;
            HttpResponseMessage response = await client.DeleteAsync("api/session/" + sessionId);
            response.EnsureSuccessStatusCode();
            logoutResponse = await response.Content.ReadAsAsync<LogoutResponse>();
            return logoutResponse;
        }

        public void MainWindowOpenProject(object sender, MouseButtonEventArgs e)
        {
            string projectNo = (string)ProjectList.SelectedItem;
            MainWindowViewModel.OpenProject(projectNo);
        }

        public void MainWindowCloseProject(object sender, RoutedEventArgs e)
        {
            TabItem projectTab = (TabItem)ProjectTabs.SelectedItem;
            MainWindowViewModel.CloseProject(projectTab);
        }

        public async void EndSession(object sender, EventArgs e)
        {
            try
            {
                LogoutResponse logoutResponse = await AttemptLogout(SessionId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
