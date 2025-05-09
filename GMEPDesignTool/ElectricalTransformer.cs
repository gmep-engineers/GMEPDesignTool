﻿using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GMEPDesignTool
{
    public class ElectricalTransformer : ElectricalComponent
    {
        private int _distanceFromParent = 0;
        private int _voltage = 1;
        private int _kva = 1;
        private bool _powered = false;
        private int _aicRating = 0;
        public ElectricalPanel ChildPanel { get; set; }
        private bool _isHiddenOnPlan = false;
        private bool _isWallMounted = true;
        private string circuits = string.Empty;
        private string parentType = string.Empty;

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
            int circuitNo,
            bool isHiddenOnPlan,
            bool isWallMounted,
            int aicRating,
            int orderNo

        )
        {
            this.id = id;
            this.projectId = projectId;
            this.colorCode = colorCode;
            this.parentId = parentId;
            this.phaseAVa = 0;
            this.phaseBVa = 0;
            this.phaseCVa = 0;
            this.amp = 0;
            this.name = name;
            this.circuitNo = circuitNo;
            this.loadCategory = 3;
            _distanceFromParent = distanceFromParent;
            _voltage = voltage;
            _kva = kva;
            this.rootKva = 0;
            _powered = powered;
            Lcl = lcl;
            Lml = lml;
            SetPole();
            _isHiddenOnPlan = isHiddenOnPlan;
            _isWallMounted = isWallMounted;
            _aicRating=aicRating;
            this.orderNo = orderNo;
            this.componentType = "Transformer";
            DetermineCircuits();
        }
        public ElectricalTransformer()
        {
            loadCategory = 3;
            SetPole();
            this.componentType = "Transformer";
            DetermineCircuits();
        }


        public int DistanceFromParent
        {
            get => _distanceFromParent;
            set
            {
                if (_distanceFromParent != value)
                {
                    _distanceFromParent = value;
                    OnPropertyChanged(nameof(DistanceFromParent));
                }
            }
        }

        public int Voltage
        {
            get => _voltage;
            set
            {
                if (_voltage != value)
                {
                    _voltage = value;
                    OnPropertyChanged(nameof(Voltage));
                    SetPole();
                    if (ChildPanel != null)
                    {
                        DetermineCompatible(ChildPanel);
                    }
                }
            }
        }
        public override int CircuitNo
        {
            get => circuitNo;
            set
            {
                if (circuitNo != value)
                {
                    circuitNo = value;
                    DetermineCircuits();
                    OnPropertyChanged(nameof(CircuitNo));
                }
            }
        }
        public override int Pole
        {
            get => pole;
            set
            {
                if (pole != value)
                {
                    pole = value;
                    DetermineCircuits();
                    OnPropertyChanged(nameof(Pole));
                }
            }
        }

        public int Kva
        {
            get => _kva;
            set
            {
                if (_kva != value)
                {
                    _kva = value;
                    OnPropertyChanged(nameof(Kva));
                }
            }
        }

        public bool Powered
        {
            get => _powered;
            set
            {
                if (_powered != value)
                {
                    _powered = value;
                    OnPropertyChanged(nameof(Powered));
                }
            }
        }
        public string ParentType
        {
            get => parentType;
            set
            {
                if (parentType != value)
                {
                    parentType = value;
                    DetermineCircuits();
                    OnPropertyChanged(nameof(ParentType));
                }
            }
        }
        public override string ParentId
        {
            get => parentId;
            set
            {
                if (parentId != value)
                {
                    parentId = value ?? string.Empty;
                    ParentType = "";
                    DetermineCircuits();
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }
        public bool IsWallMounted
        {
            get => _isWallMounted;
            set
            {
                if (_isWallMounted != value)
                {
                    _isWallMounted = value;
                    OnPropertyChanged(nameof(IsWallMounted));
                }
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
         public void DetermineCircuits()
        {
            if (ParentType != "PANEL ")
            {
                Circuits = "N/A";
                return;
            }
            if (circuitNo == 0)
            {
                Circuits = "Assign";
                return;
            }
            if (Pole == 3)
            {
                Circuits = $"{circuitNo},{circuitNo + 2},{circuitNo + 4}";
            }
            if (Pole == 2)
            {
                Circuits = $"{circuitNo},{circuitNo + 2}";
            }
            if (Pole == 1)
            {
                Circuits = $"{circuitNo}";
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
            Amp = panel.Amp;
            Kva = SetKva();
            Lcl = panel.Lcl;
            Lml = panel.Lml;
            ALcl = panel.ALcl;
            BLcl = panel.BLcl;
            CLcl = panel.CLcl;
            ALml = panel.ALml;
            BLml = panel.BLml;
            CLml = panel.CLml;
            UpdateFlag = !UpdateFlag;
            OnPropertyChanged(nameof(ChildPanel));
            DetermineCompatible(panel);
        }


        private void DetermineCompatible(ElectricalPanel panel)
        {
            panel.ErrorMessages.Remove("transformer-voltage-error");
            ErrorMessages.Remove("child-errors");
            if (panel.Type == 1 && Voltage != 1 && Voltage != 5)
            {
                panel.ErrorMessages.Add("transformer-voltage-error", "This panel has a different voltage/phase than the output of its parent transformer.");
            }
            if (panel.Type == 2 && Voltage != 8)
            {
                panel.ErrorMessages.Add("transformer-voltage-error", "This panel has a different voltage/phase than the output of its parent transformer.");
            }
            if (panel.Type == 3 && Voltage != 2 && Voltage != 4)
            {
                panel.ErrorMessages.Add("transformer-voltage-error", "This panel has a different voltage/phase than the output of its parent transformer.");
            }
            if (panel.Type == 4 && Voltage != 3 && Voltage != 6)
            {
                panel.ErrorMessages.Add("transformer-voltage-error", "This panel has a different voltage/phase than the output of its parent transformer.");
            }
            if (panel.Type == 5 && Voltage != 7)
            {
                panel.ErrorMessages.Add("transformer-voltage-error", "This panel has a different voltage/phase than the output of its parent transformer.");
            }
            if (panel.ErrorMessages.Count > 0)
            {
                ErrorMessages.Add("child-errors", "There are issues with the child of this transformer.");
            }
        }
        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.ParentId))
            {
                if (sender is ElectricalPanel panel)
                {
                    panel.PropertyChanged -= Panel_PropertyChanged;
                    ChildPanel = null;
                    PhaseAVA = 0;
                    PhaseBVA = 0;
                    PhaseCVA = 0;
                    Amp= 0;
                    Kva = 0;
                    Lcl = 0;
                    Lml = 0;
                    ALcl = 0;
                    BLcl = 0;
                    CLcl = 0;
                    ALml = 0;
                    BLml = 0;
                    CLml = 0;
                    RootKva = 0;
                    UpdateFlag = !UpdateFlag;
                    panel.ErrorMessages.Remove("transformer-voltage-error");
                    ErrorMessages.Remove("child-errors");
                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.UpdateFlag))
            {
                if (sender is ElectricalPanel panel)
                {
                    PhaseAVA = panel.PhaseAVA;
                    PhaseBVA = panel.PhaseBVA;
                    PhaseCVA = panel.PhaseCVA;
                    Amp = panel.Amp;
                    Lcl = panel.Lcl;
                    Lml = panel.Lml;
                    ALml = panel.ALml;
                    BLml = panel.BLml;
                    CLml = panel.CLml;
                    ALcl = panel.ALcl;
                    BLcl = panel.BLcl;
                    CLcl = panel.CLcl;
                    UpdateFlag = !UpdateFlag;
                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.Type))
            {
                if (sender is ElectricalPanel panel)
                {
                    DetermineCompatible(panel);
                }
            }
            Kva = SetKva();
        }
        public int SetKva()
        {
            RootKva = (PhaseAVA + PhaseBVA + PhaseCVA + (Lcl/4) + (Lml/4)) / 1000;
            var kva = (float)Math.Ceiling(RootKva);
            ErrorMessages.Remove("kva-error");
            switch (kva)
            {
                case var _ when kva <= 45:
                    return 1;
                case var _ when kva <= 75:
                    return 2;
                case var _ when kva <= 112.5:
                    return 3;
                case var _ when kva <= 150:
                    return 4;
                case var _ when kva <= 225:
                    return 5;
                case var _ when kva <= 300:
                    return 6;
                case var _ when kva <= 500:
                    return 7;
                case var _ when kva <= 750:
                    return 8;
                case var _ when kva <= 1000:
                    return 9;
                case var _ when kva <= 1500:
                    return 10;
                case var _ when kva <= 2000:
                    return 11;
                case var _ when kva <= 2500:
                    return 12;
                case var _ when kva > 2500:
                    ErrorMessages.Add("kva-error","KVA of transformer is too high!");
                    return 13;
                default:
                    return 1;
            }
        }
        public bool IsHiddenOnPlan
        {
            get => _isHiddenOnPlan;
            set
            {
                _isHiddenOnPlan = value;
                OnPropertyChanged(nameof(IsHiddenOnPlan));
            }
        }
        public int AicRating
        {
            get => _aicRating;
            set
            {
                _aicRating = value;
                OnPropertyChanged(nameof(AicRating));
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
