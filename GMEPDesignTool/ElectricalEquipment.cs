using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalEquipment : INotifyPropertyChanged
    {
        private string id;
        private string projectId;
        private string owner;
        private string equipNo;
        private int qty;
        private string panelId;
        private int voltage;
        private float amp;
        private float va;
        private bool is3Ph;
        private string specSheetId;

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalEquipment(
            string id,
            string projectId,
            string owner,
            string equipNo,
            int qty,
            string panelId,
            int voltage,
            float amp,
            float va,
            bool is3Ph,
            string specSheetId
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.owner = owner;
            this.equipNo = equipNo;
            this.qty = qty;
            this.panelId = panelId;
            this.voltage = voltage;
            this.amp = amp;
            this.va = va;
            this.is3Ph = is3Ph;
            this.specSheetId = specSheetId;
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

        public int Voltage
        {
            get => voltage;
            set
            {
                if (voltage != value)
                {
                    voltage = value;
                    OnPropertyChanged(nameof(Voltage));
                }
            }
        }

        public float Amp
        {
            get => amp;
            set
            {
                if (amp != value)
                {
                    amp = value;
                    OnPropertyChanged(nameof(Amp));
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            if (!Utils.IsUuid(PanelId))
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
