using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace GMEPDesignTool
{
    public class ElectricalLightingControl : INotifyPropertyChanged
    {
        private string id;
        private int driverTypeId;
        private bool occupancy;
        private string tag;
        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalLightingControl()
        {
            driverTypeId = 1;
            occupancy = false;
            tag = string.Empty;
            id = Guid.NewGuid().ToString();
        }

        public ElectricalLightingControl(string id, int driverTypeId, bool occupancy, string tag)
        {
            Id = id;
            DriverTypeId = driverTypeId;
            Occupancy = occupancy;
            Tag = tag;
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
