using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    ///

    public class LoginCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string SessionId { get; set; }
        public string SqlConnectionString { get; set; }
        public string AwsAccessKeyId { get; set; }
        public string AwsAccessSecretAccessKey { get; set; }
        public string AwsS3Bucket { get; set; }
        public int AccessLevelId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ulong? PhoneNumber { get; set; }
        public uint? Extension { get; set; }
        public string EmailAddress { get; set; }
    }

    public partial class LoginWindow : Window
    {
        public LoginViewModel LoginViewModel { get; set; }

        static HttpClient client = new HttpClient();

        public LoginWindow()
        {
            LoginViewModel = new LoginViewModel(this);
            DataContext = LoginViewModel;
            InitializeComponent();
            client.BaseAddress = new Uri("http://44.240.61.252:3000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        static async Task<LoginResponse> AttemptLogin(LoginCredentials credentials)
        {
            LoginResponse loginResponse = null;
            HttpResponseMessage response = await client.PostAsJsonAsync("api/session", credentials);
            response.EnsureSuccessStatusCode();
            loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
            return loginResponse;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            IncorrectPasswordLabel.Visibility = Visibility.Hidden;
            try
            {
                LoginCredentials loginCredentials = new LoginCredentials();
                loginCredentials.Username = UsernameBox.Text;
                loginCredentials.Password = PasswordBox.Password;
                LoginResponse loginResponse = await AttemptLogin(loginCredentials);
                LoginViewModel.OpenApp(loginResponse);
            }
            catch (Exception ex)
            {
                IncorrectPasswordLabel.Visibility = Visibility.Visible;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LoginViewModel.CloseApp();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }
            Login_Click(sender, e);
        }
    }
}
