using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GMEPDesignTool
{
  public class Utils
  {
    public static bool IsUuid(string s)
    {
      return Regex.IsMatch(s, @"[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}");
    }
    public static bool IsEquipName(string s)
    {
      return Regex.IsMatch(s, @"[A-Z\-0-9]{0,8}");
    }
    public static bool IsVendorName(string s)
    {
      return Regex.IsMatch(s, @"[A-Z\-0-9]{0,15}");
    }
    public Utils() { }
  }
}
