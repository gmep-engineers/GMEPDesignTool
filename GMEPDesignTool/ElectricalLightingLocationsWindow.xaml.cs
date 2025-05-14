using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;

namespace GMEPDesignTool
{
    partial class ElectricalLightingLocationsWindow : Window
    {
        ElectricalLightingLocationsViewModel ViewModel { get; set; }

        public ElectricalLightingLocationsWindow(ObservableCollection<Location> LightingLocations)
        {
            ViewModel = new ElectricalLightingLocationsViewModel(LightingLocations);
            this.DataContext = ViewModel;
            InitializeComponent();
        }

        private void RemoveSelectedElectricalLightingLocation_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (ViewModel.SelectedLocation != null)
            {
                ViewModel.LightingLocations.Remove(ViewModel.SelectedLocation);
            }
        }
    }
}
