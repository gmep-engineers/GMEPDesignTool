﻿using System;
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
using Google.Protobuf.WellKnownTypes;

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
        public ObservableCollection<KeyValuePair<string, string>> FedFromNames { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> PanelNames { get; set; }
        public string ProjectId { get; set; }
        public CollectionViewSource EquipmentViewSource { get; set; }

        public Database.Database database = new Database.Database();

        public ProjectControl(string projectName)
        {
            InitializeComponent();
            ProjectId = database.GetProjectId(projectName);
            ElectricalPanels = database.GetProjectPanels(ProjectId);
            ElectricalServices = database.GetProjectServices(ProjectId);
            ElectricalEquipments = database.GetProjectEquipment(ProjectId);
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
                0,
                "MS-1"
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
                sender is Hyperlink hyperlink
                && hyperlink.CommandParameter is ElectricalPanel electricalPanel
            )
            {
                RemoveElectricalPanel(electricalPanel);
            }
        }

        public void GetNames()
        {
            var currentFedFromSelections = new Dictionary<string, string>();
            foreach (var equipment in ElectricalEquipments)
            {
                if (!string.IsNullOrEmpty(equipment.PanelId))
                {
                    currentFedFromSelections[equipment.Id] = equipment.PanelId;
                }
            }

            var currentPanelSelections = new Dictionary<string, string>();
            foreach (var panel in ElectricalPanels)
            {
                if (!string.IsNullOrEmpty(panel.FedFromId))
                {
                    currentPanelSelections[panel.Id] = panel.FedFromId;
                }
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

                    //Adding back selection
                    if (currentPanelSelections.TryGetValue(panel.Id, out var fedFromId))
                    {
                        panel.FedFromId = fedFromId;
                    }
                    else
                    {
                        panel.FedFromId = "";
                    }
                }
            }
            //adding back equipment panelId
            foreach (var equipment in ElectricalEquipments)
            {
                if (currentFedFromSelections.TryGetValue(equipment.Id, out var panelId))
                {
                    equipment.PanelId = panelId;
                }
                else
                {
                    equipment.PanelId = "";
                }
            }
        }

        private void ElectricalPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.Name))
            {
                GetNames();
            }
            StartTimer();
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
                "",
                0
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
                GetNames();
            }
            StartTimer();
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
                ""
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
            StartTimer();
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
                    if (equipment.Voltage != selectedVoltage)
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
    }
}
