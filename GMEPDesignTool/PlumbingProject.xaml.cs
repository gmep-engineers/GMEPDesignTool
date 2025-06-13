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

        private string ProjectId;

        public ObservableCollection<PlumbingFixture> PlumbingFixtures { get; set; } = new();

        public PlumbingProject(string projectId)
        {
            InitializeComponent();
            ProjectId = projectId;
            DataContext = this;
            _db = new Database.Database(
                GMEPDesignTool.Properties.Settings.Default.ConnectionString
            );
        }

        public async Task InitializeAsync()
        {
            var results = await _db.GetPlumbingFixturesByProjectId(ProjectId);
            foreach (var item in results)
            {
                Console.WriteLine(
                    $"{item.Name} -{item.Description} - {item.HotWater}- {item.Model}"
                );
                PlumbingFixtures.Add(item);
            }
        }
    }
}
