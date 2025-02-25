using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    class LoadSummaryViewModel: ViewModelBase
    {
        public ElectricalService service { get; set; }
        public ObservableCollection<ElectricalComponent> components { get; set; }
        public LoadSummaryViewModel(ElectricalService service)
        {
            this.service=service;
            this.components = service.childComponents;
        }
    }
}
