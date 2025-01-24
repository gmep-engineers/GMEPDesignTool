using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalPanel : ElectricalComponent
    {
        private int _busSize;
        private int _mainSize;
        private bool _isMlo;
        private bool _isDistribution;
        private int _numBreakers;
        private int _distanceFromParent;
        private int _aicRating;
        private float _kva;
        private float _amp;
        private int _type;
        private bool _powered;
        private int _phaseAVa;
        private int _phaseBVa;
        private int _phaseCVa;
        
        public ObservableCollection<ElectricalComponent> leftComponents { get; set; } = new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<ElectricalComponent> rightComponents{ get; set; } = new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<Circuit> leftCircuits { get; set; } = new ObservableCollection<Circuit>();

        public ObservableCollection<Circuit> rightCircuits { get; set; } = new ObservableCollection<Circuit>();


        private bool _isRecessed;

        public ElectricalPanel(
            string id,
            string projectId,
            int busSize,
            int mainSize,
            bool isMlo,
            bool isDistribution,
            string name,
            string colorCode,
            string parentId,
            int numBreakers,
            int distanceFromParent,
            int aicRating,
            float amp,
            int type,
            bool powered,
            bool isRecessed,
            int circuitNo
        ) : base()
        {
            this.id = id;
            _busSize = busSize;
            _mainSize = mainSize;
            _isMlo = isMlo;
            _isDistribution = isDistribution;
            this.name = name;
            this.colorCode = colorCode;
            this.parentId = parentId;
            this.projectId = projectId;
            this.circuitNo = circuitNo;
            _numBreakers = numBreakers;
            _distanceFromParent = distanceFromParent;
            _aicRating = aicRating;
            _amp = amp;
            _type = type;
            _powered = powered;
            _isRecessed = isRecessed;
            _phaseAVa = 0;
            _phaseBVa = 0;
            _phaseCVa = 0;  
            SetPole();
            PopulateCircuits();
        }

        public override int Pole
        {
            get => this.pole;
            set
            {
                this.pole = value;
                OnPropertyChanged(nameof(Pole));
                SetCircuitVa();
            }
        }
  
        public bool IsRecessed
        {
            get => _isRecessed;
            set
            {
                _isRecessed = value;
                OnPropertyChanged(nameof(IsRecessed));
            }
        }
        public int PhaseAVA
        {
            get => _phaseAVa;
            set
            {
                _phaseAVa = value;
                OnPropertyChanged(nameof(PhaseAVA));
            }
        }
        public int PhaseBVA
        {
            get => _phaseBVa;
            set
            {
                _phaseBVa = value;
                OnPropertyChanged(nameof(PhaseBVA));
            }
        }
        public int PhaseCVA
        {
            get => _phaseCVa;
            set
            {
                _phaseCVa = value;
                OnPropertyChanged(nameof(PhaseCVA));
            }
        }
       /*public override float Va
        {
            get => _kva;
            set
            {
                _kva = value;
                OnPropertyChanged(nameof(Va));
            }
        }*/

        public int BusSize
        {
            get => _busSize;
            set
            {
                _busSize = value;
                OnPropertyChanged(nameof(BusSize));
            }
        }

        public int MainSize
        {
            get => _mainSize;
            set
            {
                _mainSize = value;
                OnPropertyChanged(nameof(MainSize));
            }
        }

        public bool IsMlo
        {
            get => _isMlo;
            set
            {
                _isMlo = value;
                OnPropertyChanged(nameof(IsMlo));
            }
        }

        public bool IsDistribution
        {
            get => _isDistribution;
            set
            {
                _isDistribution = value;
                OnPropertyChanged(nameof(IsDistribution));
            }
        }

        public int NumBreakers
        {
            get => _numBreakers;
            set
            {
                _numBreakers = value;
                OnPropertyChanged(nameof(NumBreakers));
                PopulateCircuits();
                SetCircuitNumbers();
                SetCircuitVa();
            }
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

        public int AicRating
        {
            get => _aicRating;
            set
            {
                _aicRating = value;
                OnPropertyChanged(nameof(AicRating));
            }
        }

        public float Kva
        {
            get => _kva;
            set
            {
                _kva = value;
                OnPropertyChanged(nameof(Kva));
            }
        }

        public float Amp
        {
            get => _amp;
            set
            {
                _amp = value;
                OnPropertyChanged(nameof(Amp));
            }
        }

        public int Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
                SetPole();
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
            switch(Type) {
                case 1:
                    Pole = 3;
                    break;
                case 3:
                    Pole = 3;
                    break;
                case 4:
                    Pole = 3;
                    break;
                default:
                    Pole = 2;
                    break;
            }
        }
        public void PopulateCircuits()
        {
            int totalCircuits = leftCircuits.Count + rightCircuits.Count;

            while (totalCircuits < NumBreakers)
            {
                if (totalCircuits % 2 == 0)
                {
                    leftCircuits.Add(new Circuit(leftCircuits.Count * 2 + 1, 0, 0));
                }
                else
                {
                    rightCircuits.Add(new Circuit(rightCircuits.Count * 2 + 2, 0, 0));
                }
                totalCircuits++;
            }

            while (totalCircuits > NumBreakers)
            {
                if (rightCircuits.Count >= leftCircuits.Count)
                {
                    rightCircuits.RemoveAt(rightCircuits.Count - 1);
                }
                else
                {
                    leftCircuits.RemoveAt(leftCircuits.Count - 1);
                }
                totalCircuits--;
            }

        }
        private void Equipment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalComponent.Va) || e.PropertyName == nameof(ElectricalComponent.Pole))
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalComponent.ParentId))
            {
                if (sender is ElectricalEquipment equipment)
                {
                    equipment.PropertyChanged -= Equipment_PropertyChanged;
                    if (leftComponents.Contains(equipment))
                    {
                        leftComponents.Remove(equipment);
                    }
                    if (rightComponents.Contains(equipment))
                    {
                        rightComponents.Remove(equipment);
                    }
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
        }
        public void AssignEquipment(ElectricalEquipment equipment)
        {
          
            if (leftComponents.Count <= rightComponents.Count)
            {
                leftComponents.Add(equipment);
            }
            else
            {
                rightComponents.Add(equipment);
            }
            SetCircuitNumbers();
            SetCircuitVa();
            equipment.PropertyChanged += Equipment_PropertyChanged;
        }
        public void AssignPanel(ElectricalPanel panel)
        {

            if (leftComponents.Count <= rightComponents.Count)
            {
                leftComponents.Add(panel);
            }
            else
            {
                rightComponents.Add(panel);
            }
            SetCircuitNumbers();
            SetCircuitVa();
            //equipment.PropertyChanged += Equipment_PropertyChanged;
        }
        public void SetCircuitNumbers()
        {
            int leftCircuitIndex = 0;
            int rightCircuitIndex = 0;

            foreach (var equipment in leftComponents)
            {
                 equipment.CircuitNo = leftCircuitIndex * 2 + 1;
                 leftCircuitIndex += equipment.Pole;
            }

            foreach (var equipment in rightComponents)
            {
               
                equipment.CircuitNo = rightCircuitIndex * 2 + 2;
                rightCircuitIndex += equipment.Pole;                    
            }
        }
        public void SetCircuitVa()
        {
            foreach (var circuit in leftCircuits)
            {
                circuit.Va = 0;
            }
            foreach (var circuit in rightCircuits)
            {
                circuit.Va = 0;
            }
            PhaseAVA = 0;
            PhaseBVA = 0;
            PhaseCVA = 0;
            Kva = 0;
            int phaseIndex = 0;
            foreach (var equipment in leftComponents)
            {
                int circuitIndex = leftCircuits.IndexOf(leftCircuits.FirstOrDefault(c => c.Number == equipment.CircuitNo));
                if (circuitIndex != -1 && circuitIndex + equipment.Pole <= leftCircuits.Count)
                {
                    for (int i = 0; i < equipment.Pole; i++)
                    {
                        
                        leftCircuits[circuitIndex + i].Va = (int)equipment.Va;
                        switch (phaseIndex % Pole)
                        {
                            case 0:
                                PhaseAVA += (int)equipment.Va;
                                break;
                            case 1:
                                PhaseBVA += (int)equipment.Va;
                                break;
                            case 2:
                                PhaseCVA += (int)equipment.Va;
                                break;
                        }
                        Kva += (float)equipment.Va;
                        phaseIndex++;
                    }
                }
            }
            phaseIndex = 0;
            foreach (var equipment in rightComponents)
            {
                int circuitIndex = rightCircuits.IndexOf(rightCircuits.FirstOrDefault(c => c.Number == equipment.CircuitNo));
                if (circuitIndex != -1  && circuitIndex + equipment.Pole <= rightCircuits.Count)
                {
                    for (int i = 0; i < equipment.Pole; i++)
                    {
                            rightCircuits[circuitIndex + i].Va = (int)equipment.Va;
                            switch (phaseIndex % Pole)
                            {
                                case 0:
                                    PhaseAVA += (int)equipment.Va;
                                    break;
                                case 1:
                                    PhaseBVA += (int)equipment.Va;
                                    break;
                                case 2:
                                    PhaseCVA += (int)equipment.Va;
                                    break;
                            }
                        Kva += (float)equipment.Va;
                        phaseIndex++;
                    }
                }
            }
            Va = Kva;
            Kva = (float)Math.Round(Kva / 1000, 10);
            Amp = SetAmp();
        }
        public float SetAmp()
        {
            int largestPhase = Math.Max(PhaseAVA, Math.Max(PhaseBVA, PhaseCVA));
            switch (Pole)
            {
                case 2:
                    Amp = (float)Math.Round((double)largestPhase / 120, 10);
                    break;
                default:
                    Amp = (float)Math.Round((double)((largestPhase * 1.732) / 208), 10);
                    break;

            }
            return Amp;
        }
        public void DownloadEquipment(ObservableCollection<ElectricalEquipment> equipment)
        {
            ObservableCollection<ElectricalEquipment> temp = new ObservableCollection<ElectricalEquipment>();
            foreach (var equip in equipment)
            {
                if (equip.ParentId == Id)
                {
                    temp.Add(equip);
                }
            }
            temp = new ObservableCollection<ElectricalEquipment>(temp.OrderBy(e => e.CircuitNo));
            foreach (var equip in temp)
            {
                if (equip.ParentId == Id)
                {
                    equip.PropertyChanged += Equipment_PropertyChanged;
                    if (equip.CircuitNo % 2 != 0)
                    {
                        leftComponents.Add(equip);

                    }
                    else
                    {
                        rightComponents.Add(equip);
                    }
                }
            }
            SetCircuitVa();
            
        }



        public bool Verify()
        {
            if (!Utils.IsUuid(Id))
            {
                return false;
            }
            if (!Utils.IsEquipName(Name))
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
    public class Circuit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int number;
        public int va;
        public int breakerSize;
        public int index;

        public Circuit(int _number, int _va, int _breakerSize)
        {
            number = _number;
            va = _va;
            breakerSize=_breakerSize;
        }

        public int BreakerSize
        {
            get => breakerSize;
            set
            {
                breakerSize = value;
                OnPropertyChanged();
            }
        }
        public int Va
        {
            get => va;
            set
            {
                va = value;
                OnPropertyChanged();
            }
        }
        public int Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
