using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for CircuitManager.xaml
    /// </summary>
    public partial class CircuitManager : Window
    {
        CircuitManagerViewModel viewModel { get; set; }
        public CircuitManager(ElectricalPanel panel)
        {
            InitializeComponent();
            viewModel = new CircuitManagerViewModel(panel);
            this.DataContext = viewModel;
            this.Closed += CircuitManager_Closed;

        }
        private void CircuitManager_Closed(object sender, EventArgs e)
        {
            // Unsubscribe from any events
            if (viewModel.Panel != null)
            {
                viewModel.Panel.PropertyChanged -= viewModel.Panel_PropertyChanged;
            }
        }
        private void AddSpaceLeft(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                viewModel.Panel.AssignSpace(true);
            }
        }
        private void AddSpaceRight(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                viewModel.Panel.AssignSpace(false);
            }
        }

        private void LeftCircuitGrid_AddNote(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid.SelectedItems.Cast<Circuit>().OrderBy(circuit => circuit.Number).ToList();
            if (selectedItems.Any())
            {
                var firstItem = selectedItems.First();
                var lastItem = selectedItems.Last();
                var range = (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number)/2) + 1;

                Note newNote = new Note();
                newNote.CircuitNo = firstItem.Number;
                newNote.Length = range;

                viewModel.LeftNodes.Add(newNote);
            }

        }
    }
    public class CircuitToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int CircuitNo)
            {
                var NewCircuitNo = (int)Math.Floor((CircuitNo - 1) / 2.0);
                // Adjust the margin based on the number of notes
                return new Thickness(0, NewCircuitNo * 28, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LengthToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int length)
            {
                return length * 28;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

