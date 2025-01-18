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



namespace GMEPDesignTool
{
   public class CircuitManagerViewModel : ViewModelBase, IDropTarget
    {
        public ElectricalPanel Panel { get; set; }

        public ObservableCollection<Circuit> Circuits { get; set; }
        public ObservableCollection<ElectricalEquipment> Equipments { get; set; }

        public int GridSize { get; set; }

        public CircuitManagerViewModel(ElectricalPanel panel, ObservableCollection<ElectricalEquipment> equipment)
        {
            Equipments=equipment;
            Panel = panel;
            Circuits = panel.circuits;
            GridSize = (int)Math.Ceiling((double)panel.NumBreakers / 2);
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
                int sourceIndex = Equipments.IndexOf(sourceItem);
                int targetIndex = Equipments.IndexOf(targetItem);

                if (sourceIndex != -1 && targetIndex != -1)
                {
                    Equipments.Move(sourceIndex, targetIndex);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class PanelCircuit : ViewModelBase
    {
        public int Id { get; set; }
        public int connectedVoltage { get; set; }
        public PanelCircuit(int id, int ConnectedVoltage)
        {
            Id = id;
            ConnectedVoltage = connectedVoltage;
        }

    }
  
}
