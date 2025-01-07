using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace GMEPDesignTool
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ElectricalService> ElectricalServices { get; set; }
        public ObservableCollection<ElectricalPanel> ElectricalPanels { get; set; }
        public ObservableCollection<ElectricalTransformer> ElectricalTransformers { get; set; }
        public ObservableCollection<ElectricalEquipment> ElectricalEquipments { get; set; }
        public ObservableCollection<ElectricalLighting> ElectricalLightings { get; set; }

        public CalculatorViewModel(
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalTransformer> transformers,
            ObservableCollection<ElectricalEquipment> equipments,
            ObservableCollection<ElectricalLighting> lightings
        )
        {
            ElectricalServices = services;
            ElectricalPanels = panels;
            ElectricalTransformers = transformers;
            ElectricalEquipments = equipments;
            ElectricalLightings = lightings;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
