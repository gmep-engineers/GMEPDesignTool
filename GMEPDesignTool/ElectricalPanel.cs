using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Windows.Controls;
using Org.BouncyCastle.Tls.Crypto;

namespace GMEPDesignTool
{
    public class ElectricalPanel : ElectricalComponent
    {
        private int _busSize = 1;
        private int _mainSize = 1;
        private bool _isMlo = true;
        private bool _isDistribution = false;
        private int _numBreakers = 42;
        private int _distanceFromParent = 0;
        private int _aicRating = 0;
        private float _kva = 0;
        private float _va = 0;
        private int _type = 1;
        private bool _powered = false;
        private bool _isHiddenOnPlan = false;
        private string _location = string.Empty;
        private string _parentName = string.Empty;
        private string _parentType = string.Empty;
        private char _highLegPhase = '-';

        public ElectricalComponent ParentComponent { get; set; }
        public ObservableCollection<ElectricalComponent> componentsCollection { get; set; } =
            new ObservableCollection<ElectricalComponent>();

        public ObservableCollection<ElectricalComponent> leftComponents { get; set; } =
            new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<ElectricalComponent> rightComponents { get; set; } =
            new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<Circuit> leftCircuits { get; set; } =
            new ObservableCollection<Circuit>();
        public ObservableCollection<Circuit> rightCircuits { get; set; } =
            new ObservableCollection<Circuit>();
        public ObservableCollection<ElectricalPanelNote> notes { get; set; } =
            new ObservableCollection<ElectricalPanelNote>();
        public ObservableCollection<ElectricalPanelNoteRel> leftNotes { get; set; } =
            new ObservableCollection<ElectricalPanelNoteRel>();
        public ObservableCollection<ElectricalPanelNoteRel> rightNotes { get; set; } =
            new ObservableCollection<ElectricalPanelNoteRel>();

