using Microsoft.Win32;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            this.Deactivated += OnWindowDeactivated;
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

        public void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && e.Key == Key.Return)
            {
                Regex r = new Regex(@"[0-9]{2}-[0-9]{3}");
                if (r.IsMatch(textBox.Text))
                {
                    MainWindowViewModel.OpenProject(textBox.Text);
                }
            }
        }

        public void MainWindowOpenProject(object sender, MouseButtonEventArgs e)
        {
            string projectNo = (string)ProjectList.SelectedItem;
            Regex r = new Regex(@"[0-9]{2}-[0-9]{3}.*");
            if (r.IsMatch(projectNo))
            {
                MainWindowViewModel.OpenProject(projectNo);
            }
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
        private static void OnWindowDeactivated(object sender, EventArgs e)
        {
            Console.WriteLine("Window is not active: ");
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsActive)
                    {
                        Console.WriteLine("Window is active: " + window.Title);
                    }
                }
            });
        }
    }
}
