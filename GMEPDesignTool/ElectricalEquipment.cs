using System;
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
        private float va;
        private bool isHiddenOnPlan;
        private int loadType;
        private bool isLcl;

        public ElectricalEquipment(
            string id,
            string projectId,
            string owner,
            string equipNo,
            int qty,
            string parentId,
            int voltage,
            float fla,
            float va,
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
            int loadType
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
            this.phaseAVa = va;
            this.phaseBVa = va;
            this.phaseCVa = va;
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
            this.lcl = 0;
            this.isLcl = false;
            DetermineLoadTypes();
            determineEquipmentPole();
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
                }
            }
        }
        public override string Name
        {
            get => equipNo;
            set
            {
                if (equipNo != value)
                {
                    equipNo = value;
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
        public float Va
        {
            get => va;
            set
            {
                if (va != value)
                {
                    va = value;
                    OnPropertyChanged(nameof(Va));
                    OnPropertyChanged(nameof(PhaseAVA));
                    OnPropertyChanged(nameof(PhaseBVA));
                    OnPropertyChanged(nameof(PhaseCVA));
                }
            }
        }
        public override float PhaseAVA
        {
            get => va;
            set
            {
                if (va != value)
                {
                    va = value;
                }
            }
        }
        public override float PhaseBVA
        {
            get => va;
            set
            {
                if (va != value)
                {
                    va = value;
                }
            }
        }
        public override float PhaseCVA
        {
            get => va;
            set
            {
                if (va != value)
                {
                    va = value;
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
                    OnPropertyChanged(nameof(LoadType));
                    DetermineLoadTypes();
                }
            }
        }

        public bool IsLcl
        {
            get => isLcl;
            set
            {
                if (isLcl != value)
                {
                    isLcl = value;
                    OnPropertyChanged(nameof(IsLcl));
                }
            }
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
        public void DetermineLoadTypes()
        {
            switch (LoadType)
            {
                case 2:
                    IsLcl = true;
                    break;
                case 3:
                    IsLcl = false;
                    break;
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
