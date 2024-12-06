using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalService : INotifyPropertyChanged
    {
        private string _id;
        private string _projectId;
        private string _name;
        private int _type;
        private int _amp;

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalService(string id, string projectId, string name, int type, int amp)
        {
            _id = id;
            _projectId = projectId;
            _name = name;
            _type = type;
            _amp = amp;
        }

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public int Amp
        {
            get => _amp;
            set
            {
                _amp = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