        private bool _isRecessed = false;
        private string circuits = string.Empty;
         

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
            float kva,
            int type,
            bool powered,
            bool isRecessed,
            int circuitNo,
            bool isHiddenOnPlan,
            string location,
            char highLegPhase,
            int orderNo
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
            this.amp = amp;
            this._kva = kva;
            _type = type;
            _powered = powered;
            _isRecessed = isRecessed;
            phaseAVa = 0;
            phaseBVa = 0;
            phaseCVa = 0;
            aLcl = 0;
            bLcl = 0;
            cLcl = 0;
            aLml = 0;
            bLml = 0;
            cLml = 0;
            loadCategory = 3;
            lcl = 0;
            lml = 0;
            _va = 0;
            _highLegPhase = highLegPhase;
            this._location = location;
            this.componentType = "Panel";
            this.orderNo = orderNo;
            SetPole();
            PopulateCircuits(Id, ProjectId);
            updateFlag = false;
            notes.CollectionChanged += ElectricalPanelNotes_CollectionChanged;
            leftNotes.CollectionChanged += ElectricalPanelNoteRels_CollectionChanged;
            rightNotes.CollectionChanged += ElectricalPanelNoteRels_CollectionChanged;
            DetermineCircuits();
        }
        public ElectricalPanel()
        {
            loadCategory = 3;
            this.componentType = "Panel";
            SetPole();
            PopulateCircuits(Id, ProjectId);
            updateFlag = false;
            notes.CollectionChanged += ElectricalPanelNotes_CollectionChanged;
            leftNotes.CollectionChanged += ElectricalPanelNoteRels_CollectionChanged;
            rightNotes.CollectionChanged += ElectricalPanelNoteRels_CollectionChanged;
            DetermineCircuits();
        }
        public string ParentName
        {
            get => _parentName;
            set
            {
                if (_parentName != value)
                {
                    this._parentName = value;
                    OnPropertyChanged(nameof(ParentName));
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
        public string ParentType
        {
            get => _parentType;
            set
            {
                if (_parentType != value)
                {
                    this._parentType = value;
                    OnPropertyChanged(nameof(ParentType));
                    DetermineCircuits();
                }
            }
        }
        public override string ParentId
        {
            get => this.parentId;
            set
            {
                if (this.parentId != value)
                {
                    this.parentId = value ?? string.Empty;
                    OnPropertyChanged(nameof(ParentId));
                    if (ParentComponent != null && string.IsNullOrEmpty(value))
                    {
                        ParentComponent.PropertyChanged -= ParentComponent_PropertyChanged;
                        ParentName = "";
                        ParentType = "";
                        ParentComponent = null;
                    }
                    DetermineCircuits();
                }
            }
        }
        public override int Pole
        {
            get => this.pole;
            set
            {
                if (this.pole != value)
                {
                    this.pole = value;
                    DetermineCircuits();
                    OnPropertyChanged(nameof(Pole));
                    SetCircuitVa();
                }
            }
        }
        public override string Circuits
        {
            get => circuits;
            set
            {
                if (circuits != value)
                {
                    circuits = value;
                    OnPropertyChanged(nameof(Circuits));
                }
            }
        }
        public char HighLegPhase
        {
            get => _highLegPhase;
            set
            {
                if (_highLegPhase != value)
                {
                    _highLegPhase = value;
                    OnPropertyChanged(nameof(HighLegPhase));
                    SetCircuitVa();
                }
            }
        }

        public bool IsRecessed
        {
            get => _isRecessed;
            set
            {
                if (_isRecessed != value)
                {
                    _isRecessed = value;
                    OnPropertyChanged(nameof(IsRecessed));
                }
            }
        }
        public override float PhaseAVA
        {
            get => phaseAVa;
            set
            {
                if (this.phaseAVa != value)
                {
                    phaseAVa = value;
                    OnPropertyChanged(nameof(PhaseAVA));
                }
            }
        }
        public override float PhaseBVA
        {
            get => phaseBVa;
            set
            {
                if (this.phaseBVa != value)
                {
                    phaseBVa = value;
                    OnPropertyChanged(nameof(PhaseBVA));
                }
            }
        }
        public override float PhaseCVA
        {
            get => phaseCVa;
            set
            {
                if (this.phaseCVa != value)
                {
                    phaseCVa = value;
                    OnPropertyChanged(nameof(PhaseCVA));
                }
            }
        }

        public float Va
        {
            get => _va;
            set
            {
                if (_va != value)
                {
                    _va = value;
                    OnPropertyChanged(nameof(Va));
                }
            }
        }

        public int BusSize
        {
            get => _busSize;
            set
            {
                if (_busSize != value)
                {
                    _busSize = value;
                    OnPropertyChanged(nameof(BusSize));
                }
            }
        }

        public int MainSize
        {
            get => _mainSize;
            set
            {
                if (_mainSize != value)
                {
                    _mainSize = value;
                    OnPropertyChanged(nameof(MainSize));
                }
            }
        }

        public bool IsMlo
        {
            get => _isMlo;
            set
            {
                if (_isMlo != value)
                {
                    _isMlo = value;
                    OnPropertyChanged(nameof(IsMlo));
                }
            }
        }
        public bool IsHiddenOnPlan
        {
            get => _isHiddenOnPlan;
            set
            {
                if (_isHiddenOnPlan != value)
                {
                    _isHiddenOnPlan = value;
                    OnPropertyChanged(nameof(IsHiddenOnPlan));
                }
            }
        }
        public bool IsDistribution
        {
            get => _isDistribution;
            set
            {
                if (_isDistribution != value)
                {
                    _isDistribution = value;
                    OnPropertyChanged(nameof(IsDistribution));
                }
            }
        }

        public int NumBreakers
        {
            get => _numBreakers;
            set
            {
                if (_numBreakers != value)
                {
                    _numBreakers = value;
                    OnPropertyChanged(nameof(NumBreakers));
                    PopulateCircuits(Id, ProjectId);
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
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

        public int AicRating
        {
            get => _aicRating;
            set
            {
                if (_aicRating != value)
                {
                    _aicRating = value;
                    OnPropertyChanged(nameof(AicRating));
                }
            }
        }

        public float Kva
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

        public int Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                    SetPole();
                    SetCircuitVa();
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
        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged(nameof(Location));
                }
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

        public void PopulateCircuits(string id, string projectId)
        {
            int totalCircuits = leftCircuits.Count + rightCircuits.Count;

            while (totalCircuits < NumBreakers)
            {
                if (totalCircuits % 2 == 0)
                {
                    Circuit newCircuit = new Circuit(
                        Guid.NewGuid().ToString(),
                        id,
                        projectId,
                        string.Empty,
                        leftCircuits.Count * 2 + 1,
                        0,
                        0,
                        "",
                        0,
                        false,
                        false
                    );
                    leftCircuits.Add(newCircuit);
                    newCircuit.PropertyChanged += Circuit_PropertyChanged;
                }
                else
                {
                    Circuit newCircuit = new Circuit(
                        Guid.NewGuid().ToString(),
                        id,
                        projectId,
                        string.Empty,
                        rightCircuits.Count * 2 + 2,
                        0,
                        0,
                        "",
                        0,
                        false,
                        false
                    );
                    rightCircuits.Add(newCircuit);
                    newCircuit.PropertyChanged += Circuit_PropertyChanged;
                }
                totalCircuits++;
            }

            while (totalCircuits > NumBreakers)
            {
                if (rightCircuits.Count >= leftCircuits.Count)
                {
                    rightCircuits.ElementAt(rightCircuits.Count - 1).PropertyChanged -=
                        Circuit_PropertyChanged;
                    rightCircuits.RemoveAt(rightCircuits.Count - 1);
                }
                else
                {
                    leftCircuits.ElementAt(leftCircuits.Count - 1).PropertyChanged -=
                        Circuit_PropertyChanged;
                    leftCircuits.RemoveAt(leftCircuits.Count - 1);
                }
                totalCircuits--;
            }
        }

        private void Equipment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(ElectricalEquipment.Va)
                || e.PropertyName == nameof(ElectricalEquipment.Amp)
                || e.PropertyName == nameof(ElectricalEquipment.Pole)
                || e.PropertyName == nameof(ElectricalEquipment.Name)
                || e.PropertyName == nameof(ElectricalEquipment.LoadType)
                || e.PropertyName == nameof(ElectricalEquipment.LoadCategory)
            )
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalEquipment.ParentId))
            {
                if (sender is ElectricalEquipment equipment)
                {
                    equipment.CircuitNo = 0;
                    equipment.PropertyChanged -= Equipment_PropertyChanged;
                    if (leftComponents.Contains(equipment))
                    {
                        leftComponents.Remove(equipment);
                    }
                    if (rightComponents.Contains(equipment))
                    {
                        rightComponents.Remove(equipment);
                    }
                    if (componentsCollection.Contains(equipment))
                    {
                        componentsCollection.Remove(equipment);
                    }
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
        }

        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(ElectricalPanel.Pole)
                || e.PropertyName == nameof(ElectricalPanel.Name)
                || e.PropertyName == nameof(ElectricalPanel.UpdateFlag)
            )
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalPanel.ParentId))
            {
                if (sender is ElectricalPanel panel)
                {
                    panel.CircuitNo = 0;
                    panel.PropertyChanged -= Panel_PropertyChanged;
                    if (leftComponents.Contains(panel))
                    {
                        leftComponents.Remove(panel);
                    }
                    if (rightComponents.Contains(panel))
                    {
                        rightComponents.Remove(panel);
                    }
                    if (componentsCollection.Contains(panel))
                    {
                        componentsCollection.Remove(panel);
                    }
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
        }

        private void Transformer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(ElectricalTransformer.Pole)
                || e.PropertyName == nameof(ElectricalEquipment.Name)
                || e.PropertyName == nameof(ElectricalTransformer.UpdateFlag)
            )
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalTransformer.ParentId))
            {
                if (sender is ElectricalTransformer transformer)
                {
                    transformer.CircuitNo = 0;
                    transformer.PropertyChanged -= Transformer_PropertyChanged;
                    if (leftComponents.Contains(transformer))
                    {
                        leftComponents.Remove(transformer);
                    }
                    if (rightComponents.Contains(transformer))
                    {
                        rightComponents.Remove(transformer);
                    }
                    if (componentsCollection.Contains(transformer))
                    {
                        componentsCollection.Remove(transformer);
                    }
                    SetCircuitNumbers();
                    SetCircuitVa();
                }
            }
        }

        private void ParentComponent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(ElectricalEquipment.Name)
                && sender is ElectricalComponent component
            )
            {
                ParentName = component.Name;
            }
        }

        private void ElectricalPanelNotes_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e
        )
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ElectricalPanelNote newNote in e.NewItems)
                {
                    newNote.ProjectId = ProjectId;
                    newNote.Tag = (notes.Count()).ToString();
                    newNote.Id = !String.IsNullOrEmpty(newNote.Id)
                        ? newNote.Id
                        : Guid.NewGuid().ToString();
                    newNote.DateCreated = !String.IsNullOrEmpty(newNote.DateCreated)
                        ? newNote.DateCreated
                        : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
            }
        }

        private void ElectricalPanelNoteRels_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e
        )
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ElectricalPanelNoteRel newNote in e.NewItems)
                {
                    newNote.ProjectId = ProjectId;
                    newNote.Id = !String.IsNullOrEmpty(newNote.Id)
                        ? newNote.Id
                        : Guid.NewGuid().ToString();
                    newNote.NoteId = !String.IsNullOrEmpty(newNote.NoteId)
                        ? newNote.NoteId
                        : string.Empty;
                    newNote.PanelId = id;
                }
            }
        }

        public void Circuit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Circuit.CustomBreakerSize) && sender is Circuit circuit)
            {
                if (!circuit.CustomBreakerSize)
                {
                    SetCircuitVa();
                }
            }
            if (e.PropertyName == nameof(Circuit.CustomDescription) && sender is Circuit circuit2)
            {
                if (!circuit2.CustomDescription)
                {
                    SetCircuitVa();
                }
            }
            if (e.PropertyName == nameof(Circuit.BreakerSize) && sender is Circuit circuit3)
            {
                if (circuit3.CustomBreakerSize)
                {
                    checkCircuitErrors();
                }
            }
        }

        public void AssignParentComponent(ElectricalComponent component)
        {
            ParentComponent = component;
            ParentName = component.Name;
            component.PropertyChanged += ParentComponent_PropertyChanged;
            switch (component)
            {
                case ElectricalService service:
                    ParentType = "";
                    break;
                case ElectricalPanel panel:
                    ParentType = "PANEL ";
                    break;
                case ElectricalTransformer transformer:
                    ParentType = "TRANSFORMER ";
                    break;
                default:
                    ParentType = "";
                    break;
            }
        }

        public void AssignEquipment(ElectricalEquipment equipment)
        {
            componentsCollection.Add(equipment);
            equipment.PropertyChanged += Equipment_PropertyChanged;
        }

        public void AssignPanel(ElectricalPanel panel)
        {
            componentsCollection.Add(panel);
            panel.PropertyChanged += Panel_PropertyChanged;
        }

        public void AssignTransformer(ElectricalTransformer transformer)
        {
            transformer.ParentType = "PANEL ";
            componentsCollection.Add(transformer);
            transformer.PropertyChanged += Transformer_PropertyChanged;
        }

        public void AssignSpace(bool isLeft)
        {
            if (isLeft)
            {
                leftComponents.Add(new Space());
            }
            else
            {
                rightComponents.Add(new Space());
            }
        }

        public void SetCircuitNumbers()
        {
            int leftCircuitIndex = 0;
            int rightCircuitIndex = 0;
            var componentsToRemove = new List<ElectricalComponent>();

            foreach (var component in leftComponents)
            {
                component.CircuitNo = leftCircuitIndex * 2 + 1;
                leftCircuitIndex += component.Pole;
                if (component is Space && (component.CircuitNo / 2) + 1 > leftCircuits.Count)
                {
                    componentsToRemove.Add(component);
                }
            }
            foreach (var component in componentsToRemove)
            {
                leftComponents.Remove(component);
            }

            componentsToRemove.Clear();

            foreach (var component in rightComponents)
            {
                component.CircuitNo = rightCircuitIndex * 2 + 2;
                rightCircuitIndex += component.Pole;
                if (component is Space && (component.CircuitNo / 2) > rightCircuits.Count)
                {
                    componentsToRemove.Add(component);
                }
            }
            foreach (var component in componentsToRemove)
            {
                rightComponents.Remove(component);
            }
            while (leftCircuitIndex < leftCircuits.Count)
            {
                Space newSpace = new Space();
                newSpace.CircuitNo = leftCircuitIndex * 2 + 1;
                leftComponents.Add(newSpace);
                leftCircuitIndex++;
            }
            while (rightCircuitIndex < rightCircuits.Count)
            {
                Space newSpace = new Space();
                newSpace.CircuitNo = rightCircuitIndex * 2 + 2;
                rightComponents.Add(newSpace);
                rightCircuitIndex++;
            }
            foreach (var component in componentsCollection)
            {
                component.CircuitNo = 0;
            }
        }

        public void SetCircuitVa()
        {
            ErrorMessages.Remove("child-errors");
            foreach (var circuit in leftCircuits)
            {
                circuit.Va = 0;
                if (!circuit.CustomBreakerSize)
                {
                    circuit.BreakerSize = 0;
                }
                if (!circuit.CustomDescription)
                {
                    circuit.Description = "";
                }
            }
            foreach (var circuit in rightCircuits)
            {
                circuit.Va = 0;
                if (!circuit.CustomBreakerSize)
                {
                    circuit.BreakerSize = 0;
                }
                if (!circuit.CustomDescription)
                {
                    circuit.Description = "";
                }
            }
            PhaseAVA = 0;
            PhaseBVA = 0;
            PhaseCVA = 0;
            ALcl = 0;
            BLcl = 0;
            CLcl = 0;
            ALml = 0;
            BLml = 0;
            CLml = 0;
            Lml = 0;
            Lcl = 0;
            Kva = 0;
            RootKva = 0;
            int phaseIndex = 0;
            foreach (var component in leftComponents)
            {
                DetermineComponentErrors(component);
                int circuitIndex = leftCircuits.IndexOf(
                    leftCircuits.FirstOrDefault(c => c.Number == component.CircuitNo)
                );
                if (circuitIndex != -1 && circuitIndex + component.Pole <= leftCircuits.Count)
                {
                    bool lmlIsLarger = false;
                    if (component.Lml > Lml)
                    {
                        lmlIsLarger = true;
                    }

                    for (int i = 0; i < component.Pole; i++)
                    {
                        var addedValue = 0;
                        float lclValue = 0;
                        float lmlValue = 0;
                        switch (i)
                        {
                            case 0:
                                addedValue = (int)component.PhaseAVA;
                                lclValue = (int)component.ALcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.ALml;
                                }
                                break;
                            case 1:
                                addedValue = (int)component.PhaseBVA;
                                lclValue = (int)component.BLcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.BLml;
                                }
                                break;
                            case 2:
                                addedValue = (int)component.PhaseCVA;
                                lclValue = (int)component.CLcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.CLml;
                                }
                                break;
                        }
                        leftCircuits[circuitIndex + i].Va = addedValue;
                        if (!leftCircuits[circuitIndex + i].CustomDescription)
                        {
                            if (i == 0)
                            {
                                leftCircuits[circuitIndex + i].Description = component.Name;
                            }
                            else
                            {
                                leftCircuits[circuitIndex + i].Description = "---";
                            }
                        }
                        if (!leftCircuits[circuitIndex + i].CustomBreakerSize)
                        {
                            if (i == component.Pole - 1)
                            {
                                leftCircuits[circuitIndex + i].BreakerSize = i + 1;
                            }
                            if (i == 0)
                            {
                                leftCircuits[circuitIndex + i].BreakerSize = DetermineBreakerSize(
                                    component
                                );
                            }
                        }

                        leftCircuits[circuitIndex + i].LoadCategory = component.LoadCategory;
                        leftCircuits[circuitIndex + i].EquipId = component.Id;

                        switch (phaseIndex % Pole)
                        {
                            case 0:
                                PhaseAVA += addedValue;
                                Kva += (float)addedValue;
                                ALcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    ALml = lmlValue;
                                }
                                break;
                            case 1:
                                PhaseBVA += addedValue;
                                Kva += (float)addedValue;
                                BLcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    BLml = lmlValue;
                                }
                                break;
                            case 2:
                                PhaseCVA += addedValue;
                                Kva += (float)addedValue;
                                CLcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    CLml = lmlValue;
                                }
                                break;
                        }
                        phaseIndex++;
                    }
                    Lcl += component.Lcl;
                    if (lmlIsLarger)
                    {
                        Lml = component.Lml;
                    }
                }
            }
            phaseIndex = 0;
            foreach (var component in rightComponents)
            {
                DetermineComponentErrors(component);
                int circuitIndex = rightCircuits.IndexOf(
                    rightCircuits.FirstOrDefault(c => c.Number == component.CircuitNo)
                );
                if (circuitIndex != -1 && circuitIndex + component.Pole <= rightCircuits.Count)
                {
                    bool lmlIsLarger = false;
                    if (component.Lml > Lml)
                    {
                        lmlIsLarger = true;
                    }
                    for (int i = 0; i < component.Pole; i++)
                    {
                        var addedValue = 0;
                        var lclValue = 0;
                        float lmlValue = 0;
                        switch (i)
                        {
                            case 0:
                                addedValue = (int)component.PhaseAVA;
                                lclValue = (int)component.ALcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.ALml;
                                }
                                break;
                            case 1:
                                addedValue = (int)component.PhaseBVA;
                                lclValue = (int)component.BLcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.BLml;
                                }
                                break;
                            case 2:
                                addedValue = (int)component.PhaseCVA;
                                lclValue = (int)component.CLcl;
                                if (lmlIsLarger)
                                {
                                    lmlValue = component.CLml;
                                }
                                break;
                        }
                        rightCircuits[circuitIndex + i].Va = addedValue;
                        if (!rightCircuits[circuitIndex + i].CustomDescription)
                        {
                            if (i == 0)
                            {
                                rightCircuits[circuitIndex + i].Description = component.Name;
                            }
                            else
                            {
                                rightCircuits[circuitIndex + i].Description = "---";
                            }
                        }
                        if (!rightCircuits[circuitIndex + i].CustomBreakerSize)
                        {
                            if (i == component.Pole - 1)
                            {
                                rightCircuits[circuitIndex + i].BreakerSize = i + 1;
                            }
                            if (i == 0)
                            {
                                rightCircuits[circuitIndex + i].BreakerSize = DetermineBreakerSize(
                                    component
                                );
                            }
                        }
                        rightCircuits[circuitIndex + i].LoadCategory = component.LoadCategory;
                        leftCircuits[circuitIndex + i].EquipId = component.Id;
                        switch (phaseIndex % Pole)
                        {
                            case 0:
                                PhaseAVA += addedValue;
                                Kva += (float)addedValue;
                                ALcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    ALml = lmlValue;
                                }
                                break;
                            case 1:
                                PhaseBVA += addedValue;
                                Kva += (float)addedValue;
                                BLcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    BLml = lmlValue;
                                }
                                break;
                            case 2:
                                PhaseCVA += addedValue;
                                Kva += (float)addedValue;
                                CLcl += lclValue;
                                if (lmlIsLarger)
                                {
                                    CLml = lmlValue;
                                }
                                break;
                        }
                        phaseIndex++;
                    }
                    Lcl += component.Lcl;
                    if (lmlIsLarger)
                    {
                        Lml = component.Lml;
                    }
                }
            }
            //CalculateLcl();
            // CalculateLml();
            Va = Kva;
            RootKva = (Kva + (Lml / 4) + (Lcl / 4)) / 1000;
            Kva = (float)Math.Ceiling(RootKva);
            Amp = (float)Math.Ceiling(SetAmp());
            checkCircuitErrors();
            UpdateFlag = !UpdateFlag;
        }

        public void DetermineComponentErrors(ElectricalComponent component)
        {
            if (component is ElectricalPanel panel)
            {
                DeterminePanelErrors(panel);
            }
            if (component is ElectricalTransformer transformer)
            {
                DetermineTransformerErrors(transformer);
            }
            if (component is ElectricalEquipment equipment)
            {
                DetermineEquipmentErrors(equipment);
            }
        }

        public void DeterminePanelErrors(ElectricalPanel panel)
        {
            panel.ErrorMessages.Remove("panel-pole-error");
            panel.ErrorMessages.Remove("panel-voltage-error");

            if (panel.Pole == 3 && Pole == 2)
            {
                panel.ErrorMessages.Add(
                    "panel-pole-error",
                    "3-pole panel is being fed from a 2-pole panel."
                );
            }
            if (panel.Type != Type)
            {
                panel.ErrorMessages.Add(
                    "panel-voltage-error",
                    "the panels voltage is not the same as its parent panel's voltage."
                );
            }
            if (panel.ErrorMessages.Count > 0 && !ErrorMessages.ContainsKey("child-errors"))
            {
                ErrorMessages.Add(
                    "child-errors",
                    "There are issues with the children being fed to this panel."
                );
            }
        }

        public void DetermineTransformerErrors(ElectricalTransformer transformer)
        {
            transformer.ErrorMessages.Remove("transformer-voltage-error");
            if (Type != findTransformerInputVoltage(transformer))
            {
                transformer.ErrorMessages.Add(
                    "transformer-voltage-error",
                    "The transformer input voltage does not match its parent panel's voltage"
                );
            }

            if (transformer.ErrorMessages.Count > 0 && !ErrorMessages.ContainsKey("child-errors"))
            {
                ErrorMessages.Add(
                    "child-errors",
                    "There are issues with the children being fed to this panel."
                );
            }
            int findTransformerInputVoltage(ElectricalTransformer transformer)
            {
                var transformerVoltageType = 5;
                switch (transformer.Voltage)
                {
                    case (1):
                        transformerVoltageType = 3;
                        break;
                    case (2):
                        transformerVoltageType = 1;
                        break;
                    case (3):
                        transformerVoltageType = 3;
                        break;
                    case (4):
                        transformerVoltageType = 4;
                        break;
                    case (5):
                        transformerVoltageType = 4;
                        break;
                    case (6):
                        transformerVoltageType = 1;
                        break;
                    case (7):
                        transformerVoltageType = 2;
                        break;
                    case (8):
                        transformerVoltageType = 5;
                        break;
                    default:
                        transformerVoltageType = 5;
                        break;
                }
                return transformerVoltageType;
            }
        }

        public void DetermineEquipmentErrors(ElectricalEquipment equipment)
        {
            List<int> compatibleVoltages = determineCompatibleVoltage(
                equipment.Is3Ph,
                equipment.Voltage
            );
            equipment.ErrorMessages.Remove("equipment-voltage-error");
            if (equipment.ErrorMessages.Count > 0 && !ErrorMessages.ContainsKey("child-errors"))
            {
                ErrorMessages.Add(
                    "child-errors",
                    "There are issues with the children being fed to this panel."
                );
            }

            foreach (var voltage in compatibleVoltages)
            {
                if (Type == voltage)
                {
                    return;
                }
            }
            equipment.ErrorMessages.Add(
                "equipment-voltage-error",
                "The equipment's voltage and phases are not compatible with its parent panel's settings."
            );

            if (equipment.ErrorMessages.Count > 0 && !ErrorMessages.ContainsKey("child-errors"))
            {
                ErrorMessages.Add(
                    "child-errors",
                    "There are issues with the children being fed to this panel."
                );
            }

            List<int> determineCompatibleVoltage(bool Is3Ph, int Voltage)
            {
                List<int> compatibleVoltages = new List<int>();
                if ((Is3Ph && Voltage == 3) || (!Is3Ph && (Voltage >= 1 && Voltage <= 3)))
                {
                    compatibleVoltages.Add(1);
                }
                if (!Is3Ph && (Voltage == 1 || Voltage == 2 || Voltage == 4 || Voltage == 5))
                {
                    compatibleVoltages.Add(2);
                }
                if (
                    (Is3Ph && (Voltage == 7 || Voltage == 8))
                    || (!Is3Ph && (Voltage == 6 || Voltage == 8 || Voltage == 7))
                )
                {
                    compatibleVoltages.Add(3);
                }
                if (
                    (Is3Ph && (Voltage == 4 || Voltage == 5))
                    || (!Is3Ph && (Voltage == 2 || Voltage == 4 || Voltage == 5))
                )
                {
                    compatibleVoltages.Add(4);
                }
                if (!Is3Ph && (Voltage == 1 || Voltage == 2 || Voltage == 3))
                {
                    compatibleVoltages.Add(5);
                }
                return compatibleVoltages;
            }
        }

        public int DetermineBreakerSize(ElectricalComponent component)
        {
            var breakerSize = component.Amp * 1.25;

            if (component is Space)
            {
                return 0;
            }

            switch (breakerSize)
            {
                case var _ when breakerSize <= 20:
                    return 20;
                case var _ when breakerSize <= 25 && breakerSize > 20:
                    return 25;
                case var _ when breakerSize <= 30 && breakerSize > 25:
                    return 30;
                case var _ when breakerSize <= 35 && breakerSize > 30:
                    return 35;
                case var _ when breakerSize <= 40 && breakerSize > 35:
                    return 40;
                case var _ when breakerSize <= 45 && breakerSize > 40:
                    return 45;
                case var _ when breakerSize <= 50 && breakerSize > 45:
                    return 50;
                case var _ when breakerSize <= 60 && breakerSize > 50:
                    return 60;
                case var _ when breakerSize <= 70 && breakerSize > 60:
                    return 70;
                case var _ when breakerSize <= 80 && breakerSize > 70:
                    return 80;
                case var _ when breakerSize <= 90 && breakerSize > 80:
                    return 90;
                case var _ when breakerSize <= 100 && breakerSize > 90:
                    return 100;
                case var _ when breakerSize <= 110 && breakerSize > 100:
                    return 110;
                case var _ when breakerSize <= 125 && breakerSize > 110:
                    return 125;
                case var _ when breakerSize <= 150 && breakerSize > 125:
                    return 150;
                case var _ when breakerSize <= 175 && breakerSize > 150:
                    return 175;
                case var _ when breakerSize <= 200 && breakerSize > 175:
                    return 200;
                case var _ when breakerSize <= 225 && breakerSize > 200:
                    return 225;
                case var _ when breakerSize <= 250 && breakerSize > 225:
                    return 250;
                case var _ when breakerSize <= 300 && breakerSize > 250:
                    return 300;
                case var _ when breakerSize <= 350 && breakerSize > 300:
                    return 350;
                case var _ when breakerSize <= 400 && breakerSize > 350:
                    return 400;
                case var _ when breakerSize <= 450 && breakerSize > 400:
                    return 450;
                case var _ when breakerSize <= 500 && breakerSize > 450:
                    return 500;
                case var _ when breakerSize <= 600 && breakerSize > 500:
                    return 600;
                default:
                    return 1000;
            }
        }

        public float SetAmp()
        {
            float Amp = 0;
            float AVA = PhaseAVA + (ALcl / 4) + (ALml / 4);
            float BVA = PhaseBVA + (BLcl / 4) + (BLml / 4);
            float CVA = PhaseCVA + (CLcl / 4) + (CLml / 4);
            int largestPhase = (int)Math.Max(AVA, Math.Max(BVA, CVA));
            switch (Type)
            {
                case 1:
                    Amp = (float)Math.Round(((double)largestPhase) / 120, 10);
                    break;
                case 2:
                    Amp = (float)Math.Round(((double)largestPhase) / 120, 10);
                    break;
                case 3:
                    Amp = (float)Math.Round(((double)largestPhase) / 277, 10);
                    break;
                case 4:
                    double l1 = PhaseBVA;
                    double l2 = PhaseAVA;
                    double l3 = PhaseCVA;

                    if (HighLegPhase == 'A')
                    {
                        l1 = PhaseAVA;
                        l2 = PhaseCVA;
                        l3 = PhaseBVA;
                    }
                    if (HighLegPhase == 'C')
                    {
                        l1 = PhaseCVA;
                        l2 = PhaseBVA;
                        l3 = PhaseAVA;
                    }
                    double fA = Math.Abs(l3 - l1);
                    double fB = Math.Abs(l1 - l2);
                    double fC = Math.Abs(l2 - l3);
                    double l3N = l3;
                    double l2N = l2;

                    //Adjust ifx depending on high leg phase
                    double iFa = (fA + (0.25 * ALml)) / 240;
                    double iFb = (fB + (0.25 * BLml)) / 240;
                    double iFc = (fC + (0.25 * CLml)) / 240;

                    double iL2N = (l2N + (0.25 * ALcl) + (0.25 * ALml)) / 120;
                    double iL3N = (l3N + (0.25 * CLcl) + (0.25 * CLml)) / 120;

                    if (HighLegPhase == 'A')
                    {
                        iL2N = (l2N + (0.25 * CLcl) + (0.25 * CLml)) / 120;
                        iL3N = (l3N + (0.25 * BLcl) + (0.25 * BLml)) / 120;
                    }
                    if (HighLegPhase == 'C')
                    {
                        iL2N = (l2N + (0.25 * BLcl) + (0.25 * BLml)) / 120;
                        iL3N = (l3N + (0.25 * ALcl) + (0.25 * ALml)) / 120;
                    }
                    double iL1 = Math.Sqrt(Math.Pow(iFa, 2) + Math.Pow(iFb, 2) + (iFa * iFb));
                    double iL2 = Math.Sqrt(
                        Math.Pow(iFb, 2) + Math.Pow((iL2N + iFc), 2) + (iFb * (iL2N + iFc))
                    );
                    double iL3 = Math.Sqrt(
                        Math.Pow(iFa, 2) + Math.Pow((iL3N + iFc), 2) + (iFa * (iL3N + iFc))
                    );
                    Amp = (float)Math.Max(Math.Max(iL3, iL1), iL2);
                    break;
                case 5:
                    Amp = (float)Math.Round(((double)largestPhase) / 120, 10);
                    break;
            }

            return Amp;
        }

        public void DownloadComponents(
            ObservableCollection<ElectricalEquipment> equipment,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalTransformer> transformers
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
            foreach (var transformer in transformers)
            {
                if (transformer.ParentId == Id)
                {
                    transformer.ParentType = "PANEL ";
                    temp.Add(transformer);
                    transformer.PropertyChanged += Transformer_PropertyChanged;
                }
            }
            temp = new ObservableCollection<ElectricalComponent>(temp.OrderBy(e => e.CircuitNo));
            int CurrentLeftCircuit = 1;
            int CurrentRightCircuit = 2;
            foreach (var component in temp)
            {
                if (component.ParentId == Id)
                {
                    if (component.CircuitNo == 0)
                    {
                        componentsCollection.Add(component);
                    }
                    else if (component.CircuitNo % 2 != 0)
                    {
                        while (CurrentLeftCircuit < component.CircuitNo)
                        {
                            AssignSpace(true);
                            CurrentLeftCircuit += 2;
                        }
                        leftComponents.Add(component);
                        CurrentLeftCircuit += (component.Pole * 2);
                    }
                    else
                    {
                        while (CurrentRightCircuit < component.CircuitNo)
                        {
                            AssignSpace(false);
                            CurrentRightCircuit += 2;
                        }
                        rightComponents.Add(component);
                        CurrentRightCircuit += (component.Pole * 2);
                    }
                }
            }
            while (CurrentLeftCircuit < leftCircuits.Count * 2)
            {
                AssignSpace(true);
                CurrentLeftCircuit += 2;
            }
            while (CurrentRightCircuit < (rightCircuits.Count * 2) + 1)
            {
                AssignSpace(false);
                CurrentRightCircuit += 2;
            }
            SetCircuitNumbers();
            SetCircuitVa();
        }

        public void fillInitialSpaces()
        {
            for (int i = 0; i < leftCircuits.Count; i++)
            {
                leftComponents.Add(new Space());
            }
            for (int i = 0; i < rightCircuits.Count; i++)
            {
                rightComponents.Add(new Space());
            }
            SetCircuitNumbers();
            SetCircuitVa();
        }

        public void checkCircuitErrors()
        {
            ErrorMessages.Remove("circuit-errors");
            char activePhase = 'A';

            for (int i = 0; i < leftCircuits.Count; i++)
            {
                leftCircuits[i].ErrorMessage = "";
                if (
                    HighLegPhase == activePhase
                    && leftCircuits[i].BreakerSize != 2
                    && leftCircuits[i].BreakerSize != 3
                    && leftCircuits[i].BreakerSize != 0
                )
                {
                    if (
                        !(
                            (i + 1 < leftCircuits.Count && leftCircuits[i + 1].BreakerSize == 2)
                            || (i + 2 < leftCircuits.Count && leftCircuits[i + 2].BreakerSize == 3)
                        )
                    )
                    {
                        leftCircuits[i].ErrorMessage =
                            "Cannot have a single phase breaker on a highleg phase.";
                        if (!ErrorMessages.ContainsKey("circuit-errors"))
                        {
                            ErrorMessages.Add(
                                "circuit-errors",
                                "This panel has issues with its circuits."
                            );
                        }
                    }
                }

                if (activePhase == 'A')
                {
                    activePhase = 'B';
                }
                else if (activePhase == 'B')
                {
                    if (Pole == 3)
                    {
                        activePhase = 'C';
                    }
                    else
                    {
                        activePhase = 'A';
                    }
                }
                else if (activePhase == 'C')
                {
                    activePhase = 'A';
                }
            }
            activePhase = 'A';
            for (int i = 0; i < rightCircuits.Count; i++)
            {
                rightCircuits[i].ErrorMessage = "";
                if (
                    HighLegPhase == activePhase
                    && rightCircuits[i].BreakerSize != 2
                    && rightCircuits[i].BreakerSize != 3
                    && rightCircuits[i].BreakerSize != 0
                )
                {
                    if (
                        !(
                            (i + 1 < rightCircuits.Count && rightCircuits[i + 1].BreakerSize == 2)
                            || (
                                i + 2 < rightCircuits.Count && rightCircuits[i + 2].BreakerSize == 3
                            )
                        )
                    )
                    {
                        rightCircuits[i].ErrorMessage =
                            "Cannot have a single phase breaker on a highleg phase.";
                        if (!ErrorMessages.ContainsKey("circuit-errors"))
                        {
                            ErrorMessages.Add(
                                "circuit-errors",
                                "This panel has issues with its circuits."
                            );
                        }
                    }
                }

                if (activePhase == 'A')
                {
                    activePhase = 'B';
                }
                else if (activePhase == 'B')
                {
                    if (Pole == 3)
                    {
                        activePhase = 'C';
                    }
                    else
                    {
                        activePhase = 'A';
                    }
                }
                else if (activePhase == 'C')
                {
                    activePhase = 'A';
                }
            }
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

        public string id;
        public string panelId;
        public string projectId;
        public int number;
        public int va;
        public int breakerSize;
        public int index;
        public string description;
        public string name;
        public int loadCategory;
        public bool customBreakerSize;
        public bool customDescription;
        public string errorMessage;
        public string equipId;

        public Circuit(
            string _id,
            string _panelId,
            string _projectId,
            string _equipId,
            int _number,
            int _va,
            int _breakerSize,
            string _description,
            int _loadCategory,
            bool _customBreakerSize,
            bool _customDescription
        )
        {
            number = _number;
            va = _va;
            breakerSize = _breakerSize;
            description = _description;
            loadCategory = _loadCategory;
            id = _id;
            panelId = _panelId;
            projectId = _projectId;
            customBreakerSize = _customBreakerSize;
            customDescription = _customDescription;
            errorMessage = "";
            equipId = string.Empty;
        }

        public string Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }
        public string PanelId
        {
            get => panelId;
            set
            {
                panelId = value;
                OnPropertyChanged();
            }
        }
        public string ProjectId
        {
            get => projectId;
            set
            {
                projectId = value;
                OnPropertyChanged();
            }
        }

        public string EquipId
        {
            get => equipId;
            set
            {
                equipId = value;
                OnPropertyChanged();
            }
        }
        public bool CustomBreakerSize
        {
            get => customBreakerSize;
            set
            {
                customBreakerSize = value;
                OnPropertyChanged(nameof(CustomBreakerSize));
            }
        }
        public bool CustomDescription
        {
            get => customDescription;
            set
            {
                customDescription = value;
                OnPropertyChanged(nameof(CustomDescription));
            }
        }
        public int BreakerSize
        {
            get => breakerSize;
            set
            {
                breakerSize = value;
                OnPropertyChanged(nameof(BreakerSize));
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
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public int LoadCategory
        {
            get => loadCategory;
            set
            {
                loadCategory = value;
                OnPropertyChanged();
            }
        }
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ElectricalPanelNoteRel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalPanelNoteRel(
            string Id,
            string ProjectId,
            string PanelId,
            string NoteId,
            string NoteText,
            int CircuitNo,
            int Length,
            int Stack,
            string Tag
        )
        {
            this.Id = Id;
            this.ProjectId = ProjectId;
            this.PanelId = PanelId;
            this.NoteId = NoteId;
            this.NoteText = NoteText;
            this.CircuitNo = CircuitNo;
            this.Length = Length;
            this.Stack = Stack;
            this.Tag = Tag;
        }

        private string _Id = String.Empty;
        public string Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string _NoteId = String.Empty;
        public string NoteId
        {
            get => _NoteId;
            set
            {
                if (_NoteId != value)
                {
                    _NoteId = value;
                    OnPropertyChanged(nameof(NoteId));
                }
            }
        }

        private string _NoteText = String.Empty;
        public string NoteText
        {
            get => _NoteText;
            set
            {
                if (_NoteText != value)
                {
                    _NoteText = value;
                    OnPropertyChanged(nameof(NoteText));
                }
            }
        }

        private string _NoteTag = string.Empty;
        public string Tag
        {
            get => _NoteTag;
            set
            {
                if (_NoteTag != value)
                {
                    _NoteTag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        private string _PanelId = string.Empty;
        public string PanelId
        {
            get => _PanelId;
            set
            {
                if (_PanelId != value)
                {
                    _PanelId = value;
                    OnPropertyChanged(nameof(PanelId));
                }
            }
        }

        private string _ProjectId = string.Empty;
        public string ProjectId
        {
            get => _ProjectId;
            set
            {
                if (_ProjectId != value)
                {
                    _ProjectId = value;
                    OnPropertyChanged(nameof(ProjectId));
                }
            }
        }

        private int _CircuitNo = 0;
        public int CircuitNo
        {
            get => _CircuitNo;
            set
            {
                if (_CircuitNo != value)
                {
                    _CircuitNo = value;
                    OnPropertyChanged(nameof(CircuitNo));
                }
            }
        }

        private int _Length = 0;
        public int Length
        {
            get => _Length;
            set
            {
                if (_Length != value)
                {
                    _Length = value;
                    OnPropertyChanged(nameof(Length));
                }
            }
        }

        private int _Stack = 0;
        public int Stack
        {
            get => _Stack;
            set
            {
                if (_Stack != value)
                {
                    _Stack = value;
                    OnPropertyChanged(nameof(Stack));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ElectricalPanelNote : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalPanelNote(string Id, string ProjectId, string Note, string Tag)
        {
            this.Id = Id;
            this.Note = Note;
            this.ProjectId = ProjectId;
            this.Tag = Tag;
        }

        public ElectricalPanelNote() { }

        private string _Id = string.Empty;
        public string Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string _Tag = string.Empty;
        public string Tag
        {
            get => _Tag;
            set
            {
                if (_Tag != value)
                {
                    _Tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        private string _ProjectId = string.Empty;
        public string ProjectId
        {
            get => _ProjectId;
            set
            {
                if (_ProjectId != value)
                {
                    _ProjectId = value;
                    OnPropertyChanged(nameof(ProjectId));
                }
            }
        }

        private string _Note = string.Empty;
        public string Note
        {
            get => _Note;
            set
            {
                if (_Note != value)
                {
                    _Note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }

        private string _DateCreated = string.Empty;
        public string DateCreated
        {
            get => _DateCreated;
            set
            {
                if (_DateCreated != value)
                {
                    _DateCreated = value;
                    OnPropertyChanged(nameof(DateCreated));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Note : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string id;

        //public int number;
        public string panelId;
        public string projectId;
        public int circuitNo;
        public int length;

        //public string description;
        public string groupId;
        public int stack;
        public SharedNoteData sharedData;

        public Note()
        {
            this.id = Guid.NewGuid().ToString();
            this.groupId = Guid.NewGuid().ToString();
            this.circuitNo = 0;
            this.length = 0;
            this.stack = 0;
            this.sharedData = new SharedNoteData();
            sharedData.PropertyChanged += SharedData_PropertyChanged;
        }

        public Note(Note note)
        {
            this.circuitNo = note.CircuitNo;
            this.length = note.Length;
            this.stack = note.stack;
            this.panelId = note.PanelId;
            this.projectId = note.ProjectId;
            this.id = Guid.NewGuid().ToString();
            this.groupId = note.GroupId;
            this.sharedData = note.sharedData;
            sharedData.PropertyChanged += SharedData_PropertyChanged;
        }

        private void SharedData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public SharedNoteData SharedData
        {
            get => sharedData;
            set
            {
                if (sharedData != value)
                {
                    sharedData.PropertyChanged -= SharedData_PropertyChanged;
                    sharedData = value;
                    sharedData.PropertyChanged += SharedData_PropertyChanged;
                    OnPropertyChanged(nameof(SharedData));
                }
            }
        }
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        public string PanelId
        {
            get => panelId;
            set
            {
                if (panelId != value)
                {
                    panelId = value;
                    OnPropertyChanged(nameof(PanelId));
                }
            }
        }
        public string ProjectId
        {
            get => projectId;
            set
            {
                if (projectId != value)
                {
                    projectId = value;
                    OnPropertyChanged(nameof(ProjectId));
                }
            }
        }
        public int CircuitNo
        {
            get => circuitNo;
            set
            {
                if (circuitNo != value)
                {
                    circuitNo = value;
                    OnPropertyChanged(nameof(CircuitNo));
                }
            }
        }
        public int Length
        {
            get => length;
            set
            {
                if (length != value)
                {
                    length = value;
                    OnPropertyChanged(nameof(Length));
                }
            }
        }
        public string GroupId
        {
            get => groupId;
            set
            {
                if (groupId != value)
                {
                    groupId = value;
                    OnPropertyChanged(nameof(GroupId));
                }
            }
        }

        public string Description
        {
            get => sharedData.Description;
            set
            {
                if (sharedData.Description != value)
                {
                    sharedData.Description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int Number
        {
            get => sharedData.Number;
            set
            {
                if (sharedData.Number != value)
                {
                    sharedData.Number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }
        public int Stack
        {
            get => stack;
            set
            {
                if (stack != value)
                {
                    stack = value;
                    OnPropertyChanged(nameof(Stack));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SharedNoteData : INotifyPropertyChanged
    {
        private string description = "";
        private int number;

        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public int Number
        {
            get => number;
            set
            {
                if (number != value)
                {
                    number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Space : ElectricalComponent
    {
        public Space()
        {
            Pole = 1;
            Name = "Space";
            this.componentType = "Space";
        }
    }
}
