using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GMEPDesignTool
{
    class LoadSummaryViewModel : ViewModelBase
    {
        public ElectricalService service { get; set; }
        public string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        public ObservableCollection<ElectricalComponent> components { get; set; }
        public LoadSummaryViewModel(ElectricalService service)
        {
            this.service=service;
            this.components = service.childComponents;
            this.name = service.Name;
            service.PropertyChanged += Service_PropertyChanged;
        }

        public void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(ElectricalService.Name))
            {
                Name = service.Name;
                OnPropertyChanged(nameof(Name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
