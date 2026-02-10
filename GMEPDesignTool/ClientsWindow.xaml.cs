using System;
using System.Collections.Generic;
using System.Diagnostics;
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
          !string.IsNullOrEmpty(ClientNameFilter.Text)
          && (
            employee.LastName == null
            || !employee.LastName.Contains(
              ClientNameFilter.Text,
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
      AddEditContactWindow addEditClientWindow = new AddEditContactWindow(
        ViewModel.SelectedClient.CompanyName,
        ViewModel.SelectedClient.CompanyId,
        null,
        LoginResponse
      );
      addEditClientWindow.Show();
    }

    private void ClientFilter_TextChanged(object sender, EventArgs e) { }

    private void ContactsFilter_TextChanged(object sender, EventArgs e) { }

    public void SaveClick(object sender, RoutedEventArgs e)
    {
      ViewModel.Save();
    }

    private void ClientDataGrid_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        ViewModel.SaveClientOnEnter();
      }
    }

    private void ContactDataGrid_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        ViewModel.SaveContactOnEnter();
      }
    }

    private void FlagClientForDeletion_Click(object sender, RoutedEventArgs e)
    {
      ViewModel.FlagClientForDeletion();
    }

    private void FlagArchitectForDeletion_Click(object sender, RoutedEventArgs e)
    {
      ViewModel.FlagArchitectForDeletion();
    }

    private void FlagContactForDeletion_Click(object sender, RoutedEventArgs e)
    {
      ViewModel.FlagContactForDeletion();
    }

    private void SyncFromNetSuite_Click(object sender, RoutedEventArgs e)
    {
      // HERE implement
    }

    private void IsArchitect_Checked(object sender, RoutedEventArgs e)
    {
      ViewModel.AddClientToArchitects();
    }

    private void IsArchitect_Unchecked(object sender, RoutedEventArgs e)
    {
      ViewModel.RemoveClientFromArchitects();
    }

    private void IsClient_Checked(object sender, RoutedEventArgs e)
    {
      ViewModel.AddArchitectToClients();
    }

    private void IsClient_Unchecked(object sender, RoutedEventArgs e)
    {
      ViewModel.RemoveArchitectFromClients();
    }
  }
}
