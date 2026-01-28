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
using MySqlX.XDevAPI;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for ClientSelectionWindow.xaml
  /// </summary>
  public partial class ClientSelectionWindow : Window
  {
    ClientSelectionViewModel ViewModel { get; set; }

    LoginResponse LoginResponse { get; set; }

    Proposal Proposal { get; set; }

    public ClientSelectionWindow(
      LoginResponse loginResponse,
      NetSuiteAuth netSuiteAuth,
      Proposal proposal,
      bool setAsArchitect = false
    )
    {
      InitializeComponent();
      ViewModel = new ClientSelectionViewModel(
        loginResponse,
        netSuiteAuth,
        proposal,
        setAsArchitect
      );
      this.DataContext = ViewModel;
      LoginResponse = loginResponse;
      if (setAsArchitect)
      {
        this.Title = "Architect Selection";
      }
      else
      {
        this.Title = "Client Selection";
      }
      Proposal = proposal;
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
      if (Title == "Client Selection")
      {
        // Do this to update the button text since async functions cannot update the UI
        ViewModel.Proposal.ClientCompanyName = "";
        ViewModel.Proposal.ClientCompanyName = c.Name;
        ViewModel.Proposal.ClientCompanyId = c.Id;
      }
      else
      {
        ViewModel.Proposal.ArchitectCompanyName = "";
        ViewModel.Proposal.ArchitectCompanyName = c.Name;
        ViewModel.Proposal.ArchitectCompanyId = c.Id;
      }
    }

    public void CurrentCompanyName_DoubleClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel.CurrentCompanyName != null)
      {
        AddEditContactWindow addEditContactWindow = new AddEditContactWindow(
          ViewModel.CurrentCompanyName,
          ViewModel.CurrentCompanyId,
          Proposal,
          LoginResponse
        );
        addEditContactWindow.Show();
      }
    }

    public void RemoveLabel_DoubleClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel.CurrentCompanyName != null)
      {
        ViewModel.RemoveCurrentCompany();
      }
    }
  }
}
