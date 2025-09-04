using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPDesignTool
{
  public class SetVaPerPhaseViewModel : INotifyPropertyChanged
  {
    private string _PhaseA;
    private string _PhaseB;
    private string _PhaseC;
    private readonly float _OriginalVa;
    private float _NewVa;

    public Visibility ShowCPhase { get; set; }

    private ElectricalEquipment ElectricalEquipment { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

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
      _OriginalVa = equip.OriginalVa;
      _NewVa = equip.OriginalVa;
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

    public string OriginalVa
    {
      get { return _OriginalVa.ToString(); }
    }

    public float NewVa
    {
      get { return _NewVa; }
      set
      {
        if (value != _NewVa)
        {
          _NewVa = value;
          OnPropertyChanged(nameof(NewVa));
        }
      }
    }

    public void Reset()
    {
      ElectricalEquipment.PhaseAVA = -1;
      ElectricalEquipment.PhaseBVA = -1;
      ElectricalEquipment.PhaseCVA = -1;
      ElectricalEquipment.Va = ElectricalEquipment.OriginalVa;
    }

    private void Aggregate()
    {
      if (_PhaseA == _PhaseB && _PhaseB == _PhaseC)
      {
        float f = 1.732F;
        ElectricalEquipment.Va =
          (ElectricalEquipment.PhaseAVA > -1 ? ElectricalEquipment.PhaseAVA / f : 0)
          + (ElectricalEquipment.PhaseBVA > -1 ? ElectricalEquipment.PhaseBVA / f : 0)
          + (ElectricalEquipment.PhaseCVA > -1 ? ElectricalEquipment.PhaseCVA / f : 0);
      }
      else
      {
        ElectricalEquipment.Va =
          (ElectricalEquipment.PhaseAVA > -1 ? ElectricalEquipment.PhaseAVA : 0)
          + (ElectricalEquipment.PhaseBVA > -1 ? ElectricalEquipment.PhaseBVA : 0)
          + (ElectricalEquipment.PhaseCVA > -1 ? ElectricalEquipment.PhaseCVA : 0);
      }
      NewVa = ElectricalEquipment.Va;
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
