using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private AdminViewModel adminViewModel;
        private string ProjectId;
        private LoginResponse loginResponse;

        public AdminProject(string projectId, LoginResponse loginResponse)
        {
            InitializeComponent();
            adminViewModel = new AdminViewModel(projectId);
            this.loginResponse = loginResponse;
            this.DataContext = adminViewModel;
            ProjectId = projectId;
        }

        private async void SaveAdminProject(object sender, RoutedEventArgs e)
        {
            if (!Saving && !Loading)
            {
                Saving = true;
                Console.WriteLine("save start");
                if (adminViewModel != null)
                {
                    try
                    {
                        // Construct your AdminModel from ViewModel data
                        var model = new AdminModel
                        {
                            ProjectNo = adminViewModel.ProjectNo,
                            ProjectName = adminViewModel.ProjectName,
                            Client = adminViewModel.Client,
                            Architect = adminViewModel.Architect,
                            StreetAddress = adminViewModel.StreetAddress,
                            City = adminViewModel.City,
                            State = adminViewModel.State,
                            PostalCode = adminViewModel.PostalCode,
                            Directory = adminViewModel.FileDictionary,
                            IsCheckedS = adminViewModel.IsCheckedS,
                            IsCheckedM = adminViewModel.IsCheckedM,
                            IsCheckedE = adminViewModel.IsCheckedE,
                            IsCheckedP = adminViewModel.IsCheckedP,
                            Descriptions = adminViewModel.Descriptions,
                        };

                        var db = new Database.Database(
                            Properties.Settings.Default.ConnectionString
                        );
                        await db.UpdateAdminProject(model, ProjectId);
                        Console.WriteLine("s:" + model.IsCheckedS);
                        Console.WriteLine("save successed");

                        MessageBox.Show("Project updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving project: " + ex.Message);
                    }
                }

                Saving = false;
                Console.WriteLine("save end");
            }
        }

        private void OpenSelectProposalTypeWindow(object sender, RoutedEventArgs e)
        {
            SelectProposalTypeWindow selectProposalTypeWindow = new SelectProposalTypeWindow(
                loginResponse,
                ProjectId
            );
            selectProposalTypeWindow.Show();
        }
    }
}
