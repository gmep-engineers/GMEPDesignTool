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

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for ClientSelectionWindow.xaml
  /// </summary>
  public partial class ClientSelectionWindow : Window
  {
    ClientSelectionViewModel ViewModel { get; set; }

    public ClientSelectionWindow(
      LoginResponse loginResponse,
      NetSuiteAuth netSuiteAuth,
      Proposal proposal
    )
    {
      InitializeComponent();
      ViewModel = new ClientSelectionViewModel(loginResponse, netSuiteAuth, proposal);
      this.DataContext = ViewModel;
    }

    public void TextBox_TextChanged(object sender, EventArgs e)
    {
      string text = SearchTextBox.Text;
      if (text.Length > 2)
      {
        ViewModel.SearchNetSuitCompanyNames(text);
      }

      // HERE search
    }

    public void Select_Click(object sender, RoutedEventArgs e)
    {
      ClientSearchResult? c = ClientsDataGrid.SelectedItem as ClientSearchResult;
      if (c == null)
      {
        return;
      }
      ViewModel.SetClientCompanyId(c);
    }
  }
}
