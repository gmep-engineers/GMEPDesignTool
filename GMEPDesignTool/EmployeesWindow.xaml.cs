using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public EmployeesViewModel EmployeesViewModel { get; set; }
        LoginResponse LoginResponse { get; set; }
        public CollectionViewSource EmployeesViewSource { get; set; }

        public EmployeesWindow(LoginResponse loginResponse)
        {
            LoginResponse = loginResponse;
            EmployeesViewModel = new EmployeesViewModel(loginResponse);
            
            InitializeComponent();
            EmployeesViewSource = (CollectionViewSource)FindResource("EmployeesViewSource");
            EmployeesViewSource.Filter += EmployeesViewSource_Filter;
            this.DataContext = EmployeesViewModel;
        }
        private void EmployeesViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Employee employee)
            {
                // Replace "FilterString" with the actual filter string
                bool isAccepted = true;

                if (
                    !string.IsNullOrEmpty(LastNameFilter.Text)
                    && (employee.LastName == null || !employee.LastName.Contains(LastNameFilter.Text, StringComparison.OrdinalIgnoreCase)))

                {
                    isAccepted = false;
                }

                e.Accepted = isAccepted;
            }
        }
        private void LastNameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EmployeesViewSource?.View != null)
            {
                EmployeesViewSource.View.Refresh();
            }
        }
        public void UpdatePasswordClick(object sender, RoutedEventArgs e)
        {
            EmployeesViewModel.OpenUpdatePasswordWindow();
        }

        public void SaveClick(object sender, RoutedEventArgs e)
        {
            EmployeesViewModel.Save();
        }

        public void AddUserClick(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow(LoginResponse, this);
            addEmployeeWindow.Show();
        }
    }
}
