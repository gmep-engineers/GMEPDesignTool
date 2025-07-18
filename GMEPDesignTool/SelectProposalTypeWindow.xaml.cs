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

        string ProjectId { get; set; }

        public SelectProposalTypeWindow(LoginResponse loginResponse, string projectId)
        {
            InitializeComponent();
            this.LoginResponse = loginResponse;
            this.ProjectId = projectId;
            ViewModel = new SelectProposalTypeViewModel();
            this.DataContext = ViewModel;
        }

        public void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Database.Database database = new Database.Database(LoginResponse.SqlConnectionString);
            string id = database.CreateProposal(
                LoginResponse.EmployeeId,
                ViewModel.TypeId,
                ProjectId
            );
            CommercialViewModel = new ProposalCommercialViewModel();
            var vm = new ProposalCommercialViewModel();
            ProposalCommercialWindow newWindow = new ProposalCommercialWindow(vm);
            newWindow.DataContext = CommercialViewModel;
            newWindow.Show();
            this.Close();
        }
    }
}
