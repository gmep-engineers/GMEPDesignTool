﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;

namespace GMEPDesignTool
{
    partial class ElectricalLightingLocationsWindow : Window
    {
        ElectricalLightingLocationsViewModel viewModel { get; set; }

        public ElectricalLightingLocationsWindow(ObservableCollection<Location> LightingLocations)
        {
            viewModel = new ElectricalLightingLocationsViewModel(LightingLocations);
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
