using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ProposalCommercialViewModel : INotifyPropertyChanged
    {
        public ProposalCommercialViewModel(AdminViewModel adminViewModel) {
            this.adminViewModel = adminViewModel;
        }

        private AdminViewModel adminViewModel;
        public AdminViewModel AdminViewModel
        {
            get { return adminViewModel; }
            set { adminViewModel = value; }

        }
        private bool structuralGeoReport = false;
        public bool StructuralGeoReport
        {
            get { return structuralGeoReport; }
            set { structuralGeoReport = value; }
        }

        private bool structuralFramingDepths = false;
        public bool StructuralFramingDepths
        {
            get { return structuralFramingDepths; }
            set { structuralFramingDepths = value; }
        }

        private bool structuralAnalysis = false;
        public bool StructuralAnalysis
        {
            get { return structuralAnalysis; }
            set { structuralAnalysis = value; }
        }

        private bool structuralPlans = false;
        public bool StructuralPlans
        {
            get { return structuralPlans; }
            set { structuralPlans = value; }
        }

        private bool structuralDetailsCalculations = false;
        public bool StructuralDetailsCalculations
        {
            get { return structuralDetailsCalculations; }
            set { structuralDetailsCalculations = value; }
        }

        private bool structuralCodeCompliance = false;
        public bool StructuralCodeCompliance
        {
            get { return structuralCodeCompliance; }
            set { structuralCodeCompliance = value; }
        }

        private bool mechanicalExhaustSupply = false;
        public bool MechanicalExhaustSupply
        {
            get { return mechanicalExhaustSupply; }
            set { mechanicalExhaustSupply = value; }
        }

        private bool mechanicalHvacEquipSpec = false;
        public bool MechanicalHvacEquipSpec
        {
            get { return mechanicalHvacEquipSpec; }
            set { MechanicalHvacEquipSpec = value; }
        }

        private bool mechanicalTitle24 = false;
        public bool MechanicalTitle24
        {
            get { return mechanicalTitle24; }
            set { value = mechanicalTitle24; }
        }

        private bool electricalPowerDesign = false;
        public bool ElectricalPowerDesign
        {
            get { return electricalPowerDesign; }
            set { electricalPowerDesign = value; }
        }

        private bool electricalServiceLoadCalc = false;
        public bool ElectricalServiceLoadCalc
        {
            get { return electricalServiceLoadCalc; }
            set { electricalServiceLoadCalc = value; }
        }

        private bool electricalSingleLineDiagram = false;
        public bool ElectricalSingleLineDiagram
        {
            get { return electricalSingleLineDiagram; }
            set { electricalSingleLineDiagram = value; }
        }

        private bool electricalLightingDesign = false;
        public bool ElectricalLightingDesign
        {
            get { return electricalLightingDesign; }
            set { electricalLightingDesign = value; }
        }

        private bool plumbingHotColdWater = false;
        public bool PlumbingHotColdWater
        {
            get { return plumbingHotColdWater; }
            set { plumbingHotColdWater = value; }
        }

        private bool plumbingWasteVent = false;
        public bool PlumbingWasteVent
        {
            get { return plumbingWasteVent; }
            set { plumbingWasteVent = value; }
        }

        public ObservableCollection<string> Types { get; set; } =
            new ObservableCollection<string>
            {
                "loyal",
                "returning",
                "new",
            };

        private string clientType;
        public string ClientType
        {
            get => clientType;
            set
            {
                if (clientType != value)
                {
                    clientType = value;
                    OnPropertyChanged(nameof(ClientType));
                }
            }
        }

        private bool newConstruction;
        public bool NewConstruction
        {
            get => newConstruction;
            set
            {
                if (newConstruction != value)
                {
                    newConstruction = value;
                    OnPropertyChanged(nameof(NewConstruction));
                }
            }
        }
        private bool hasSiteVisit;
        public bool HasSiteVisit
        {
            get => hasSiteVisit;
            set
            {
                if (hasSiteVisit != value)
                {
                    hasSiteVisit = value;
                    OnPropertyChanged(nameof(HasSiteVisit));
                }
            }
        }
        private bool hasInitialRecommendationsMeeting;
        public bool HasInitialRecommendationsMeeting
        {
            get => hasInitialRecommendationsMeeting;
            set
            {
                if (hasInitialRecommendationsMeeting != value)
                {
                    hasInitialRecommendationsMeeting = value;
                    OnPropertyChanged(nameof(HasInitialRecommendationsMeeting));
                }
            }
        }
        private bool hasCommericalShellConnection;
        public bool HasCommericalShellConnection
        {
            get => hasCommericalShellConnection;
            set
            {
                if (hasCommericalShellConnection != value)
                {
                    hasCommericalShellConnection = value;
                    OnPropertyChanged(nameof(HasCommericalShellConnection));
                }
            }
        }
        private bool hasEmergencyPower;
        public bool HasEmergencyPower
        {
            get => hasEmergencyPower;
            set
            {
                if (hasEmergencyPower != value)
                {
                    hasEmergencyPower = value;
                    OnPropertyChanged(nameof(HasEmergencyPower));
                }
            }
        }
        private bool hasIndoorCommonArea;
        public bool HasIndoorCommonArea
        {
            get => hasIndoorCommonArea;
            set
            {
                if (hasIndoorCommonArea != value)
                {
                    hasIndoorCommonArea = value;
                    OnPropertyChanged(nameof(HasIndoorCommonArea));
                }
            }
        }
        private bool hasGarageExhaust;
        public bool HasGarageExhaust
        {
            get => hasGarageExhaust;
            set
            {
                if (hasGarageExhaust != value)
                {
                    hasGarageExhaust = value;
                    OnPropertyChanged(nameof(HasGarageExhaust));
                }
            }
        }
        private bool hasSiteLighting;
        public bool HasSiteLighting
        {
            get => hasSiteLighting;
            set
            {
                if (hasSiteLighting != value)
                {
                    hasSiteLighting = value;
                    OnPropertyChanged(nameof(HasSiteLighting));
                }
            }
        }

        private DateTime? dateSent;
        public DateTime? DateSent
        {
            get => dateSent;
            set
            {
                if (dateSent != value)
                {
                    dateSent = value;
                    OnPropertyChanged(nameof(DateSent));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
