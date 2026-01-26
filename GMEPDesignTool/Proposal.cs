using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ProposalContact : INotifyPropertyChanged
  {
    private string _FullName = string.Empty;
    public string FullName
    {
      get => _FullName;
      set
      {
        if (_FullName != value)
        {
          _FullName = value;
          OnPropertyChanged(nameof(FullName));
        }
      }
    }

    private string _Id = string.Empty;
    public string Id
    {
      get => _Id;
      set
      {
        if (_Id != value)
        {
          _Id = value;
          OnPropertyChanged(nameof(Id));
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class Proposal : INotifyPropertyChanged
  {
    public Database.Database db { get; set; }
    private bool _Modified = false;

    public ProposalsViewModel? ProposalsViewModel { get; set; }
    public bool Modified
    {
      get => _Modified;
      set => _Modified = value;
    }
    private string _Id = Guid.NewGuid().ToString();
    public string Id
    {
      get => _Id;
      set
      {
        if (_Id != value)
        {
          _Id = value;
          OnPropertyChanged(nameof(Id));
          _Modified = true;
        }
      }
    }

    public bool New = true;

    private string _ProjectId;
    public string ProjectId
    {
      get => _ProjectId;
      set
      {
        if (_ProjectId != value)
        {
          _ProjectId = value;
          OnPropertyChanged(nameof(ProjectId));
        }
      }
    }

    private string _ProjectNo;
    public string ProjectNo
    {
      get => _ProjectNo;
      set
      {
        if (_ProjectNo != value)
        {
          _ProjectNo = value;
          OnPropertyChanged(nameof(ProjectNo));
        }
      }
    }

    private DateTime _DateCreated;
    public DateTime DateCreated
    {
      get => _DateCreated;
      set
      {
        if (_DateCreated != value)
        {
          _DateCreated = value;
          OnPropertyChanged(nameof(DateCreated));
        }
      }
    }
    private DateTime _RfpDate = DateTime.Now;
    public DateTime RfpDate
    {
      get => _RfpDate;
      set
      {
        if (value != _RfpDate)
        {
          _RfpDate = value;
          OnPropertyChanged(nameof(RfpDate));
          _Modified = true;
        }
      }
    }

    private DateTime? _ProposalDate = DateTime.Now;
    public DateTime? ProposalDate
    {
      get => _ProposalDate;
      set
      {
        if (value != _ProposalDate)
        {
          _ProposalDate = value;
          OnPropertyChanged(nameof(ProposalDate));
          _Modified = true;
        }
      }
    }

    private bool _IsEstimate;
    public bool IsEstimate
    {
      get => _IsEstimate;
      set
      {
        if (_IsEstimate != value)
        {
          _IsEstimate = value;
          OnPropertyChanged(nameof(IsEstimate));
          _Modified = true;
        }
      }
    }
    public string Type { get; set; }

    private int _TypeId;
    public int TypeId
    {
      get => _TypeId;
      set
      {
        if (_TypeId != value)
        {
          _TypeId = value;
          OnPropertyChanged(nameof(TypeId));
          _Modified = true;
        }
      }
    }
    public string EmployeeUsername { get; set; }

    private string _SentByEmployeeId;

    public string SentByEmployeeId
    {
      get => _SentByEmployeeId;
      set
      {
        if (_SentByEmployeeId != value)
        {
          _SentByEmployeeId = value;
          OnPropertyChanged(nameof(SentByEmployeeId));
          _Modified = true;
        }
      }
    }

    private string _ContactName;
    public string ContactName
    {
      get => _ContactName;
      set
      {
        if (value != _ContactName)
        {
          _ContactName = value;
          OnPropertyChanged(nameof(ContactName));
          _Modified = true;
        }
      }
    }

    private ObservableCollection<ProposalContact> _Contacts;
    public ObservableCollection<ProposalContact> Contacts
    {
      get => _Contacts;
      set
      {
        _Contacts = value;
        OnPropertyChanged(nameof(Contacts));
      }
    }

    private string _ContactId;
    public string ContactId
    {
      get => _ContactId;
      set
      {
        if (value != _ContactId)
        {
          _ContactId = value;
          OnPropertyChanged(nameof(ContactId));
          _Modified = true;
        }
      }
    }

    private string _ClientCompanyName;

    public string CompanyName
    {
      get => _ClientCompanyName;
      set
      {
        if (value != _ClientCompanyName)
        {
          _ClientCompanyName = value;
          OnPropertyChanged(nameof(CompanyName));
        }
      }
    }

    public string ClientCompanyName
    {
      get => _ClientCompanyName;
      set
      {
        if (value != _ClientCompanyName)
        {
          _ClientCompanyName = value;
          OnPropertyChanged(nameof(ClientCompanyName));
        }
      }
    }

    private string _ClientCompanyId;

    public string ClientCompanyId
    {
      get => _ClientCompanyId;
      set
      {
        if (_ClientCompanyId != value)
        {
          _ClientCompanyId = value;
          OnPropertyChanged(nameof(ClientCompanyId));
          if (db != null)
          {
            ContactName = db.GetCompanyPrimaryContactName(_ClientCompanyId);
            CompanyName = db.GetCompanyName(_ClientCompanyId);
          }
          _Modified = true;
        }
      }
    }

    private string _ArchitectCompanyName;

    public string ArchitectCompanyName
    {
      get => _ArchitectCompanyName;
      set
      {
        if (value != _ArchitectCompanyName)
        {
          _ArchitectCompanyName = value;
          OnPropertyChanged(nameof(ArchitectCompanyName));
        }
      }
    }

    private string _ArchitectCompanyId;
    public string ArchitectCompanyId
    {
      get => _ArchitectCompanyId;
      set
      {
        if (_ArchitectCompanyId != value)
        {
          _ArchitectCompanyId = value;
          OnPropertyChanged(nameof(ArchitectCompanyId));
          if (db != null)
          {
            ArchitectCompanyName = db.GetCompanyName(ArchitectCompanyId);
          }
          _Modified = true;
        }
      }
    }

    private string _ProjectName;
    public string ProjectName
    {
      get => _ProjectName;
      set
      {
        if (_ProjectName != value)
        {
          _ProjectName = value;
          OnPropertyChanged(nameof(ProjectName));
          _Modified = true;
        }
      }
    }

    private int _Fees;
    public int Fees
    {
      get => _Fees;
      set
      {
        if (_Fees != value)
        {
          _Fees = value;
          OnPropertyChanged(nameof(Fees));
          _Modified = true;
        }
      }
    }

    private string _Notes;
    public string Notes
    {
      get => _Notes;
      set
      {
        if (_Notes != value)
        {
          _Notes = value;
          OnPropertyChanged(nameof(Notes));
          _Modified = true;
        }
      }
    }

    private DateTime? _LastFollowUpDate;
    public DateTime? LastFollowUpDate
    {
      get => _LastFollowUpDate;
      set
      {
        if (_LastFollowUpDate != value)
        {
          _LastFollowUpDate = value;
          OnPropertyChanged(nameof(LastFollowUpDate));
          _Modified = true;
        }
      }
    }

    private string _FollowedUpByEmployeeId;
    public string FollowedUpByEmployeeId
    {
      get => _FollowedUpByEmployeeId;
      set
      {
        if (value != _FollowedUpByEmployeeId)
        {
          _FollowedUpByEmployeeId = value;
          OnPropertyChanged(nameof(_FollowedUpByEmployeeId));
          _Modified = true;
        }
      }
    }

    private int _RegionId;
    public int RegionId
    {
      get => _RegionId;
      set
      {
        if (_RegionId != value)
        {
          _RegionId = value;
          OnPropertyChanged(nameof(RegionId));
          _Modified = true;
        }
      }
    }

    private string _SDrivePath;
    public string SDrivePath
    {
      get => _SDrivePath;
      set
      {
        if (_SDrivePath != value)
        {
          _SDrivePath = value;
          OnPropertyChanged(nameof(SDrivePath));
          _Modified = true;
        }
      }
    }

    public string PdfName { get; set; }
    public string Status { get; set; }

    private int _StatusId;
    public int StatusId
    {
      get => _StatusId;
      set
      {
        if (StatusId != value)
        {
          _StatusId = value;
          OnPropertyChanged(nameof(StatusId));
          _Modified = true;
        }
      }
    }
    public ProposalData? Data { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (ProposalsViewModel != null)
      {
        ProposalsViewModel.Saved = false;
      }
    }

    public Proposal() { }

    public Proposal(
      string id,
      string projectId,
      DateTime dateCreated,
      string type,
      int typeId,
      string employeeUsername,
      string pdfName,
      string status,
      int statusId
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
      StatusId = statusId;
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

  public class ProposalListItem
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string ProjectNo { get; set; }
    public string ProjectId { get; set; }
    public string Type { get; set; }

    public ProposalListItem(string id, string name, string projectId, string projectNo, string type)
    {
      Id = id;
      Name = name;
      ProjectId = projectId;
      ProjectNo = projectNo;
      Type = type;
    }
  }
}
