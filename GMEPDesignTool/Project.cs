using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class Project
   {
    // public Structural? Stru;
    // public Mechanical? Mech;
    public Electrical? Elec;
    // public Plumbing? Plbg;
    public string Scope;
    public Project(string scope)
    {
      Scope = scope;
      if (Scope.Contains("E"))
      {
        Elec = new Electrical();
      }
    }
   }
}
