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
        public float rootKva;
        public float RootKva
        {
            get => rootKva;
            set => SetProperty(ref rootKva, value);
        }
        public string configuration;

        public string Configuration
        {
            get => configuration;
            set => SetProperty(ref configuration, value);
        }
        public ObservableCollection<ElectricalComponent> components { get; set; }
        public LoadSummaryViewModel(ElectricalService service)
        {
            this.service=service;
            this.components = service.childComponents;
            this.name = service.Name;
            this.rootKva = service.RootKva;
            //this.configuration = SetConfiguration(service.Type);
            service.PropertyChanged += Service_PropertyChanged;
        }

        public void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(ElectricalService.Name))
            {
                Name = service.Name;
                OnPropertyChanged(nameof(Name));
            }
            if (e.PropertyName == nameof(ElectricalService.RootKva))
            {
                RootKva = service.RootKva;
                OnPropertyChanged(nameof(RootKva));
            }
        }
        public string SetConfiguration(int type)
        {
            switch (type)
            {
                case 1:
                    return "120/208V-3P-4W";

                case 2:
                    return "120/240V-1P-3W";
                case 3:
                    return "277/480V-3P-4W";
                case 4:
                    return "120/240V-3P-4W";
                case 5:
                    return "120/208V-1P-3W";
                default:
                    return "";
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
