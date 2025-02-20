using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private void LeftCircuitGrid_AddNote(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid.SelectedItems.Cast<Circuit>().OrderBy(circuit => circuit.Number).ToList();
            if (sender is ListBox listBox && listBox.SelectedItem is Note selectedNote)
            {
                if (selectedItems.Any())
                {
                    if (viewModel.RightNodes.Contains(selectedNote))
                    {
                        viewModel.RightNodes.Remove(selectedNote);
                    }
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range = (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number)/2) + 1;

                    //Note newNote = new Note();
                    selectedNote.CircuitNo = firstItem.Number;
                    selectedNote.Length = range;

                    List<Note> toRemove = new List<Note>();
                    int startCircuit = selectedNote.CircuitNo;
                    int endCircuit = selectedNote.CircuitNo + (selectedNote.Length - 1)*2;

                    foreach (var node in viewModel.LeftNodes)
                    {
                        int startCircuit2 = node.CircuitNo;
                        int endCircuit2 = node.CircuitNo + (node.Length - 1)*2;
                        if ((startCircuit <= endCircuit2 && endCircuit > startCircuit2) || (startCircuit2 <= endCircuit && endCircuit2 > startCircuit))
                        {
                            if (!node.Equals(selectedNote))
                            {
                                toRemove.Add(node);
                            }
                        }
                    }
                    foreach (var node in toRemove)
                    {
                        node.CircuitNo = 0;
                        node.Length = 0;
                        viewModel.LeftNodes.Remove(node);
                    }

                    if (!viewModel.LeftNodes.Contains(selectedNote))
                    {
                        viewModel.LeftNodes.Add(selectedNote);
                    }
                }

            }
            var listBox2 = sender as ListBox;
            listBox2.SelectedValue = null;
        }
        private void LeftCircuitGrid_DeleteNotes(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid.SelectedItems.Cast<Circuit>().OrderBy(circuit => circuit.Number).ToList();
            if (sender is MenuItem menuItem)
            {
                List<Note> toRemove = new List<Note>();

                foreach (var node in viewModel.LeftNodes)
                {
                    int startCircuit = node.CircuitNo;
                    int endCircuit = node.CircuitNo + (node.Length - 1)*2;
                    foreach (var item in selectedItems)
                    {
                        if (item.Number >= startCircuit && item.Number <= endCircuit)
                        {
                            toRemove.Add(node);
                        }
                    }
                }
                foreach (var node in toRemove)
                {
                    node.CircuitNo = 0;
                    node.Length = 0;
                    viewModel.LeftNodes.Remove(node);
                }
            }
        }
        private void RightCircuitGrid_AddNote(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RightCircuitGrid.SelectedItems.Cast<Circuit>().OrderBy(circuit => circuit.Number).ToList();
            if (sender is ListBox listBox && listBox.SelectedItem is Note selectedNote)
            {
                if (selectedItems.Any())
                {
                    if (viewModel.LeftNodes.Contains(selectedNote))
                    {
                        viewModel.LeftNodes.Remove(selectedNote);
                    }
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range = (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number)/2) + 1;

                    //Note newNote = new Note();
                    selectedNote.CircuitNo = firstItem.Number;
                    selectedNote.Length = range;


                    List<Note> toRemove = new List<Note>();
                    int startCircuit = selectedNote.CircuitNo;
                    int endCircuit = selectedNote.CircuitNo + (selectedNote.Length - 1)*2;

                    foreach (var node in viewModel.RightNodes)
                    {
                        int startCircuit2 = node.CircuitNo;
                        int endCircuit2 = node.CircuitNo + (node.Length - 1)*2;
                        if ((startCircuit <= endCircuit2 && endCircuit > startCircuit2) || (startCircuit2 <= endCircuit && endCircuit2 > startCircuit))
                        {
                            if (!node.Equals(selectedNote))
                            {
                                toRemove.Add(node);
                            }
                        }
                    }
                    foreach (var node in toRemove)
                    {
                        node.CircuitNo = 0;
                        node.Length = 0;
                        viewModel.RightNodes.Remove(node);
                    }

                    if (!viewModel.RightNodes.Contains(selectedNote))
                    {
                        viewModel.RightNodes.Add(selectedNote);
                    }
                }

            }
            var listBox2 = sender as ListBox;
            listBox2.SelectedValue = null;
        }
        private void RightCircuitGrid_DeleteNotes(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid.SelectedItems.Cast<Circuit>().OrderBy(circuit => circuit.Number).ToList();
            if (sender is MenuItem menuItem)
            {
                List<Note> toRemove = new List<Note>();

                foreach (var node in viewModel.RightNodes)
                {
                    int startCircuit = node.CircuitNo;
                    int endCircuit = node.CircuitNo + (node.Length - 1)*2;
                    foreach (var item in selectedItems)
                    {
                        if (item.Number >= startCircuit && item.Number <= endCircuit)
                        {
                            toRemove.Add(node);
                        }
                    }
                }
                foreach(var node in toRemove)
                {
                    node.CircuitNo = 0;
                    node.Length = 0;
                    viewModel.RightNodes.Remove(node);
                }
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
                return (length * 28) + 1;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

