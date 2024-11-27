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
    public class ElectricalPanel : INotifyPropertyChanged
    {
        private string _id;
        private int _busSize;
        private int _mainSize;
        private bool _isMlo;
        private bool _isDistribution;
        private string _name;
        private int _colorIndex;
        private string _fedFromId;
        private bool _powered;

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricalPanel(
            string id,
            int busSize,
            int mainSize,
            bool isMlo,
            bool isDistribution,
            string name,
            int colorIndex,
            string fedFromId,
            bool powered
        )
        {
            _id = id;
            _busSize = busSize;
            _mainSize = mainSize;
            _isMlo = isMlo;
            _isDistribution = isDistribution;
            _name = name;
            _colorIndex = colorIndex;
            _fedFromId = fedFromId;
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

        public int BusSize
        {
            get => _busSize;
            set
            {
                _busSize = value;
                OnPropertyChanged();
            }
        }

        public int MainSize
        {
            get => _mainSize;
            set
            {
                _mainSize = value;
                OnPropertyChanged();
            }
        }

        public bool IsMlo
        {
            get => _isMlo;
            set
            {
                _isMlo = value;
                OnPropertyChanged();
            }
        }

        public bool IsDistribution
        {
            get => _isDistribution;
            set
            {
                _isDistribution = value;
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

        public int ColorIndex
        {
            get => _colorIndex;
            set
            {
                _colorIndex = value;
                OnPropertyChanged();
            }
        }

        public string FedFromId
        {
            get => _fedFromId;
            set
            {
                _fedFromId = value;
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
            if (!Utils.IsEquipName(Name))
            {
                return false;
            }
            if (!Utils.IsUuid(FedFromId))
            {
                return false;
            }
            return true;
        }
    }
}
