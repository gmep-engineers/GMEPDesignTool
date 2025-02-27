using Org.BouncyCastle.Tls.Crypto;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
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
        private float _va;
        private int _type;
        private bool _powered;
        private bool _isHiddenOnPlan;
        private string _location;
        private string _parentName;
        private string _parentType;


        public ElectricalComponent ParentComponent { get; set; }
        public ObservableCollection<ElectricalComponent> componentsCollection { get; set; } = new ObservableCollection<ElectricalComponent>();

        public ObservableCollection<ElectricalComponent> leftComponents { get; set; } = new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<ElectricalComponent> rightComponents { get; set; } = new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<Circuit> leftCircuits { get; set; } = new ObservableCollection<Circuit>();
        public ObservableCollection<Circuit> rightCircuits { get; set; } = new ObservableCollection<Circuit>();
        public ObservableCollection<Note> notes { get; set; } = new ObservableCollection<Note>();
        public ObservableCollection<Note> leftNodes { get; set; } = new ObservableCollection<Note>();
        public ObservableCollection<Note> rightNodes { get; set; } = new ObservableCollection<Note>();

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
            bool isHiddenOnPlan,
            string location
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
            this._location = location;
            this.componentType = "Panel";
            SetPole();
            PopulateCircuits(Id, ProjectId);
            updateFlag = false;
            notes.CollectionChanged += Notes_CollectionChanged;
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
        public string ParentType
        {
            get => _parentType;
            set
            {
                if (_parentType != value)
                {
                    this._parentType = value;
                    OnPropertyChanged(nameof(ParentType));
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
                    this.parentId = value ?? "";
                    OnPropertyChanged(nameof(ParentId));
                    if (ParentComponent != null && string.IsNullOrEmpty(value))
                    {
                        ParentComponent.PropertyChanged -= ParentComponent_PropertyChanged;
                        ParentName = "";
                        ParentType = "";
                        ParentComponent = null;
                    }
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
                    OnPropertyChanged(nameof(Pole));
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

       public  float Va
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
                    leftCircuits.Add(new Circuit(Guid.NewGuid().ToString(), id, projectId, leftCircuits.Count * 2 + 1, 0, 0, "", 0));
                }
                else
                {
                    rightCircuits.Add(new Circuit(Guid.NewGuid().ToString(), id, projectId, rightCircuits.Count * 2 + 2, 0, 0, "", 0));
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
            if (e.PropertyName == nameof(ElectricalEquipment.Va) || e.PropertyName == nameof(ElectricalEquipment.Amp) ||  e.PropertyName == nameof(ElectricalEquipment.Pole) || e.PropertyName == nameof(ElectricalEquipment.Name) || e.PropertyName == nameof(ElectricalEquipment.LoadType) || e.PropertyName == nameof(ElectricalEquipment.LoadCategory))
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

            if (e.PropertyName == nameof(ElectricalPanel.Pole) || e.PropertyName == nameof(ElectricalPanel.Name) || e.PropertyName == nameof(ElectricalPanel.UpdateFlag))
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
            if (e.PropertyName == nameof(ElectricalTransformer.Pole) || e.PropertyName == nameof(ElectricalEquipment.Name)  || e.PropertyName == nameof(ElectricalTransformer.UpdateFlag))
            {
                SetCircuitNumbers();
                SetCircuitVa();
            }
            if (e.PropertyName == nameof(ElectricalTransformer.ParentId))
            {
                if (sender is ElectricalTransformer transformer)
                {
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
            if (e.PropertyName == nameof(ElectricalEquipment.Name) && sender is ElectricalComponent component)
            {
               ParentName = component.Name;
            }
        }
        private void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Note newNote in e.NewItems)
                {
                    newNote.PanelId = this.Id;
                    newNote.ProjectId = this.ProjectId;
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
                int circuitIndex = leftCircuits.IndexOf(
                    leftCircuits.FirstOrDefault(c => c.Number == component.CircuitNo)
                );
                if (circuitIndex != -1 && circuitIndex + component.Pole <= leftCircuits.Count)
                {
                    bool lmlIsLarger = false;
                    if (component.Lml > Lml){
                        lmlIsLarger = true;
                    }

                    for (int i = 0; i < component.Pole; i++)
                    {
                        var addedValue = 0;
                        float lclValue = 0;
                        float lmlValue = 0;
                        switch(i)
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
                                leftCircuits[circuitIndex + i].BreakerSize = DetermineBreakerSize(component);
                            }
                        }

                        leftCircuits[circuitIndex + i].LoadCategory = component.LoadCategory;
                       
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
                        if (i == 0)
                        {
                            rightCircuits[circuitIndex + i].Description = component.Name;
                        }
                        else
                        {
                            rightCircuits[circuitIndex + i].Description = "---";
                        }
                        if (i == component.Pole - 1)
                        {
                            rightCircuits[circuitIndex + i].BreakerSize = i + 1;
                        }
                        if (i == 0)
                        {
                            rightCircuits[circuitIndex + i].BreakerSize = DetermineBreakerSize(component);
                        }

                        rightCircuits[circuitIndex + i].LoadCategory = component.LoadCategory;
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
            RootKva = (Kva + (Lml/4) + (Lcl/4)) / 1000;
            Kva = (float)Math.Ceiling(RootKva);
            Amp = (float)Math.Ceiling(SetAmp());
            UpdateFlag = !UpdateFlag;
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
            float AVA = PhaseAVA + (ALcl/4) + (ALml/4);
            float BVA = PhaseBVA + (BLcl/4) + (BLml/4);
            float CVA = PhaseCVA + (CLcl/4) + (CLml/4);
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
                    double l1 = PhaseAVA;
                    double l2 = PhaseBVA;
                    double l3 = PhaseCVA;
                    double fA = Math.Abs(l3 - l1);
                    double fB = Math.Abs(l1 - l2);
                    double fC = Math.Abs(l2 - l3);
                    double l3N = l3;
                    double l2N = l2;
                    double iFa = (fA + (0.25 * ALml)) / 240;
                    double iFb = (fB + (0.25 * BLml)) / 240;
                    double iFc = (fC + (0.25 * CLml)) / 240;
                    double iL2N = (l2N + (0.25 * Lcl) + (0.25 * BLml)) / 120;
                    double iL3N = (l3N + (0.25 * Lcl) + (0.25 * CLml)) / 120;
                    double iL1 = Math.Sqrt(Math.Pow(iFa, 2) + Math.Pow(iFb, 2) + (iFa * iFb));
                    double iL2 = Math.Sqrt(Math.Pow(iFb, 2) + Math.Pow((iL2N + iFc), 2) + (iFb * (iL2N + iFc)));
                    double iL3 = Math.Sqrt(Math.Pow(iFa, 2) + Math.Pow((iL3N + iFc), 2) + (iFa * (iL3N + iFc)));
                    Amp = (float)Math.Max(Math.Max(iL3, iL1), iL2);
                    break;
                case 5:
                    Amp = (float)Math.Round(((double)largestPhase) / 120, 10);
                    break;

            }
            
            return Amp;
        }
        public void DownloadComponents(ObservableCollection<ElectricalEquipment> equipment, ObservableCollection<ElectricalPanel> panels, ObservableCollection<ElectricalTransformer> transformers)
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

        public Circuit(string _id, string _panelId, string _projectId, int _number, int _va, int _breakerSize, string _description, int _loadCategory)
        {
            number = _number;
            va = _va;
            breakerSize = _breakerSize;
            description = _description;
            loadCategory = _loadCategory;
            id = _id;
            panelId = _panelId;
            projectId = _projectId;
            customBreakerSize = false;
            customDescription = false;
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
        public bool CustomBreakerSize
        {
            get => customBreakerSize;
            set
            {
                customBreakerSize = value;
                OnPropertyChanged();
            }
        }
        public bool CustomDescription
        {
            get => customDescription;
            set
            {
                customDescription = value;
                OnPropertyChanged();
            }
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
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Note: INotifyPropertyChanged
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
