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

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        public ObservableCollection<ElectricalPanel> ElectricalPanels { get; set; }
        public ObservableCollection<ElectricalService> ElectricalServices { get; set; }
        public ObservableCollection<ElectricalEquipment> ElectricalEquipments { get; set; }
        public ObservableCollection<string> FedFromNames { get; set; }

        public Database.Database database = new Database.Database();

        public ProjectControl(string projectName)
        {
            InitializeComponent();
            ElectricalPanels = database.GetProjectPanels(projectName);
            ElectricalServices = database.GetProjectServices(projectName);
            ElectricalEquipments = new ObservableCollection<ElectricalEquipment>();
            FedFromNames = new ObservableCollection<string>();

            this.DataContext = this;
            GetFedFromNames();
        }

        //Electrical Panel Functions
        public void AddElectricalPanel(ElectricalPanel electricalPanel)
        {
            ElectricalPanels.Add(electricalPanel);
        }

        public void AddNewElectricalPanel(object sender, EventArgs e)
        {
            Trace.WriteLine("new panel");
            ElectricalPanel electricalPanel = new ElectricalPanel(
                "0",
                100,
                100,
                false,
                false,
                "",
                0,
                "MS-1"
            );
            ElectricalPanels.Add(electricalPanel);
        }

        public void RemoveElectricalPanel(ElectricalPanel electricalPanel)
        {
            ElectricalPanels.Remove(electricalPanel);
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
                FedFromNames.Add(service.Name);
                service.PropertyChanged += ElectricalService_PropertyChanged;
            }
        }

        private void ElectricalService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalService.Name))
            {
                GetFedFromNames();
            }
        }

        //Service Functions
        public void AddElectricalService(ElectricalService electricalService)
        {
            ElectricalServices.Add(electricalService);
        }

        public void AddNewElectricalService(object sender, EventArgs e)
        {
            Trace.WriteLine("new service");
            ElectricalService electricalService = new ElectricalService("0", "0", "", "", 0);
            ElectricalServices.Add(electricalService);
            GetFedFromNames();
        }

        public void RemoveElectricalService(ElectricalService electricalService)
        {
            electricalService.PropertyChanged -= ElectricalService_PropertyChanged;
            ElectricalServices.Remove(electricalService);
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
            GetFedFromNames();
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
                "",
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
