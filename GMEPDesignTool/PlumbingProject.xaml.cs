using System.Collections.ObjectModel;

using System.Windows.Controls;


namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for PlumbingProject.xaml
    /// </summary>
    public partial class PlumbingProject : UserControl
    {
        private readonly Database.Database _db;

        public ObservableCollection<PlumbingModel> Fixtures { get; set; } = new();

        public PlumbingProject(string projectId)
        {
            InitializeComponent();
            DataContext = this;
            _db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);
            LoadData(projectId);
        }

        private async void LoadData(string projectId)
        {
            var results = await _db.GetPlumbingFixturesByProjectId(projectId);
            foreach (var item in results)
            {
                Console.WriteLine($"{item.Name} -{item.Description} - {item.HotWater}- {item.Model}");
                Fixtures.Add(item);
            }
        }
    }
}