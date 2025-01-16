using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        public ObservableCollection<ElectricalLighting> ElectricalLightings { get; set; }
        public ObservableCollection<ElectricalTransformer> ElectricalTransformers { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> FedFromNames { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> PanelNames { get; set; }

        public ObservableCollection<string> ImagePaths { get; set; }
        public Dictionary<string, string> Owners { get; set; }
        public string ProjectId { get; set; }
        public CollectionViewSource EquipmentViewSource { get; set; }

        public CollectionViewSource LightingViewSource { get; set; }

        public Database.Database database = new Database.Database();

        public Database.S3 s3 = new Database.S3();

        public ProjectControl(string projectNo)
        {
            InitializeComponent();
            ProjectId = database.GetProjectId(projectNo);
            ElectricalPanels = database.GetProjectPanels(ProjectId);
            ElectricalServices = database.GetProjectServices(ProjectId);
            ElectricalEquipments = database.GetProjectEquipment(ProjectId);
            ElectricalTransformers = database.GetProjectTransformers(ProjectId);
            ElectricalLightings = database.GetProjectLighting(ProjectId);
            FedFromNames = new ObservableCollection<KeyValuePair<string, string>>();
            PanelNames = new ObservableCollection<KeyValuePair<string, string>>();
            Owners = database.getOwners();
            EquipmentViewSource = (CollectionViewSource)FindResource("EquipmentViewSource");
            EquipmentViewSource.Filter += EquipmentViewSource_Filter;
            LightingViewSource = (CollectionViewSource)FindResource("LightingViewSource");
            LightingViewSource.Filter += LightingViewSource_Filter;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string symbolsPath = System.IO.Path.Combine(basePath, "..", "..", "..", "symbols");
            symbolsPath = System.IO.Path.GetFullPath(symbolsPath);
            ImagePaths = new ObservableCollection<string>(Directory.GetFiles(symbolsPath, "*.png").Select(System.IO.Path.GetFullPath));

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
            foreach (var lighting in ElectricalLightings)
            {
                lighting.PropertyChanged += ElectricalLighting_PropertyChanged;
            }

            this.DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;

            SaveText.Text = "";
            GetNames();
            setPower();
            this.Unloaded += new RoutedEventHandler(Project_Unloaded);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            database.UpdateProject(
                ProjectId,
                ElectricalServices,
                ElectricalPanels,
                ElectricalEquipments,
                ElectricalTransformers,
                ElectricalLightings
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
            Dictionary<string, ElectricalEquipment> equipments =
                new Dictionary<string, ElectricalEquipment>();
            foreach (var equipment in ElectricalEquipments)
            {
                equipments[equipment.Id] = equipment;
                equipment.Powered = false;
            }
            Dictionary<string, ElectricalLighting> lightings =
                new Dictionary<string, ElectricalLighting>();
            foreach (var lighting in ElectricalLightings)
            {
                lightings[lighting.Id] = lighting;
                lighting.Powered = false;
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
                else if (equipments.TryGetValue(id, out var equipment))
                {
                    foreach (
                        var voltage in determineCompatibleVoltage(
                            equipment.Is3Ph,
                            equipment.Voltage
                        )
                    )
                    {
                        if (panels.TryGetValue(equipment.ParentId, out var panel2))
                        {
                            if (panel2.Type == voltage)
                            {
                                equipment.Powered = SetPowerRecursive(equipment.ParentId, voltage);
                                return equipment.Powered;
                            }
                        }
                        if (transformers.TryGetValue(equipment.ParentId, out var transformer2))
                        {
                            if (findTransformerOutputVoltage(transformer2) == voltage)
                            {
                                equipment.Powered = SetPowerRecursive(
                                    transformer2.ParentId,
                                    findTransformerInputVoltage(transformer2)
                                );
                                return equipment.Powered;
                            }
                        }
                    }
                }
                else if (lightings.TryGetValue(id, out var lighting))
                {
                    foreach (var voltage in determineCompatibleVoltage(false, lighting.VoltageId))
                    {
                        if (panels.TryGetValue(lighting.ParentId, out var panel2))
                        {
                            if (panel2.Type == voltage)
                            {
                                lighting.Powered = SetPowerRecursive(lighting.ParentId, voltage);
                                return equipment.Powered;
                            }
                        }
                        if (transformers.TryGetValue(lighting.ParentId, out var transformer2))
                        {
                            if (findTransformerOutputVoltage(transformer2) == voltage)
                            {
                                lighting.Powered = SetPowerRecursive(
                                    transformer2.ParentId,
                                    findTransformerInputVoltage(transformer2)
                                );
                                return lighting.Powered;
                            }
                        }
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
            List<int> determineCompatibleVoltage(bool Is3Ph, int Voltage)
            {
                List<int> compatibleVoltages = new List<int>();
                if ((Is3Ph && Voltage == 3) || (!Is3Ph && (Voltage >= 1 && Voltage <= 3)))
                {
                    compatibleVoltages.Add(1);
                }
                if (!Is3Ph && (Voltage == 1 || Voltage == 2 || Voltage == 4 || Voltage == 5))
                {
                    compatibleVoltages.Add(2);
                }
                if (
                    (Is3Ph && (Voltage == 7 || Voltage == 8))
                    || (!Is3Ph && (Voltage == 6 || Voltage == 8 || Voltage == 7))
                )
                {
                    compatibleVoltages.Add(3);
                }
                if (
                    (Is3Ph && (Voltage == 4 || Voltage == 5))
                    || (!Is3Ph && (Voltage == 2 || Voltage == 4 || Voltage == 5))
                )
                {
                    compatibleVoltages.Add(4);
                }
                return compatibleVoltages;
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
            foreach (var equipment in equipments)
            {
                foreach (
                    var voltage in determineCompatibleVoltage(
                        equipment.Value.Is3Ph,
                        equipment.Value.Voltage
                    )
                )
                {
                    if (
                        !string.IsNullOrEmpty(equipment.Value.ParentId)
                        && (panels.TryGetValue(equipment.Value.ParentId, out var panel))
                    )
                    {
                        if (panel.Type == voltage)
                        {
                            equipment.Value.Powered = SetPowerRecursive(
                                equipment.Value.ParentId,
                                voltage
                            );
                        }
                    }
                    if (
                        !string.IsNullOrEmpty(equipment.Value.ParentId)
                        && transformers.TryGetValue(equipment.Value.ParentId, out var transformer)
                    )
                    {
                        if (findTransformerOutputVoltage(transformer) == voltage)
                        {
                            equipment.Value.Powered = SetPowerRecursive(
                                transformer.ParentId,
                                findTransformerInputVoltage(transformer)
                            );
                        }
                    }
                }
            }
            foreach (var lighting in lightings)
            {
                foreach (var voltage in determineCompatibleVoltage(false, lighting.Value.VoltageId))
                {
                    if (
                        !string.IsNullOrEmpty(lighting.Value.ParentId)
                        && (panels.TryGetValue(lighting.Value.ParentId, out var panel))
                    )
                    {
                        if (panel.Type == voltage)
                        {
                            lighting.Value.Powered = SetPowerRecursive(
                                lighting.Value.ParentId,
                                voltage
                            );
                        }
                    }
                    if (
                        !string.IsNullOrEmpty(lighting.Value.ParentId)
                        && transformers.TryGetValue(lighting.Value.ParentId, out var transformer)
                    )
                    {
                        if (findTransformerOutputVoltage(transformer) == voltage)
                        {
                            lighting.Value.Powered = SetPowerRecursive(
                                transformer.ParentId,
                                findTransformerInputVoltage(transformer)
                            );
                        }
                    }
                }
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
            foreach (var lighting in ElectricalLightings)
            {
                if (lighting.ParentId == id)
                {
                    lighting.ColorCode = colorCode;
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
                        amp += equipment.Fla * equipment.Qty;
                    }
                }
            }

            return amp;
        }

        public void setKVAs()
        {
            foreach (var panel in ElectricalPanels)
            {
                panel.Kva = Convert.ToInt32(calculateKVA(panel.Id) / 1000);
            }
            foreach (var transformer in ElectricalTransformers)
            {
                transformer.Kva = setKVAId(Convert.ToInt32(calculateKVA(transformer.Id) / 1000));
            }
            int setKVAId(int kva)
            {
                int id = 0;
                switch (kva)
                {
                    case int n when (n < 45):
                        id = 1;
                        break;
                    case int n when (n >= 45 && n < 75):
                        id = 2;
                        break;
                    case int n when (n >= 75 && n < 112.5):
                        id = 3;
                        break;
                    case int n when (n >= 112.5 && n < 150):
                        id = 4;
                        break;
                    case int n when (n >= 150 && n < 225):
                        id = 5;
                        break;
                    case int n when (n >= 225 && n < 300):
                        id = 6;
                        break;
                    case int n when (n >= 300 && n < 500):
                        id = 7;
                        break;
                    case int n when (n >= 500 && n < 750):
                        id = 8;
                        break;
                    case int n when (n >= 750 && n < 1000):
                        id = 9;
                        break;
                    case int n when (n >= 1000 && n < 1500):
                        id = 10;
                        break;
                    case int n when (n >= 1500 && n < 2000):
                        id = 11;
                        break;
                    case int n when (n >= 2000 && n < 2500):
                        id = 12;
                        break;
                    case int n when (n >= 2500):
                        id = 13;
                        break;
                }
                return id;
            }
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
                    kva += equipment.Va * equipment.Qty;
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
                false,
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

        private float idToVoltage(int voltageId)
        {
            int voltage = 0;
            switch (voltageId)
            {
                case (1):
                    voltage = 115;
                    break;
                case (2):
                    voltage = 120;
                    break;
                case (3):
                    voltage = 208;
                    break;
                case (4):
                    voltage = 230;
                    break;
                case (5):
                    voltage = 240;
                    break;
                case (6):
                    voltage = 277;
                    break;
                case (7):
                    voltage = 460;
                    break;
                case (8):
                    voltage = 480;
                    break;
            }
            return voltage;
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
            Dictionary<string, string> lightingBackup = new Dictionary<string, string>();
            foreach (var lighting in ElectricalLightings)
            {
                lightingBackup[lighting.Id] = lighting.ParentId;
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
            foreach (var lighting in ElectricalLightings)
            {
                if (lightingBackup.ContainsKey(lighting.Id))
                {
                    lighting.ParentId = lightingBackup[lighting.Id];
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
                false,
                1,
                "",
                0,
                "",
                true,
                false,
                "",
                0,
                0,
                0
            );
            AddElectricalEquipment(electricalEquipment);
        }

        public void RemoveElectricalEquipment(ElectricalEquipment electricalEquipment)
        {
            if (electricalEquipment.SpecSheetId.Length != 0)
            {
                s3.DeleteFileAsync(electricalEquipment.SpecSheetId);
            }
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
                    || e.PropertyName == nameof(ElectricalEquipment.Fla)
                )
                {
                    equipment.Va = (float)Math.Round(idToVoltage(equipment.Voltage) * equipment.Fla, 0, MidpointRounding.AwayFromZero);
                }
                if (
                    e.PropertyName == nameof(ElectricalEquipment.Voltage)
                    || e.PropertyName == nameof(ElectricalEquipment.Fla)
                    || e.PropertyName == nameof(ElectricalEquipment.ParentId)
                    || e.PropertyName == nameof(ElectricalEquipment.Qty)
                )
                {
                    setKVAs();
                    setAmps();
                }
                if (
                    e.PropertyName == nameof(ElectricalEquipment.Voltage)
                    || e.PropertyName == nameof(ElectricalEquipment.Is3Ph)
                    || e.PropertyName == nameof(ElectricalEquipment.ParentId)
                )
                {
                    setPower();
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



        //Lighting Functions

        public void AddElectricalLighting(ElectricalLighting electricalLighting)
        {
            electricalLighting.PropertyChanged += ElectricalLighting_PropertyChanged;
            ElectricalLightings.Add(electricalLighting);
            StartTimer();
        }

        public void AddNewElectricalLighting(object sender, EventArgs e)
        {
            Trace.WriteLine("new panel");
            ElectricalLighting electricalLighting = new ElectricalLighting(
                Guid.NewGuid().ToString(),
                ProjectId,
                "",
                "",
                "",
                1,
                false,
                0,
                false,
                1,
                "",
                "",
                2,
                1,
                "White",
                false,
                "",
                3,
                false,
                ""
            );
            AddElectricalLighting(electricalLighting);
        }

        public void RemoveElectricalLighting(ElectricalLighting electricalLighting)
        {
            if (electricalLighting.SpecSheetId.Length != 0)
            {
                s3.DeleteFileAsync(electricalLighting.SpecSheetId);
            }
            electricalLighting.PropertyChanged -= ElectricalLighting_PropertyChanged;
            ElectricalLightings.Remove(electricalLighting);
            StartTimer();
        }

        public void DeleteSelectedElectricalLighting(object sender, EventArgs e)
        {
            if (
                sender is Button button
                && button.CommandParameter is ElectricalLighting electricalLighting
            )
            {
                RemoveElectricalLighting(electricalLighting);
            }
        }

        private void ElectricalLighting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalLighting lighting)
            {
                if (
                    e.PropertyName == nameof(ElectricalLighting.VoltageId)
                    || e.PropertyName == nameof(ElectricalLighting.Wattage)
                    || e.PropertyName == nameof(ElectricalLighting.ParentId)
                    || e.PropertyName == nameof(ElectricalLighting.Qty)
                )
                {
                    setKVAs();
                    setAmps();
                }
                if (
                    e.PropertyName == nameof(ElectricalLighting.VoltageId)
                    || e.PropertyName == nameof(ElectricalLighting.ParentId)
                )
                {
                    setPower();
                }

                StartTimer();
            }
        }

        private void LightingViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is ElectricalLighting lighting)
            {
                // Replace "FilterString" with the actual filter string
                bool isAccepted = true;
                if (
                    !string.IsNullOrEmpty(ModelNumberFilter.Text)
                    && (
                        lighting.ModelNo == null
                        || !lighting.ModelNo.Contains(
                            ModelNumberFilter.Text,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                {
                    isAccepted = false;
                }

                if (LightingPanelFilter.SelectedItem is KeyValuePair<string, string> selectedPanel)
                {
                    string panelKey = selectedPanel.Key;
                    if (
                        !string.IsNullOrEmpty(panelKey)
                        && (lighting.ParentId == null || lighting.ParentId != panelKey)
                    )
                    {
                        isAccepted = false;
                    }
                }
                if (
                    LightingVoltageFilter.SelectedValue is string selectedVoltageString
                    && int.TryParse(selectedVoltageString, out int selectedVoltage)
                )
                {
                    Trace.Write(selectedVoltageString);
                    if (lighting.VoltageId != selectedVoltage)
                    {
                        isAccepted = false;
                    }
                }
                if (
                    MountingFilter.SelectedValue is string selectedCategory
                    && selectedCategory != ""
                )
                {
                    if (lighting.MountingType.ToString() != selectedCategory)
                    {
                        isAccepted = false;
                    }
                }

                e.Accepted = isAccepted;
            }
        }

        private void LightingFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            LightingViewSource.View.Refresh();
        }

        private void LightingFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LightingViewSource.View.Refresh();
        }

        private void LightingResetFilters_Click(object sender, RoutedEventArgs e)
        {
            MountingFilter.SelectedIndex = 0;
            LightingVoltageFilter.SelectedValue = "";
            LightingPanelFilter.SelectedValue = "";
            ModelNumberFilter.Text = "";
        }

   

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

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Calculator calculator1)
                    {
                        if (calculator1.ProjectId == ProjectId)
                        {
                            calculator1.Activate();
                            return;
                        }
                    }
                }
                Calculator calculator = new Calculator(
                    ProjectId,
                    ElectricalServices,
                    ElectricalPanels,
                    ElectricalTransformers,
                    ElectricalEquipments,
                    ElectricalLightings
                );

                calculator.Show();
            });
        }

        public void Project_Unloaded(object? sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Calculator calculator)
                    {
                        if (calculator.ProjectId == ProjectId)
                        {
                            window.Close();
                        }
                    }
                }
            });
        }
        public void UploadSpec(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (sender is Button button && button.CommandParameter is ElectricalLighting lighting)
                {
                    if (lighting.SpecSheetId.Length != 0)
                    {
                        s3.DeleteFileAsync(lighting.SpecSheetId);
                    }
                    string keyName = Guid.NewGuid().ToString();
                    s3.UploadFileAsync(keyName, filePath);
                    lighting.SpecSheetId = keyName;
                }
                else if (sender is Button button2 && button2.CommandParameter is ElectricalEquipment equipment)
                {
                    if (equipment.SpecSheetId.Length != 0)
                    {
                        s3.DeleteFileAsync(equipment.SpecSheetId);
                    }
                    string keyName = Guid.NewGuid().ToString();
                    s3.UploadFileAsync(keyName, filePath);
                    equipment.SpecSheetId = keyName;
                }
            }
        }
        

        public void ViewSpec(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ElectricalLighting lighting)
            {
                if (lighting.SpecSheetId != null && lighting.SpecSheetId.Length > 0)
                {
                    string filePath = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        $"{lighting.Id}_SpecSheet.pdf"
                    );
                    s3.DownloadAndOpenFileAsync(lighting.SpecSheetId, filePath);
                }
                else
                {
                    MessageBox.Show(
                        "No spec sheet available for this lighting.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            else if (sender is Button button2 && button2.CommandParameter is ElectricalEquipment equipment)
            {
                if (equipment.SpecSheetId != null && equipment.SpecSheetId.Length > 0)
                {
                    string filePath = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        $"{equipment.Id}_SpecSheet.pdf"
                    );
                    s3.DownloadAndOpenFileAsync(equipment.SpecSheetId, filePath);
                }
                else
                {
                    MessageBox.Show(
                        "No spec sheet available for this equipment.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }

        public void DeleteSpec(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ElectricalLighting lighting)
            {
                s3.DeleteFileAsync(lighting.SpecSheetId);
                lighting.SpecSheetId = "";
            }
            else if (sender is Button button2 && button2.CommandParameter is ElectricalEquipment equipment)
            {
                s3.DeleteFileAsync(equipment.SpecSheetId);
                equipment.SpecSheetId = "";
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
   
    public class RoundToNearestDecimalConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return Math.Round(doubleValue, 1, MidpointRounding.AwayFromZero).ToString("F1");
            }
            return value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double doubleValue))
            {
                return Math.Round(doubleValue, 1, MidpointRounding.AwayFromZero);
            }
            return value;
        }
    }
}
