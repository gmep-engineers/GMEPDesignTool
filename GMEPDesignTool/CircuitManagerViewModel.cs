﻿using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Globalization;
using System.Windows.Data;



namespace GMEPDesignTool
{
   public class CircuitManagerViewModel : ViewModelBase, IDropTarget
    {
        public ElectricalPanel Panel { get; set; }

        public ObservableCollection<Circuit> LeftCircuits { get; set; }
        public ObservableCollection<Circuit> RightCircuits { get; set; }
        public ObservableCollection<ElectricalEquipment> LeftEquipments { get; set; }
        public ObservableCollection<ElectricalEquipment> RightEquipments { get; set; }


        private int _phaseAVa;
        public int PhaseAVa
        {
            get => _phaseAVa;
            set => SetProperty(ref _phaseAVa, value);
        }

        private int _phaseBVa;
        public int PhaseBVa
        {
            get => _phaseBVa;
            set => SetProperty(ref _phaseBVa, value);
        }

        private int _phaseCVa;
        public int PhaseCVa
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
        public string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _pole;
        public int Pole
        {
            get => _pole;
            set => SetProperty(ref _pole, value);
        }
        public CircuitManagerViewModel(ElectricalPanel panel)
        {
            RightEquipments = panel.rightEquipments;
            LeftEquipments = panel.leftEquipments;
            Panel = panel;
            LeftCircuits = panel.leftCircuits;
            RightCircuits = panel.rightCircuits;
            _phaseAVa = panel.PhaseAVA;
            _phaseBVa = panel.PhaseBVA;
            _phaseCVa = panel.PhaseCVA;
            _kva = panel.Kva;
            _name = panel.Name;
            _pole = panel.Pole;
            Panel.PropertyChanged += Panel_PropertyChanged;
        }
        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                if (e.PropertyName == nameof(ElectricalPanel.Name))
                {
                    Name = Panel.Name;
                    OnPropertyChanged(nameof(Name));
                }
                if (e.PropertyName == nameof(Pole))
            {
                Pole = Panel.Pole;
            }
        }
            void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            ElectricalEquipment sourceItem = dropInfo.Data as ElectricalEquipment;
            ElectricalEquipment targetItem = dropInfo.TargetItem as ElectricalEquipment;

            if (sourceItem != null && targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            ElectricalEquipment sourceItem = dropInfo.Data as ElectricalEquipment;
            ElectricalEquipment targetItem = dropInfo.TargetItem as ElectricalEquipment;

            if (sourceItem != null && targetItem != null)
            {
                ObservableCollection<ElectricalEquipment> sourceCollection = LeftEquipments.Contains(sourceItem) ? LeftEquipments : RightEquipments;
                ObservableCollection<ElectricalEquipment> targetCollection = LeftEquipments.Contains(targetItem) ? LeftEquipments : RightEquipments;

                int sourceIndex = sourceCollection.IndexOf(sourceItem);
                int targetIndex = targetCollection.IndexOf(targetItem);

                if (sourceIndex != -1 && targetIndex != -1)
                {
                    sourceCollection.RemoveAt(sourceIndex);
                    targetCollection.Insert(targetIndex, sourceItem);
                    Panel.SetCircuitNumbers();
                    Panel.SetCircuitVa();

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }


}
