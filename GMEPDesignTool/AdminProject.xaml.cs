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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.S3;
using Amazon.S3.Model;
using GMEPDesignTool.Database;
using Mysqlx.Crud;
using Org.BouncyCastle.Bcpg.Sig;
using System.IO;

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
        private readonly Database.Database db;
        private LoginResponse loginResponse;
        

        public ObservableCollection<Proposal> Proposals { get; set; } = new();

        public AdminProject(string projectId, LoginResponse loginResponse)
        {
            InitializeComponent();
            adminViewModel = new AdminViewModel(projectId);
            this.loginResponse = loginResponse;
            this.DataContext = adminViewModel;
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

        private void MyDataGrid_Scroll(
            object sender,
            System.Windows.Controls.Primitives.ScrollEventArgs e
        ) { }

        private void OpenSelectProposalTypeWindow(object sender, RoutedEventArgs e)
        {
            SelectProposalTypeWindow selectProposalTypeWindow = new SelectProposalTypeWindow(
                loginResponse,
                ProjectId,
                adminViewModel
            );
            selectProposalTypeWindow.Show();
        }

    private async void Click_to_download(object sender, RoutedEventArgs e)
        {
            Proposal proposal;
            string proposalId;
            Button clickedButton = sender as Button;
            
            if (clickedButton != null)
            {
                var rowData = clickedButton.DataContext as Proposal; 
                if (rowData != null)
                {
                    proposalId = rowData.Id;
                    proposal = await db.GetProposalById(proposalId);
                    S3 s3 = new S3();
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string downloadPath = System.IO.Path.Combine(desktopPath, proposal.Pdf_name);
                    await s3.DownloadFileAsync(proposal.Pdf_name, downloadPath);
                    MessageBox.Show($"Successfully downloaded to {downloadPath}");
                }
            }

        }
        
        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
