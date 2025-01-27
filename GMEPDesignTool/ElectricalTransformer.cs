﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalTransformer : ElectricalComponent
    {
        private int _distanceFromParent;
        private int _voltage;
        private int _kva;
        private bool _powered;

        public ElectricalPanel ChildPanel { get; set; }


        public ElectricalTransformer(
            string id,
            string projectId,
            string parentId,
            int distanceFromParent,
            string colorCode,
            int voltage,
            string name,
            int kva,
            bool powered,
            int circuitNo
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.colorCode = colorCode;
            this.parentId = parentId;
            this.phaseAVa = 0;
            this.phaseBVa = 0;
            this.phaseCVa = 0;
            this.name = name;
            this.circuitNo = circuitNo;
            _distanceFromParent = distanceFromParent;
            _voltage = voltage;
            _kva = kva;
            _powered = powered;
            SetPole();
        }


        public int DistanceFromParent
        {
            get => _distanceFromParent;
            set
            {
                _distanceFromParent = value;
                OnPropertyChanged(nameof(DistanceFromParent));
            }
        }

        public int Voltage
        {
            get => _voltage;
            set
            {
                _voltage = value;
                OnPropertyChanged(nameof(Voltage));
            }
        }

        public int Kva
        {
            get => _kva;
            set
            {
                _kva = value;
                OnPropertyChanged(nameof(Kva));
            }
        }

        public bool Powered
        {
            get => _powered;
            set
            {
                _powered = value;
                OnPropertyChanged(nameof(Powered));
            }
        }
        public void SetPole()
        {
            switch (Voltage)
            {
                case 7:
                    Pole = 2;
                    break;
                case 8:
                    Pole = 2;
                    break;
                default:
                    Pole = 3;
                    break;
            }
        }
        public void AddChildPanel(ElectricalPanel panel)
        {
            if (ChildPanel != null)
            {
                ChildPanel.ParentId = "";
            }
            ChildPanel = panel;
            ChildPanel.PropertyChanged +=Panel_PropertyChanged;
            PhaseAVA = panel.PhaseAVA;
            PhaseBVA = panel.PhaseBVA;
            PhaseCVA = panel.PhaseCVA;
        }



        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.ParentId))
            {
                if (sender is ElectricalPanel panel)
                {
                    panel.PropertyChanged -= Panel_PropertyChanged;
                    ChildPanel = null;
                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.PhaseAVA))
            {
                if (sender is ElectricalPanel panel)
                {
                    PhaseAVA = panel.PhaseAVA;
                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.PhaseBVA))
            {
                if (sender is ElectricalPanel panel)
                {
                    PhaseBVA = panel.PhaseBVA;
                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.PhaseCVA))
            {
                if (sender is ElectricalPanel panel)
                {
                    PhaseCVA = panel.PhaseCVA;
                }
            }
          
        }
        public bool Verify()
        {
            if (!Utils.IsUuid(Id))
            {
                return false;
            }

            if (!Utils.IsUuid(ParentId))
            {
                return false;
            }
            return true;
        }
    }
}
