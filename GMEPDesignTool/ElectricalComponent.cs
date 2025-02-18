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
        public float aLcl;
        public float bLcl;
        public float cLcl;
        public float lml;
        public float aLml;
        public float bLml;
        public float cLml;
        public int loadCategory;
        public string componentType;
        public int orderNo;


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual float Lcl
        {
            get => lcl;
            set
            {
                lcl = value;
                _ = OnPropertyChanged(nameof(Lcl));
            }
        }

        public virtual float ALcl
        {
            get => aLcl;
            set
            {
                aLcl = value;
                _ = OnPropertyChanged(nameof(ALcl));
            }
        }
        public virtual float BLcl
        {
            get => bLcl;
            set
            {
                bLcl = value;
                _ = OnPropertyChanged(nameof(BLcl));
            }
        }
        public virtual float CLcl
        {
            get => cLcl;
            set
            {
                cLcl = value;
                _ = OnPropertyChanged(nameof(CLcl));
            }
        }
        public virtual float Lml
        {
            get => lml;
            set
            {
                lml = value;
                _ = OnPropertyChanged(nameof(Lml));
            }
        }
        public virtual float ALml
        {
            get => aLml;
            set
            {
                aLml = value;
                _ = OnPropertyChanged(nameof(ALml));
            }
        }
        public virtual float BLml
        {
            get => bLml;
            set
            {
                bLml = value;
                _ = OnPropertyChanged(nameof(BLml));
            }
        }
        public virtual float CLml
        {
            get => cLml;
            set
            {
                cLml = value;
                _ = OnPropertyChanged(nameof(CLml));
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
                    _ = OnPropertyChanged(nameof(Id));
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
                    _ = OnPropertyChanged(nameof(ParentId));
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
                    _ = OnPropertyChanged(nameof(PhaseAVA));
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
                    _ = OnPropertyChanged(nameof(PhaseBVA));
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
                    _ = OnPropertyChanged(nameof(PhaseCVA));
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
                    _ = OnPropertyChanged(nameof(ProjectId));
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
                    _ = OnPropertyChanged(nameof(ColorCode));
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
                    _ = OnPropertyChanged(nameof(CircuitNo));
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
                    _ = OnPropertyChanged(nameof(Pole));
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
                    _ = OnPropertyChanged(nameof(Name));
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
                    _ = OnPropertyChanged(nameof(Amp));
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
                    _ = OnPropertyChanged(nameof(LoadCategory));
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
                    _ = OnPropertyChanged(nameof(ComponentType));
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
                    _ = OnPropertyChanged(nameof(OrderNo));
                }
            }
        }
        protected async Task OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                await Task.Run(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}
