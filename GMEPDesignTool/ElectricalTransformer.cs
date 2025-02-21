using Amazon.S3.Model;
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
        private int _distanceFromParent;
        private int _voltage;
        private int _kva;
        private bool _powered;
        private int _aicRating;
        public ElectricalPanel ChildPanel { get; set; }
        private bool _isHiddenOnPlan;
        private bool _isWallMounted;

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
            int aicRating

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
            _powered = powered;
            Lcl = lcl;
            Lml = lml;
            SetPole();
            _isHiddenOnPlan = isHiddenOnPlan;
            _isWallMounted = isWallMounted;
            _aicRating=aicRating;
            this.componentType = "Transformer";
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

                }
            }
            if (e.PropertyName == nameof(ElectricalPanel.UpdateFlag))
            {
                if (sender is ElectricalPanel panel)
                {
                    PhaseAVA = panel.PhaseAVA;
                    PhaseBVA = panel.PhaseBVA;
                    PhaseCVA = panel.PhaseCVA;
                    Lcl = panel.Lcl;
                    Lml = panel.Lml;
                    ALml = panel.ALml;
                    BLml = panel.BLml;
                    CLml = panel.CLml;
                    ALcl = panel.ALcl;
                    BLcl = panel.BLcl;
                    CLcl = panel.CLcl;
                }
            }
      
            if (e.PropertyName == nameof(ElectricalPanel.Amp))
            {
                if (sender is ElectricalPanel panel)
                {
                    Amp = panel.Amp;
                }
            }

            Kva = SetKva();
        }
        public int SetKva()
        {
            var kva = (float)Math.Ceiling((PhaseAVA + PhaseBVA + PhaseCVA + (Lcl/4) + (Lml/4)) / 1000);

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
