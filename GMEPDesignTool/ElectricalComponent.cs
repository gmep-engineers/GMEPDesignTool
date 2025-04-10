using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mysqlx.Crud;
using Xceed.Wpf.Toolkit;

namespace GMEPDesignTool
{
    public abstract class ElectricalComponent : INotifyPropertyChanged
    {
        public string id = Guid.NewGuid().ToString();
        public string parentId = string.Empty;
        public float phaseAVa = 0;
        public float phaseBVa = 0;
        public float phaseCVa = 0;
        public string projectId = string.Empty;
        public string colorCode = "#FFFFFFFF";
        public int circuitNo = 0;
        public string circuits = string.Empty;
        public int pole = 1;
        public string name = string.Empty;
        public float amp = 0;
        public float lcl = 0;
        public float aLcl = 0;
        public float bLcl = 0;
        public float cLcl = 0;
        public float lml = 0;
        public float aLml = 0;
        public float bLml = 0;
        public float cLml = 0;
        public float rootKva = 0;
        public int loadCategory = 1;
        public string componentType = string.Empty;
        public int orderNo = 1;
        public bool updateFlag = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableDictionary<string, string> ErrorMessages { get; set; } =
            new ObservableDictionary<string, string>();

        //public ObservableDictionary<string, string> BaseErrorMessages { get; set; } = new ObservableDictionary<string, string>();

        public virtual float Lcl
        {
            get => lcl;
            set
            {
                lcl = value;
                OnPropertyChanged(nameof(Lcl));
            }
        }
        public bool UpdateFlag
        {
            get => updateFlag;
            set
            {
                if (updateFlag != value)
                {
                    updateFlag = value;
                    OnPropertyChanged(nameof(UpdateFlag));
                }
            }
        }

        public virtual float ALcl
        {
            get => aLcl;
            set
            {
                aLcl = value;
                OnPropertyChanged(nameof(ALcl));
            }
        }
        public virtual float BLcl
        {
            get => bLcl;
            set
            {
                bLcl = value;
                OnPropertyChanged(nameof(BLcl));
            }
        }
        public virtual float CLcl
        {
            get => cLcl;
            set
            {
                cLcl = value;
                OnPropertyChanged(nameof(CLcl));
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
        public virtual float ALml
        {
            get => aLml;
            set
            {
                aLml = value;
                OnPropertyChanged(nameof(ALml));
            }
        }
        public virtual float BLml
        {
            get => bLml;
            set
            {
                bLml = value;
                OnPropertyChanged(nameof(BLml));
            }
        }
        public virtual float CLml
        {
            get => cLml;
            set
            {
                cLml = value;
                OnPropertyChanged(nameof(CLml));
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

        public virtual string Circuits
        {
            get => circuits;
            set
            {
                if (circuits != value)
                {
                    circuits = value;
                    OnPropertyChanged(nameof(Circuits));
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

        public virtual int LoadCategory
        {
            get => loadCategory;
            set
            {
                if (loadCategory != value)
                {
                    loadCategory = value;
                    OnPropertyChanged(nameof(LoadCategory));
                }
            }
        }
        public virtual string ComponentType
        {
            get => componentType;
            set
            {
                if (componentType != value)
                {
                    componentType = value;
                    OnPropertyChanged(nameof(ComponentType));
                }
            }
        }
        public virtual int OrderNo
        {
            get => orderNo;
            set
            {
                if (orderNo != value)
                {
                    orderNo = value;
                    OnPropertyChanged(nameof(OrderNo));
                }
            }
        }

        public virtual float RootKva
        {
            get => rootKva;
            set
            {
                if (rootKva != value)
                {
                    rootKva = value;
                    OnPropertyChanged(nameof(RootKva));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
