using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        private DispatcherTimer _timer;
        public ObservableCollection<ElectricalPanel> ElectricalPanels { get; set; }
        public ObservableCollection<ElectricalService> ElectricalServices { get; set; }
        public ObservableCollection<ElectricalEquipment> ElectricalEquipments { get; set; }
        public ObservableCollection<string> FedFromNames { get; set; }
        public string ProjectId { get; set; }

        public Database.Database database = new Database.Database();

        public ProjectControl(string projectName)
        {
            InitializeComponent();
            ProjectId = database.GetProjectId(projectName);
            ElectricalPanels = database.GetProjectPanels(ProjectId);
            ElectricalServices = database.GetProjectServices(ProjectId);

            ElectricalEquipments = new ObservableCollection<ElectricalEquipment>();
            FedFromNames = new ObservableCollection<string>();

            foreach (var service in ElectricalServices)
            {
                service.PropertyChanged += ElectricalService_PropertyChanged;
            }
            foreach (var panel in ElectricalPanels)
            {
                panel.PropertyChanged += ElectricalPanel_PropertyChanged;
            }

            GetFedFromNames();

            this.DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            database.UpdateProject(ProjectId, ElectricalServices, ElectricalPanels);
        }

        //Electrical Panel Functions
        public void AddElectricalPanel(ElectricalPanel electricalPanel)
        {
            electricalPanel.PropertyChanged += ElectricalPanel_PropertyChanged;
            ElectricalPanels.Add(electricalPanel);
            GetFedFromNames();
        }

        public void AddNewElectricalPanel(object sender, EventArgs e)
        {
            Trace.WriteLine("new panel");
            ElectricalPanel electricalPanel = new ElectricalPanel(
                Guid.NewGuid().ToString(),
                ProjectId,
                100,
                100,
                false,
                false,
                "",
                0,
                "MS-1"
            );
            AddElectricalPanel(electricalPanel);
        }

        public void RemoveElectricalPanel(ElectricalPanel electricalPanel)
        {
            electricalPanel.PropertyChanged -= ElectricalPanel_PropertyChanged;
            ElectricalPanels.Remove(electricalPanel);
            GetFedFromNames();
        }

        public void DeleteSelectedElectricalPanel(object sender, EventArgs e)
        {
            if (
                sender is Hyperlink hyperlink
                && hyperlink.CommandParameter is ElectricalPanel electricalPanel
            )
            {
                RemoveElectricalPanel(electricalPanel);
            }
        }

        public void GetFedFromNames()
        {
            FedFromNames.Clear();
            foreach (ElectricalService service in ElectricalServices)
            {
                if (service.Name != "")
                {
                    FedFromNames.Add(service.Name);
                }
            }
            foreach (ElectricalPanel panel in ElectricalPanels)
            {
                if (panel.Name != "")
                {
                    FedFromNames.Add(panel.Name);
                }
            }
        }

        private void ElectricalPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.Name))
            {
                GetFedFromNames();
            }
        }

        //Service Functions
        public void AddElectricalService(ElectricalService electricalService)
        {
            electricalService.PropertyChanged += ElectricalService_PropertyChanged;
            ElectricalServices.Add(electricalService);
            GetFedFromNames();
        }

        public void AddNewElectricalService(object sender, EventArgs e)
        {
            Trace.WriteLine("new service");
            ElectricalService electricalService = new ElectricalService(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                "",
                0
            );
            AddElectricalService(electricalService);
        }

        public void RemoveElectricalService(ElectricalService electricalService)
        {
            electricalService.PropertyChanged -= ElectricalService_PropertyChanged;
            ElectricalServices.Remove(electricalService);
            GetFedFromNames();
        }

        public void DeleteSelectedElectricalService(object sender, EventArgs e)
        {
            if (
                sender is Hyperlink hyperlink
                && hyperlink.CommandParameter is ElectricalService electricalService
            )
            {
                RemoveElectricalService(electricalService);
            }
        }

        private void ElectricalService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalService.Name))
            {
                Trace.WriteLine("ElectricalService name changed");
                GetFedFromNames();
            }
        }

        //Equipment Functions
        public void AddElectricalEquipment(ElectricalPanel electricalEquipment)
        {
            ElectricalPanels.Add(electricalEquipment);
        }

        public void AddNewElectricalEquipment(object sender, EventArgs e)
        {
            Trace.WriteLine("new panel");
            ElectricalEquipment electricalEquipment = new ElectricalEquipment(
                Guid.NewGuid().ToString(),
                "",
                1,
                "",
                115,
                0,
                0,
                false,
                ""
            );
            ElectricalEquipments.Add(electricalEquipment);
        }

        public void RemoveElectricalEquipment(ElectricalEquipment electricalEquipment)
        {
            ElectricalEquipments.Remove(electricalEquipment);
        }

        public void DeleteSelectedElectricalEquipment(object sender, EventArgs e)
        {
            if (
                sender is Button button
                && button.CommandParameter is ElectricalEquipment electricalEquipment
            )
            {
                RemoveElectricalEquipment(electricalEquipment);
            }
        }
    }
}
