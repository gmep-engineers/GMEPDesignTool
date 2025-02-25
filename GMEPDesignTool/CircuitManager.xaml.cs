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
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range = (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number)/2) + 1;

                    var newNote = new Note(selectedNote);
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<Note> toRemove = new List<Note>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1)*2;

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
                    viewModel.LeftNodes.Add(newNote);
   
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
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range = (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number)/2) + 1;

                    var newNote = new Note(selectedNote);
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<Note> existingNodes = new List<Note>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1)*2;
                   

                    foreach (var node in viewModel.RightNodes)
                    {
                        int startCircuit2 = node.CircuitNo;
                        int endCircuit2 = node.CircuitNo + (node.Length - 1)*2;
                        if ((startCircuit <= endCircuit2 && endCircuit > startCircuit2) || (startCircuit2 <= endCircuit && endCircuit2 > startCircuit))
                        {
                            existingNodes.Add(node);
                        }
                    }

                   //existingNodes.OrderBy(note => note.Stack);

                    if (existingNodes.Count > 0)
                    {
                        for (int i = 0; i <= existingNodes.Last().Stack + 1; i++)
                        {
                            var node = existingNodes.Find(note => note.Stack == i);
                            if (node == null)
                            {
                                newNote.Stack = i;
                            }
                        }
                    }
                

                    


                    /*foreach (var node in toRemove)
                    {
                        node.CircuitNo = 0;
                        node.Length = 0;
                        viewModel.RightNodes.Remove(node);
                    }*/
                    viewModel.RightNodes.Add(newNote);

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
                    viewModel.RightNodes.Remove(node);
                }
            }
        }
    }

    public class LeftMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int CircuitNo && values[1] is int stack)
            {
                var NewCircuitNo = (int)Math.Floor((CircuitNo - 1) / 2.0);
                // Adjust the margin based on the number of notes

                return new Thickness(0, NewCircuitNo * 28, stack * 30, 0);
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RightMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int CircuitNo && values[1] is int stack)
            {
                var NewCircuitNo = (int)Math.Floor((CircuitNo - 1) / 2.0);
                // Adjust the margin based on the number of notes
              
                return new Thickness(stack * 30, NewCircuitNo * 28, 0, 0);
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
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

