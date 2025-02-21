using System;
using System.Collections.Generic;
using System.Linq;
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
using Org.BouncyCastle.Bcpg;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for EmployeesWindow.xaml
    /// </summary>
    ///


    public partial class EmployeesWindow : Window
    {
        EmployeesViewModel EmployeesViewModel { get; set; }
        LoginResponse LoginResponse { get; set; }

        public EmployeesWindow(LoginResponse loginResponse)
        {
            LoginResponse = loginResponse;
            EmployeesViewModel = new EmployeesViewModel(loginResponse);
            this.DataContext = EmployeesViewModel;
            InitializeComponent();
        }

        public void UpdatePasswordClick(object sender, RoutedEventArgs e)
        {
            EmployeesViewModel.OpenUpdatePasswordWindow();
        }

        public void SaveClick(object sender, RoutedEventArgs e)
        {
            EmployeesViewModel.Save();
        }
    }
}
