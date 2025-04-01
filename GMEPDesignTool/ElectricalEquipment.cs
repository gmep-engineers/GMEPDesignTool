using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalEquipment : ElectricalComponent
    {
        private string owner;
        private string equipNo;
        private int qty;
        private int voltage;
        private float fla;
        private bool is3Ph;
        private string specSheetId;
        private int aicRating;
        private bool specSheetFromClient;
        int distanceFromParent;
        int category;
        int connection;
        private string description;
        private int mcaId;
        private string hp;
        private float width;
        private float depth;
        public float height;
        private bool powered;
        private bool hasPlug;
        private bool lockingConnector;
        private int va;
        private bool isHiddenOnPlan;
        private int loadType;
        private string circuits;

        public ElectricalEquipment(
            string id,
            string projectId,
            string owner,
            string equipNo,
            int qty,
            string parentId,
            int voltage,
            float fla,
            int va,
            bool is3Ph,
            string specSheetId,
            int aicRating,
            bool specSheetFromClient,
            int distanceFromParent,
            int category,
            string colorCode,
            bool powered,
            int connection,
            string description,
            int mcaId,
            string hp,
            bool hasPlug,
            bool lockingConnector,
            float width,
            float depth,
            float height,
            int circuitNo,
            bool isHiddenOnPlan,
            int loadType,
            int orderNo
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.owner = owner;
            this.equipNo = equipNo;
            this.qty = qty;
            this.parentId = parentId;
            this.voltage = voltage;
            this.fla = fla;
            this.va = va;
            this.amp = fla;
            this.is3Ph = is3Ph;
            this.specSheetId = specSheetId;
            this.aicRating = aicRating;
            this.specSheetFromClient = specSheetFromClient;
            this.distanceFromParent = distanceFromParent;
            this.category = category;
            this.colorCode = colorCode;
            this.powered = powered;
            this.connection = connection;
            this.description = description;
            this.mcaId = mcaId;
            this.hp = hp;
            this.hasPlug = hasPlug;
            this.lockingConnector = lockingConnector;
            this.width = width;
            this.depth = depth;
            this.height = height;
            this.circuitNo = circuitNo;
            this.isHiddenOnPlan = isHiddenOnPlan;
            this.loadType = loadType;
            this.lcl = va;
            this.lml = va;
            this.phaseAVa = 0;
            this.phaseBVa = 0;
            this.phaseCVa = 0;
            this.ALml = 0;
            this.BLml = 0;
            this.CLml = 0;
            this.ALcl = 0;
            this.BLcl = 0;
            this.CLcl = 0;
            this.componentType = "Equipment";
            this.orderNo = orderNo;
            DetermineLoadCategory();
            //DetermineLoadTypes();
            determineEquipmentPole();
            DetermineCircuits();
            SetPhaseVa();
        }

        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public override string Name
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                }
            }
        }

        public string Owner
        {
            get => owner;
            set
            {
                if (owner != value)
                {
                    owner = value;
                    OnPropertyChanged(nameof(Owner));
                }
            }
        }

        public string EquipNo
        {
            get => equipNo;
            set
            {
                if (equipNo != value)
                {
                    equipNo = value;
                    OnPropertyChanged(nameof(EquipNo));
                }
            }
        }

        public int Qty
        {
            get => qty;
            set
            {
                if (qty != value)
                {
                    qty = value;
                    OnPropertyChanged(nameof(Qty));
                }
            }
        }

        public int Voltage
        {
            get => voltage;
            set
            {
                if (voltage != value)
                {
                    voltage = value;
                    OnPropertyChanged(nameof(Voltage));
                    determineEquipmentPole();
                }
            }
        }

        public float Fla
        {
            get => fla;
            set
            {
                if (fla != value)
                {
                    fla = value;
                    OnPropertyChanged(nameof(Fla));
                    OnPropertyChanged(nameof(Amp));
                }
            }
        }
        public override float Amp
        {
            get => fla;
            set
            {
                if (fla != value)
                {
                    fla = value;
                }
            }
        }

        public bool Is3Ph
        {
            get => is3Ph;
            set
            {
                if (is3Ph != value)
                {
                    is3Ph = value;
                    OnPropertyChanged(nameof(Is3Ph));
                    determineEquipmentPole();
                }
            }
        }

        public string SpecSheetId
        {
            get => specSheetId;
            set
            {
                if (specSheetId != value)
                {
                    specSheetId = value;
                    OnPropertyChanged(nameof(SpecSheetId));
                }
            }
        }

        public bool Powered
        {
            get => powered;
            set
            {
                if (powered != value)
                {
                    powered = value;
                    OnPropertyChanged(nameof(Powered));
                }
            }
        }

        public bool HasPlug
        {
            get => hasPlug;
            set
            {
                if (hasPlug != value)
                {
                    hasPlug = value;
                    OnPropertyChanged(nameof(HasPlug));
                    DetermineLoadCategory();
                }
            }
        }

        public bool LockingConnector
        {
            get => lockingConnector;
            set
            {
                if (lockingConnector != value)
                {
                    lockingConnector = value;
                    OnPropertyChanged(nameof(LockingConnector));
                }
            }
        }

        public bool SpecSheetFromClient
        {
            get => specSheetFromClient;
            set
            {
                if (specSheetFromClient != value)
                {
                    specSheetFromClient = value;
                    OnPropertyChanged(nameof(SpecSheetFromClient));
                }
            }
        }
        public int DistanceFromParent
        {
            get => distanceFromParent;
            set
            {
                if (distanceFromParent != value)
                {
                    distanceFromParent = value;
                    OnPropertyChanged(nameof(DistanceFromParent));
                }
            }
        }
        public int Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    category = value;
                    DetermineLoadCategory();
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public int AicRating
        {
            get => aicRating;
            set
            {
                if (aicRating != value)
                {
                    aicRating = value;
                    OnPropertyChanged(nameof(AicRating));
                }
            }
        }
        public int Connection
        {
            get => connection;
            set
            {
                if (connection != value)
                {
                    connection = value;
                    OnPropertyChanged(nameof(Connection));
                }
            }
        }

        public int McaId
        {
            get => mcaId;
            set
            {
                if (mcaId != value)
                {
                    mcaId = value;
                    OnPropertyChanged(nameof(McaId));
                }
            }
        }

        public string Hp
        {
            get => hp;
            set
            {
                if (hp != value)
                {
                    hp = value;
                    OnPropertyChanged(nameof(Hp));
                }
            }
        }

        public float Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }
        public float Depth
        {
            get => depth;
            set
            {
                if (depth != value)
                {
                    depth = value;
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }
        public float Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        public bool IsHiddenOnPlan
        {
            get => isHiddenOnPlan;
            set
            {
                if (isHiddenOnPlan != value)
                {
                    isHiddenOnPlan = value;
                    OnPropertyChanged(nameof(IsHiddenOnPlan));
                }
            }
        }
        public int Va
        {
            get => va;
            set
            {
                if (va != value)
                {
                    va = value;
                    SetPhaseVa();
                    OnPropertyChanged(nameof(Va));
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
                    SetPhaseVa();
                    OnPropertyChanged(nameof(Pole));
                }
            }
        }

        public int LoadType
        {
            get => loadType;
            set
            {
                if (loadType != value)
                {
                    loadType = value;
                    SetPhaseVa();
                    OnPropertyChanged(nameof(LoadType));
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

        public void SetPhaseVa()
        {
            double vaTemp = Va;
            switch (Pole)
            {
                case 2:
                    vaTemp /= 2;
                    break;
                case 3:
                    vaTemp /= 1.732;
                    break;
            }
            PhaseAVA = (float)vaTemp;
            PhaseBVA = (float)vaTemp;
            PhaseCVA = (float)vaTemp;

            switch (loadType)
            {
                case 3:
                    Lcl = Va;
                    ALcl = (float)vaTemp;
                    BLcl = (float)vaTemp;
                    CLcl = (float)vaTemp;
                    Lml = 0;
                    ALml = 0;
                    BLml = 0;
                    CLml = 0;
                    break;
                case 2:
                    Lcl = 0;
                    ALcl = 0;
                    BLcl = 0;
                    CLcl = 0;
                    Lml = Va;
                    ALml = (float)vaTemp;
                    BLml = (float)vaTemp;
                    CLml = (float)vaTemp;
                    break;
                default:
                    Lcl = 0;
                    ALcl = 0;
                    BLcl = 0;
                    CLcl = 0;
                    Lml = 0;
                    ALml = 0;
                    BLml = 0;
                    CLml = 0;
                    break;
            }
            // OnPropertyChanged(nameof())
        }

        private void determineEquipmentPole()
        {
            int pole = 3;
            if (Is3Ph == false)
            {
                if (Voltage == 1 || Voltage == 2 || Voltage == 6)
                {
                    pole = 1;
                }
                else
                {
                    pole = 2;
                }
            }
            this.Pole = pole;
        }

        public void DetermineCircuits()
        {
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

        /*public void DetermineLoadTypes()
        {
            switch (LoadType)
            {
                case 3:
                    IsLcl = true;
                    IsLml = false;
                    break;
                case 2:
                    IsLcl = false;
                    IsLml = true;
                    break;
                case 1:
                    IsLcl = false;
                    IsLml = false;
                    break;
            }
        }*/
        public void DetermineLoadCategory()
        {
            if (Category == 5)
            {
                LoadCategory = 1;
            }
            else if (hasPlug)
            {
                LoadCategory = 2;
            }
            else
            {
                LoadCategory = 3;
            }
        }

        public bool Verify()
        {
            if (!Utils.IsOwnerName(Owner))
            {
                return false;
            }
            if (!Utils.IsEquipName(EquipNo))
            {
                return false;
            }
            if (!Utils.IsUuid(ParentId))
            {
                return false;
            }
            if (!Utils.IsUuid(SpecSheetId))
            {
                return false;
            }
            return true;
        }
    }
}
