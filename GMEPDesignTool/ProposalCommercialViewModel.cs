using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ProposalCommercialViewModel
    {
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
        public bool PumbingWasteVent
        {
            get { return plumbingWasteVent; }
            set { plumbingWasteVent = value; }
        }

        public ProposalCommercialViewModel() { }
    }
}
