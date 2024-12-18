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
using System.Windows.Automation;
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
using Org.BouncyCastle.Asn1.Cmp;
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
        public Dictionary<string, string> Owners { get; set; }
        public string ProjectId { get; set; }
        public CollectionViewSource EquipmentViewSource { get; set; }

        public Database.Database database = new Database.Database();

        public ProjectControl(string projectNo)
        {
            InitializeComponent();
            ProjectId = database.GetProjectId(projectNo);
            ElectricalPanels = database.GetProjectPanels(ProjectId);
            ElectricalServices = database.GetProjectServices(ProjectId);
            //ElectricalServices = new ObservableCollection<ElectricalService>();
            ElectricalEquipments = database.GetProjectEquipment(ProjectId);
            //ElectricalTransformers = database.GetProjectTransformers(ProjectId);
            ElectricalTransformers = new ObservableCollection<ElectricalTransformer>();
            FedFromNames = new ObservableCollection<KeyValuePair<string, string>>();
            PanelNames = new ObservableCollection<KeyValuePair<string, string>>();
            Owners = database.getOwners();
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
            foreach (var transformer in ElectricalTransformers)
            {
                transformer.PropertyChanged += ElectricalTransformer_PropertyChanged;
            }

            this.DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;

            SaveText.Text = "";
            GetNames();
            setPower();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            database.UpdateProject(
                ProjectId,
                ElectricalServices,
                ElectricalPanels,
                ElectricalEquipments,
                ElectricalTransformers
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

        public void setPower()
        {
            Dictionary<string, ElectricalService> services =
                new Dictionary<string, ElectricalService>();
            foreach (var service in ElectricalServices)
            {
                services[service.Id] = service;
            }
            Dictionary<string, ElectricalPanel> panels = new Dictionary<string, ElectricalPanel>();
            foreach (var panel in ElectricalPanels)
            {
                panels[panel.Id] = panel;
                panel.Powered = false;
            }
            Dictionary<string, ElectricalTransformer> transformers =
                new Dictionary<string, ElectricalTransformer>();
            foreach (var transformer in ElectricalTransformers)
            {
                transformers[transformer.Id] = transformer;
                transformer.Powered = false;
            }

            // Recursive function to set power for panels and transformers
            bool SetPowerRecursive(string id, int requiredVoltage)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
                if (panels.TryGetValue(id, out var panel))
                {
                    if (panel.Type == requiredVoltage)
                    {
                        panel.Powered = SetPowerRecursive(panel.FedFromId, panel.Type);
                        return panel.Powered;
                    }
                }
                else if (transformers.TryGetValue(id, out var transformer))
                {
                    if (findTransformerOutputVoltage(transformer) == requiredVoltage)
                    {
                        transformer.Powered = SetPowerRecursive(
                            transformer.ParentId,
                            findTransformerInputVoltage(transformer)
                        );
                        return transformer.Powered;
                    }
                }
                else if (services.TryGetValue(id, out var service))
                {
                    if (service.Type == requiredVoltage)
                    {
                        return true;
                    }
                }
                return false;
            }
            int findTransformerInputVoltage(ElectricalTransformer transformer)
            {
                var transformerVoltageType = 5;
                switch (transformer.Voltage)
                {
                    case (1):
                        transformerVoltageType = 3;
                        break;
                    case (2):
                        transformerVoltageType = 1;
                        break;
                    case (3):
                        transformerVoltageType = 3;
                        break;
                    case (4):
                        transformerVoltageType = 4;
                        break;
                    case (5):
                        transformerVoltageType = 4;
                        break;
                    case (6):
                        transformerVoltageType = 1;
                        break;
                    case (7):
                        transformerVoltageType = 2;
                        break;
                    case (8):
                        transformerVoltageType = 5;
                        break;
                    default:
                        transformerVoltageType = 5;
                        break;
                }
                return transformerVoltageType;
            }
            int findTransformerOutputVoltage(ElectricalTransformer transformer)
            {
                var transformerVoltageType = 5;
                switch (transformer.Voltage)
                {
                    case (1):
                        transformerVoltageType = 1;
                        break;
                    case (2):
                        transformerVoltageType = 3;
                        break;
                    case (3):
                        transformerVoltageType = 4;
                        break;
                    case (4):
                        transformerVoltageType = 3;
                        break;
                    case (5):
                        transformerVoltageType = 1;
                        break;
                    case (6):
                        transformerVoltageType = 4;
                        break;
                    case (7):
                        transformerVoltageType = 2;
                        break;
                    case (8):
                        transformerVoltageType = 2;
                        break;
                    default:
                        transformerVoltageType = 5;
                        break;
                }
                return transformerVoltageType;
            }

            // Start the recursion from services
            foreach (var panel in panels)
            {
                panel.Value.Powered = SetPowerRecursive(panel.Value.FedFromId, panel.Value.Type);
            }
            foreach (var transformer in transformers)
            {
                transformer.Value.Powered = SetPowerRecursive(
                    transformer.Value.ParentId,
                    findTransformerInputVoltage(transformer.Value)
                );
            }
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
            foreach (var transformer in ElectricalTransformers)
            {
                if (transformer.ParentId == id)
                {
                    transformer.ColorCode = colorCode;
                    ChangeColors(transformer.Id, colorCode);
                }
            }
            foreach (var equipment in ElectricalEquipments)
            {
                if (equipment.ParentId == id)
                {
                    equipment.ColorCode = colorCode;
                }
            }
        }

        public void setAmps()
        {
            foreach (var panel in ElectricalPanels)
            {
                float poles = getServicePoles(panel.Id, panel.FedFromId);
                if (poles == 1)
                {
                    panel.Amp = 0;
                }
                else
                {
                    panel.Amp = (int)((1.25 * calculateChildrenAmps(panel.Id)) / poles);
                }
            }
        }

        public float getServicePoles(string id, string parent)
        {
            float poles = 1;

            // Traverse the panels connected to the given id
            foreach (var panel in ElectricalPanels)
            {
                if (parent == panel.Id)
                {
                    poles = getServicePoles(panel.Id, panel.FedFromId);
                }
            }
            foreach (var transformer in ElectricalTransformers)
            {
                if (parent == transformer.Id)
                {
                    poles = getServicePoles(transformer.Id, transformer.ParentId);
                }
            }

            // Traverse the services connected to the given id
            foreach (var service in ElectricalServices)
            {
                if (service.Id == parent)
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

        public float calculateChildrenAmps(string id)
        {
            float amp = 0;

            // Find the panel with the given id
            var panelcheck = ElectricalPanels.FirstOrDefault(p => p.Id == id);
            var transformercheck = ElectricalTransformers.FirstOrDefault(p => p.Id == id);

            if (panelcheck != null || transformercheck != null)
            {
                // Calculate the amp for the panel
                foreach (var panel in ElectricalPanels)
                {
                    if (panel.FedFromId == id && panel.Id != panel.FedFromId && id != panel.Id)
                    {
                        amp += calculateChildrenAmps(panel.Id);
                    }
                }
                foreach (var transformer in ElectricalTransformers)
                {
                    if (
                        transformer.ParentId == id
                        && transformer.Id != transformer.ParentId
                        && id != transformer.Id
                    )
                    {
                        amp += calculateChildrenAmps(transformer.Id);
                    }
                }
                // Add amps for equipment directly under the given id
                foreach (var equipment in ElectricalEquipments)
                {
                    if (equipment.ParentId == id)
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
                panel.Kva = Convert.ToInt32(calculateKVA(panel.Id));
            }
            //foreach (var transformer in ElectricalTransformers)
            //{
            //transformer.Kva = Convert.ToInt32(calculateKVA(transformer.Id));
            //}
        }

        public float calculateKVA(string Id)
        {
            float kva = 0;
            foreach (var panel in ElectricalPanels)
            {
                if (panel.FedFromId == Id && panel.Id != panel.FedFromId && Id != panel.Id)
                {
                    kva += calculateKVA(panel.Id);
                }
            }
            foreach (var transformer in ElectricalTransformers)
            {
                if (
                    transformer.ParentId == Id
                    && transformer.Id != transformer.ParentId
                    && Id != transformer.Id
                )
                {
                    kva += calculateKVA(transformer.Id);
                }
            }
            foreach (var equipment in ElectricalEquipments)
            {
                if (equipment.ParentId == Id)
                {
                    kva += equipment.Voltage * equipment.Amp * equipment.Qty;
                }
            }
            return kva;
        }

        private bool checkCycles(string Id)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();

            return HasCycle(Id, visited, stack);
        }

        private bool HasCycle(string Id, HashSet<string> visited, HashSet<string> stack)
        {
            if (stack.Contains(Id))
            {
                return true;
            }

            if (visited.Contains(Id))
            {
                return false;
            }

            visited.Add(Id);
            stack.Add(Id);

            var panel = ElectricalPanels.FirstOrDefault(p => p.Id == Id);
            if (panel != null && !string.IsNullOrEmpty(panel.FedFromId))
            {
                if (HasCycle(panel.FedFromId, visited, stack))
                {
                    return true;
                }
            }
            var transformer = ElectricalTransformers.FirstOrDefault(t => t.Id == Id);
            if (transformer != null && !string.IsNullOrEmpty(transformer.ParentId))
            {
                if (HasCycle(transformer.ParentId, visited, stack))
                {
                    return true;
                }
            }

            stack.Remove(Id);
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
                1,
                1,
                false,
                false,
                "",
                "White",
                "",
                0,
                0,
                0,
                0,
                0,
                1,
                false
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
                panelBackup[equipment.Id] = equipment.ParentId;
            }

            Dictionary<string, string> transformerBackup = new Dictionary<string, string>();
            foreach (var transformer in ElectricalTransformers)
            {
                transformerBackup[transformer.Id] = transformer.ParentId;
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
            foreach (ElectricalTransformer transformer in ElectricalTransformers)
            {
                if (transformer.Name != "")
                {
                    KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                        transformer.Id,
                        transformer.Name
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
                    equipment.ParentId = panelBackup[equipment.Id];
                }
            }
            foreach (var panel in ElectricalPanels)
            {
                if (fedFromBackup.ContainsKey(panel.Id))
                {
                    panel.FedFromId = fedFromBackup[panel.Id];
                }
            }
            foreach (var transformer in ElectricalTransformers)
            {
                if (transformerBackup.ContainsKey(transformer.Id))
                {
                    transformer.ParentId = transformerBackup[transformer.Id];
                }
            }
        }

        private void ElectricalPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalPanel panel)
            {
                if (
                    e.PropertyName == nameof(ElectricalPanel.Type)
                    || e.PropertyName == nameof(ElectricalPanel.FedFromId)
                    || e.PropertyName == nameof(ElectricalPanel.Name)
                )
                {
                    if (checkCycles(panel.Id))
                    {
                        MessageBox.Show(
                            $"Cycle detected in the panel hierarchy involving panel {panel.Id}.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        //Task.Run(() => panel.FedFromId = "");
                        Dispatcher.BeginInvoke(() => panel.FedFromId = "");
                        return;
                    }
                    else
                    {
                        setPower();
                    }
                }
                if (e.PropertyName == nameof(ElectricalPanel.FedFromId))
                {
                    setKVAs();
                    setAmps();
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
                1,
                1,
                1,
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
                if (
                    e.PropertyName == nameof(ElectricalService.Type)
                    || e.PropertyName == nameof(ElectricalService.Name)
                )
                {
                    setPower();
                }
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
                1,
                0,
                0,
                false,
                "",
                0,
                false,
                0,
                1,
                "White",
                false
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
                    || e.PropertyName == nameof(ElectricalEquipment.ParentId)
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
                        && (equipment.ParentId == null || equipment.ParentId != panelKey)
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
            electricalTransformer.PropertyChanged += ElectricalTransformer_PropertyChanged;
            ElectricalTransformers.Add(electricalTransformer);
            GetNames();
            StartTimer();
        }

        public void AddNewElectricalTransformer(object sender, EventArgs e)
        {
            Trace.WriteLine("new transformer");
            ElectricalTransformer electricalTransformer = new ElectricalTransformer(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                0,
                "White",
                1,
                "",
                1,
                false
            );
            AddElectricalTransformer(electricalTransformer);
        }

        public void RemoveElectricalTransformer(ElectricalTransformer electricalTransformer)
        {
            electricalTransformer.PropertyChanged -= ElectricalService_PropertyChanged;
            ElectricalTransformers.Remove(electricalTransformer);
            GetNames();
            StartTimer();
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

        private void ElectricalTransformer_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e
        )
        {
            if (sender is ElectricalTransformer transformer)
            {
                if (
                    e.PropertyName == nameof(ElectricalTransformer.Voltage)
                    || e.PropertyName == nameof(ElectricalTransformer.ParentId)
                    || e.PropertyName == nameof(ElectricalTransformer.Name)
                )
                {
                    if (checkCycles(transformer.Id))
                    {
                        MessageBox.Show(
                            $"Cycle detected in the panel hierarchy involving transformer {transformer.Id}.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        //Task.Run(() => panel.FedFromId = "");
                        Dispatcher.BeginInvoke(() => transformer.ParentId = "");
                        return;
                    }
                    else
                    {
                        setPower();
                    }
                }
                if (e.PropertyName == nameof(ElectricalTransformer.ParentId))
                {
                    setKVAs();
                    setAmps();
                }
                if (e.PropertyName == nameof(ElectricalTransformer.ColorCode))
                {
                    ChangeColors(transformer.Id, transformer.ColorCode);
                }

                if (e.PropertyName == nameof(ElectricalTransformer.Name))
                {
                    GetNames();
                }
                StartTimer();
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
