﻿using System;
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
        private string id;
        private string projectId;
        private string parentId;
        private string manufacturer;
        private string modelNo;
        private int qty;
        private bool occupancy;
        public int wattage;
        public bool emCapable;
        private int mountingType;
        private string tag;
        public string notes;
        public int voltageId;
        public int symbolId;
        public string colorCode;
        private bool powered;
        private string description;
        private int driverTypeId;
        private bool specSheetFromClient;
        private string specSheetId;
        private bool hasPhotoCell;
        private string locationId;

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalLighting(
            string id,
            string projectId,
            string parentId,
            string manufacturer,
            string modelNo,
            int qty,
            bool occupancy,
            int wattage,
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
            string locationId
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.parentId = parentId;
            this.manufacturer = manufacturer;
            this.modelNo = modelNo;
            this.qty = qty;
            this.occupancy = occupancy;
            this.wattage = wattage;
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

        public int Wattage
        {
            get => wattage;
            set
            {
                if (wattage != value)
                {
                    wattage = value;
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
