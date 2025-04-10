using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalEquipment : ElectricalComponent
    {
        private string owner = string.Empty;
        private string equipNo = string.Empty;
        private int qty = 1;
        private int voltage = 2;
        private float fla = 0;
        private bool is3Ph = false;
        private string specSheetId = string.Empty;
        private int aicRating = 0;
        private bool specSheetFromClient = false;
        int distanceFromParent = 0;
        int category = 1;
        int connection = 1;
        private string description = string.Empty;
        private int mcaId = 1;
        private string hp = string.Empty;
        private float width = 0;
        private float depth = 0;
        public float height = 0;
        private bool powered = false;
        private bool hasPlug = true;
        private bool lockingConnector = false;
        private int va = 0;
        private bool isHiddenOnPlan = false;
        private int loadType = 1;
        private string circuits = string.Empty;
        private DateTime dateCreated = DateTime.Now;
        private int statusId = 1;

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
            int orderNo,
            int statusId
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
            this.statusId = statusId;
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

        public ElectricalEquipment()
        {
            DetermineCircuits();
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
            get => equipNo + " - " + description;
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
                    OnPropertyChanged(nameof(Name));
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
        public int StatusId
        {
            get => statusId;
            set
            {
                if (statusId != value)
                {
                    statusId = value;
                    OnPropertyChanged(nameof(StatusId));
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
                    setVa();
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
                    setVa();
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
                    setVa();
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
                    setVa();
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
                    SetFla();
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
                    DetermineCircuits();
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

        public override string ParentId
        {
            get => parentId;
            set
            {
                if (parentId != value)
                {
                    parentId = value;
                    OnPropertyChanged(nameof(ParentId));
                    Circuits = "Assign";
                }
            }
        }

        public DateTime DateCreated
        {
            get => dateCreated;
        }

        public void SetPhaseVa()
        {
            determineEquipmentPole();
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

        public void SetFla()
        {
            double fla = 0;
            double va = Convert.ToDouble(Va);
            switch (Voltage)
            {
                case 1:
                    fla = va / 115;
                    break;
                case 2:
                    fla = va / 120;
                    break;
                case 3:
                    if (Pole == 3)
                        fla = va / 208 / 1.732;
                    else
                        fla = va / 208;
                    break;
                case 4:
                    if (Pole == 3)
                        fla = va / 230 / 1.732;
                    else
                        fla = va / 230;
                    break;
                case 5:
                    if (Pole == 3)
                        fla = va / 240 / 1.732;
                    else
                        fla = va / 240;
                    Trace.WriteLine(va);
                    break;
                case 6:
                    if (Pole == 3)
                        fla = va / 277 / 1.732;
                    else
                        fla = va / 277;
                    break;
                case 7:
                    if (Pole == 3)
                        fla = va * 460 / 1.732;
                    else
                        fla = va * 460;
                    break;
                case 8:
                    if (Pole == 3)
                        fla = va / 480 / 1.732;
                    else
                        fla = va / 480;
                    break;
            }
            this.va = Convert.ToInt32(va);
        }

        public void setVa()
        {
            double va = 0;

            switch (Voltage)
            {
                case 1:
                    va = Fla * 115;
                    break;
                case 2:
                    va = Fla * 120;
                    break;
                case 3:
                    if (Pole == 3)
                        va = Fla * 208 / 1.732;
                    else
                        va = Fla * 208;
                    break;
                case 4:
                    if (Pole == 3)
                        va = Fla * 230 / 1.732;
                    else
                        va = Fla * 230;
                    break;
                case 5:
                    if (Pole == 3)
                        va = Fla * 240 / 1.732;
                    else
                        va = Fla * 240;
                    break;
                case 6:
                    if (Pole == 3)
                        va = Fla * 277 / 1.732;
                    else
                        va = Fla * 277;
                    break;
                case 7:
                    if (Pole == 3)
                        va = Fla * 460 / 1.732;
                    else
                        va = Fla * 460;
                    break;
                case 8:
                    if (Pole == 3)
                        va = Fla * 480 / 1.732;
                    else
                        va = Fla * 480;
                    break;
            }
            this.va = Convert.ToInt32(va);
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
