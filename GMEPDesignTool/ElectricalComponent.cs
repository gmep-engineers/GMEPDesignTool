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
    public abstract class ElectricalComponent : INotifyPropertyChanged
    {
        public string id;
        public string parentId;
        public float phaseAVa;
        public float phaseBVa;
        public float phaseCVa;
        public string projectId;
        public string colorCode;
        public int circuitNo;
        public int pole;
        public string name;
        public float amp;
        public float lcl;
        public float lml;


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual float Lcl
        {
            get => lcl;
            set
            {
                lcl = value;
                OnPropertyChanged(nameof(Lcl));
            }
        }
        public virtual float Lml
        {
            get => lml;
            set
            {
                lml = value;
                OnPropertyChanged(nameof(Lml));
            }
        }

        public virtual string Id
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
        public virtual string ParentId
        {
            get => parentId;
            set
            {
                if (parentId != value)
                {
                    parentId = value ?? string.Empty;
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }
        public virtual float PhaseAVA
        {
            get => phaseAVa;
            set
            {
                if (phaseAVa != value)
                {
                    phaseAVa = value;
                    OnPropertyChanged(nameof(PhaseAVA));
                }
            }
        }
        public virtual float PhaseBVA
        {
            get => phaseBVa;
            set
            {
                if (phaseBVa != value)
                {
                    phaseBVa = value;
                    OnPropertyChanged(nameof(PhaseBVA));
                }
            }
        }
        public virtual float PhaseCVA
        {
            get => phaseCVa;
            set
            {
                if (phaseCVa != value)
                {
                    phaseCVa = value;
                    OnPropertyChanged(nameof(PhaseCVA));
                }
            }
        }
        public virtual string ProjectId
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
        public virtual string ColorCode
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
        public virtual int CircuitNo
        {
            get => circuitNo;
            set
            {
                if (circuitNo != value)
                {
                    circuitNo = value;
                    OnPropertyChanged(nameof(CircuitNo));
                }
            }
        }
       
        public virtual int Pole
        {
            get => pole;
            set
            {
                if (pole != value)
                {
                    pole = value;
                    OnPropertyChanged(nameof(Pole));
                }
            }
        }
        public virtual string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public virtual float Amp
        {
            get => amp;
            set
            {
                if (amp != value)
                {
                    amp = value;
                    OnPropertyChanged(nameof(Amp));
                }
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
