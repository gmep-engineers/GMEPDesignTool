using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;

namespace GMEPDesignTool
{
    public class CircuitManagerViewModel : ViewModelBase, IDropTarget
    {
        public ElectricalPanel Panel { get; set; }
        public ObservableCollection<Circuit> LeftCircuits { get; set; }
        public ObservableCollection<Circuit> RightCircuits { get; set; }
        public ObservableCollection<ElectricalComponent> ComponentsCollection { get; set; }
        public ObservableCollection<ElectricalComponent> LeftComponents { get; set; }
        public ObservableCollection<ElectricalComponent> RightComponents { get; set; }
        public ObservableCollection<Note> Notes { get; set; }
        public ObservableCollection<ElectricalPanelNote> ElectricalPanelNotes { get; set; }
        public ObservableCollection<Note> LeftNodes { get; set; }
        public ObservableCollection<Note> RightNodes { get; set; }
        public ObservableCollection<ElectricalPanelNoteRel> LeftNotes { get; set; }
        public ObservableCollection<ElectricalPanelNoteRel> RightNotes { get; set; }

        private float _phaseAVa;
        public float PhaseAVa
        {
            get => _phaseAVa;
            set => SetProperty(ref _phaseAVa, value);
        }

        private float _phaseBVa;
        public float PhaseBVa
        {
            get => _phaseBVa;
            set => SetProperty(ref _phaseBVa, value);
        }

        private float _phaseCVa;
        public float PhaseCVa
        {
            get => _phaseCVa;
            set => SetProperty(ref _phaseCVa, value);
        }
        public float _kva;
        public float Kva
        {
            get => _kva;
            set => SetProperty(ref _kva, value);
        }
        public float _va;

        public float Va
        {
            get => _va;
            set => SetProperty(ref _va, value);
        }
        public string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string _location;

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private int _pole;
        public int Pole
        {
            get => _pole;
            set => SetProperty(ref _pole, value);
        }

        private float _amp;
        public float Amp
        {
            get => _amp;
            set => SetProperty(ref _amp, value);
        }
        private float _lcl;
        public float Lcl
        {
            get => _lcl;
            set => SetProperty(ref _lcl, value);
        }
        private float _lml;
        public float Lml
        {
            get => _lml;
            set => SetProperty(ref _lml, value);
        }
        private string busRating;
        public string BusRating
        {
            get => busRating;
            set => SetProperty(ref busRating, value);
        }
        private string mainRating;
        public string MainRating
        {
            get => mainRating;
            set => SetProperty(ref mainRating, value);
        }
        public string voltage;
        public string Voltage
        {
            get => voltage;
            set => SetProperty(ref voltage, value);
        }
        public string phases;
        public string Phases
        {
            get => phases;
            set => SetProperty(ref phases, value);
        }
        private string wire;
        public string Wire
        {
            get => wire;
            set => SetProperty(ref wire, value);
        }

        private string mounting;
        public string Mounting
        {
            get => mounting;
            set => SetProperty(ref mounting, value);
        }

        private string parentName;

        public string ParentName
        {
            get => parentName;
            set => SetProperty(ref parentName, value);
        }

        private string parentType;

        public string ParentType
        {
            get => parentType;
            set => SetProperty(ref parentType, value);
        }

        private char highLegPhase;

        public char HighLegPhase
        {
            get => highLegPhase;
            set => SetProperty(ref highLegPhase, value);
        }

        public CircuitManagerViewModel(ElectricalPanel panel)
        {
            RightComponents = panel.rightComponents;
            LeftComponents = panel.leftComponents;
            ComponentsCollection = panel.componentsCollection;
            Panel = panel;
            ElectricalPanelNotes = panel.notes;

            LeftCircuits = panel.leftCircuits;
            RightCircuits = panel.rightCircuits;
            LeftNodes = panel.leftNodes;
            RightNodes = panel.rightNodes;
            LeftNotes = panel.leftNotes;
            RightNotes = panel.rightNotes;
            _phaseAVa = panel.PhaseAVA;
            _phaseBVa = panel.PhaseBVA;
            _phaseCVa = panel.PhaseCVA;
            _kva = panel.Kva;
            _name = panel.Name;
            _pole = panel.Pole;
            _amp = panel.Amp;
            _lcl = panel.Lcl;
            _lml = panel.Lml;
            _va = panel.Va;
            highLegPhase = panel.HighLegPhase;
            parentName = panel.ParentName;
            parentType = panel.ParentType;
            _location = panel.Location;
            wire = determineWire(panel.Type);
            busRating = setBusRating(panel.BusSize);
            MainRating = setMainRating(panel);
            voltage = determineVoltage(panel.Type);
            phases = determinePhases(panel.Type);
            mounting = setMounting(panel.IsRecessed);
            Panel.PropertyChanged += Panel_PropertyChanged;
            //Notes.CollectionChanged += Notes_CollectionChanged;
            ElectricalPanelNotes.CollectionChanged += ElectricalPanelNotes_CollectionChanged;
        }

