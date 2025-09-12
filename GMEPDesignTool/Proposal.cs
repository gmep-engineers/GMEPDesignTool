using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class Proposal
  {
    public string Id { get; set; }
    public string ProjectId { get; set; }
    public DateTime DateCreated { get; set; }
    public string Type { get; set; }
    public int TypeId { get; set; }
    public string EmployeeUsername { get; set; }
    public string PdfName { get; set; }
    public string Status { get; set; }
    public ProposalData? Data { get; set; }

    public Proposal() { }

    public Proposal(
      string id,
      string projectId,
      DateTime dateCreated,
      string type,
      int typeId,
      string employeeUsername,
      string pdfName,
      string status
    )
    {
      Id = id;
      ProjectId = projectId;
      DateCreated = dateCreated;
      Type = type;
      TypeId = typeId;
      EmployeeUsername = employeeUsername;
      PdfName = pdfName;
      Status = status;
    }
  }

  public class ProposalData
  {
    public StructuralScope StructuralScope { get; set; }
    public MechanicalScope MechanicalScope { get; set; }
    public ElectricalScope ElectricalScope { get; set; }
    public PlumbingScope PlumbingScope { get; set; }
    public string TotalPrice { get; set; }
    public DateTime DateSent { get; set; }
    public DateTime DateDrawingsReceived { get; set; }
    public bool HasSiteVisit { get; set; }
    public bool NewConstruction { get; set; }
    public string RetainerPercent { get; set; }
    public string NumMeetings { get; set; }
    public bool HasInitialRecommendationsMeeting { get; set; }
    public string TarrarNo { get; set; }
    public bool HasCommercialShellConnection { get; set; }
    public bool HasIndoorCommonArea { get; set; }
    public bool HasEmergencyPower { get; set; }
    public bool HasGarageExhaust { get; set; }
    public bool HasSiteLighting { get; set; }

    public ProposalData() { }
  }

  public class StructuralScope
  {
    public bool StructuralGeoReport { get; set; }
    public bool StructuralFramingDepths { get; set; }
    public bool StructuralAnalysis { get; set; }
    public bool StructuralPlans { get; set; }
    public bool StructuralDetailsCalculations { get; set; }
    public bool StructuralCodeCompliance { get; set; }

    public StructuralScope()
    {
      StructuralGeoReport = false;
      StructuralFramingDepths = false;
      StructuralAnalysis = false;
      StructuralPlans = false;
      StructuralDetailsCalculations = false;
      StructuralCodeCompliance = false;
    }
  }

  public class MechanicalScope
  {
    public bool MechanicalExhaustSupply { get; set; }
    public bool MechanicalHvacEquipSpec { get; set; }
    public bool MechanicalTitle24 { get; set; }

    public MechanicalScope()
    {
      MechanicalExhaustSupply = false;
      MechanicalHvacEquipSpec = false;
      MechanicalTitle24 = false;
    }
  }

  public class ElectricalScope
  {
    public bool ElectricalPowerDesign { get; set; }
    public bool ElectricalServiceLoadCalc { get; set; }
    public bool ElectricalSingleLineDiagram { get; set; }
    public bool ElectricalLightingDesign { get; set; }

    public ElectricalScope()
    {
      ElectricalPowerDesign = false;
      ElectricalServiceLoadCalc = false;
      ElectricalSingleLineDiagram = false;
      ElectricalLightingDesign = false;
    }
  }

  public class PlumbingScope
  {
    public bool PlumbingHotColdWater { get; set; }
    public bool PlumbingWasteVent { get; set; }

    public PlumbingScope()
    {
      PlumbingHotColdWater = false;
      PlumbingWasteVent = false;
    }
  }
}
