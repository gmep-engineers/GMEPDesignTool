using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GMEPDesignTool
{
    public abstract class ElectricalComponent : INotifyPropertyChanged
    {
        private string _id;
        private string _projectId;
        private string _colorCode;
        private string _parentId;
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
            get => _parentId ?? "";
            set
            {
                _parentId = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
