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
  /// Interaction logic for AddEditContactWindow.xaml
  /// </summary>
  public partial class AddEditContactWindow : Window
  {
    AddEditContactViewModel ViewModel { get; set; }

    public AddEditContactWindow(Client client, LoginResponse loginResponse)
    {
      InitializeComponent();
      ViewModel = new AddEditContactViewModel(loginResponse, client.CompanyId, client.CompanyName);
      this.DataContext = ViewModel;
    }

    public void SaveClick(object sender, RoutedEventArgs e)
    {
      ViewModel.Save();
    }

    private void ContactDataGrid_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        ViewModel.SaveContactOnEnter();
      }
    }

    private void FlagContactForDeletion_Click(object sender, RoutedEventArgs e)
    {
      ViewModel.FlagForDeletion();
    }
  }
}
