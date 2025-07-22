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
    /// Interaction logic for SelectProposalTypeWindow.xaml
    /// </summary>
    public partial class SelectProposalTypeWindow : Window
    {
        SelectProposalTypeViewModel ViewModel { get; set; }
        ProposalCommercialViewModel CommercialViewModel { get;set; }
        LoginResponse LoginResponse { get; set; }
        AdminViewModel adminViewModel { get; set; }
        string ProjectId { get; set; }

        public SelectProposalTypeWindow(LoginResponse loginResponse, string projectId, AdminViewModel adminViewModel)
        {
            InitializeComponent();
            this.LoginResponse = loginResponse;
            this.ProjectId = projectId;
            this.adminViewModel = adminViewModel;
            ViewModel = new SelectProposalTypeViewModel();
            this.DataContext = ViewModel;
        }

        public void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show($"Selected TypeId: {ViewModel.TypeId}");
            Database.Database database = new Database.Database(LoginResponse.SqlConnectionString);
            string id = database.CreateProposal(
                LoginResponse.EmployeeId,
                ViewModel.TypeId,
                ProjectId
            );
            CommercialViewModel = new ProposalCommercialViewModel(adminViewModel, ViewModel);
            ProposalCommercialWindow newWindow = new ProposalCommercialWindow(CommercialViewModel);
            newWindow.DataContext = CommercialViewModel;
            newWindow.Show();
            this.Close();
        }
    }
}
