using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalTransformer : INotifyPropertyChanged
    {
        private string _id;
        private string _projectId;
        private string _colorCode;
        private string _parentId;
        private int _distanceFromParent;
        private int _voltage;
        private string _name;
        private int _kva;
        private bool _powered;
        private bool _isHiddenOnPlan;
        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalTransformer(
            string id,
            string projectId,
            string parentId,
            int distanceFromParent,
            string colorCode,
            int voltage,
            string name,
            int kva,
            bool powered,
            bool isHiddenOnPlan
        )
        {
            _id = id;
            _projectId = projectId;
            _colorCode = colorCode;
            _parentId = parentId;
            _distanceFromParent = distanceFromParent;
            _voltage = voltage;
            _name = name;
            _kva = kva;
            _powered = powered;
            _isHiddenOnPlan = isHiddenOnPlan;
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

        public string ColorCode
        {
            get => _colorCode;
            set
            {
                _colorCode = value;
                OnPropertyChanged();
            }
        }

        public string ParentId
        {
            get => _parentId;
            set
            {
                _parentId = value;
                OnPropertyChanged();
            }
        }

        public int DistanceFromParent
        {
            get => _distanceFromParent;
            set
            {
                _distanceFromParent = value;
                OnPropertyChanged();
            }
        }

        public int Voltage
        {
            get => _voltage;
            set
            {
                _voltage = value;
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

        public int Kva
        {
            get => _kva;
            set
            {
                _kva = value;
                OnPropertyChanged();
            }
        }

        public bool Powered
        {
            get => _powered;
            set
            {
                _powered = value;
                OnPropertyChanged();
            }
        }
        public bool IsHiddenOnPlan
        {
            get => _isHiddenOnPlan;
            set
            {
                _isHiddenOnPlan = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Verify()
        {
            if (!Utils.IsUuid(Id))
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
