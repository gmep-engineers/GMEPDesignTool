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
        private readonly Database.Database db;
        

        public ObservableCollection<Proposal> Proposals { get; set; } = new();
        public AdminProject(string projectId)
        {
            InitializeComponent();
            adminViewModel = new AdminViewModel(projectId);
            this.DataContext = adminViewModel;
            ProjectId = projectId;
            db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);
            LoadData();

        }
        private async void LoadData()
        {
            var results = await db.GetProposals();
            foreach (var item in results)
            {
                Console.WriteLine($"{item.DateCreated} -{item.Type} - {item.EmployeeUsername}");
                Proposals.Add(item);
            }
            MyDataGrid.ItemsSource = Proposals;
        }
        private async void saveAdminProject(object sender, RoutedEventArgs e)
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
                            Descriptions = adminViewModel.Descriptions
                        };

                        await db.UpdateAdminProject(model, ProjectId);
                        Console.WriteLine("s:"+ model.IsCheckedS);
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

        private void MyDataGrid_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}