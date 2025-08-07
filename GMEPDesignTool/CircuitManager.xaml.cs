using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for CircuitManager.xaml
    /// </summary>
    public partial class CircuitManager : Window
    {
        CircuitManagerViewModel viewModel { get; set; }
        ElectricalPanel Panel { get; set; }

        ProjectControlViewModel ProjectControlViewModel { get; set; }

        public CircuitManager(
            ElectricalPanel panel,
            ProjectControlViewModel projectControlViewModel
        )
        {
            InitializeComponent();
            viewModel = new CircuitManagerViewModel(panel);
            Panel = panel;
            ProjectControlViewModel = projectControlViewModel;
            Panel.SetKitchenDemandFactor();
            Panel.SetCircuitVa();
            this.DataContext = viewModel;
            this.Closed += CircuitManager_Closed;
        }

        private async void CircuitManager_Closed(object sender, EventArgs e)
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

        private void LeftCircuitGrid_ToggleCustomDescription(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem && selectedItems.Any())
            {
                bool allCustom = false;
                foreach (var circuit in selectedItems)
                {
                    if (!circuit.CustomDescription)
                    {
                        allCustom = true;
                        break;
                    }
                }

                foreach (var circuit in selectedItems)
                {
                    circuit.CustomDescription = allCustom;
                }
            }
        }

        private void LeftCircuitGrid_ToggleCustomBreakerSize(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem && selectedItems.Any())
            {
                bool allCustom = false;
                foreach (var circuit in selectedItems)
                {
                    if (!circuit.CustomBreakerSize)
                    {
                        allCustom = true;
                        break;
                    }
                }

                foreach (var circuit in selectedItems)
                {
                    circuit.CustomBreakerSize = allCustom;
                }
            }
        }

        private void LeftCircuitGrid_AddNote(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (
                sender is ListBox listBox
                && listBox.SelectedItem is ElectricalPanelNote selectedNote
            )
            {
                if (selectedItems.Any())
                {
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range =
                        (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number) / 2) + 1;

                    var newNote = new ElectricalPanelNoteRel(
                        Guid.NewGuid().ToString(),
                        selectedNote.ProjectId,
                        Panel.Id,
                        selectedNote.Id,
                        selectedNote.Note,
                        0,
                        0,
                        0,
                        selectedNote.Tag
                    );
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<ElectricalPanelNoteRel> existingNodes = new List<ElectricalPanelNoteRel>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1) * 2;

                    foreach (var note in viewModel.LeftNotes)
                    {
                        int startCircuit2 = note.CircuitNo;
                        int endCircuit2 = note.CircuitNo + (note.Length - 1) * 2;
                        if (
                            (startCircuit <= endCircuit2 && endCircuit >= startCircuit2)
                            || (startCircuit2 <= endCircuit && endCircuit2 >= startCircuit)
                        )
                        {
                            existingNodes.Add(note);
                        }
                    }

                    List<ElectricalPanelNoteRel> existingNodes2 = existingNodes
                        .OrderBy(note => note.Stack)
                        .ToList();

                    if (existingNodes2.Count > 0)
                    {
                        for (int i = 0; i <= existingNodes2.Last().Stack + 1; i++)
                        {
                            var node = existingNodes2.Find(note => note.Stack == i);
                            if (node == null)
                            {
                                newNote.Stack = i;
                                break;
                            }
                        }
                    }
                    viewModel.LeftNotes.Add(newNote);
                }
            }
            var listBox2 = sender as ListBox;
            listBox2.SelectedValue = null;
        }

        private void LeftCircuitGrid_ToggleExistingBreaker(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem)
            {
                if (selectedItems.Any())
                {
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range =
                        (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number) / 2) + 1;

                    var noteText =
                        "DENOTES EXISTING CIRCUIT BREAKER TO REMAIN; ALL OTHERS ARE NEW TO MATCH EXISTING.";

                    var existingBreakerNote = viewModel.ElectricalPanelNotes.FirstOrDefault(n =>
                        n.Note == noteText
                    );

                    if (existingBreakerNote == null)
                    {
                        // Only add if it does not exist
                        existingBreakerNote = new ElectricalPanelNote() { Note = noteText };
                        viewModel.ElectricalPanelNotes.Add(existingBreakerNote);
                        existingBreakerNote = viewModel.ElectricalPanelNotes.FirstOrDefault(n =>
                            n.Note == noteText
                        );
                    }

                    var newNote = new ElectricalPanelNoteRel(
                        Guid.NewGuid().ToString(),
                        existingBreakerNote.ProjectId,
                        Panel.Id,
                        existingBreakerNote.Id,
                        existingBreakerNote.Note,
                        0,
                        0,
                        0,
                        existingBreakerNote.Tag
                    );
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<ElectricalPanelNoteRel> existingNodes = new List<ElectricalPanelNoteRel>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1) * 2;

                    foreach (var note in viewModel.LeftNotes)
                    {
                        int startCircuit2 = note.CircuitNo;
                        int endCircuit2 = note.CircuitNo + (note.Length - 1) * 2;
                        if (
                            (startCircuit <= endCircuit2 && endCircuit >= startCircuit2)
                            || (startCircuit2 <= endCircuit && endCircuit2 >= startCircuit)
                        )
                        {
                            existingNodes.Add(note);
                        }
                    }

                    List<ElectricalPanelNoteRel> existingNodes2 = existingNodes
                        .OrderBy(note => note.Stack)
                        .ToList();

                    if (existingNodes2.Count > 0)
                    {
                        for (int i = 0; i <= existingNodes2.Last().Stack + 1; i++)
                        {
                            var node = existingNodes2.Find(note => note.Stack == i);
                            if (node == null)
                            {
                                newNote.Stack = i;
                                break;
                            }
                        }
                    }
                    viewModel.LeftNotes.Add(newNote);
                }
            }
        }

        private void LeftCircuitGrid_DeleteNotes(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem)
            {
                List<ElectricalPanelNoteRel> toRemove = new List<ElectricalPanelNoteRel>();

                foreach (var note in viewModel.LeftNotes)
                {
                    int startCircuit = note.CircuitNo;
                    int endCircuit = note.CircuitNo + (note.Length - 1) * 2;
                    foreach (var item in selectedItems)
                    {
                        if (item.Number >= startCircuit && item.Number <= endCircuit)
                        {
                            toRemove.Add(note);
                        }
                    }
                }
                foreach (var note in toRemove)
                {
                    viewModel.LeftNotes.Remove(note);
                }
            }
        }

        private void LeftCircuitGrid_MiniBreakerOptions(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            Circuit circuit = selectedItems.First();
            Circuit? nextCircuit = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .FirstOrDefault(c => c.Number == circuit.Number + 2);
            ElectricalPanelMiniBreakerWindow window = new ElectricalPanelMiniBreakerWindow(
                ProjectControlViewModel.database,
                Panel.componentsCollection,
                Panel.Id,
                Panel.Name,
                circuit,
                nextCircuit
            );
            window.Show();
        }

        private void RightCircuitGrid_ToggleCustomDescription(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem && selectedItems.Any())
            {
                bool allCustom = false;
                foreach (var circuit in selectedItems)
                {
                    if (!circuit.CustomDescription)
                    {
                        allCustom = true;
                        break;
                    }
                }

                foreach (var circuit in selectedItems)
                {
                    circuit.CustomDescription = allCustom;
                }
            }
        }

        private void RightCircuitGrid_ToggleCustomBreakerSize(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem && selectedItems.Any())
            {
                bool allCustom = false;
                foreach (var circuit in selectedItems)
                {
                    if (!circuit.CustomBreakerSize)
                    {
                        allCustom = true;
                        break;
                    }
                }

                foreach (var circuit in selectedItems)
                {
                    circuit.CustomBreakerSize = allCustom;
                }
            }
        }

        private void RightCircuitGrid_AddNote(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (
                sender is ListBox listBox
                && listBox.SelectedItem is ElectricalPanelNote selectedNote
            )
            {
                if (selectedItems.Any())
                {
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range =
                        (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number) / 2) + 1;

                    var newNote = new ElectricalPanelNoteRel(
                        Guid.NewGuid().ToString(),
                        selectedNote.ProjectId,
                        Panel.Id,
                        selectedNote.Id,
                        selectedNote.Note,
                        0,
                        0,
                        0,
                        selectedNote.Tag
                    );
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<ElectricalPanelNoteRel> existingNotes = new List<ElectricalPanelNoteRel>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1) * 2;

                    foreach (var note in viewModel.RightNotes)
                    {
                        int startCircuit2 = note.CircuitNo;
                        int endCircuit2 = note.CircuitNo + (note.Length - 1) * 2;
                        if (
                            (startCircuit <= endCircuit2 && endCircuit >= startCircuit2)
                            || (startCircuit2 <= endCircuit && endCircuit2 >= startCircuit)
                        )
                        {
                            existingNotes.Add(note);
                        }
                    }

                    List<ElectricalPanelNoteRel> existingNotes2 = existingNotes
                        .OrderBy(note => note.Stack)
                        .ToList();

                    if (existingNotes2.Count > 0)
                    {
                        for (int i = 0; i <= existingNotes2.Last().Stack + 1; i++)
                        {
                            var node = existingNotes2.Find(note => note.Stack == i);
                            if (node == null)
                            {
                                newNote.Stack = i;
                                break;
                            }
                        }
                    }

                    viewModel.RightNotes.Add(newNote);
                }
            }
            var listBox2 = sender as ListBox;
            listBox2.SelectedValue = null;
        }

        private void RightCircuitGrid_ToggleExistingBreaker(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem)
            {
                if (selectedItems.Any())
                {
                    var firstItem = selectedItems.First();
                    var lastItem = selectedItems.Last();
                    var range =
                        (int)Math.Ceiling((double)(lastItem.Number - firstItem.Number) / 2) + 1;

                    var noteText =
                        "DENOTES EXISTING CIRCUIT BREAKER TO REMAIN; ALL OTHERS ARE NEW TO MATCH EXISTING.";

                    var existingBreakerNote = viewModel.ElectricalPanelNotes.FirstOrDefault(n =>
                        n.Note == noteText
                    );

                    if (existingBreakerNote == null)
                    {
                        // Only add if it does not exist
                        existingBreakerNote = new ElectricalPanelNote() { Note = noteText };
                        viewModel.ElectricalPanelNotes.Add(existingBreakerNote);
                        existingBreakerNote = viewModel.ElectricalPanelNotes.FirstOrDefault(n =>
                            n.Note == noteText
                        );
                    }

                    var newNote = new ElectricalPanelNoteRel(
                        Guid.NewGuid().ToString(),
                        existingBreakerNote.ProjectId,
                        Panel.Id,
                        existingBreakerNote.Id,
                        existingBreakerNote.Note,
                        0,
                        0,
                        0,
                        existingBreakerNote.Tag
                    );
                    newNote.CircuitNo = firstItem.Number;
                    newNote.Length = range;

                    List<ElectricalPanelNoteRel> existingNotes = new List<ElectricalPanelNoteRel>();
                    int startCircuit = newNote.CircuitNo;
                    int endCircuit = newNote.CircuitNo + (newNote.Length - 1) * 2;

                    foreach (var note in viewModel.RightNotes)
                    {
                        int startCircuit2 = note.CircuitNo;
                        int endCircuit2 = note.CircuitNo + (note.Length - 1) * 2;
                        if (
                            (startCircuit <= endCircuit2 && endCircuit >= startCircuit2)
                            || (startCircuit2 <= endCircuit && endCircuit2 >= startCircuit)
                        )
                        {
                            existingNotes.Add(note);
                        }
                    }

                    List<ElectricalPanelNoteRel> existingNotes2 = existingNotes
                        .OrderBy(note => note.Stack)
                        .ToList();

                    if (existingNotes2.Count > 0)
                    {
                        for (int i = 0; i <= existingNotes2.Last().Stack + 1; i++)
                        {
                            var node = existingNotes2.Find(note => note.Stack == i);
                            if (node == null)
                            {
                                newNote.Stack = i;
                                break;
                            }
                        }
                    }

                    viewModel.RightNotes.Add(newNote);
                }
            }
        }

        private void RightCircuitGrid_DeleteNotes(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number)
                .ToList();
            if (sender is MenuItem menuItem)
            {
                List<ElectricalPanelNoteRel> toRemove = new List<ElectricalPanelNoteRel>();

                foreach (var note in viewModel.RightNotes)
                {
                    int startCircuit = note.CircuitNo;
                    int endCircuit = note.CircuitNo + (note.Length - 1) * 2;
                    foreach (var item in selectedItems)
                    {
                        if (item.Number >= startCircuit && item.Number <= endCircuit)
                        {
                            toRemove.Add(note);
                        }
                    }
                }
                foreach (var note in toRemove)
                {
                    viewModel.RightNotes.Remove(note);
                }
            }
        }

        private void RightCircuitGrid_MiniBreakerOptions(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            Circuit circuit = selectedItems.First();
            Circuit? nextCircuit = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .FirstOrDefault(c => c.Number == circuit.Number + 2);

            ElectricalPanelMiniBreakerWindow window = new ElectricalPanelMiniBreakerWindow(
                ProjectControlViewModel.database,
                Panel.componentsCollection,
                Panel.Id,
                Panel.Name,
                circuit,
                nextCircuit
            );
            window.Show();
        }

        private void Component_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                if (item.DataContext is ElectricalPanel panel)
                {
                    PanelPopup.DataContext = panel;
                    PanelPopup.IsOpen = true;
                    TransformerPopup.IsOpen = false;
                    EquipmentPopup.IsOpen = false;
                    e.Handled = true;
                }
                else if (item.DataContext is ElectricalTransformer transformer)
                {
                    TransformerPopup.DataContext = transformer;
                    TransformerPopup.IsOpen = true;
                    EquipmentPopup.IsOpen = false;
                    PanelPopup.IsOpen = false;
                    e.Handled = true;
                }
                else if (item.DataContext is ElectricalEquipment equipment)
                {
                    EquipmentPopup.DataContext = equipment;
                    EquipmentPopup.IsOpen = true;
                    PanelPopup.IsOpen = false;
                    TransformerPopup.IsOpen = false;
                    e.Handled = true;
                }
            }
        }

        private async void AddSpareBreaker1pLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);
            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(
                            1,
                            mocpId,
                            Panel,
                            selectedItems.ElementAt(0).number
                        );
                        selectedItems.ElementAt(0).EquipId = spare.Id;
                        selectedItems.ElementAt(0).Description = spare.EquipNo;
                        selectedItems.ElementAt(0).BreakerSize = mocp;
                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.leftComponents.Add(spare);
                    }
                }
            }
        }

        private async void AddSpareBreaker2pLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            int startCircuitNumber = selectedItems.ElementAt(0).Number;
            var allItems = LeftCircuitGrid.Items.Cast<Circuit>().OrderBy(circuit => circuit.Number);

            List<Circuit> targetItems = new List<Circuit>();
            targetItems.Add(selectedItems.ElementAt(0));
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 2)
            );

            if (targetItems.ElementAt(1) == null)
            {
                return;
            }
            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(2, mocpId, Panel, targetItems.ElementAt(0).Number);
                        targetItems.ElementAt(0).EquipId = spare.Id;
                        targetItems.ElementAt(0).Description = spare.EquipNo;
                        targetItems.ElementAt(0).BreakerSize = mocp;

                        targetItems.ElementAt(1).Description = "---";
                        targetItems.ElementAt(1).BreakerSize = 2;

                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.leftComponents.Add(spare);
                    }
                }
            }
        }

        private async void AddSpareBreaker3pLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            int startCircuitNumber = selectedItems.ElementAt(0).Number;
            var allItems = LeftCircuitGrid.Items.Cast<Circuit>().OrderBy(circuit => circuit.Number);

            List<Circuit> targetItems = new List<Circuit>();
            targetItems.Add(selectedItems.ElementAt(0));
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 2)
            );
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 4)
            );

            if (targetItems.ElementAt(1) == null || targetItems.ElementAt(2) == null)
            {
                return;
            }

            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(3, mocpId, Panel, targetItems.ElementAt(0).number);
                        targetItems.ElementAt(0).EquipId = spare.Id;
                        targetItems.ElementAt(0).Description = spare.EquipNo;
                        targetItems.ElementAt(0).BreakerSize = mocp;

                        targetItems.ElementAt(1).Description = "---";
                        targetItems.ElementAt(2).Description = "---";
                        targetItems.ElementAt(2).BreakerSize = 3;
                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.leftComponents.Add(spare);
                    }
                }
            }
        }

        private async void AddSpareBreaker1pRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);
            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(
                            1,
                            mocpId,
                            Panel,
                            selectedItems.ElementAt(0).number
                        );
                        selectedItems.ElementAt(0).EquipId = spare.Id;
                        selectedItems.ElementAt(0).Description = spare.EquipNo;
                        selectedItems.ElementAt(0).BreakerSize = mocp;
                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.rightComponents.Add(spare);
                    }
                }
            }
        }

        private async void AddSpareBreaker2pRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            int startCircuitNumber = selectedItems.ElementAt(0).Number;
            var allItems = RightCircuitGrid
                .Items.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            List<Circuit> targetItems = new List<Circuit>();
            targetItems.Add(selectedItems.ElementAt(0));
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 2)
            );

            if (targetItems.ElementAt(1) == null)
            {
                return;
            }
            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(2, mocpId, Panel, targetItems.ElementAt(0).Number);
                        targetItems.ElementAt(0).EquipId = spare.Id;
                        targetItems.ElementAt(0).Description = spare.EquipNo;
                        targetItems.ElementAt(0).BreakerSize = mocp;

                        targetItems.ElementAt(1).Description = "---";
                        targetItems.ElementAt(1).BreakerSize = 2;

                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.rightComponents.Add(spare);
                    }
                }
            }
        }

        private async void AddSpareBreaker3pRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightCircuitGrid
                .SelectedItems.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            int startCircuitNumber = selectedItems.ElementAt(0).Number;
            var allItems = RightCircuitGrid
                .Items.Cast<Circuit>()
                .OrderBy(circuit => circuit.Number);

            List<Circuit> targetItems = new List<Circuit>();
            targetItems.Add(selectedItems.ElementAt(0));
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 2)
            );
            targetItems.Add(
                allItems.FirstOrDefault(circuit => circuit.Number == startCircuitNumber + 4)
            );

            if (targetItems.ElementAt(1) == null || targetItems.ElementAt(2) == null)
            {
                return;
            }

            if (sender is MenuItem item)
            {
                string mocpIdStr = item.Tag.ToString();
                int mocpId;
                if (int.TryParse(mocpIdStr, out mocpId))
                {
                    string mocpStr = item.Header.ToString().Replace("A", "");
                    int mocp;
                    if (int.TryParse(mocpStr, out mocp))
                    {
                        Spare spare = new Spare(3, mocpId, Panel, targetItems.ElementAt(0).number);
                        targetItems.ElementAt(0).EquipId = spare.Id;
                        targetItems.ElementAt(0).Description = spare.EquipNo;
                        targetItems.ElementAt(0).BreakerSize = mocp;

                        targetItems.ElementAt(1).Description = "---";
                        targetItems.ElementAt(2).Description = "---";
                        targetItems.ElementAt(2).BreakerSize = 3;
                        MySqlConnection conn = new MySqlConnection(Panel.Database.ConnectionString);
                        await Panel.Database.OpenConnectionAsync(conn);
                        await Panel.Database.InsertEquipment(Panel.ProjectId, spare, conn);
                        await Panel.Database.CloseConnectionAsync(conn);
                        ProjectControlViewModel.ActiveElectricalProject.ElectricalEquipments.Add(
                            spare
                        );
                        Panel.rightComponents.Add(spare);
                    }
                }
            }
        }
    }

    public class LeftMarginConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values[0] is int CircuitNo && values[1] is int stack)
            {
                var NewCircuitNo = (int)Math.Floor((CircuitNo - 1) / 2.0);
                // Adjust the margin based on the number of notes

                return new Thickness(0, NewCircuitNo * 28, stack * 30, 0);
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(
            object values,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }

    public class RightMarginConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values[0] is int CircuitNo && values[1] is int stack)
            {
                var NewCircuitNo = (int)Math.Floor((CircuitNo - 1) / 2.0);
                // Adjust the margin based on the number of notes

                return new Thickness(stack * 30, NewCircuitNo * 28, 0, 0);
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(
            object values,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
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

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }

    public class BreakerSizeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int breakerSize)
            {
                if (breakerSize == 0)
                {
                    return "";
                }
            }
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            // Implement ConvertBack if you need two way binding
            if (value is string strValue && int.TryParse(strValue, out int result))
            {
                return result;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    public class NotEmptyStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string length && length != "")
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
