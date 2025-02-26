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
        private int _type;
        private int _config;
        private int _aicRating;
        public ObservableCollection<ElectricalComponent> childComponents { get; set; } = new ObservableCollection<ElectricalComponent>();
        public ElectricalService(
            string id,
            string projectId,
            string name,
            int type,
            int amp,
            int config,
            string colorCode,
            int aicRating
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
        public void AssignPanel(ElectricalPanel panel)
        {
            childComponents.Add(panel);
            panel.PropertyChanged += Panel_PropertyChanged;
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
        public void AssignTransformer(ElectricalTransformer transformer)
        {
            childComponents.Add(transformer);
            transformer.PropertyChanged += Transformer_PropertyChanged;
        }
        private void Transformer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElectricalTransformer.ParentId))
            {
                if (sender is ElectricalTransformer transformer)
                {
                    transformer.PropertyChanged -= Transformer_PropertyChanged;
                    childComponents.Remove(transformer);
                    calculateRootKva();
                }
            }
            if (e.PropertyName == nameof(ElectricalTransformer.RootKva))
            {
                calculateRootKva();
            }
        }
        public void DownloadComponents(ObservableCollection<ElectricalPanel> panels, ObservableCollection<ElectricalTransformer> transformers)
        {
            foreach (var panel in panels)
            {
                if (panel.ParentId == Id)
                {
                    childComponents.Add(panel);
                    panel.PropertyChanged += Panel_PropertyChanged;
                }
            }
            foreach (var transformer in transformers)
            {
                if (transformer.ParentId == Id)
                {
                    childComponents.Add(transformer);
                    transformer.PropertyChanged += Transformer_PropertyChanged;
                }
            }
            calculateRootKva();
        }
        public void calculateRootKva()
        {
            RootKva = 0;
            foreach(var component in childComponents)
            {
                RootKva += component.RootKva;
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
