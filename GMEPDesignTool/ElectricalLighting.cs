using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mysqlx.Crud;

namespace GMEPDesignTool
{
    public class ElectricalLighting : INotifyPropertyChanged
    {
        private string id = Guid.NewGuid().ToString();
        private string projectId = string.Empty;
        private string parentId = string.Empty;
        private string manufacturer = string.Empty;
        private string modelNo = string.Empty;
        private int qty = 1;
        private bool occupancy = false;
        public double wattage = 0;
        public bool emCapable = false;
        private int mountingType = 1;
        private string tag = string.Empty;
        public string notes = string.Empty;
        public int voltageId = 2;
        public int symbolId = 1;
        public string colorCode = "#FFFFFFFF";
        private bool powered = false;
        private string description = string.Empty;
        private int driverTypeId = 1;
        private bool specSheetFromClient = false;
        private string specSheetId = string.Empty;
        private bool hasPhotoCell = false;
        private string locationId = string.Empty;
        private int orderNo = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalLighting(
            string id,
            string projectId,
            string parentId,
            string manufacturer,
            string modelNo,
            int qty,
            bool occupancy,
            double wattage,
            bool emCapable,
            int mountingType,
            string tag,
            string notes,
            int voltageId,
            int symbolId,
            string colorCode,
            bool powered,
            string description,
            int driverTypeId,
            bool specSheetFromClient,
            string specSheetId,
            bool hasPhotoCell,
            string locationId,
            int orderNo
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.parentId = parentId;
            this.manufacturer = manufacturer;
            this.modelNo = modelNo;
            this.qty = qty;
            this.occupancy = occupancy;
            this.wattage = Math.Round(wattage, 1);
            this.emCapable = emCapable;
            this.mountingType = mountingType;
            this.tag = tag;
            this.notes = notes;
            this.voltageId = voltageId;
            this.symbolId = symbolId;
            this.colorCode = colorCode;
            this.powered = powered;
            this.description = description;
            this.driverTypeId = driverTypeId;
            this.specSheetFromClient = specSheetFromClient;
            this.specSheetId = specSheetId;
            this.hasPhotoCell = hasPhotoCell;
            this.locationId = locationId;
            this.orderNo = orderNo;
        }

        public ElectricalLighting() { }

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

        public string ParentId
        {
            get => parentId;
            set
            {
                if (parentId != value)
                {
                    parentId = value;
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }

        public string Manufacturer
        {
            get => manufacturer;
            set
            {
                if (manufacturer != value)
                {
                    manufacturer = value;
                    OnPropertyChanged(nameof(Manufacturer));
                }
            }
        }

        public string ModelNo
        {
            get => modelNo;
            set
            {
                if (modelNo != value)
                {
                    modelNo = value;
                    OnPropertyChanged(nameof(ModelNo));
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

        public bool Occupancy
        {
            get => occupancy;
            set
            {
                if (occupancy != value)
                {
                    occupancy = value;
                    OnPropertyChanged(nameof(Occupancy));
                }
            }
        }

        public double Wattage
        {
            get => wattage;
            set
            {
                if (wattage != value)
                {
                    wattage = Math.Round(value, 1);
                    OnPropertyChanged(nameof(Wattage));
                }
            }
        }

        public bool EmCapable
        {
            get => emCapable;
            set
            {
                if (emCapable != value)
                {
                    emCapable = value;
                    OnPropertyChanged(nameof(EmCapable));
                }
            }
        }

        public int MountingType
        {
            get => mountingType;
            set
            {
                if (mountingType != value)
                {
                    mountingType = value;
                    OnPropertyChanged(nameof(MountingType));
                }
            }
        }

        public string Tag
        {
            get => tag;
            set
            {
                if (tag != value)
                {
                    tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public string Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public int VoltageId
        {
            get => voltageId;
            set
            {
                if (voltageId != value)
                {
                    voltageId = value;
                    OnPropertyChanged(nameof(VoltageId));
                }
            }
        }

        public int SymbolId
        {
            get => symbolId;
            set
            {
                if (symbolId != value)
                {
                    symbolId = value;
                    OnPropertyChanged(nameof(SymbolId));
                }
            }
        }

        public string ColorCode
        {
            get => colorCode;
            set
            {
                if (colorCode != value)
                {
                    colorCode = value;
                    OnPropertyChanged(nameof(ColorCode));
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

        public int DriverTypeId
        {
            get => driverTypeId;
            set
            {
                if (driverTypeId != value)
                {
                    driverTypeId = value;
                    OnPropertyChanged(nameof(DriverTypeId));
                }
            }
        }

        public bool SpecSheetFromClient // Added property
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
        public bool HasPhotoCell
        {
            get => hasPhotoCell;
            set
            {
                if (hasPhotoCell != value)
                {
                    hasPhotoCell = value;
                    OnPropertyChanged(nameof(HasPhotoCell));
                }
            }
        }
        public string LocationId
        {
            get => locationId;
            set
            {
                if (locationId != value)
                {
                    locationId = value;
                    OnPropertyChanged(nameof(LocationId));
                }
            }
        }
        public int OrderNo
        {
            get => orderNo;
            set
            {
                if (orderNo != value)
                {
                    orderNo = value;
                    OnPropertyChanged(nameof(OrderNo));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Verify()
        {
            if (!Utils.IsEquipName(Manufacturer))
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
}
