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
        private int _inputVoltageIndex;
        private int _outputVoltageIndex;
        private string _name;
        private bool _isThreePhase;
        private int _kva;
        private bool _powered;
        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalTransformer(
            string id,
            string projectId,
            string parentId,
            int distanceFromParent,
            string colorCode,
            int inputVoltageIndex,
            int outputVoltageIndex,
            string name,
            bool isThreePhase,
            int kva,
            bool powered
        )
        {
            _id = id;
            _projectId = projectId;
            _colorCode = colorCode;
            _parentId = parentId;
            _distanceFromParent = distanceFromParent;
            _inputVoltageIndex = inputVoltageIndex;
            _outputVoltageIndex = outputVoltageIndex;
            _name = name;
            _isThreePhase = isThreePhase;
            _kva = kva;
            _powered = powered;
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

        public int InputVoltageIndex
        {
            get => _inputVoltageIndex;
            set
            {
                _inputVoltageIndex = value;
                OnPropertyChanged();
            }
        }

        public int OutputVoltageIndex
        {
            get => _outputVoltageIndex;
            set
            {
                _outputVoltageIndex = value;
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

        public bool IsThreePhase
        {
            get => _isThreePhase;
            set
            {
                _isThreePhase = value;
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
