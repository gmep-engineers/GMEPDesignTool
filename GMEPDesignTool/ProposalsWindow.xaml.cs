using System;
using System.Collections.Generic;
using System.ComponentModel;
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
  /// Interaction logic for ProposalsWindow.xaml
  /// </summary>
  ///

  public partial class ProposalsWindow : Window
  {
    public ProposalsViewModel ViewModel { get; set; }

    private LoginResponse LoginResponse { get; set; }

    public ProposalsWindow(LoginResponse loginResponse)
    {
      ViewModel = new ProposalsViewModel(loginResponse);
      this.LoginResponse = loginResponse;
      this.DataContext = ViewModel;
      InitializeComponent();
    }

    public void SaveClick(object sender, RoutedEventArgs e)
    {
      Save();
    }

    public void Save(CancelEventArgs? e = null)
    {
      ViewModel.Save();
      ViewModel.Saved = true;
    }

    private void ProposalsDataGrid_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
      {
        if (e.Source == ProposalsDataGrid)
        {
          ProposalsDataGrid.CommitEdit();
        }
        Save();
      }
    }

    private void EditProposal_Click(object sender, RoutedEventArgs e)
    {
      Proposal? p = ProposalsDataGrid.SelectedItem as Proposal;
      if (p == null)
      {
        return;
      }
      AdminViewModel adminViewModel = new AdminViewModel(p.ProjectId, LoginResponse);
      SelectProposalTypeViewModel selectProposalTypeViewModel = new SelectProposalTypeViewModel();
      selectProposalTypeViewModel.TypeId = p.TypeId;
      Database.Database db = new Database.Database(LoginResponse.SqlConnectionString);
      ProposalCommercialViewModel vm = new ProposalCommercialViewModel(
        adminViewModel,
        selectProposalTypeViewModel,
        db
      );
      ProposalCommercialWindow proposalCommercialWindow = new ProposalCommercialWindow(
        vm,
        p.Id,
        LoginResponse,
        p.Data,
        p
      );
      proposalCommercialWindow.Show();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (ViewModel != null && !ViewModel.Saved)
      {
        MessageBoxResult result = MessageBox.Show(
          "Save changes?",
          "Confirmation",
          MessageBoxButton.YesNoCancel
        );
        if (result == MessageBoxResult.Yes)
        {
          Save(e);
          base.OnClosing(e);
        }
        else if (result == MessageBoxResult.No)
        {
          base.OnClosing(e);
        }
        else
        {
          e.Cancel = true;
        }
      }
      else
      {
        base.OnClosing(e);
      }
    }

    public void ProposalYearListViewItem_Click(object sender, RoutedEventArgs e)
    {
      string year = (string)ProposalYearListView.SelectedItem;
      ViewModel.FilterDataGridByYear(year);
    }
  }
}
