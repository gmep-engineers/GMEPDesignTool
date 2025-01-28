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
            this.amp = 0;
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
                SetPole();
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
            Amp = panel.Amp;
            Kva = SetKva();
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
            var kva = (float)Math.Ceiling((PhaseAVA + PhaseBVA + PhaseCVA) / 1000);

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
