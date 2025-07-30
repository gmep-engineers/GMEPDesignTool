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
        public ProposalCommercialViewModel(AdminViewModel adminViewModel, SelectProposalTypeViewModel selectProposalTypeViewModel) {
            this.adminViewModel = adminViewModel;
            this.selectProposalTypeViewModel = selectProposalTypeViewModel;
        }

        private AdminViewModel adminViewModel;
        public AdminViewModel AdminViewModel
        {
            get { return adminViewModel; }
            set { adminViewModel = value; }

        }


        private SelectProposalTypeViewModel selectProposalTypeViewModel;
        public SelectProposalTypeViewModel SelectProposalTypeViewModel
        {
            get { return selectProposalTypeViewModel; }
            set { selectProposalTypeViewModel = value; }

        }

        private bool structuralGeoReport = false;
        public bool StructuralGeoReport
        {
            get { return structuralGeoReport; }
            set {
                if (structuralGeoReport != value)
                {
                    structuralGeoReport = value;
                    OnPropertyChanged(nameof(StructuralGeoReport));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralFramingDepths = false;
        public bool StructuralFramingDepths
        {
            get { return structuralFramingDepths; }
            set {
                if (structuralFramingDepths != value)
                {
                    structuralFramingDepths = value;
                    OnPropertyChanged(nameof(StructuralFramingDepths));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralAnalysis = false;
        public bool StructuralAnalysis
        {
            get { return structuralAnalysis; }
            set {
                if (structuralAnalysis != value)
                {
                    structuralAnalysis = value;
                    OnPropertyChanged(nameof(StructuralAnalysis));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralPlans = false;
        public bool StructuralPlans
        {
            get { return structuralPlans; }
            set {
                if (structuralPlans != value)
                {
                    structuralPlans = value;
                    OnPropertyChanged(nameof(StructuralPlans));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralDetailsCalculations = false;
        public bool StructuralDetailsCalculations
        {
            get { return structuralDetailsCalculations; }
            set {
                if (structuralDetailsCalculations != value)
                {
                    structuralDetailsCalculations = value;
                    OnPropertyChanged(nameof(StructuralDetailsCalculations));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralCodeCompliance = false;
        public bool StructuralCodeCompliance
        {
            get { return structuralCodeCompliance; }
            set {
                if (structuralCodeCompliance != value)
                {
                    structuralCodeCompliance = value;
                    OnPropertyChanged(nameof(StructuralCodeCompliance));
                    UpdateStructuralCheckAll();
                }
            }
        }

        private bool structuralCheckAll;
        public bool StructuralCheckAll
        {
            get => structuralCheckAll;
            set
            {
                if (structuralCheckAll != value)
                {
                    structuralCheckAll = value;
                    OnPropertyChanged(nameof(StructuralCheckAll));

                    if (value)
                    {
                        // Check all
                        StructuralGeoReport = true;
                        StructuralFramingDepths = true;
                        StructuralAnalysis = true;
                        StructuralPlans = true;
                        StructuralDetailsCalculations = true;
                        StructuralCodeCompliance = true;
                    }
                    else
                    {
                        // Uncheck all
                        StructuralGeoReport = false;
                        StructuralFramingDepths = false;
                        StructuralAnalysis = false;
                        StructuralPlans = false;
                        StructuralDetailsCalculations = false;
                        StructuralCodeCompliance = false;
                    }
                }
            }
        }


        private void UpdateStructuralCheckAll()
        {
            bool allChecked =
                StructuralGeoReport &&
                StructuralFramingDepths &&
                StructuralAnalysis &&
                StructuralPlans &&
                StructuralDetailsCalculations &&
                StructuralCodeCompliance;

            if (structuralCheckAll != allChecked)
            {
                structuralCheckAll = allChecked;
                OnPropertyChanged(nameof(StructuralCheckAll));
            }
        }


        private bool mechanicalExhaustSupply = false;
        public bool MechanicalExhaustSupply
        {
            get { return mechanicalExhaustSupply; }
            set {
                if (mechanicalExhaustSupply != value)
                {
                    mechanicalExhaustSupply = value;
                    OnPropertyChanged(nameof(MechanicalExhaustSupply));
                    UpdateMechanicalCheckAll();
                }
            }
        }

        private bool mechanicalHvacEquipSpec = false;
        public bool MechanicalHvacEquipSpec
        {
            get { return mechanicalHvacEquipSpec; }
            set {
                if (mechanicalHvacEquipSpec != value)
                {
                    mechanicalHvacEquipSpec = value;
                    OnPropertyChanged(nameof(MechanicalHvacEquipSpec));
                    UpdateMechanicalCheckAll();
                }
            }
        }

        private bool mechanicalTitle24 = false;
        public bool MechanicalTitle24
        {
            get { return mechanicalTitle24; }
            set {
                if (mechanicalTitle24 != value)
                {
                    mechanicalTitle24 = value;
                    OnPropertyChanged(nameof(MechanicalTitle24));
                    UpdateMechanicalCheckAll();
                }
            }
        }

        private bool mechanicalCheckAll;
        public bool MechanicalCheckAll
        {
            get => mechanicalCheckAll;
            set
            {
                if (mechanicalCheckAll != value)
                {
                    mechanicalCheckAll = value;
                    OnPropertyChanged(nameof(MechanicalCheckAll));

                    if (value)
                    {
                        // Check all
                        MechanicalExhaustSupply = true;
                        MechanicalHvacEquipSpec = true;
                        MechanicalTitle24 = true;
                    }
                    else
                    {
                        // Uncheck all
                        MechanicalExhaustSupply = false;
                        MechanicalHvacEquipSpec = false;
                        MechanicalTitle24 = false;
                    }
                }
            }
        }


        private void UpdateMechanicalCheckAll()
        {
            bool allChecked =
                MechanicalExhaustSupply &&
                MechanicalHvacEquipSpec &&
                MechanicalTitle24;

            if (mechanicalCheckAll != allChecked)
            {
                mechanicalCheckAll = allChecked;
                OnPropertyChanged(nameof(MechanicalCheckAll));
            }
        }

        private bool electricalPowerDesign = false;
        public bool ElectricalPowerDesign
        {
            get { return electricalPowerDesign; }
            set {
                if (electricalPowerDesign != value)
                {
                    electricalPowerDesign = value;
                    OnPropertyChanged(nameof(ElectricalPowerDesign));
                    UpdateElectricalCheckAll();
                }
            }
        }

        private bool electricalServiceLoadCalc = false;
        public bool ElectricalServiceLoadCalc
        {
            get { return electricalServiceLoadCalc; }
            set {
                if (electricalServiceLoadCalc != value)
                {
                    electricalServiceLoadCalc = value;
                    OnPropertyChanged(nameof(ElectricalServiceLoadCalc));
                    UpdateElectricalCheckAll();
                }
            }
        }

        private bool electricalSingleLineDiagram = false;
        public bool ElectricalSingleLineDiagram
        {
            get { return electricalSingleLineDiagram; }
            set {
                if (electricalSingleLineDiagram != value)
                {
                    electricalSingleLineDiagram = value;
                    OnPropertyChanged(nameof(ElectricalSingleLineDiagram));
                    UpdateElectricalCheckAll();
                }
            }
        }

        private bool electricalLightingDesign = false;
        public bool ElectricalLightingDesign
        {
            get { return electricalLightingDesign; }
            set {
                if (electricalLightingDesign != value)
                {
                    electricalLightingDesign = value;
                    OnPropertyChanged(nameof(ElectricalLightingDesign));
                    UpdateElectricalCheckAll();
                }
            }
        }

        private bool electricalCheckAll;
        public bool ElectricalCheckAll
        {
            get => electricalCheckAll;
            set
            {
                if (electricalCheckAll != value)
                {
                    electricalCheckAll = value;
                    OnPropertyChanged(nameof(ElectricalCheckAll));

                    if (value)
                    {
                        // Check all
                        ElectricalPowerDesign = true;
                        ElectricalServiceLoadCalc = true;
                        ElectricalSingleLineDiagram = true;
                        ElectricalLightingDesign = true;
                    }
                    else
                    {
                        // Uncheck all
                        ElectricalPowerDesign = false;
                        ElectricalServiceLoadCalc = false;
                        ElectricalSingleLineDiagram = false;
                        ElectricalLightingDesign = false;
                    }
                }
            }
        }


        private void UpdateElectricalCheckAll()
        {
            bool allChecked =
                ElectricalPowerDesign &&
                ElectricalServiceLoadCalc &&
                ElectricalSingleLineDiagram &&
                ElectricalLightingDesign;

            if (electricalCheckAll != allChecked)
            {
                electricalCheckAll = allChecked;
                OnPropertyChanged(nameof(ElectricalCheckAll));
            }
        }

        private bool plumbingHotColdWater = false;
        public bool PlumbingHotColdWater
        {
            get { return plumbingHotColdWater; }
            set {
                if (plumbingHotColdWater != value)
                {
                    plumbingHotColdWater = value;
                    OnPropertyChanged(nameof(PlumbingHotColdWater));
                    UpdatePlumbingCheckAll();
                }
            }
        }

        private bool plumbingWasteVent = false;
        public bool PlumbingWasteVent
        {
            get { return plumbingWasteVent; }
            set {
                if (plumbingWasteVent != value)
                {
                    plumbingWasteVent = value;
                    OnPropertyChanged(nameof(PlumbingWasteVent));
                    UpdatePlumbingCheckAll();
                }
            }
        }

        private bool plumbingCheckAll;
        public bool PlumbingCheckAll
        {
            get => plumbingCheckAll;
            set
            {
                if (plumbingCheckAll != value)
                {
                    plumbingCheckAll = value;
                    OnPropertyChanged(nameof(PlumbingCheckAll));

                    if (value)
                    {
                        // Check all
                        PlumbingHotColdWater = true;
                        PlumbingWasteVent = true;
                    }
                    else
                    {
                        // Uncheck all
                        PlumbingHotColdWater = false;
                        PlumbingWasteVent = false;
                    }
                }
            }
        }


        private void UpdatePlumbingCheckAll()
        {
            bool allChecked =
                PlumbingHotColdWater &&
                PlumbingWasteVent;

            if (plumbingCheckAll != allChecked)
            {
                plumbingCheckAll = allChecked;
                OnPropertyChanged(nameof(PlumbingCheckAll));
            }
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

        
        private DateTime? dateDrawingsReceived;
        public DateTime? DateDrawingsReceived
        {
            get => dateDrawingsReceived;
            set
            {
                if (dateDrawingsReceived != value)
                {
                    dateDrawingsReceived = value;
                    OnPropertyChanged(nameof(DateDrawingsReceived));
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
