using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ElectricalPanel
  {
    private string Id;
    private int BusSize;
    private int MainSize;
    private bool IsMlo;
    private bool IsDistribution;
    private string Name;
    private int ColorIndex;
    private string FedFromId;
    public ElectricalPanel(string id, int busSize, int mainSize, bool isMlo, bool isDistribution, string name, int colorIndex, string fedFromId)
    {
      Id = id;
      BusSize = busSize;
      MainSize = mainSize;
      IsMlo = isMlo;
      IsDistribution = isDistribution;
      Name = name;
      ColorIndex = colorIndex;
      FedFromId = fedFromId;
    }
    public bool Verify()
    {
      if (!Utils.IsUuid(Id)) { return false; }
      if (!Utils.IsEquipName(Name)) { return false; }
      if (!Utils.IsUuid(FedFromId)) { return false; }
      return true;
    }
  }
}
