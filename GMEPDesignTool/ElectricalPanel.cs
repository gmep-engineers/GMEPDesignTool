﻿using System;
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
        private bool _isHiddenOnPlan;

        //private int _phaseAVa;
        // private int _phaseBVa;
        //private int _phaseCVa;

        public ObservableCollection<ElectricalComponent> leftComponents { get; set; } =
            new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<ElectricalComponent> rightComponents { get; set; } =
            new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<Circuit> leftCircuits { get; set; } =
            new ObservableCollection<Circuit>();

        public ObservableCollection<Circuit> rightCircuits { get; set; } =
            new ObservableCollection<Circuit>();

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
            int circuitNo,
            bool isHiddenOnPlan
        )
            : base()
        {
            this.id = id;
            _busSize = busSize;
            _mainSize = mainSize;
            _isMlo = isMlo;
            _isHiddenOnPlan = isHiddenOnPlan;
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
            phaseAVa = 0;
            phaseBVa = 0;
            phaseCVa = 0;
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
        public override float PhaseAVA
        {
            get => phaseAVa;
            set
            {
                phaseAVa = value;
                OnPropertyChanged(nameof(PhaseAVA));
            }
        }
        public override float PhaseBVA
        {
            get => phaseBVa;
            set
            {
                phaseBVa = value;
                OnPropertyChanged(nameof(PhaseBVA));
            }
        }
        public override float PhaseCVA
        {
            get => phaseCVa;
            set
            {
                phaseCVa = value;
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
        public bool IsHiddenOnPlan
        {
            get => _isHiddenOnPlan;
            set
            {
                _isHiddenOnPlan = value;
                OnPropertyChanged(nameof(IsHiddenOnPlan));
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
            switch (Type)
            {
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
            if (
                e.PropertyName == nameof(ElectricalEquipment.Va)
                || e.PropertyName == nameof(ElectricalEquipment.Pole)
            )
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalEquipment.ParentId))
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

        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(ElectricalPanel.PhaseAVA)
                || e.PropertyName == nameof(ElectricalPanel.PhaseBVA)
                || e.PropertyName == nameof(ElectricalPanel.PhaseCVA)
                || e.PropertyName == nameof(ElectricalPanel.Pole)
            )
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalPanel.ParentId))
            {
                if (sender is ElectricalPanel panel)
                {
                    panel.PropertyChanged -= Panel_PropertyChanged;
                    if (leftComponents.Contains(panel))
                    {
                        leftComponents.Remove(panel);
                    }
                    if (rightComponents.Contains(panel))
                    {
                        rightComponents.Remove(panel);
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
            panel.PropertyChanged += Panel_PropertyChanged;
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
            foreach (var component in leftComponents)
            {
                int circuitIndex = leftCircuits.IndexOf(
                    leftCircuits.FirstOrDefault(c => c.Number == component.CircuitNo)
                );
                if (circuitIndex != -1 && circuitIndex + component.Pole <= leftCircuits.Count)
                {
                    for (int i = 0; i < component.Pole; i++)
                    {
                        switch (phaseIndex % Pole)
                        {
                            case 0:
                                leftCircuits[circuitIndex + i].Va = (int)component.PhaseAVA;
                                PhaseAVA += (int)component.PhaseAVA;
                                Kva += (float)component.PhaseAVA;
                                break;
                            case 1:
                                leftCircuits[circuitIndex + i].Va = (int)component.PhaseBVA;
                                PhaseBVA += (int)component.PhaseBVA;
                                Kva += (float)component.PhaseBVA;
                                break;
                            case 2:
                                leftCircuits[circuitIndex + i].Va = (int)component.PhaseCVA;
                                PhaseCVA += (int)component.PhaseCVA;
                                Kva += (float)component.PhaseCVA;
                                break;
                        }
                        phaseIndex++;
                    }
                }
            }
            phaseIndex = 0;
            foreach (var component in rightComponents)
            {
                int circuitIndex = rightCircuits.IndexOf(
                    rightCircuits.FirstOrDefault(c => c.Number == component.CircuitNo)
                );
                if (circuitIndex != -1 && circuitIndex + component.Pole <= rightCircuits.Count)
                {
                    for (int i = 0; i < component.Pole; i++)
                    {
                        switch (phaseIndex % Pole)
                        {
                            case 0:
                                rightCircuits[circuitIndex + i].Va = (int)component.PhaseAVA;
                                PhaseAVA += (int)component.PhaseAVA;
                                Kva += (float)component.PhaseAVA;
                                break;
                            case 1:
                                rightCircuits[circuitIndex + i].Va = (int)component.PhaseBVA;
                                PhaseBVA += (int)component.PhaseBVA;
                                Kva += (float)component.PhaseBVA;
                                break;
                            case 2:
                                rightCircuits[circuitIndex + i].Va = (int)component.PhaseCVA;
                                PhaseCVA += (int)component.PhaseCVA;
                                Kva += (float)component.PhaseCVA;
                                break;
                        }
                        phaseIndex++;
                    }
                }
            }
            Kva = (float)Math.Ceiling(Kva / 1000);
            Amp = (float)Math.Ceiling(SetAmp());
        }

        public float SetAmp()
        {
            int largestPhase = (int)Math.Max(PhaseAVA, Math.Max(PhaseBVA, PhaseCVA));
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

        public void DownloadComponents(
            ObservableCollection<ElectricalEquipment> equipment,
            ObservableCollection<ElectricalPanel> panels
        )
        {
            ObservableCollection<ElectricalComponent> temp =
                new ObservableCollection<ElectricalComponent>();
            foreach (var equip in equipment)
            {
                if (equip.ParentId == Id)
                {
                    temp.Add(equip);
                    equip.PropertyChanged += Equipment_PropertyChanged;
                }
            }
            foreach (var panel in panels)
            {
                if (panel.ParentId == Id)
                {
                    temp.Add(panel);
                    panel.PropertyChanged += Panel_PropertyChanged;
                }
            }
            temp = new ObservableCollection<ElectricalComponent>(temp.OrderBy(e => e.CircuitNo));
            foreach (var component in temp)
            {
                if (component.ParentId == Id)
                {
                    if (component.CircuitNo % 2 != 0)
                    {
                        leftComponents.Add(component);
                    }
                    else
                    {
                        rightComponents.Add(component);
                    }
                }
            }
            SetCircuitVa();
        }

        private void Equip_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
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
            breakerSize = _breakerSize;
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