        public void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.PhaseAVA))
            {
                PhaseAVa = Panel.PhaseAVA;
                OnPropertyChanged(nameof(PhaseAVa));
            }
            if (e.PropertyName == nameof(ElectricalPanel.PhaseBVA))
            {
                PhaseBVa = Panel.PhaseBVA;
                OnPropertyChanged(nameof(PhaseBVa));
            }
            if (e.PropertyName == nameof(ElectricalPanel.PhaseCVA))
            {
                PhaseCVa = Panel.PhaseCVA;
                OnPropertyChanged(nameof(PhaseCVa));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Kva))
            {
                Kva = Panel.Kva;
                OnPropertyChanged(nameof(Kva));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Va))
            {
                Va = Panel.Va;
                OnPropertyChanged(nameof(Va));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Name))
            {
                Name = Panel.Name;
                OnPropertyChanged(nameof(Name));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Pole))
            {
                Pole = Panel.Pole;
                OnPropertyChanged(nameof(Pole));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Amp))
            {
                Amp = Panel.Amp;
                OnPropertyChanged(nameof(Amp));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Lcl))
            {
                Lcl = Panel.Lcl;
                OnPropertyChanged(nameof(Lcl));
            }
            if (e.PropertyName == nameof(ElectricalPanel.Lml))
            {
                Lml = Panel.Lml;
                OnPropertyChanged(nameof(Lml));
            }
            if (e.PropertyName == nameof(ElectricalPanel.BusSize))
            {
                BusRating = setBusRating(Panel.BusSize);
            }
            if (e.PropertyName == nameof(ElectricalPanel.MainSize))
            {
                MainRating = setMainRating(Panel);
            }
            if (e.PropertyName == nameof(ElectricalPanel.IsMlo))
            {
                MainRating = setMainRating(Panel);
            }
            if (e.PropertyName == nameof(ElectricalPanel.Type))
            {
                Voltage = determineVoltage(Panel.Type);
                Phases = determinePhases(Panel.Type);
                Wire = determineWire(Panel.Type);
            }
            if (e.PropertyName == nameof(ElectricalPanel.IsRecessed))
            {
                Mounting = setMounting(Panel.IsRecessed);
            }
            if (e.PropertyName == nameof(ElectricalPanel.Location))
            {
                Location = Panel.Location;
                OnPropertyChanged(nameof(Location));
            }
            if (e.PropertyName == nameof(ElectricalPanel.ParentName))
            {
                ParentName = Panel.ParentName;
                OnPropertyChanged(nameof(ParentName));
            }
            if (e.PropertyName == nameof(ElectricalPanel.ParentType))
            {
                ParentType = Panel.ParentType;
                OnPropertyChanged(nameof(ParentType));
            }
            if (e.PropertyName == nameof(ElectricalPanel.HighLegPhase))
            {
                HighLegPhase = Panel.HighLegPhase;
                OnPropertyChanged(nameof(HighLegPhase));
            }
        }

        public string setBusRating(int bus)
        {
            string result = "0A";
            switch (bus)
            {
                case 1:
                    result = "60A";
                    break;
                case 2:
                    result = "100A";
                    break;
                case 3:
                    result = "125A";
                    break;
                case 4:
                    result = "150A";
                    break;
                case 5:
                    result = "175A";
                    break;
                case 6:
                    result = "200A";
                    break;
                case 7:
                    result = "225A";
                    break;
                case 8:
                    result = "250A";
                    break;
                case 9:
                    result = "275A";
                    break;
                case 10:
                    result = "400A";
                    break;
                case 11:
                    result = "500A";
                    break;
                case 12:
                    result = "600A";
                    break;
                case 13:
                    result = "800A";
                    break;
            }
            return result;
        }

        public string setMainRating(ElectricalPanel panel)
        {
            string result = "0";
            switch (panel.MainSize)
            {
                case 1:
                    result = "60A";
                    break;
                case 2:
                    result = "100A";
                    break;
                case 3:
                    result = "125A";
                    break;
                case 4:
                    result = "150A";
                    break;
                case 5:
                    result = "175A";
                    break;
                case 6:
                    result = "200A";
                    break;
                case 7:
                    result = "225A";
                    break;
                case 8:
                    result = "250A";
                    break;
                case 9:
                    result = "275A";
                    break;
                case 10:
                    result = "400A";
                    break;
                case 11:
                    result = "500A";
                    break;
                case 12:
                    result = "600A";
                    break;
                case 13:
                    result = "800";
                    break;
            }
            if (panel.IsMlo)
            {
                result = "M.L.O";
            }
            return result;
        }

        public string determineVoltage(int type)
        {
            switch (type)
            {
                case 1:
                    return "120/208V";
                case 2:
                    return "120/240V";
                case 3:
                    return "277/480V";
                case 4:
                    return "120/240V";
                case 5:
                    return "120/208V";
            }
            return "";
        }

        public string determineWire(int type)
        {
            switch (type)
            {
                case 1:
                    return "4W";
                case 2:
                    return "3W";
                case 3:
                    return "4W";
                case 4:
                    return "4W";
                case 5:
                    return "3W";
            }
            return "";
        }

        public string determinePhases(int type)
        {
            switch (type)
            {
                case 1:
                    return "3ɸ";
                case 2:
                    return "1ɸ";
                case 3:
                    return "3ɸ";
                case 4:
                    return "3ɸ";
                case 5:
                    return "1ɸ";
            }
            return "";
        }

        public string setMounting(bool isRecessed)
        {
            if (isRecessed)
            {
                return "RECESSED";
            }
            return "SURFACE";
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            ElectricalComponent sourceItem = dropInfo.Data as ElectricalComponent;
            ElectricalComponent targetItem = dropInfo.TargetItem as ElectricalComponent;

            if (sourceItem != null && !(sourceItem is Space))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            ElectricalComponent sourceItem = dropInfo.Data as ElectricalComponent;
            ElectricalComponent targetItem = dropInfo.TargetItem as ElectricalComponent;

            if (sourceItem != null)
            {
                ObservableCollection<ElectricalComponent> sourceCollection = GetSourceCollection(
                    sourceItem
                );
                ObservableCollection<ElectricalComponent> targetCollection = GetTargetCollection(
                    targetItem
                );

                if (targetItem == null)
                {
                    if (LeftComponents.Count == 0)
                    {
                        targetCollection = LeftComponents;
                    }
                    else if (RightComponents.Count == 0)
                    {
                        targetCollection = RightComponents;
                    }
                    else if (ComponentsCollection.Count == 0)
                    {
                        targetCollection = ComponentsCollection;
                    }
                }

                int sourceIndex = sourceCollection.IndexOf(sourceItem);
                int targetIndex =
                    targetItem != null
                        ? targetCollection.IndexOf(targetItem)
                        : targetCollection.Count;

                if (targetIndex > targetCollection.Count - 1 && targetIndex != 0)
                {
                    targetIndex = targetCollection.Count - 1;
                }

                if (sourceIndex != -1)
                {
                    sourceCollection.RemoveAt(sourceIndex);
                    targetCollection.Insert(targetIndex, sourceItem);
                    Panel.SetCircuitNumbers();
                    Panel.SetCircuitVa();
                }
            }
        }

        private ObservableCollection<ElectricalComponent> GetSourceCollection(
            ElectricalComponent item
        )
        {
            if (LeftComponents.Contains(item))
            {
                return LeftComponents;
            }
            else if (RightComponents.Contains(item))
            {
                return RightComponents;
            }
            else
            {
                return ComponentsCollection;
            }
        }

        private ObservableCollection<ElectricalComponent> GetTargetCollection(
            ElectricalComponent item
        )
        {
            if (item == null)
            {
                return ComponentsCollection;
            }
            else if (LeftComponents.Contains(item))
            {
                return LeftComponents;
            }
            else if (RightComponents.Contains(item))
            {
                return RightComponents;
            }
            else
            {
                return ComponentsCollection;
            }
        }

        private void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Note removedItem in e.OldItems)
                {
                    var groupId = removedItem.GroupId;

                    var leftNodesToRemove = LeftNodes
                        .Where(node => node.GroupId == groupId)
                        .ToList();
                    foreach (var node in leftNodesToRemove)
                    {
                        LeftNodes.Remove(node);
                    }

                    var rightNodesToRemove = RightNodes
                        .Where(node => node.GroupId == groupId)
                        .ToList();
                    foreach (var node in rightNodesToRemove)
                    {
                        RightNodes.Remove(node);
                    }
                }
            }
        }

        private void ElectricalPanelNotes_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e
        )
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ElectricalPanelNote removedItem in e.OldItems)
                {
                    var leftNotesToRemove = LeftNotes
                        .Where(note => note.NoteId == removedItem.Id)
                        .ToList();
                    foreach (var note in leftNotesToRemove)
                    {
                        LeftNotes.Remove(note);
                    }

                    var rightNotesToRemove = RightNotes
                        .Where(note => note.NoteId == removedItem.Id)
                        .ToList();
                    foreach (var note in rightNotesToRemove)
                    {
                        RightNotes.Remove(note);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int pole)
            {
                return pole == 3 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
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

    public class LclConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float newVal)
            {
                return Math.Ceiling(newVal * 1.25);
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

    public class ValueRounder : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float newVal)
            {
                return Math.Ceiling(newVal);
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

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data",
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null)
        );
    }
}
