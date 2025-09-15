using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.S3;
using Amazon.S3.Model;
using GMEPDesignTool.Database;
using Microsoft.Win32;
using Mysqlx.Crud;
using Org.BouncyCastle.Bcpg.Sig;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for Admin.xaml
  /// </summary>
  public partial class AdminProject : UserControl
  {
    internal readonly object Parameters;

    private bool Saving = false;
    private bool Loading = false;
    private AdminViewModel AdminViewModel;
    private string ProjectId;
    private readonly Database.Database db;
    private LoginResponse LoginResponse;
    private TabItem TabItem;

    public ObservableCollection<Proposal> Proposals { get; set; } = new();

    public AdminProject(string projectId, LoginResponse LoginResponse, TabItem tabItem)
    {
      InitializeComponent();
      AdminViewModel = new AdminViewModel(projectId, LoginResponse);
      this.LoginResponse = LoginResponse;
      this.DataContext = AdminViewModel;
      TabItem = tabItem;
      ProjectId = projectId;
      db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);
      LoadData();
    }

    private async void LoadData()
    {
      var results = await db.GetProposals(ProjectId);
      foreach (var item in results)
      {
        Console.WriteLine($"{item.DateCreated} -{item.Type} - {item.EmployeeUsername}");
        Proposals.Add(item);
      }
      MyDataGrid.ItemsSource = Proposals;
    }

    private async void SaveAdminProject(object sender, RoutedEventArgs e)
    {
      if (!Saving && !Loading)
      {
        Saving = true;
        if (AdminViewModel != null)
        {
          var model = new AdminModel
          {
            ProjectNo = AdminViewModel.ProjectNo,
            ProjectName = AdminViewModel.ProjectName,
            Client = AdminViewModel.Client,
            ClientCompanyId = AdminViewModel.SelectedClientCompanyId,
            Architect = AdminViewModel.Architect,
            ArchitectCompanyId = AdminViewModel.SelectedArchitectCompanyId,
            StreetAddress = AdminViewModel.StreetAddress,
            City = AdminViewModel.City,
            State = AdminViewModel.State,
            PostalCode = AdminViewModel.PostalCode,
            Directory = AdminViewModel.FileDictionary,
            IsCheckedS = AdminViewModel.IsCheckedS,
            IsCheckedM = AdminViewModel.IsCheckedM,
            IsCheckedE = AdminViewModel.IsCheckedE,
            IsCheckedP = AdminViewModel.IsCheckedP,
            Descriptions = AdminViewModel.Descriptions,
          };

          var db = new Database.Database(Properties.Settings.Default.ConnectionString);
          await db.UpdateAdminProject(model, ProjectId);
          TabItem.Header = AdminViewModel.ProjectNo;
        }

        Saving = false;
      }
    }

    private void MyDataGrid_Scroll(
      object sender,
      System.Windows.Controls.Primitives.ScrollEventArgs e
    ) { }

    private void OpenSelectProposalTypeWindow(object sender, RoutedEventArgs e)
    {
      SelectProposalTypeWindow selectProposalTypeWindow = new SelectProposalTypeWindow(
        LoginResponse,
        ProjectId,
        AdminViewModel
      );
      selectProposalTypeWindow.Show();
    }

    private async void EditProposal_Click(object sender, RoutedEventArgs e)
    {
      Proposal? proposal = await db.GetProposalById(AdminViewModel.SelectedProposal.Id);
      if (proposal != null)
      {
        SelectProposalTypeViewModel svm = new SelectProposalTypeViewModel();
        ProposalCommercialViewModel pvm = new ProposalCommercialViewModel(AdminViewModel, svm, db);
        ProposalCommercialWindow window = new ProposalCommercialWindow(
          pvm,
          AdminViewModel.SelectedProposal.Id,
          LoginResponse,
          proposal.Data
        );
        window.Show();
      }
    }

    private async void DownloadProposal_Click(object sender, RoutedEventArgs e)
    {
      AdminViewModel.DownloadProposal();
    }

    public void UploadRfpEmail_Click(object sender, EventArgs e)
    {
      AdminViewModel.UploadRfp(ProjectId, db);
    }

    public void DownloadRfpEmail_Click(object sender, EventArgs e)
    {
      AdminViewModel.DownloadRfp(ProjectId, db);
    }
  }
}
