using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ElectricalService
  {
    public string Id;
    public string ProjectId;
    public string Name;
    public string Voltage;
    public int Amp;
    public ElectricalService(string id, string projectId, string name, string voltage, int amp)
    {
      Id = id;
      ProjectId = projectId;
      Name = name;
      Voltage = voltage;
      Amp = amp;
    }
    public bool Verify()
    {
      if (!Utils.IsUuid(Name))
      { return false; }
      if (!Utils.IsUuid(ProjectId))
      { return false; }
      if (!Utils.IsEquipName(Name))
      { return false; }
      return true;
    }
  }
}
