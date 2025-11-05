using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  /// Interaction logic for SelectProposalTypeWindow.xaml
  /// </summary>
  public partial class SelectProposalTypeWindow : Window
  {
    SelectProposalTypeViewModel ViewModel { get; set; }
    ProposalCommercialViewModel CommercialViewModel { get; set; }
    LoginResponse LoginResponse { get; set; }
    AdminViewModel adminViewModel { get; set; }
    string ProjectId { get; set; }

    ObservableCollection<Proposal> Proposals { get; set; }

    public SelectProposalTypeWindow(
      LoginResponse loginResponse,
      string projectId,
      AdminViewModel adminViewModel,
      ObservableCollection<Proposal> proposals
    )
    {
      InitializeComponent();
      this.LoginResponse = loginResponse;
      this.ProjectId = projectId;
      this.adminViewModel = adminViewModel;
      ViewModel = new SelectProposalTypeViewModel();
      this.DataContext = ViewModel;
      Proposals = proposals;
    }

    public async void SelectButton_Click(object sender, RoutedEventArgs e)
    {
      Database.Database database = new Database.Database(LoginResponse.SqlConnectionString);
      Proposal p = new Proposal();
      string id = database.CreateProposal(p, LoginResponse.EmployeeId, ViewModel.TypeId, ProjectId);
      //MessageBox.Show($"proposal TypeId: {ViewModel.TypeId}");
      CommercialViewModel = new ProposalCommercialViewModel(adminViewModel, ViewModel, database);

      Proposals = await database.GetProposals(ProjectId);
      ProposalCommercialWindow newWindow = new ProposalCommercialWindow(
        CommercialViewModel,
        id,
        LoginResponse,
        adminViewModel
      );
      newWindow.DataContext = CommercialViewModel;
      newWindow.Show();
      this.Close();
    }
  }
}
