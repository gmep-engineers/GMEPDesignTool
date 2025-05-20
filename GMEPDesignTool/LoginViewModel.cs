using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPDesignTool
{
    public class LoginViewModel : ViewModelBase
    {
        private Window _loginWindow;
        public string Version { get; set; }

        public LoginViewModel(Window loginWindow)
        {
            _loginWindow = loginWindow;
            using (var fileStream = File.OpenRead("version.txt"))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string line;
                    if ((line = reader.ReadLine()) != null)
                    {
                        Version = line;
                    }
                }
            }
        }

        public void OpenApp(LoginResponse loginResponse)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow(loginResponse);
                mainWindow.Show();
                _loginWindow.Close();
            });
        }

        public void CloseApp()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _loginWindow.Close();
            });
        }
    }
}
