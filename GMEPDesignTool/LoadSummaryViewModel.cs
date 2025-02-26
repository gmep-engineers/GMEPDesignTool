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

        public float totalAmp;
        public float TotalAmp
        {
            get => totalAmp;
            set => SetProperty(ref totalAmp, value);
        }
        public float amp;
        public float Amp
        {
            get => amp;
            set => SetProperty(ref amp, value);
        }

        public bool canHandleLoad;
        public bool CanHandleLoad
        {
            get => canHandleLoad;
            set => SetProperty(ref canHandleLoad, value);
        }
        public ObservableCollection<ElectricalComponent> components { get; set; }
        public LoadSummaryViewModel(ElectricalService service)
        {
            this.service=service;
            this.components = service.childComponents;
            this.name = service.Name;
            this.rootKva = service.RootKva;
            this.totalAmp = service.TotalAmp;
            this.configuration = SetConfiguration(service.Type);
            this.Amp = SetAmp(service.Amp);
            this.CanHandleLoad = DetermineCanHandleLoad();
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
            if (e.PropertyName == nameof(ElectricalService.Type))
            {
                Configuration = SetConfiguration(service.Type);
                OnPropertyChanged(nameof(Type));
            }
            if (e.PropertyName == nameof(ElectricalService.TotalAmp))
            {
                TotalAmp = service.TotalAmp;
                CanHandleLoad = DetermineCanHandleLoad();
                OnPropertyChanged(nameof(Type));
            }
            if (e.PropertyName == nameof(ElectricalService.Amp))
            {
                Amp = SetAmp(service.Amp);
                CanHandleLoad = DetermineCanHandleLoad();
                OnPropertyChanged(nameof(Type));
            }
        }
        public string SetConfiguration(int type)
        {
            switch (type)
            {
                case 1:
                    return "120/208V-3Φ-4W";
                case 2:
                    return "120/240V-1Φ-3W";
                case 3:
                    return "277/480V-3Φ-4W";
                case 4:
                    return "120/240V-3Φ-4W";
                case 5:
                    return "120/208V-1Φ-3W";
                default:
                    return "";
            }
        }
        public float SetAmp(float amp)
        {
            switch (amp)
            {
                case 1:
                    return 100;
                case 2:
                    return 200;
                case 3:
                    return 400;
                case 4:
                    return 600;
                case 5:
                    return 800;
                case 6:
                    return 1000;
                case 7:
                    return 1200;
                case 8:
                    return 1600;
                case 9:
                    return 2000;
                case 10:
                    return 2200;
                case 11:
                    return 3000;
                case 12:
                    return 4000;
                default:
                    return 1;
            }
        }
        public bool DetermineCanHandleLoad()
        {
            if (TotalAmp <= Amp)
            {
               return true;
            }
            return false;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
