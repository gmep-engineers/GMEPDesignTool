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
using Amazon.S3.Model;
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
        public ObservableDictionary<string, string> ParentNames { get; set; }
        public ObservableDictionary<string, string> PanelTransformerNames { get; set; }

        public ObservableDictionary<string, string> PanelNames { get; set; }
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
            ParentNames = new ObservableDictionary<string, string>();
            PanelTransformerNames = new ObservableDictionary<string, string>();
            PanelNames = new ObservableDictionary<string, string>();
            ParentNames.Add("", "");
            PanelTransformerNames.Add("", "");
            PanelNames.Add("", "");
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

            Dictionary<string, ElectricalPanel> panelParentIds = new Dictionary<string, ElectricalPanel>();

            foreach (var panel in ElectricalPanels)
            {
                panel.DownloadComponents(ElectricalEquipments, ElectricalPanels, ElectricalTransformers);
                if (!string.IsNullOrEmpty(panel.ParentId))
                {
                    panelParentIds.Add(panel.ParentId, panel);
                }
            }
            foreach(var transformer in ElectricalTransformers)
            {
                if (panelParentIds.ContainsKey(transformer.Id))
                {
                    transformer.AddChildPanel(panelParentIds[transformer.Id]);
                }
            }

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
                        panel.Powered = SetPowerRecursive(panel.ParentId, panel.Type);
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
                panel.Value.Powered = SetPowerRecursive(panel.Value.ParentId, panel.Value.Type);
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
                if (panel.ParentId == id)
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
                float poles = getServicePoles(panel.Id, panel.ParentId);
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
                    poles = getServicePoles(panel.Id, panel.ParentId);
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
                    if (panel.ParentId == id && panel.Id != panel.ParentId && id != panel.Id)
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
                if (panel.ParentId == Id && panel.Id != panel.ParentId && Id != panel.Id)
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
            if (panel != null && !string.IsNullOrEmpty(panel.ParentId))
            {
                if (HasCycle(panel.ParentId, visited, stack))
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
                true,
                false,
                "",
                "White",
                "",
                0,
                0,
                0,
                0,
                1,
                false,
                false,
                0
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

            foreach (ElectricalService service in ElectricalServices)
            {

                KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                    service.Id,
                    service.Name
                );
                AddToParentNames(value);
            }
            foreach (ElectricalPanel panel in ElectricalPanels)
            {
                KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                    panel.Id,
                    panel.Name
                );
                AddToParentNames(value);
                AddToPanelTransformerNames(value);
                AddToPanelNames(value);
            }
            foreach (ElectricalTransformer transformer in ElectricalTransformers)
            {
                KeyValuePair<string, string> value = new KeyValuePair<string, string>(
                    transformer.Id,
                    transformer.Name
                );
                AddToParentNames(value);
                AddToPanelTransformerNames(value);
            }
            CleanUpNames();
         

            void AddToPanelTransformerNames(KeyValuePair<string, string> value)
            {
                if (PanelTransformerNames.ContainsKey(value.Key))
                {
                    if (PanelTransformerNames[value.Key] != value.Value)
                    {
                        if (!string.IsNullOrEmpty(value.Value))
                        {
                            PanelTransformerNames[value.Key] =  value.Value;
                        }
                        else
                        {
                            PanelTransformerNames.Remove(value.Key);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.Value))
                    {
                        PanelTransformerNames.Add(value.Key, value.Value);
                    }
                }
            }
           
            void AddToParentNames(KeyValuePair<string, string> value)
            {
                if (ParentNames.ContainsKey(value.Key))
                {
                    if (ParentNames[value.Key] != value.Value)
                    {
                        if (!string.IsNullOrEmpty(value.Value))
                        {
                            ParentNames[value.Key] = value.Value;
                        }
                        else
                        {
                            ParentNames.Remove(value.Key);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.Value))
                    {
                        ParentNames.Add(value.Key, value.Value);
                    }
                }
            }
            void AddToPanelNames(KeyValuePair<string, string> value)
            {
                if (PanelNames.ContainsKey(value.Key))
                {
                    if (PanelNames[value.Key] != value.Value)
                    {
                        if (!string.IsNullOrEmpty(value.Value))
                        {
                            PanelNames[value.Key] = value.Value;
                        }
                        else
                        {
                            PanelNames.Remove(value.Key);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.Value))
                    {
                        PanelNames.Add(value.Key, value.Value);
                    }
                }
            }
            void CleanUpNames()
            {
                var validIds = new HashSet<string>(
                    ElectricalServices.Select(es => es.Id)
                    .Concat(ElectricalTransformers.Select(et => et.Id))
                    .Concat(ElectricalPanels.Select(ep => ep.Id))
                    .Concat(new[] { "" }) // Add empty string to validIds
                );

                var ParentNamesKeys = ParentNames.Keys.ToList();
                foreach (var key in ParentNamesKeys)
                {
                    if (!validIds.Contains(key))
                    {
                        ParentNames.Remove(key);
                    }
                }

                var PanelTransformerNamesKeys = PanelTransformerNames.Keys.ToList();
                foreach (var key in PanelTransformerNamesKeys)
                {
                    if (!validIds.Contains(key))
                    {
                        PanelTransformerNames.Remove(key);
                    }
                }
                var PanelNamesKeys = PanelNames.Keys.ToList();
                foreach (var key in PanelNamesKeys)
                {
                    if (!validIds.Contains(key))
                    {
                        PanelNames.Remove(key);
                    }
                }
            }
        }


        private void ElectricalPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ElectricalPanel panel)
            {
                if (
                    e.PropertyName == nameof(ElectricalPanel.Type)
                    || e.PropertyName == nameof(ElectricalPanel.ParentId)
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
                        //Task.Run(() => panel.ParentId = "");
                        Dispatcher.BeginInvoke(() => panel.ParentId = "");
                        return;
                    }
                    else
                    {
                        setPower();
                    }
                }
                if (e.PropertyName == nameof(ElectricalPanel.ParentId))
                {
                    //setKVAs();
                    // setAmps();
                    var transformer = ElectricalTransformers.FirstOrDefault(transformer => transformer.Id == panel.ParentId);
                    if (transformer != null)
                    {
                        transformer.AddChildPanel(panel);
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
                if (e.PropertyName == nameof(ElectricalPanel.ParentId))
                {
                    foreach (var panel2 in ElectricalPanels)
                    {
                        if (panel2.Id == panel.ParentId)
                        {
                            panel2.AssignPanel(panel);
                        }
                    }
                }
                StartTimer();
            }
        }
        private void CircuitManager_Click(object sender, RoutedEventArgs e)
        {

            if (
                sender is Button button
                && button.CommandParameter is ElectricalPanel panel
            )
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CircuitManager manager = new CircuitManager(panel);
                    manager.Show();
                });
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
                    //setAmps();
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
                0,
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
            electricalEquipment.ParentId = "";
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
                    //setKVAs();
                    //setAmps();
                }
                if (
                    e.PropertyName == nameof(ElectricalEquipment.Voltage)
                    || e.PropertyName == nameof(ElectricalEquipment.Is3Ph)
                    || e.PropertyName == nameof(ElectricalEquipment.ParentId)
                )
                {
                    setPower();
                }
               
                if (e.PropertyName == nameof(ElectricalEquipment.ParentId))
                {
                    foreach(var panel in ElectricalPanels)
                    {
                        if (panel.Id == equipment.ParentId)
                        {
                            panel.AssignEquipment(equipment);
                        }
                    }
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
                "",
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
                   // setKVAs();
                   // setAmps();
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
                false,
                0
            );
            AddElectricalTransformer(electricalTransformer);
        }

        public void RemoveElectricalTransformer(ElectricalTransformer electricalTransformer)
        {
            electricalTransformer.PropertyChanged -= ElectricalService_PropertyChanged;
            electricalTransformer.ParentId = "";
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
                        //Task.Run(() => panel.ParentId = "");
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
                    foreach (var panel in ElectricalPanels)
                    {
                        if (panel.Id == transformer.ParentId)
                        {
                            panel.AssignTransformer(transformer);
                        }
                    }
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

    [Serializable]
    public class ObservableKeyValuePair<TKey, TValue> : INotifyPropertyChanged
    {
        #region properties
        private TKey key;
        private TValue value;

        public TKey Key
        {
            get { return key; }
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }

        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }

    [Serializable]
    public class ObservableDictionary<TKey, TValue> : ObservableCollection<ObservableKeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("The dictionary already contains the key");
            }
            base.Add(new ObservableKeyValuePair<TKey, TValue>() { Key = key, Value = value });
        }

        public bool ContainsKey(TKey key)
        {
            //var m=base.FirstOrDefault((i) => i.Key == key);
            var r = ThisAsCollection().FirstOrDefault((i) => Equals(key, i.Key));

            return !Equals(default(ObservableKeyValuePair<TKey, TValue>), r);
        }

        bool Equals<TKey>(TKey a, TKey b)
        {
            return EqualityComparer<TKey>.Default.Equals(a, b);
        }

        private ObservableCollection<ObservableKeyValuePair<TKey, TValue>> ThisAsCollection()
        {
            return this;
        }

        public ICollection<TKey> Keys
        {
            get { return (from i in ThisAsCollection() select i.Key).ToList(); }
        }

        public bool Remove(TKey key)
        {
            var remove = ThisAsCollection().Where(pair => Equals(key, pair.Key)).ToList();
            foreach (var pair in remove)
            {
                ThisAsCollection().Remove(pair);
            }
            return remove.Count > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            var r = GetKvpByTheKey(key);
            if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
            {
                return false;
            }
            value = r.Value;
            return true;
        }

        private ObservableKeyValuePair<TKey, TValue> GetKvpByTheKey(TKey key)
        {
            return ThisAsCollection().FirstOrDefault((i) => i.Key.Equals(key));
        }

        public ICollection<TValue> Values
        {
            get { return (from i in ThisAsCollection() select i.Value).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                {
                    throw new ArgumentException("Key not found");
                }
                return result;
            }
            set
            {
                if (ContainsKey(key))
                {
                    GetKvpByTheKey(key).Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
            {
                return false;
            }
            return Equals(r.Value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
            {
                return false;
            }
            if (!Equals(r.Value, item.Value))
            {
                return false;
            }
            return ThisAsCollection().Remove(r);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from i in ThisAsCollection() select new KeyValuePair<TKey, TValue>(i.Key, i.Value)).ToList().GetEnumerator();
        }

        #endregion
    }



}
