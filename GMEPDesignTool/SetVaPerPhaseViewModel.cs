using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPDesignTool
{
    public class SetVaPerPhaseViewModel
    {
        private string _PhaseA;
        private string _PhaseB;
        private string _PhaseC;

        public Visibility ShowCPhase { get; set; }

        private ElectricalEquipment ElectricalEquipment { get; set; }

        public SetVaPerPhaseViewModel(ElectricalEquipment equip)
        {
            if (equip.Pole == 3)
            {
                ShowCPhase = Visibility.Visible;
            }
            else
            {
                ShowCPhase = Visibility.Collapsed;
            }
            ElectricalEquipment = equip;
            _PhaseA = equip.PhaseAVA > -1 ? equip.PhaseAVA.ToString() : "";
            _PhaseB = equip.PhaseBVA > -1 ? equip.PhaseBVA.ToString() : "";
            _PhaseC = equip.PhaseCVA > -1 ? equip.PhaseCVA.ToString() : "";
        }

        public string PhaseA
        {
            get { return _PhaseA; }
            set
            {
                if (value != _PhaseA)
                {
                    _PhaseA = value;
                    if (Double.TryParse(_PhaseA, out double val))
                    {
                        ElectricalEquipment.PhaseAVA = (float)val;
                        Aggregate();
                    }
                }
            }
        }

        public string PhaseB
        {
            get { return _PhaseB; }
            set
            {
                if (value != _PhaseB)
                {
                    _PhaseB = value;
                    if (Double.TryParse(_PhaseB, out double val))
                    {
                        ElectricalEquipment.PhaseBVA = (float)val;
                        Aggregate();
                    }
                }
            }
        }

        public string PhaseC
        {
            get { return _PhaseC; }
            set
            {
                if (value != _PhaseC)
                {
                    _PhaseC = value;
                    if (Double.TryParse(_PhaseC, out double val))
                    {
                        ElectricalEquipment.PhaseCVA = (float)val;
                        Aggregate();
                    }
                }
            }
        }

        public void Reset()
        {
            ElectricalEquipment.PhaseAVA = -1;
            ElectricalEquipment.PhaseBVA = -1;
            ElectricalEquipment.PhaseCVA = -1;
        }

        private void Aggregate()
        {
            ElectricalEquipment.Va =
                (ElectricalEquipment.PhaseAVA > -1 ? ElectricalEquipment.PhaseAVA : 0)
                + (ElectricalEquipment.PhaseBVA > -1 ? ElectricalEquipment.PhaseBVA : 0)
                + (ElectricalEquipment.PhaseCVA > -1 ? ElectricalEquipment.PhaseCVA : 0);
        }
    }
}
