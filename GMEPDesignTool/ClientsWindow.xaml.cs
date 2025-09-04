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
    public CollectionViewSource ClientsViewSource { get; set; }

    public ClientsWindow(LoginResponse loginResponse)
    {
      ViewModel = new ClientsViewModel(loginResponse);
      ClientsViewSource = (CollectionViewSource)FindResource("ClientsViewSource");
      ClientsViewSource.Filter += ClientsViewSource_Filter;
      this.DataContext = ViewModel;
      InitializeComponent();
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
  }
}
