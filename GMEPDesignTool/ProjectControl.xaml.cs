using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        private DispatcherTimer timer;
        public ObservableCollection<ElectricalPanel> ElectricalPanels { get; set; }
        public ObservableCollection<ElectricalService> ElectricalServices { get; set; }
        public ObservableCollection<ElectricalEquipment> ElectricalEquipments { get; set; }
        public ObservableCollection<ElectricalTransformer> ElectricalTransformers { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> FedFromNames { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> PanelNames { get; set; }
        public string ProjectId { get; set; }
        public CollectionViewSource EquipmentViewSource { get; set; }

        public Database.Database database = new Database.Database();

        public ProjectControl(string projectNo)
        {
            InitializeComponent();
            ProjectId = database.GetProjectId(projectNo);
            ElectricalPanels = database.GetProjectPanels(ProjectId);
            ElectricalServices = database.GetProjectServices(ProjectId);
            ElectricalEquipments = database.GetProjectEquipment(ProjectId);
            ElectricalTransformers = new ObservableCollection<ElectricalTransformer>();
            FedFromNames = new ObservableCollection<KeyValuePair<string, string>>();
            PanelNames = new ObservableCollection<KeyValuePair<string, string>>();
            EquipmentViewSource = (CollectionViewSource)FindResource("EquipmentViewSource");
            EquipmentViewSource.Filter += EquipmentViewSource_Filter;

            foreach (var service in ElectricalServices)
            {
                service.PropertyChanged += ElectricalService_PropertyChanged;
            }
            foreach (var panel in ElectricalPanels)
            {
                panel.PropertyChanged += ElectricalPanel_PropertyChanged;
            }
            foreach (var equipment in ElectricalEquipments)
            {
                equipment.PropertyChanged += ElectricalEquipment_PropertyChanged;
            }

            this.DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;

            SaveText.Text = "";
            GetNames();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            database.UpdateProject(
                ProjectId,
                ElectricalServices,
                ElectricalPanels,
                ElectricalEquipments
            );
            SaveText.Text = "Last Save: " + DateTime.Now.ToString();
            timer.Stop();
        }

        private void StartTimer()
        {
            timer.Stop();
            timer.Start();
            SaveText.Text = "*SAVING*";
        }

        public void ChangeColors(string id, string colorCode)
        {
            foreach (var panel in ElectricalPanels)
            {
                if (panel.FedFromId == id)
                {
                    panel.ColorCode = colorCode;
                    ChangeColors(panel.Id, colorCode);
                }
            }
            foreach (var equipment in ElectricalEquipments)
            {
                if (equipment.PanelId == id)
                {
                    equipment.ColorCode = colorCode;
                }
            }
        }

        public void setAmps()
        {
            foreach (var panel in ElectricalPanels)
            {
                float poles = getServicePoles(panel);
                if (poles == 1)
                {
                    panel.Amp = 0;
                }
                else
                {
                    panel.Amp = (int)((1.25 * calculateChildrenAmps(panel)) / poles);
                }
            }
        }

        public float getServicePoles(ElectricalPanel panel)
        {
            float poles = 1;

            // Traverse the panels connected to the given id
            foreach (var panel2 in ElectricalPanels)
            {
                if (panel.FedFromId == panel2.Id)
                {
                    poles = getServicePoles(panel2);
                }
            }

            // Traverse the services connected to the given id
            foreach (var service in ElectricalServices)
            {
                if (service.Id == panel.FedFromId)
                {
                    if (service.Type == 1)
                    {
                        poles = 2;
                    }
                    else
                    {
                        poles = 3;
                    }
                }
            }

            return poles;
        }

        public float calculateChildrenAmps(ElectricalPanel panel)
        {
            float amp = 0;

            // Find the panel with the given id
            var panelcheck = ElectricalPanels.FirstOrDefault(p => p.Id == panel.Id);
            if (panelcheck != null)
            {
                // Calculate the amp for the panel
                foreach (var childPanel in ElectricalPanels)
                {
                    if (
                        childPanel.FedFromId == panel.Id
                        && childPanel.Id != childPanel.FedFromId
                        && panel.Id != childPanel.Id
                    )
                    {
                        amp += calculateChildrenAmps(childPanel);
                    }
                }

                foreach (var equipment in ElectricalEquipments)
                {
                    if (equipment.PanelId == panel.Id)
                    {
                        amp += equipment.Amp * equipment.Qty;
                    }
                }
            }

            return amp;
        }

        public void setKVAs()
        {
            foreach (var panel in ElectricalPanels)
            {
                panel.Kva = Convert.ToInt32(calculateKVA(panel));
            }
        }

        public float calculateKVA(ElectricalPanel panel)
        {
            float kva = 0;
            foreach (var childPanel in ElectricalPanels)
            {
                if (
                    childPanel.FedFromId == panel.Id
                    && childPanel.Id != childPanel.FedFromId
                    && panel.Id != childPanel.Id
                )
                {
                    kva += calculateKVA(childPanel);
                }
            }

            foreach (var equipment in ElectricalEquipments)
            {
                if (equipment.PanelId == panel.Id)
                {
                    kva += equipment.Voltage * equipment.Amp * equipment.Qty;
                }
            }
            return kva;
        }

        private bool checkCycles(ElectricalPanel startingPanel)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();

            return HasCycle(startingPanel.Id, visited, stack);
        }

        private bool HasCycle(string panelId, HashSet<string> visited, HashSet<string> stack)
        {
            if (stack.Contains(panelId))
            {
                return true;
            }

            if (visited.Contains(panelId))
            {
                return false;
            }

            visited.Add(panelId);
            stack.Add(panelId);

            var panel = ElectricalPanels.FirstOrDefault(p => p.Id == panelId);
            if (panel != null && !string.IsNullOrEmpty(panel.FedFromId))
            {
                if (HasCycle(panel.FedFromId, visited, stack))
                {
                    return true;
                }
            }

            stack.Remove(panelId);
            return false;
        }

        //Electrical Panel Functions
        public void AddElectricalPanel(ElectricalPanel electricalPanel)
        {
            electricalPanel.PropertyChanged += ElectricalPanel_PropertyChanged;
            ElectricalPanels.Add(electricalPanel);
            GetNames();
            StartTimer();
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
                "White",
                "MS-1",
                0,
                0,
                0,
                0,
                0,
                0,
                new Dictionary<string, string>()
            );
            AddElectricalPanel(electricalPanel);
        }

        public void RemoveElectricalPanel(ElectricalPanel electricalPanel)
        {
            electricalPanel.PropertyChanged -= ElectricalPanel_PropertyChanged;
            ElectricalPanels.Remove(electricalPanel);
            GetNames();
            StartTimer();
        }

        public void DeleteSelectedElectricalPanel(object sender, EventArgs e)
        {
            if (
                sender is Button button
                && button.CommandParameter is ElectricalPanel electricalPanel
            )
            {
                RemoveElectricalPanel(electricalPanel);
            }
        }

        public void GetNames()
        {
            Dictionary<string, string> fedFromBackup = new Dictionary<string, string>();
            foreach (var panel in ElectricalPanels)
            {
                fedFromBackup[panel.Id] = panel.FedFromId;
            }

            Dictionary<string, string> panelBackup = new Dictionary<string, string>();
            foreach (var equipment in ElectricalEquipments)
            {
                panelBackup[equipment.Id] = equipment.PanelId;
            }

            FedFromNames.Clear();
            PanelNames.Clear();
            PanelNames.Add(new KeyValuePair<string, string>("", ""));
            FedFromNames.Add(new KeyValuePair<string, string>("", ""));
            foreach (ElectricalService service in ElectricalServices)
            {
                if (service.Name != "")
                {
                    KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                        service.Id,
                        service.Name
                    );
                    FedFromNames.Add(value);
                }
            }
            foreach (ElectricalPanel panel in ElectricalPanels)
            {
                if (panel.Name != "")
                {
                    KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                        panel.Id,
                        panel.Name
                    );
                    FedFromNames.Add(value);
                    PanelNames.Add(value);
                }
            }
            //adding backup values
            foreach (var equipment in ElectricalEquipments)
            {
                if (panelBackup.ContainsKey(equipment.Id))
                {
                    equipment.PanelId = panelBackup[equipment.Id];
                }
            }
            foreach (var panel in ElectricalPanels)
            {
                if (fedFromBackup.ContainsKey(panel.Id))
                {
                    panel.FedFromId = fedFromBackup[panel.Id];
                }
            }
        }

        private void ElectricalPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalPanel panel)
            {
                if (e.PropertyName == nameof(ElectricalPanel.FedFromId))
                {
                    if (checkCycles(panel))
                    {
                        MessageBox.Show(
                            $"Cycle detected in the panel hierarchy involving panel {panel.Id}.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        //Task.Run(() => panel.FedFromId = "");
                        Dispatcher.BeginInvoke(() => panel.FedFromId = "");
                    }
                    else
                    {
                        populateTransformers();
                        setKVAs();
                        setAmps();
                    }
                }

                if (e.PropertyName == nameof(ElectricalPanel.Name))
                {
                    GetNames();
                }
                if (e.PropertyName == nameof(ElectricalPanel.ColorCode))
                {
                    ChangeColors(panel.Id, panel.ColorCode);
                }
                StartTimer();
            }
        }

        //Service Functions
        public void AddElectricalService(ElectricalService electricalService)
        {
            electricalService.PropertyChanged += ElectricalService_PropertyChanged;
            ElectricalServices.Add(electricalService);
            GetNames();
            StartTimer();
        }

        public void AddNewElectricalService(object sender, EventArgs e)
        {
            Trace.WriteLine("new service");
            ElectricalService electricalService = new ElectricalService(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                0,
                0,
                "Switch Gear",
                "White"
            );
            AddElectricalService(electricalService);
        }

        public void RemoveElectricalService(ElectricalService electricalService)
        {
            electricalService.PropertyChanged -= ElectricalService_PropertyChanged;
            ElectricalServices.Remove(electricalService);
            GetNames();
            StartTimer();
        }

        public void DeleteSelectedElectricalService(object sender, EventArgs e)
        {
            if (
                sender is Button button
                && button.CommandParameter is ElectricalService electricalService
            )
            {
                RemoveElectricalService(electricalService);
            }
        }

        private void ElectricalService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalService service)
            {
                if (e.PropertyName == nameof(ElectricalService.Name))
                {
                    Trace.WriteLine("ElectricalService name changed");
                    GetNames();
                }
                if (e.PropertyName == nameof(ElectricalService.ColorCode))
                {
                    ChangeColors(service.Id, service.ColorCode);
                }
                if (e.PropertyName == nameof(ElectricalService.Type))
                {
                    setAmps();
                }
                StartTimer();
            }
        }

        //Equipment Functions
        public void AddElectricalEquipment(ElectricalEquipment electricalEquipment)
        {
            electricalEquipment.PropertyChanged += ElectricalEquipment_PropertyChanged;
            ElectricalEquipments.Add(electricalEquipment);
            StartTimer();
        }

        public void AddNewElectricalEquipment(object sender, EventArgs e)
        {
            Trace.WriteLine("new panel");
            ElectricalEquipment electricalEquipment = new ElectricalEquipment(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                "",
                1,
                "",
                115,
                0,
                0,
                false,
                "",
                0,
                false,
                0,
                "General",
                "White"
            );
            AddElectricalEquipment(electricalEquipment);
        }

        public void RemoveElectricalEquipment(ElectricalEquipment electricalEquipment)
        {
            electricalEquipment.PropertyChanged -= ElectricalEquipment_PropertyChanged;
            ElectricalEquipments.Remove(electricalEquipment);
            StartTimer();
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

        private void ElectricalEquipment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalEquipment equipment)
            {
                if (
                    e.PropertyName == nameof(ElectricalEquipment.Voltage)
                    || e.PropertyName == nameof(ElectricalEquipment.Amp)
                    || e.PropertyName == nameof(ElectricalEquipment.PanelId)
                    || e.PropertyName == nameof(ElectricalEquipment.Qty)
                )
                {
                    setKVAs();
                    setAmps();
                }
                if (
                    e.PropertyName == nameof(ElectricalEquipment.Voltage)
                    || e.PropertyName == nameof(ElectricalEquipment.Amp)
                )
                {
                    equipment.Va = equipment.Voltage * equipment.Amp;
                }
                StartTimer();
            }
        }

        private void EquipmentViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is ElectricalEquipment equipment)
            {
                // Replace "FilterString" with the actual filter string
                bool isAccepted = true;
                if (
                    !string.IsNullOrEmpty(EquipmentFilter.Text)
                    && (
                        equipment.EquipNo == null
                        || !equipment.EquipNo.Contains(
                            EquipmentFilter.Text,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                {
                    isAccepted = false;
                }

                if (PanelFilter.SelectedItem is KeyValuePair<string, string> selectedPanel)
                {
                    string panelKey = selectedPanel.Key;
                    if (
                        !string.IsNullOrEmpty(panelKey)
                        && (equipment.PanelId == null || equipment.PanelId != panelKey)
                    )
                    {
                        isAccepted = false;
                    }
                }
                if (
                    VoltageFilter.SelectedValue is string selectedVoltageString
                    && int.TryParse(selectedVoltageString, out int selectedVoltage)
                )
                {
                    Trace.Write(selectedVoltageString);
                    if (equipment.Voltage != selectedVoltage)
                    {
                        isAccepted = false;
                    }
                }
                if (
                    CategoryFilter.SelectedValue is string selectedCategory
                    && selectedCategory != ""
                )
                {
                    if (equipment.Category.ToString() != selectedCategory)
                    {
                        isAccepted = false;
                    }
                }
                if (PhaseFilter.SelectedIndex > 0)
                {
                    bool is3Ph = PhaseFilter.SelectedIndex == 2;
                    if (equipment.Is3Ph != is3Ph)
                    {
                        isAccepted = false;
                    }
                }
                e.Accepted = isAccepted;
            }
        }

        private void EquipmentFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            EquipmentViewSource.View.Refresh();
        }

        private void EquipmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EquipmentViewSource.View.Refresh();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            PhaseFilter.SelectedIndex = 0;
            VoltageFilter.SelectedValue = "";
            PanelFilter.SelectedValue = "";
            EquipmentFilter.Text = "";
        }

        private void ClrPcker_Background_SelectedColorChanged(
            object sender,
            RoutedPropertyChangedEventArgs<Color?> e
        ) { }

        //Transformer Functions
        public void AddElectricalTransformer(ElectricalTransformer electricalTransformer)
        {
            //electricalTransformer.PropertyChanged += ElectricalTransformer_PropertyChanged;
            ElectricalTransformers.Add(electricalTransformer);
            //GetNames();
            // StartTimer();
        }

        public void AddNewElectricalTransformer(object sender, EventArgs e)
        {
            Trace.WriteLine("new transformer");
            ElectricalTransformer electricalTransformer = new ElectricalTransformer(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                0,
                "",
                0,
                0,
                "",
                false,
                0
            );
            AddElectricalTransformer(electricalTransformer);
        }

        public void populateTransformers()
        {
            Dictionary<string, ElectricalService> services =
                new Dictionary<string, ElectricalService>();
            foreach (var service in ElectricalServices)
            {
                services[service.Id] = service;
            }

            foreach (var panel in ElectricalPanels)
            {
                if (
                    services.ContainsKey(panel.FedFromId)
                    && services[panel.FedFromId].Type != panel.Type
                )
                {
                    ElectricalTransformer electricalTransformer = new ElectricalTransformer(
                        Guid.NewGuid().ToString(),
                        ProjectId,
                        panel.FedFromId,
                        0,
                        "",
                        panel.Type,
                        services[panel.FedFromId].Type,
                        services[panel.FedFromId].Name + "->To->" + panel.Name,
                        false,
                        0
                    );
                    AddElectricalTransformer(electricalTransformer);
                    panel.FedFromId = electricalTransformer.Id;
                }
            }
        }

        public void RemoveElectricalTransformer(ElectricalTransformer electricalTransformer)
        {
            //electricalService.PropertyChanged -= ElectricalService_PropertyChanged;
            ElectricalTransformers.Remove(electricalTransformer);
            // GetNames();
            // StartTimer();
        }

        public void DeleteSelectedElectricalTransformer(object sender, EventArgs e)
        {
            if (
                sender is Button button
                && button.CommandParameter is ElectricalTransformer electricalTransformer
            )
            {
                RemoveElectricalTransformer(electricalTransformer);
            }
        }
    }

    public class MinimumValueValidationRule : ValidationRule
    {
        public int Minimum { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int intValue;
            if (int.TryParse(value.ToString(), out intValue))
            {
                if (intValue >= Minimum)
                {
                    return ValidationResult.ValidResult;
                }
            }
            return new ValidationResult(false, $"Value should be at least {Minimum}.");
        }
    }
}
