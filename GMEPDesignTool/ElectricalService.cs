using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalService : ElectricalComponent
    {
        private int _type = 1;
        private int _config = 1;
        private int _aicRating = 0;
        private float _totalAmp = 0;
        public ObservableCollection<ElectricalComponent> childComponents { get; set; } = new ObservableCollection<ElectricalComponent>();
        public ObservableCollection<ElectricalTransformer> childTransformers { get; set; } = new ObservableCollection<ElectricalTransformer>();
        public ElectricalService(
            string id,
            string projectId,
            string name,
            int type,
            int amp,
            int config,
            string colorCode,
            int aicRating,
            string parentId
        )
        {
            this.id = id;
            this.projectId = projectId;
            this.name = name;
            _type = type;
            this.amp = amp;
            _config = config;
            this.colorCode = colorCode;
            _aicRating = aicRating;
            this.componentType = "Service";
            this.parentId = parentId;
        }
        public ElectricalService()
        {
            this.amp = 1;
            this.componentType = "Service";
        }

        public int Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                    calculateRootKva();
                }
            }
        }

       
        public int Config
        {
            get => _config;
            set
            {
                if (_config != value)
                {
                    _config = value;
                    OnPropertyChanged(nameof(Config));
                }
            }
        }

        public int AicRating
        {
            get => _aicRating;
            set
            {
                if (_aicRating != value)
                {
                    _aicRating = value;
                    OnPropertyChanged(nameof(AicRating));
                }
            }
        }
        public float TotalAmp
        {
            get => _totalAmp;
            set
            {
                if (_totalAmp != value)
                {
                    _totalAmp = value;
                    OnPropertyChanged(nameof(TotalAmp));
                }
            }
        }
        public void AssignPanel(ElectricalPanel panel)
        {
            childComponents.Add(panel);
            panel.PropertyChanged += Panel_PropertyChanged;
            calculateRootKva();
        }
        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalPanel.ParentId))
            {
                if (sender is ElectricalPanel panel)
                {
                    panel.PropertyChanged -= Panel_PropertyChanged;
                    childComponents.Remove(panel);
                    calculateRootKva();
                }
                
            }
            if (e.PropertyName == nameof(ElectricalPanel.RootKva))
            {
                calculateRootKva();
            }
        }
        public void AssignService(ElectricalService service)
        {
            childComponents.Add(service);
            service.PropertyChanged += Service_PropertyChanged;
            calculateRootKva();
        }
        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalService.ParentId))
            {
                if (sender is ElectricalService service)
                {
                    service.PropertyChanged -= Service_PropertyChanged;
                    childComponents.Remove(service);
                    calculateRootKva();
                }

            }
            if (e.PropertyName == nameof(ElectricalService.RootKva))
            {
                calculateRootKva();
            }
        }
        public void AssignTransformer(ElectricalTransformer transformer)
        {
            childTransformers.Add(transformer);
            transformer.PropertyChanged += Transformer_PropertyChanged;
            if (transformer.ChildPanel != null)
            {
                childComponents.Add(transformer.ChildPanel);
                transformer.ChildPanel.PropertyChanged += Panel_PropertyChanged;
            }
            calculateRootKva();
        }

        private void Transformer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalTransformer.ParentId))
            {
                if (sender is ElectricalTransformer transformer)
                {
                    transformer.PropertyChanged -= Transformer_PropertyChanged;                   
                    childTransformers.Remove(transformer);
                    if (transformer.ChildPanel != null)
                    {
                        childComponents.Remove(transformer.ChildPanel);
                        transformer.ChildPanel.PropertyChanged -= Panel_PropertyChanged;
                    }
                    calculateRootKva();
                }
            }
            
            if (e.PropertyName == nameof(ElectricalTransformer.ChildPanel))
            {
                if (sender is ElectricalTransformer transformer)
                {
                    transformer.ChildPanel.PropertyChanged += Panel_PropertyChanged;
                    childComponents.Add(transformer.ChildPanel);
                    calculateRootKva();
                }
            }
        }
        public void DownloadComponents(ObservableCollection<ElectricalPanel> panels, ObservableCollection<ElectricalTransformer> transformers, ObservableCollection<ElectricalService> services)
        {
            foreach (var panel in panels)
            {
                if (panel.ParentId == Id)
                {
                    childComponents.Add(panel);
                    panel.PropertyChanged += Panel_PropertyChanged;
                }
            }
            foreach (var service in services)
            {
                if (service.ParentId == Id)
                {
                    childComponents.Add(service);
                    service.PropertyChanged += Service_PropertyChanged;
                }
            }
            foreach (var transformer in transformers)
            {
                if (transformer.ParentId == Id)
                {
                    childTransformers.Add(transformer);
                    transformer.PropertyChanged += Transformer_PropertyChanged;

                    if (transformer.ChildPanel != null)
                    {
                        childComponents.Add(transformer.ChildPanel);
                        transformer.ChildPanel.PropertyChanged += Panel_PropertyChanged;
                    }
                }
            }
            calculateRootKva();
        }
        public void calculateRootKva()
        {
            RootKva = 0;
            TotalAmp = 0;
            foreach(var component in childComponents)
            {
                RootKva += component.RootKva;
            }
            TotalAmp = (float)((RootKva*1000)/(typeToVoltage(Type)));
            if (Type == 1 || Type == 3 || Type == 4)
            {
                TotalAmp = (float)(TotalAmp / 1.732);
            }
        }
        public int typeToVoltage(int type)
        {
            switch (type)
            {
                case 1:
                    return 208;
                case 2:
                    return 240;
                case 3:
                    return 480;
                case 4:
                    return 240;
                case 5:
                    return 208;
                default:
                    return 1;

            }
        }

        public bool Verify()
        {
            if (!Utils.IsUuid(Name))
            {
                return false;
            }
            if (!Utils.IsUuid(ProjectId))
            {
                return false;
            }
            if (!Utils.IsEquipName(Name))
            {
                return false;
            }
            return true;
        }
    }
}
