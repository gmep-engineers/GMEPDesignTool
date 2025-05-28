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

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        AddEmployeeViewModel ViewModel { get; set; }

        LoginResponse LoginResponse { get; set; }

        EmployeesWindow EmployeesWindow { get; set; }

        public AddEmployeeWindow(LoginResponse loginResponse, EmployeesWindow employeesWindow)
        {
            LoginResponse = loginResponse;
            ViewModel = new AddEmployeeViewModel(loginResponse);
            this.DataContext = ViewModel;
            EmployeesWindow = employeesWindow;
            InitializeComponent();
        }

        public void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CreateEmployee())
            {
                EmployeesWindow.EmployeesViewModel = new EmployeesViewModel(LoginResponse);
                EmployeesWindow.DataContext = EmployeesWindow.EmployeesViewModel;
                Close();
            }
        }
    }
}
