using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
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

    private NetSuiteAuth NetSuiteAuth { get; set; }

    public ProposalsWindow(LoginResponse loginResponse, NetSuiteAuth netSuiteAuth)
    {
      ViewModel = new ProposalsViewModel(loginResponse, netSuiteAuth);
      NetSuiteAuth = netSuiteAuth;
      this.LoginResponse = loginResponse;
      this.DataContext = ViewModel;
      InitializeComponent();

      if (netSuiteAuth.refresh_token == null)
      {
        OAuthLoginWindow oAuthLoginWindow = new OAuthLoginWindow(loginResponse, netSuiteAuth);
        oAuthLoginWindow.Show();
      }
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

    private async void EditProposal_Click(object sender, RoutedEventArgs e)
    {
      Proposal? p = ProposalsDataGrid.SelectedItem as Proposal;

      Database.Database db = new Database.Database(LoginResponse.SqlConnectionString);
      if (p == null)
      {
        return;
      }
      if (String.IsNullOrEmpty(p.ProjectId))
      {
        p.ProjectId = db.CreateBlankProject();
        p.ProjectNo = "n" + p.ProjectId.Substring(0, 6);
        db.CreateProposal(p, LoginResponse.EmployeeId, 0, p.ProjectId, p.Id);
        db.SaveProposal(p);
      }

      AdminViewModel adminViewModel = new AdminViewModel(p.ProjectId, LoginResponse);
      SelectProposalTypeViewModel selectProposalTypeViewModel = new SelectProposalTypeViewModel();
      selectProposalTypeViewModel.TypeId = p.TypeId;
      ProposalCommercialViewModel vm = new ProposalCommercialViewModel(
        adminViewModel,
        selectProposalTypeViewModel,
        db
      );
      AdminModel adminModel = await db.GetAdminByProjectId(p.ProjectId);
      adminViewModel.ProjectNo = adminModel.ProjectNo;
      adminViewModel.ProjectName = adminModel.ProjectName;
      adminViewModel.StreetAddress = adminModel.StreetAddress;
      adminViewModel.City = adminModel.City;
      adminViewModel.State = adminModel.State;
      adminViewModel.PostalCode = adminModel.PostalCode;
      adminViewModel.Descriptions = adminModel.Descriptions;
      ProposalCommercialWindow proposalCommercialWindow = new ProposalCommercialWindow(
        vm,
        p.Id,
        LoginResponse,
        adminViewModel,
        p.Data,
        p
      );
      proposalCommercialWindow.Show();
    }

    private void CreateEstimate_Click(object sender, RoutedEventArgs e)
    {
      Proposal? p = ProposalsDataGrid.SelectedItem as Proposal;
      if (p == null)
      {
        return;
      }
      ViewModel.CreateEstimate(p);
    }

    private void ClientSelection_Click(object sender, RoutedEventArgs args)
    {
      Proposal? p = ProposalsDataGrid.SelectedItem as Proposal;
      if (p == null)
      {
        return;
      }
      ClientSelectionWindow clientSelectionWindow = new ClientSelectionWindow(
        LoginResponse,
        NetSuiteAuth,
        p
      );
      clientSelectionWindow.Show();
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
