using System;
using System.Collections.Generic;
using System.Globalization;
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
  /// Interaction logic for ClientsWindow.xaml
  /// </summary>
  public partial class ClientsWindow : Window
  {
    ClientsViewModel ViewModel { get; set; }
    LoginResponse LoginResponse { get; set; }
    public CollectionViewSource ClientsViewSource { get; set; }

    public ClientsWindow(LoginResponse loginResponse)
    {
      ViewModel = new ClientsViewModel(loginResponse);
      LoginResponse = loginResponse;
      InitializeComponent();
      ClientsViewSource = (CollectionViewSource)FindResource("ClientsViewSource");
      ClientsViewSource.Filter += ClientsViewSource_Filter;
      this.DataContext = ViewModel;
    }

    private void ClientsViewSource_Filter(object sender, FilterEventArgs e)
    {
      if (e.Item is Employee employee)
      {
        // Replace "FilterString" with the actual filter string
        bool isAccepted = true;

        if (
          !string.IsNullOrEmpty(CompanyNameFilter.Text)
          && (
            employee.LastName == null
            || !employee.LastName.Contains(
              CompanyNameFilter.Text,
              StringComparison.OrdinalIgnoreCase
            )
          )
        )
        {
          isAccepted = false;
        }

        e.Accepted = isAccepted;
      }
    }

    private void AddEditPrimaryContact_Click(object sender, RoutedEventArgs e)
    {
      AddEditContactWindow addEditClientWindow = new AddEditContactWindow(ViewModel.SelectedClient);
      addEditClientWindow.Show();
    }

    private void ClientFilter_TextChanged(object sender, EventArgs e) { }

    private void ContactsFilter_TextChanged(object sender, EventArgs e) { }

    public void SaveClick(object sender, RoutedEventArgs e)
    {
      ViewModel.Save();
    }

    private void ClientDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      // HERE check for enter pressed. If so, add or edit the client in the database;
    }
  }
}
