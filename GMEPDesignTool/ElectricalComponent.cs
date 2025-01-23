using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit;

namespace GMEPDesignTool
{
    internal abstract class ElectricalComponent
    {
        public string _id;
        public string _parentId;
        public float _va;
        public string _projectId;
        public string _colorCode;
        public int _pole;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        public string ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }
        public float Va
        {
            get => _va;
            set
            {
                if (_va != value)
                {
                    _va = value;
                    OnPropertyChanged(nameof(Va));
                }
            }
        }
        public string ProjectId
        {
            get => _projectId;
            set
            {
                if (_projectId != value)
                {
                    _projectId = value;
                    OnPropertyChanged(nameof(ProjectId));
                }
            }
        }
        public string ColorCode
        {
            get => _colorCode;
            set
            {
                if (_colorCode != value)
                {
                    _colorCode = value;
                    OnPropertyChanged(nameof(ColorCode));
                }
            }
        }

        public int Pole
        {
            get => _pole;
            set
            {
                if (_pole != value)
                {
                    _pole = value;
                }
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
