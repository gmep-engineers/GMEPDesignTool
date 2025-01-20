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
    public class ElectricalPanel : INotifyPropertyChanged
    {
        private string _id;
        private int _busSize;
        private int _mainSize;
        private bool _isMlo;
        private bool _isDistribution;
        private string _name;
        private string _colorCode;
        private string _fedFromId;
        private string _projectId;
        private int _numBreakers;
        private int _distanceFromParent;
        private int _aicRating;
        private int _kva;
        private int _amp;
        private int _type;
        private bool _powered;
        private int _phaseAVa;
        private int _phaseBVa;
        private int _phaseCVa;
        public ObservableCollection<ElectricalEquipment> leftEquipments { get; set; }
        public ObservableCollection<ElectricalEquipment> rightEquipments { get; set; }
        public ObservableCollection<Circuit> leftCircuits { get; set; }

        public ObservableCollection<Circuit> rightCircuits { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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
            string fedFromId,
            int numBreakers,
            int distanceFromParent,
            int aicRating,
            int kva,
            int amp,
            int type,
            bool powered,
            bool isRecessed
        )
        {
            _id = id;
            _busSize = busSize;
            _mainSize = mainSize;
            _isMlo = isMlo;
            _isDistribution = isDistribution;
            _name = name;
            _colorCode = colorCode;
            _fedFromId = fedFromId;
            _projectId = projectId;
            _numBreakers = numBreakers;
            _distanceFromParent = distanceFromParent;
            _aicRating = aicRating;
            _kva = kva;
            _amp = amp;
            _type = type;
            _powered = powered;
            _isRecessed = isRecessed;
            _phaseAVa = 14;
            _phaseBVa = 14;
            _phaseCVa = 14;
            leftCircuits = new ObservableCollection<Circuit>();
            rightCircuits = new ObservableCollection<Circuit>();
            leftEquipments = new ObservableCollection<ElectricalEquipment>();
            rightEquipments = new ObservableCollection<ElectricalEquipment>();
            PopulateCircuits();
        }

        public bool IsRecessed
        {
            get => _isRecessed;
            set
            {
                _isRecessed = value;
                OnPropertyChanged();
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
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public int BusSize
        {
            get => _busSize;
            set
            {
                _busSize = value;
                OnPropertyChanged();
            }
        }

        public int MainSize
        {
            get => _mainSize;
            set
            {
                _mainSize = value;
                OnPropertyChanged();
            }
        }

        public bool IsMlo
        {
            get => _isMlo;
            set
            {
                _isMlo = value;
                OnPropertyChanged();
            }
        }

        public bool IsDistribution
        {
            get => _isDistribution;
            set
            {
                _isDistribution = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string ColorCode
        {
            get => _colorCode;
            set
            {
                _colorCode = value;
                OnPropertyChanged();
            }
        }

        public string FedFromId
        {
            get => _fedFromId;
            set
            {
                _fedFromId = value;
                OnPropertyChanged();
            }
        }
        public string ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public int AicRating
        {
            get => _aicRating;
            set
            {
                _aicRating = value;
                OnPropertyChanged();
            }
        }

        public int Kva
        {
            get => _kva;
            set
            {
                _kva = value;
                OnPropertyChanged();
            }
        }

        public int Amp
        {
            get => _amp;
            set
            {
                _amp = value;
                OnPropertyChanged();
            }
        }

        public int Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        public bool Powered
        {
            get => _powered;
            set
            {
                _powered = value;
                OnPropertyChanged();
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
                if (rightCircuits.Count > leftCircuits.Count)
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
            if (e.PropertyName == nameof(ElectricalEquipment.Va) || e.PropertyName == nameof(ElectricalEquipment.Pole))
            {
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalEquipment.ParentId))
            {
                if (sender is ElectricalEquipment equipment)
                {
                    equipment.PropertyChanged -= Equipment_PropertyChanged;
                    if (leftEquipments.Contains(equipment))
                    {
                        leftEquipments.Remove(equipment);
                    }
                    if (rightEquipments.Contains(equipment))
                    {
                        rightEquipments.Remove(equipment);
                    }
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
        }
        public void AssignEquipment(ElectricalEquipment equipment)
        {

            if (leftEquipments.Count <= rightEquipments.Count)
            {
                leftEquipments.Add(equipment);
            }
            else
            {
                rightEquipments.Add(equipment);
            }
            SetCircuitNumbers();
            SetCircuitVa();
            equipment.PropertyChanged += Equipment_PropertyChanged;
        }
        public void SetCircuitNumbers()
        {
            int leftCircuitIndex = 0;
            int rightCircuitIndex = 0;

            foreach (var equipment in leftEquipments)
            {
                 equipment.CircuitNo = leftCircuits[leftCircuitIndex].Number;
                 leftCircuitIndex += equipment.Pole;
            }

            foreach (var equipment in rightEquipments)
            {
                  equipment.CircuitNo = rightCircuits[rightCircuitIndex].Number;
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
            int phaseNo = 0;
            int phaseNo2 = 0;
            foreach (var equipment in leftEquipments)
            {
                int circuitIndex = leftCircuits.IndexOf(leftCircuits.FirstOrDefault(c => c.Number == equipment.CircuitNo));
                if (circuitIndex != -1 && circuitIndex + equipment.Pole <= leftCircuits.Count)
                {
                    for (int i = 0; i < equipment.Pole; i++)
                    {
                        
                         leftCircuits[circuitIndex + i].Va = (int)equipment.Va;
                        if (phaseNo == 0)
                        {
                            PhaseAVA += (int)equipment.Va;
                            phaseNo++;
                        }
                        if (phaseNo == 1)
                        {
                            PhaseBVA += (int)equipment.Va;
                            phaseNo++;
                        }
                        if (phaseNo == 2)
                        {
                            PhaseCVA += (int)equipment.Va;
                            phaseNo = 0;
                        }
                    }
                }
            }

            foreach (var equipment in rightEquipments)
            {
                int circuitIndex = rightCircuits.IndexOf(rightCircuits.FirstOrDefault(c => c.Number == equipment.CircuitNo));
                if (circuitIndex != -1  && circuitIndex + equipment.Pole <= rightCircuits.Count)
                {
                    for (int i = 0; i < equipment.Pole; i++)
                    {
                            rightCircuits[circuitIndex + i].Va = (int)equipment.Va;
                        if (phaseNo2 == 0)
                        {
                            PhaseAVA += (int)equipment.Va;
                            phaseNo2++;
                        }
                        if (phaseNo == 1)
                        {
                            PhaseBVA += (int)equipment.Va;
                            phaseNo2++;
                        }
                        if (phaseNo2 == 2)
                        {
                            PhaseCVA += (int)equipment.Va;
                            phaseNo2 = 0;
                        }
                    }
                }
            }
        }
        public void DownloadEquipment(ObservableCollection<ElectricalEquipment> equipment)
        {
            ObservableCollection<ElectricalEquipment> temp = new ObservableCollection<ElectricalEquipment>(equipment);
            foreach (var equip in equipment)
            {
                if (equip.ParentId == Id)
                {
                    temp.Add(equip);
                }
            }
            temp = new ObservableCollection<ElectricalEquipment>(temp.OrderBy(e => e.CircuitNo));
            foreach (var equip in equipment)
            {
                if (equip.ParentId == Id)
                {
                    if (equip.CircuitNo % 2 != 0)
                    {
                        leftEquipments.Add(equip);
                    }
                    else
                    {
                        rightEquipments.Add(equip);
                    }
                }
            }
            SetCircuitVa();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            if (!Utils.IsUuid(FedFromId))
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
        public int amp;
        public int index;

        public Circuit(int _number, int _va, int _amp)
        {
            number = _number;
            va = _va;
            amp=_amp;
        }

        public int Amp
        {
            get => amp;
            set
            {
                amp = value;
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
