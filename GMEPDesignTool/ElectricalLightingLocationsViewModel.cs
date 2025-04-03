using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    class ElectricalLightingLocationsViewModel : ViewModelBase
    {
        public ObservableCollection<Location> LightingLocations { get; set; }
        Location? SelectedLocation { get; set; }

        public ElectricalLightingLocationsViewModel(
            ObservableCollection<Location> lightingLocations
        )
        {
            LightingLocations = lightingLocations;
        }
    }
}
