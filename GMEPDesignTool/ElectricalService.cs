using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalService : ElectricalComponent
    {
        private int _type;
        private int _config;
        private int _aicRating;
        public ObservableCollection<ElectricalPanel> childPanels { get; set; } = new ObservableCollection<ElectricalPanel>();
        public ElectricalService(
            string id,
            string projectId,
            string name,
            int type,
            int amp,
            int config,
            string colorCode,
            int aicRating
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.name = name;
            _type = type;
            this.amp = amp;
            _config = config;
            this.colorCode = colorCode;
            _aicRating = aicRating;
            this.componentType = "Service";
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
                }
            }
        }

       
        public int Config
        {
            get => _config;
            set
            {
                if (_config != value)
                {
                    _config = value;
                    OnPropertyChanged(nameof(Config));
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
        private void ParentComponent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }



        public bool Verify()
        {
            if (!Utils.IsUuid(Name))
            {
                return false;
            }
            if (!Utils.IsUuid(ProjectId))
            {
                return false;
            }
            if (!Utils.IsEquipName(Name))
            {
                return false;
            }
            return true;
        }
    }
}
